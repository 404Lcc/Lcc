using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.Profiling;

namespace Pathfinding.PID {
	using Pathfinding.Drawing;
	using Pathfinding.Util;
	using Palette = Pathfinding.Drawing.Palette.Colorbrewer.Set1;
	using Unity.Jobs;
	using Unity.Profiling;
	using UnityEngine.Assertions;
	using Unity.Burst;
	using Unity.Collections.LowLevel.Unsafe;
	using Pathfinding.RVO;

	/// <summary>Core control loop for the <see cref="FollowerEntity"/> movement script</summary>
	[System.Serializable]
	[BurstCompile]
	public struct PIDMovement {
		public struct PersistentState {
			public float maxDesiredWallDistance;
		}

		/// <summary>
		/// Desired rotation speed in degrees per second.
		///
		/// If the agent is in an open area and gets a new destination directly behind itself, it will start to rotate around with exactly this rotation speed.
		///
		/// The agent will slow down its rotation speed as it approaches its desired facing direction.
		/// So for example, when it is only 90 degrees away from its desired facing direction, it will only rotate with about half this speed.
		///
		/// See: <see cref="maxRotationSpeed"/>
		/// </summary>
		public float rotationSpeed;

		/// <summary>
		/// Desired speed of the agent in meters per second.
		///
		/// This will be multiplied by the agent's scale to get the actual speed.
		/// </summary>
		public float speed;

		/// <summary>
		/// Maximum rotation speed in degrees per second.
		///
		/// If the agent would have to rotate faster than this, it will instead slow down to get more time to rotate.
		///
		/// The agent may want to rotate faster than <see cref="rotationSpeed"/> if there's not enough space, so that it has to move in a more narrow arc.
		/// It may also want to rotate faster if it is very close to its destination and it wants to make sure it ends up on the right spot without any circling.
		///
		/// It is recommended to keep this at a value slightly larger than <see cref="rotationSpeed"/>.
		///
		/// See: <see cref="rotationSpeed"/>
		/// </summary>
		public float maxRotationSpeed;

		/// <summary>
		/// Maximum rotation speed in degrees per second while rotating on the spot.
		///
		/// Only used if <see cref="allowRotatingOnSpot"/> is enabled.
		/// </summary>
		public float maxOnSpotRotationSpeed;

		/// <summary>
		/// Time for the agent to slow down to a complete stop when it approaches the destination point, in seconds.
		///
		/// One can calculate the deceleration like: <see cref="speed"/>/<see cref="slowdownTime"/> (with units m/s^2).
		/// </summary>
		public float slowdownTime;

		/// <summary>
		/// Time for the agent to slow down to a complete stop when rotating on the spot.
		///
		/// If set to zero, the agent will instantly stop and start to turn around.
		///
		/// Only used if <see cref="allowRotatingOnSpot"/> is enabled.
		/// </summary>
		public float slowdownTimeWhenTurningOnSpot;

		/// <summary>
		/// How big of a distance to try to keep from obstacles.
		///
		/// Typically around 1 or 2 times the agent radius is a good value for this.
		///
		/// Try to avoid making it so large that there might not be enough space for the agent to keep this amount of distance from obstacles.
		/// It may start to move less optimally if it is not possible to keep this distance.
		///
		/// This works well in open spaces, but if your game consists of a lot of tight corridors, a low, or zero value may be better.
		///
		/// This will be multiplied by the agent's scale to get the actual distance.
		/// </summary>
		public float desiredWallDistance;

		/// <summary>
		/// How wide of a turn to make when approaching a destination for which a desired facing direction has been set.
		///
		/// The following video shows three agents, one with no facing direction set, and then two agents with varying values of the lead in radius.
		/// [Open online documentation to see videos]
		///
		/// Setting this to zero will make the agent move directly to the end of the path and rotate on the spot to face the desired facing direction, once it is there.
		///
		/// When approaching a destination for which no desired facing direction has been set, this field has no effect.
		///
		/// Warning: Setting this to a too small (but non-zero) value may look bad if the agent cannot rotate fast enough to stay on the arc.
		///
		/// This will be multiplied by the agent's scale to get the actual radius.
		/// </summary>
		public float leadInRadiusWhenApproachingDestination;

		/// <summary>
		/// If rotation on the spot is allowed or not.
		///
		/// When the agent wants to turn significantly, enabling this will make it turn on the spot instead of moving in an arc.
		/// This can make for more responsive and natural movement for humanoid characters.
		/// </summary>
		public bool allowRotatingOnSpot {
			get => allowRotatingOnSpotBacking != 0;
			set => allowRotatingOnSpotBacking = (byte)(value ? 1 : 0);
		}

		/// <summary>
		/// If rotation on the spot is allowed or not.
		/// 1 for allowed, 0 for not allowed.
		///
		/// That we have to use a byte instead of a boolean is due to a Burst limitation.
		/// </summary>
		[SerializeField]
		byte allowRotatingOnSpotBacking;

		public const float DESTINATION_CLEARANCE_FACTOR = 4f;

		private static readonly ProfilerMarker MarkerSidewaysAvoidance = new ProfilerMarker("SidewaysAvoidance");
		private static readonly ProfilerMarker MarkerPID = new ProfilerMarker("PID");
		private static readonly ProfilerMarker MarkerOptimizeDirection = new ProfilerMarker("OptimizeDirection");
		private static readonly ProfilerMarker MarkerSmallestDistance = new ProfilerMarker("ClosestDistance");
		private static readonly ProfilerMarker MarkerConvertObstacles = new ProfilerMarker("ConvertObstacles");

		[System.Flags]
		public enum DebugFlags {
			Nothing = 0,
			Position = 1 << 0,
			Tangent = 1 << 1,
			SidewaysClearance = 1 << 2,
			ForwardClearance = 1 << 3,
			Obstacles = 1 << 4,
			Funnel = 1 << 5,
			Path = 1 << 6,
			ApproachWithOrientation = 1 << 7,
			Rotation = 1 << 8,
		}

		public void ScaleByAgentScale (float agentScale) {
			speed *= agentScale;
			leadInRadiusWhenApproachingDestination *= agentScale;
			desiredWallDistance *= agentScale;
		}

		public float Speed (float remainingDistance) {
			if (speed <= 0) return 0;
			if (this.slowdownTime <= 0) return remainingDistance <= 0.0001f ? 0 : speed;

			// This is what you get if you apply a constant deceleration per unit of time of this.speed/this.slowdownTime
			float slowdownFactor = Mathf.Min(1.0f, Mathf.Sqrt(2 * remainingDistance / (speed * this.slowdownTime)));

			var res = speed * slowdownFactor;
			Assert.IsTrue(math.isfinite(res));
			return res;
		}

		/// <summary>
		/// Accelerates as quickly as possible.
		///
		/// This follows the same curve as the <see cref="Speed"/> function, as a function of the remaining distance.
		///
		/// Returns: The speed the agent should have after accelerating for dt seconds. Assuming dt is small.
		/// </summary>
		/// <param name="speed">The current speed of the agent.</param>
		/// <param name="timeToReachMaxSpeed">The time it takes for the agent to reach the maximum speed, starting from a standstill.</param>
		/// <param name="dt">The time to accelerate for. Can be negative to decelerate instead.</param>
		public float Accelerate (float speed, float timeToReachMaxSpeed, float dt) {
			// This can be derived by assuming a constant deceleration per unit of time:
			// x''(t) = A
			// Integrating twice gives us
			// x'(t) = A * t
			// x(t) = A * t^2 / 2
			//
			// Ensuring it yields the same output as the Speed function gives us the equation:
			// x'(t) = S * sqrt(2 * x(t) / (S * T))
			// A * t = S * sqrt(2 * (A * t^2 / 2) / (S * T))
			//
			// Which yields the acceleration when solved:
			// A = S / T
			if (timeToReachMaxSpeed > 0.001f) {
				var a = this.speed / timeToReachMaxSpeed;
				return math.clamp(speed + dt * a, 0, this.speed);
			} else {
				return dt > 0 ? this.speed : 0;
			}
		}

		public float CurveFollowingStrength (float signedDistToClearArea, float radiusToWall, float remainingDistance) {
			var speed = math.max(0.00001f, this.speed);
			var followingStrength = AnglePIDController.RotationSpeedToFollowingStrength(speed, math.radians(this.rotationSpeed));
			var modifiedAlpha = math.max(followingStrength, 40.0f * math.pow(math.abs(signedDistToClearArea) / math.max(0.0001f, radiusToWall), 1));
			var remainingTime = remainingDistance / speed;

			// Just before reaching the end of the path, the agent should try to follow the path very closely to avoid overshooting,
			// and potentially spinning in place.
			const float HIGH_EFFORT_TIME = 0.2f;
			modifiedAlpha = math.max(modifiedAlpha, math.min(80.0f, math.pow(1.0f / math.max(0, remainingTime - HIGH_EFFORT_TIME), 3)));

			Assert.IsTrue(math.isfinite(modifiedAlpha));
			return modifiedAlpha;
		}

		static bool ClipLineByHalfPlaneX (ref float2 a, ref float2 b, float x, float side) {
			var wrongSideA = (a.x - x)*side < 0;
			var wrongSideB = (b.x - x)*side < 0;
			if (wrongSideA && wrongSideB) return false;
			if (wrongSideA != wrongSideB) {
				var t = math.unlerp(a.x, b.x, x);
				var intersection = math.lerp(a, b, t);
				if (wrongSideA) a = intersection;
				else b = intersection;
			}
			return true;
		}

		static void ClipLineByHalfPlaneYt (float2 a, float2 b, float y, float side, ref float mnT, ref float mxT) {
			var wrongSideA = (a.y - y)*side < 0;
			var wrongSideB = (b.y - y)*side < 0;
			if (wrongSideA && wrongSideB) {
				mnT = 1;
				mxT = 0;
			} else if (wrongSideA != wrongSideB) {
				var t = math.unlerp(a.y, b.y, y);
				if (wrongSideA) mnT = math.max(mnT, t);
				else mxT = math.min(mxT, t);
			}
		}

		/// <summary>
		/// Returns either the most clockwise, or most counter-clockwise direction of the three given directions.
		/// The directions are compared pairwise, not using any global reference angle.
		/// </summary>
		static float2 MaxAngle (float2 a, float2 b, float2 c, bool clockwise) {
			a = math.select(a, b, VectorMath.Determinant(a, b) < 0 == clockwise);
			a = math.select(a, c, VectorMath.Determinant(a, c) < 0 == clockwise);
			return a;
		}

		/// <summary>
		/// Returns either the most clockwise, or most counter-clockwise direction of the two given directions.
		/// The directions are compared pairwise, not using any global reference angle.
		/// </summary>
		static float2 MaxAngle (float2 a, float2 b, bool clockwise) {
			return math.select(a, b, VectorMath.Determinant(a, b) < 0 == clockwise);
		}

		const float ALLOWED_OVERLAP_FACTOR = 0.1f;
		const float STEP_MULTIPLIER = 1.0f;
		const float MAX_FRACTION_OF_REMAINING_DISTANCE = 0.9f;
		const int OPTIMIZATION_ITERATIONS = 8;

		static void DrawChisel (float2 start, float2 direction, float pointiness, float length, float width, CommandBuilder draw, Color col) {
			draw.PushColor(col);
			var cornerL = start + (direction * pointiness + new float2(-direction.y, direction.x)) * width;
			var cornerR = start + (direction * pointiness - new float2(-direction.y, direction.x)) * width;
			draw.xz.Line(start, cornerL, col);
			draw.xz.Line(start, cornerR, col);
			var remainingLength = length - pointiness * width;
			if (remainingLength > 0) {
				draw.xz.Ray(cornerL, direction * remainingLength, col);
				draw.xz.Ray(cornerR, direction * remainingLength, col);
			}
			draw.PopColor();
		}

		static void SplitSegment (float2 e1, float2 e2, float desiredRadius, float length, float pointiness, ref EdgeBuffers buffers) {
			// Check if it is completely outside the range we concern ourselves with.
			// When the direction is rotated, we may end up caring about segments further to the side than #desiredRadius, but with
			// a safety margin of 2, we should catch all potential segments that we care about.
			float radiusWithMargin = desiredRadius * 2f;
			if ((e1.y < -radiusWithMargin && e2.y < -radiusWithMargin) || (e1.y > radiusWithMargin && e2.y > radiusWithMargin)) return;

			// Remove the part of the segment that is behind the agent
			if (!ClipLineByHalfPlaneX(ref e1, ref e2, 0, 1)) return;

			// We don't care about any segments further away than #length
			if (!VectorMath.SegmentCircleIntersectionFactors(e1, e2, length*length, out var t1, out var t2)) {
				// Completely outside the circle
				return;
			}

			// Remove the parts of segments that are really close to the agent.
			// Otherwise it can try to aggressively avoid segments that are super close, but are only obstacles due to minimal floating point errors.
			var thresholdRadius = desiredRadius*0.01f;
			if (VectorMath.SegmentCircleIntersectionFactors(e1, e2, thresholdRadius*thresholdRadius, out var tInner1, out var tInner2) && tInner1 < t2 && tInner2 > t1) {
				// Remove the intersection with the inner circle.
				// This may split the segment into 0, 1 or 2 parts.
				if (tInner1 > t1 && tInner1 < t2) SplitSegment2(math.lerp(e1, e2, t1), math.lerp(e1, e2, tInner1), desiredRadius, pointiness, ref buffers);
				if (tInner2 > t1 && tInner2 < t2) SplitSegment2(math.lerp(e1, e2, tInner2), math.lerp(e1, e2, t2), desiredRadius, pointiness, ref buffers);
			} else {
				// No intersection with the inner circle. This is the common case.
				SplitSegment2(math.lerp(e1, e2, t1), math.lerp(e1, e2, t2), desiredRadius, pointiness, ref buffers);
			}
		}

		static void SplitSegment2 (float2 e1, float2 e2, float desiredRadius, float pointiness, ref EdgeBuffers buffers) {
			// The shape that we use for avoidance looks like this:
			//    __________
			//   /
			//  /
			//  \
			//   \ __________
			//
			// With the agent at the pointy end of the shape.
			// Here we check if the segment overlaps the triangular part of the shape, as defined by a circle with the same radius
			// as the sides of the triangle.
			if (VectorMath.SegmentCircleIntersectionFactors(e1, e2, (pointiness*pointiness + 1)*desiredRadius*desiredRadius, out var t1, out var t2)) {
				// Split the segment at the intersection with the circle
				// This may split the segment into 0, 1, 2 or 3 parts.
				if (t1 > 0.0f && t2 < 1.0f) {
					SplitSegment3(e1, math.lerp(e1, e2, t1), desiredRadius, false, ref buffers);
					SplitSegment3(math.lerp(e1, e2, t1), math.lerp(e1, e2, t2), desiredRadius, true, ref buffers);
					SplitSegment3(math.lerp(e1, e2, t2), e2, desiredRadius, false, ref buffers);
				} else if (t1 > 0.0f) {
					SplitSegment3(e1, math.lerp(e1, e2, t1), desiredRadius, false, ref buffers);
					SplitSegment3(math.lerp(e1, e2, t1), e2, desiredRadius, true, ref buffers);
				} else if (t2 < 1.0f) {
					SplitSegment3(e1, math.lerp(e1, e2, t2), desiredRadius, true, ref buffers);
					SplitSegment3(math.lerp(e1, e2, t2), e2, desiredRadius, false, ref buffers);
				} else {
					// Whole segment
					SplitSegment3(e1, e2, desiredRadius, true, ref buffers);
				}
			} else {
				// Outside the circle
				SplitSegment3(e1, e2, desiredRadius, false, ref buffers);
			}
		}

		static void SplitSegment3 (float2 e1, float2 e2, float desiredRadius, bool inTriangularRegion, ref EdgeBuffers buffers) {
			// Check the orientation of the segment, and bias it so that the agent is
			// more like to try to pass on the "correct" side of the segment.
			// All obstacle edges that we get from the navmesh are oriented.
			// However, we only bias the segment when we calculate which side the segment is on,
			// and when calculating the intersection factor. After that, we return to using
			// the original segment.
			//
			// If we don't do this, then the agent can get stuck at the border of the navmesh.
			// Even if the agent is clamped to the navmesh, it may end up a tiiny bit outside it,
			// and then it would try to avoid the border of the navmesh by moving even further outside it.
			var r1 = e1;
			var r2 = e2;
			if (r2.x < r1.x) {
				r1.y -= 0.01f;
				r2.y -= 0.01f;
			} else {
				r1.y += 0.01f;
				r2.y += 0.01f;
			}

			var e1Left = r1.y > 0;

			// Ensure e1 is to the left of the midpoint line
			if (!e1Left) {
				Memory.Swap(ref e1, ref e2);
				Memory.Swap(ref r1, ref r2);
			}

			// Intersection of the line e1 -> e2 with the line y=0
			var tIntersection = math.unlerp(r1.y, r2.y, 0f);

			var anyIntersection = math.isfinite(tIntersection);
			if (tIntersection <= 0.0f || tIntersection >= 1.0f || !anyIntersection) {
				// No intersection
				SplitSegment4(e1, e2, inTriangularRegion, e1Left, ref buffers);
			} else {
				// Intersection. Split the segment into two parts, one for the left side, and one for the right side.
				var intersection = e1 + tIntersection * (e2 - e1);
				var l1 = math.lengthsq(e1 - intersection);
				var l2 = math.lengthsq(e2 - intersection);
				var allowedLineOverlap = desiredRadius * ALLOWED_OVERLAP_FACTOR;
				float allowedLineOverlapSq = allowedLineOverlap*allowedLineOverlap;

				// Check both the left and right subsegments. Ignore them if they are really short.
				if (l1 > allowedLineOverlapSq || l1 >= l2) SplitSegment4(e1, intersection, inTriangularRegion, true, ref buffers);
				if (l2 > allowedLineOverlapSq || l2 >= l1) SplitSegment4(intersection, e2, inTriangularRegion, false, ref buffers);
			}
		}

		static void SplitSegment4 (float2 e1, float2 e2, bool inTriangularRegion, bool left, ref EdgeBuffers buffers) {
			// Ignore tiiiny edges
			// Not quite sure when they get generated, but they do exist.
			// Including these can cause issues if end up almost, but not quite, on the midpoint line,
			// near the end.
			// Ideally we'd have some better code for tolerating cases when there's only a tiny obstacle on the left/right side.
			if (math.all(math.abs(e1 - e2) < 0.01f)) return;

			ref var buffer = ref buffers.triangleRegionEdgesL;
			if (inTriangularRegion) {
				if (left) {} // NOOP
				else buffer = ref buffers.triangleRegionEdgesR;
			} else {
				if (left) buffer = ref buffers.straightRegionEdgesL;
				else buffer = ref buffers.straightRegionEdgesR;
			}

			if (buffer.Length + 2 > buffer.Capacity) return;
			buffer.AddNoResize(e1);
			buffer.AddNoResize(e2);
		}

		private struct EdgeBuffers {
			public FixedList512Bytes<float2> triangleRegionEdgesL;
			public FixedList512Bytes<float2> triangleRegionEdgesR;
			public FixedList512Bytes<float2> straightRegionEdgesL;
			public FixedList512Bytes<float2> straightRegionEdgesR;
		}

		/// <summary>
		/// Finds a direction to move in that is as close as possible to the desired direction while being clear of obstacles, if possible.
		/// This keeps the agent from moving too close to walls.
		/// </summary>
		/// <param name="start">Current position of the agent.</param>
		/// <param name="end">Point the agent is moving towards.</param>
		/// <param name="desiredRadius">The distance the agent should try to keep from obstacles.</param>
		/// <param name="remainingDistance">Remaining distance in the path.</param>
		/// <param name="pointiness">Essentially controls how much the agent will cut corners. A higher value will lead to a smoother path,
		///        but it will also lead to the agent not staying as far away from corners as the desired wall distance parameter would suggest.
		///        It is a unitless quantity.</param>
		/// <param name="edges">Edges of obstacles. Each edge is represented by two points.</param>
		/// <param name="draw">CommandBuilder to use for drawing debug information.</param>
		/// <param name="debugFlags">Flags to control what debug information to draw.</param>
		public static float2 OptimizeDirection (float2 start, float2 end, float desiredRadius, float remainingDistance, float pointiness, NativeArray<float2> edges, CommandBuilder draw, DebugFlags debugFlags) {
			var length = math.length(end - start);
			var direction0 = math.normalizesafe(end - start);
			length *= 0.999f;
			length = math.min(MAX_FRACTION_OF_REMAINING_DISTANCE * remainingDistance, length);
			if (desiredRadius <= 0.0001f) return direction0;

			var lengthOrig = length;
			var lengthInvOrig = 1 / lengthOrig;

			// Pre-process all edges by splitting them up and grouping them by zone.
			// We have 4 zones that we care about:
			// 1. Within the triangular region near the agent, on the left side of the line from #start to #end
			// 2. Within the triangular region near the agent, on the right side
			// 3. Outside the triangular region, on the left side
			// 4. Outside the triangular region, on the right side
			// We assume that about 32 edges for each zone is enough. If we find more edges, the remainder will be discarded.
			// Usually there are only a few edges, so this is not a problem in practice.
			var buffers = new EdgeBuffers();
			for (int i = 0; i < edges.Length; i += 2) {
				// Rotate the edge so that the x-axis corresponds to #direction0
				var e1 = VectorMath.ComplexMultiplyConjugate(edges[i] - start, direction0);
				var e2 = VectorMath.ComplexMultiplyConjugate(edges[i+1] - start, direction0);
				SplitSegment(e1, e2, desiredRadius, length, pointiness, ref buffers);
			}

			// if ((debugFlags & DebugFlags.ForwardClearance) != 0) {
			// 	for (int i = 0; i < buffers.straightRegionEdgesL.Length; i += 2) {
			// 		draw.xz.Line(start + VectorMath.ComplexMultiply(buffers.straightRegionEdgesL[i], direction0), start + VectorMath.ComplexMultiply(buffers.straightRegionEdgesL[i+1], direction0), Palette.Orange);
			// 	}
			// 	for (int i = 0; i < buffers.straightRegionEdgesR.Length; i += 2) {
			// 		draw.xz.Line(start + VectorMath.ComplexMultiply(buffers.straightRegionEdgesR[i], direction0), start + VectorMath.ComplexMultiply(buffers.straightRegionEdgesR[i+1], direction0), Palette.Red);
			// 	}
			// 	for (int i = 0; i < buffers.triangleRegionEdgesL.Length; i += 2) {
			// 		draw.xz.Line(start + VectorMath.ComplexMultiply(buffers.triangleRegionEdgesL[i], direction0), start + VectorMath.ComplexMultiply(buffers.triangleRegionEdgesL[i+1], direction0), Palette.Pink);
			// 	}
			// 	for (int i = 0; i < buffers.triangleRegionEdgesR.Length; i += 2) {
			// 		draw.xz.Line(start + VectorMath.ComplexMultiply(buffers.triangleRegionEdgesR[i], direction0), start + VectorMath.ComplexMultiply(buffers.triangleRegionEdgesR[i+1], direction0), Palette.Purple);
			// 	}
			// }

			// Complex number representing how much to rotate the original direction by.
			// The number (1,0) indicates no rotation.
			var direction = new float2(1, 0);

			// The optimization usually converges very quickly. Error is approximately O(0.5^n)
			for (int it = 0; it < OPTIMIZATION_ITERATIONS; it++) {
				if ((debugFlags & DebugFlags.ForwardClearance) != 0) {
					var col = Palette.Blue;
					col.a = 0.5f;
					var d = VectorMath.ComplexMultiply(direction, direction0);
					DrawChisel(start, d, pointiness, length, desiredRadius, draw, col);
					draw.xz.Ray(start, d * length, Palette.Purple);
					draw.xz.Circle(start, remainingDistance, col);
				}

				var leftReference = new float2(0, desiredRadius);
				var rightReference = new float2(0, -desiredRadius);
				var leftObstacleDir = new float2(length, 0);
				var rightObstacleDir = new float2(length, 0);

				// Iterate through all edges and calculate how much we need to rotate the direction to avoid them.
				// We store all directions as complex numbers.
				for (int i = 0; i < buffers.straightRegionEdgesL.Length; i += 2) {
					// Rotate the edge so that the x-axis corresponds to #direction
					var e1 = VectorMath.ComplexMultiplyConjugate(buffers.straightRegionEdgesL[i], direction);
					var e2 = VectorMath.ComplexMultiplyConjugate(buffers.straightRegionEdgesL[i+1], direction);
					leftObstacleDir = MaxAngle(leftObstacleDir, e1 - leftReference, e2 - leftReference, true);
				}

				for (int i = 0; i < buffers.straightRegionEdgesR.Length; i += 2) {
					var e1 = VectorMath.ComplexMultiplyConjugate(buffers.straightRegionEdgesR[i], direction);
					var e2 = VectorMath.ComplexMultiplyConjugate(buffers.straightRegionEdgesR[i+1], direction);
					rightObstacleDir = MaxAngle(rightObstacleDir, e1 - rightReference, e2 - rightReference, false);
				}

				var referenceDiagonalL = math.normalizesafe(VectorMath.ComplexMultiply(new float2(pointiness*desiredRadius, desiredRadius), direction));
				var referenceDiagonalR = math.normalizesafe(VectorMath.ComplexMultiply(new float2(pointiness*desiredRadius, -desiredRadius), direction));
				for (int i = 0; i < buffers.triangleRegionEdgesL.Length; i += 2) {
					// Rotate the edge so that the x-axis corresponds to #referenceDiagonalL
					var offset1 = VectorMath.ComplexMultiplyConjugate(buffers.triangleRegionEdgesL[i], referenceDiagonalL);
					var offset2 = VectorMath.ComplexMultiplyConjugate(buffers.triangleRegionEdgesL[i+1], referenceDiagonalL);
					var offset = offset2.y < offset1.y ? offset2 : offset1;
					if (offset.y < 0) leftObstacleDir = MaxAngle(leftObstacleDir, offset, true);
				}

				for (int i = 0; i < buffers.triangleRegionEdgesR.Length; i += 2) {
					var offset1 = VectorMath.ComplexMultiplyConjugate(buffers.triangleRegionEdgesR[i], referenceDiagonalR);
					var offset2 = VectorMath.ComplexMultiplyConjugate(buffers.triangleRegionEdgesR[i+1], referenceDiagonalR);
					var offset = offset2.y > offset1.y ? offset2 : offset1;
					if (offset.y > 0) rightObstacleDir = MaxAngle(rightObstacleDir, offset, false);
				}

				// Do some kind of weighted average of the two directions.
				// Here we map the length of the obstacle directions as 0=>0 and L=>infinity (but we clamp it to a finite but large value).
				// Basically we want to give more weight to obstacles closer to the agent.
				var leftInverseWeight = 1 / math.max(0.000001f, lengthOrig - leftObstacleDir.x*leftObstacleDir.x) - lengthInvOrig;
				var rightInverseWeight = 1 / math.max(0.000001f, lengthOrig - rightObstacleDir.x*rightObstacleDir.x) - lengthInvOrig;
				var rTot = math.normalizesafe(leftObstacleDir * rightInverseWeight + rightObstacleDir * leftInverseWeight);

				// Alternative averaging which only takes the sum of the angles
				// var rTot2 = math.normalizesafe(VectorMath.ComplexMultiply(leftObstacleDir, rightObstacleDir));

				// Approximately multiplying the angle by STEP_MULTIPLIER
				var rStep = math.lerp(new float2(1, 0), rTot, STEP_MULTIPLIER);
				direction = math.normalizesafe(VectorMath.ComplexMultiply(direction, rStep));
				if (leftObstacleDir.y == 0 && rightObstacleDir.y == 0) {
					// Apparently there were NO obstacles.
					// We can afford to increase our length check a little bit.
					// This is important in case we encounter a corner which is on a very pointy obstacle.
					//             _______
					//   _ _ _ _  /
					// A _ _ _ _ <______
					//
					// Where A is the agent, trying to move towards the corner marked with a '<'.
					// In that case, we will find no edges to avoid and we will end up moving directly towards the corner instead
					// of staying slightly away from the walls. Unless we increase the length check a little bit, that is.
					//
					// However, we don't want to increase the length more than the remaining distance to the target minus a small margin,
					// as that can cause weird movement when approaching a target near a wall. It would try to unnecessarily avoid the wall
					// causing ocillating movement.
					length = math.min(remainingDistance * MAX_FRACTION_OF_REMAINING_DISTANCE, math.min(length * 1.1f, lengthOrig * 1.2f));
				} else {
					// Decrease the length a bit, to bias the optimization towards closer obstacles
					length = math.min(length, math.max(desiredRadius * 2.0f, math.min(leftObstacleDir.x, rightObstacleDir.x) * 2.0f));
				}
			}

			direction = VectorMath.ComplexMultiply(direction, direction0);

			if ((debugFlags & DebugFlags.ForwardClearance) != 0) {
				DrawChisel(start, direction, pointiness, length, desiredRadius, draw, Color.black);
			}

			Assert.IsTrue(!math.any(math.isnan(direction)));
			return direction;
		}

		/// <summary>
		/// Calculates the closest point on any point of an edge that is inside a wedge.
		///
		/// Returns: The distance to the closest point on any edge that is inside the wedge.
		/// </summary>
		/// <param name="point">The origin point of the wedge (the pointy end).</param>
		/// <param name="dir1">The first direction of the wedge.</param>
		/// <param name="dir2">The second direction of the wedge.</param>
		/// <param name="shrinkAmount">The wedge is shrunk by this amount. In the same units as the input points.</param>
		/// <param name="edges">The edges to check for intersection with.</param>
		public static float SmallestDistanceWithinWedge (float2 point, float2 dir1, float2 dir2, float shrinkAmount, NativeArray<float2> edges) {
			dir1 = math.normalizesafe(dir1);
			dir2 = math.normalizesafe(dir2);

			// Early out in case the wedge is very narrow.
			// This is primarily a performance optimization.
			// If the agent is almost facing the correct direction, then it shouldn't be heading towards an obstacle.
			const float MIN_ANGLE_COS = 0.999f;
			if (math.dot(dir1, dir2) > MIN_ANGLE_COS) return float.PositiveInfinity;

			var side = math.sign(VectorMath.Determinant(dir1, dir2));
			shrinkAmount *= side;

			var closestDistanceSq = float.PositiveInfinity;
			for (int i = 0; i < edges.Length; i += 2) {
				var e1 = edges[i] - point;
				var e2 = edges[i+1] - point;

				// Clip the line by the two half planes that the wedge consists of
				var e1a = VectorMath.ComplexMultiplyConjugate(e1, dir1);
				var e2a = VectorMath.ComplexMultiplyConjugate(e2, dir1);
				var e1b = VectorMath.ComplexMultiplyConjugate(e1, dir2);
				var e2b = VectorMath.ComplexMultiplyConjugate(e2, dir2);
				var mnT = 0f;
				var mxT = 1f;

				ClipLineByHalfPlaneYt(e1a, e2a, shrinkAmount, side, ref mnT, ref mxT);
				if (mnT > mxT) continue;
				ClipLineByHalfPlaneYt(e1b, e2b, -shrinkAmount, -side, ref mnT, ref mxT);
				if (mnT > mxT) continue;

				// Find the distance to the closest point on the clipped line segment
				var lengthsq = math.lengthsq(e2 - e1);
				var t = math.clamp(math.dot(e1, e1 - e2) * math.rcp(lengthsq), mnT, mxT);
				var d = math.lengthsq(math.lerp(e1, e2, t));
				closestDistanceSq = math.select(closestDistanceSq, math.min(closestDistanceSq, d), lengthsq > math.FLT_MIN_NORMAL);
			}
			Assert.IsTrue(!float.IsNaN(closestDistanceSq));
			return math.sqrt(closestDistanceSq);
		}

		public static float2 Linecast (float2 a, float2 b, NativeArray<float2> edges) {
			var k = 1f;
			for (int i = 0; i < edges.Length; i += 2) {
				var e1 = edges[i];
				var e2 = edges[i+1];
				VectorMath.LineLineIntersectionFactors(a, b - a, e1, e2 - e1, out var t1, out var t2);
				if (t2 >= 0 && t2 <= 1 && t1 > 0) {
					k = math.min(k, t1);
				}
			}
			return a + (b - a) * k;
		}

		public struct ControlParams {
			public Vector3 p;
			public float speed;
			public float rotation;
			public float maxDesiredWallDistance;
			public float3 endOfPath;
			public float3 facingDirectionAtEndOfPath;
			public NativeArray<float2> edges;
			public float3 nextCorner;
			public float agentRadius;
			public float remainingDistance;
			public float3 closestOnNavmesh;
			public DebugFlags debugFlags;
			public NativeMovementPlane movementPlane;
		}

		/// <summary>
		/// Finds the bounding box in which this controller is interested in navmesh edges.
		///
		/// The edges should be assigned to <see cref="ControlParams.edges"/>.
		/// The bounding box is relative to the given movement plane.
		/// </summary>
		public static Bounds InterestingEdgeBounds (ref PIDMovement settings, float3 position, float3 nextCorner, float height, NativeMovementPlane plane) {
			// Convert the position and next corner to local space, relative to the movement plane
			var localPos = math.mul(math.conjugate(plane.rotation), position);
			var localNextCorner = math.mul(math.conjugate(plane.rotation), nextCorner);
			// Default bounds which extend from 1/2*height below the agent to the agent's head (assuming its pivot is at the agent's feet).
			var localBounds = new Bounds(localPos + new float3(0, height * 0.25f, 0), new Vector3(0, 1.5f*height, 0));
			// Don't allow the next corner to push the bounding box up or down too much, since that can let us include obstacle edges
			// that are e.g. on a floor below or a floor above the agent. Especially if the agent is currently moving on a sloped surface.
			localNextCorner.y = localPos.y;

			localBounds.Encapsulate(localNextCorner);
			// If an agent needs to make a full 180 degree turn, then we need a diameter instead of a radius.
			// However since the agent gets a lower rotation speed the closer it gets to the desired rotation,
			// this factor of two gets sort of compensated for already.
			if (settings.rotationSpeed > 0) {
				var approximateTurningDiameter = settings.speed / math.radians(settings.rotationSpeed); //2.0f * AnglePIDController.ApproximateTurningRadius(settings.followingStrength);
				//
				localBounds.Expand(new Vector3(1, 0, 1) * math.max(approximateTurningDiameter, settings.desiredWallDistance * OPTIMIZATION_ITERATIONS * STEP_MULTIPLIER));
			}
			return localBounds;
		}

		static float2 OffsetCornerForApproach (float2 position2D, float2 endOfPath2D, float2 facingDir2D, ref PIDMovement settings, float2 nextCorner2D, ref float gammaAngle, ref float gammaAngleWeight, DebugFlags debugFlags, ref CommandBuilder draw, NativeArray<float2> edges) {
			var d1 = endOfPath2D - position2D;

			// Cosine of the approach angle that is considered too steep to try to turn in an arc
			const float STEEP_ANGLE_THRESHOLD_COS = -0.2f;

			if (math.dot(math.normalizesafe(d1), facingDir2D) < STEEP_ANGLE_THRESHOLD_COS) {
				// Too steep
				return nextCorner2D;
			}

			// Line orthogonal to d1
			var n1 = new float2(-d1.y, d1.x);

			// Line orthogonal to facingDir2D
			var n2 = new float2(-facingDir2D.y, facingDir2D.x);
			var mid = (position2D + endOfPath2D) * 0.5f;

			// Find the center of the circle which touches both the points endOfPath2D and position2D, and has a tangent parallel to facingDir2D at endOfPath2D.
			var circleCenter = (float2)VectorMath.LineIntersectionPoint(mid, mid + n1, endOfPath2D, endOfPath2D + n2, out bool intersects);

			if (!intersects) return nextCorner2D;

			// Do not try to approach the destination with a large arc if there might be an obstacle in the way
			// Check within a wedge and offset it sliightly backwards to take care of the case when the end of the path
			// is right at the end of the navmesh. This is a common case when for example ordering an agent to interact
			// with some prop.
			//
			//      Agent
			//   |    |
			// <-x----/
			//   |
			//
			var distToObstacle = SmallestDistanceWithinWedge(endOfPath2D - 0.01f * facingDir2D, n2 - 0.1f * facingDir2D, -n2 - 0.1f * facingDir2D, 0.001f, edges);
			var maxRadius = settings.leadInRadiusWhenApproachingDestination;
			maxRadius = math.min(maxRadius, distToObstacle * 0.9f);
			var circleRadius = math.length(circleCenter - endOfPath2D);

			// Calculate the intersection point of the two tangents of the circle, one at endOfPath2D and one at position2D.
			// Offset is the distance from endOfPath2D to the intersection point
			var dot = math.abs(math.dot(math.normalizesafe(d1), n2));
			var offset = 1.0f / math.sqrt(1 - dot*dot) * math.length(d1) * 0.5f;

			// Tweak the offset slightly to account for the maximum radius.
			// Limit the radius using a smooth thresholding function.
			offset /= math.min(maxRadius, circleRadius);
			offset = math.tanh(offset);
			offset *= math.min(maxRadius, circleRadius);

			// Offset the next corner backwards along the facing direction,
			// so that the agent will approach the destination along a curve.
			var newNextCorner2D = nextCorner2D - facingDir2D * offset;

			if ((debugFlags & DebugFlags.ApproachWithOrientation) != 0) {
				draw.xz.Circle(circleCenter, circleRadius, Color.blue);
				draw.xz.Arrow(position2D, newNextCorner2D, Palette.Orange);
			}

			// If the new corner is not visible from the agent's current position,
			// then return the original corner, as we do not want to try to walk into a wall.
			if (math.lengthsq(Linecast(position2D, newNextCorner2D, edges) - newNextCorner2D) > 0.01f) {
				return nextCorner2D;
			} else {
				return newNextCorner2D;
			}
		}

		public static AnglePIDControlOutput2D Control (ref PIDMovement settings, float dt, ref ControlParams controlParams, ref CommandBuilder draw, out float maxDesiredWallDistance) {
			if (dt <= 0) {
				// If the game is paused, then do not move or rotate.
				maxDesiredWallDistance = controlParams.maxDesiredWallDistance;
				return new AnglePIDControlOutput2D {
						   rotationDelta = 0,
						   positionDelta = float2.zero,
				};
			}
			var movementPlane = controlParams.movementPlane;
			var position2D = movementPlane.ToPlane(controlParams.p, out float positionElevation);

			// If we are drawing any debug information, push a matrix so that we can draw in local space.
			// If not, skip pushing the matrix to improve performance.
			if (controlParams.debugFlags != 0) draw.PushMatrix(math.mul(new float4x4(movementPlane.rotation, float3.zero), float4x4.Translate(new float3(0, positionElevation, 0))));

			if ((controlParams.debugFlags & DebugFlags.Position) != 0) {
				draw.xz.Cross(controlParams.closestOnNavmesh, 0.05f, Color.red);
			}

			var edges = controlParams.edges;
			if ((controlParams.debugFlags & DebugFlags.Obstacles) != 0) {
				draw.PushLineWidth(2);
				draw.PushColor(Color.red);
				for (int i = 0; i < edges.Length; i += 2) {
					draw.xz.Line(edges[i], edges[i+1]);
				}
				draw.PopColor();
				draw.PopLineWidth();
			}

			var nextCorner2D = movementPlane.ToPlane(controlParams.nextCorner);
			float gamma = 0;
			float gammaAngle = 0;
			float gammaAngleWeight = 0;
			// +Y is our forward direction, so add 90 degrees so that rotation2D = curveAngle means we are following the curve.
			// Mathematically it makes much more sense if rotations are relative to the +X axis. So we use this convention internally.
			var rotation2D = controlParams.rotation + Mathf.PI / 2;
			var facingDir2D = math.normalizesafe(movementPlane.ToPlane(controlParams.facingDirectionAtEndOfPath));
			bool isVeryCloseToEndOfPath = controlParams.remainingDistance < controlParams.agentRadius*0.1f;

			if (!isVeryCloseToEndOfPath && settings.leadInRadiusWhenApproachingDestination > 0 && math.any(facingDir2D != 0)) {
				var endOfPath2D = movementPlane.ToPlane(controlParams.endOfPath);
				bool isAtLastCorner = math.lengthsq(endOfPath2D - nextCorner2D) <= 0.1f;
				if (isAtLastCorner) {
					var c1 = OffsetCornerForApproach(
						position2D,
						endOfPath2D,
						facingDir2D,
						ref settings,
						nextCorner2D,
						ref gammaAngle,
						ref gammaAngleWeight,
						controlParams.debugFlags,
						ref draw,
						edges
						);
					nextCorner2D = c1;

					var simDx = settings.speed * 0.1f;
					if (simDx > 0.001f) {
						math.sincos(rotation2D, out var sin, out var cos);
						var forward = new float2(cos, sin);
						var c2 = OffsetCornerForApproach(
							position2D + forward * simDx,
							endOfPath2D,
							facingDir2D,
							ref settings,
							nextCorner2D,
							ref gammaAngle,
							ref gammaAngleWeight,
							DebugFlags.Nothing,
							ref draw,
							edges
							);

						// Calculate the number of radians between c1 and c2 from the agent's perspective.
						// This is the amount that the agent must rotate to stay on the desired curve.
						var s = VectorMath.Determinant(math.normalizesafe(c1 - position2D), math.normalizesafe(c2 - position2D));
						gamma = math.asin(s)/simDx;
					}
				}
			}

			var desiredForwardClearanceRadius = settings.desiredWallDistance;
			desiredForwardClearanceRadius = math.max(0, math.min(desiredForwardClearanceRadius, (controlParams.remainingDistance - desiredForwardClearanceRadius) / DESTINATION_CLEARANCE_FACTOR));
			MarkerOptimizeDirection.Begin();

			// In case the next corner is not visible from the agent's current position, then instead move towards the first intersection with an obstacle.
			// This is important in some cases even when one would think that the next corner should be visible.
			// This is because when unwrapping and flattening the funnel, the next corner may end up being move slightly due to various projections.
			// This may cause it to end up inside a wall. If we didn't use a linecast here, the OptimizeDirection function
			// would likely just give up and the agent would not stay away from the wall as it should.
			nextCorner2D = Linecast(position2D, nextCorner2D, edges);

			const float Pointiness = 2f;
			var estimatedForward = OptimizeDirection(position2D, nextCorner2D, desiredForwardClearanceRadius, controlParams.remainingDistance, Pointiness, edges, draw, controlParams.debugFlags);
			MarkerOptimizeDirection.End();

			// Increase the maxDesiredWallDistance over time, to slowly push the agent away from walls.
			maxDesiredWallDistance = controlParams.maxDesiredWallDistance + settings.speed * 0.1f * dt;
			var desiredPositionClearance = maxDesiredWallDistance;
			var signedDist = 0f;
			var signedDistToClearArea = 0f;
			maxDesiredWallDistance = math.min(maxDesiredWallDistance, desiredPositionClearance);

			if ((controlParams.debugFlags & DebugFlags.Tangent) != 0) {
				draw.Arrow(controlParams.p, controlParams.p + new Vector3(estimatedForward.x, 0, estimatedForward.y), Palette.Orange);
			}

			AnglePIDControlOutput2D output;
			if (isVeryCloseToEndOfPath) {
				// When we are really close to the endpoint, move directly towards the end and do not rotate (unless a facing direction has been set).

				// Accelerate, but only up to the very low speed we use when we are very close to the endpoint.
				// We must be able to accelerate here, as otherwise we may never reach the endpoint if we started
				// very close to the endpoint with zero speed.
				var speed = math.min(settings.Speed(controlParams.remainingDistance), settings.Accelerate(controlParams.speed, settings.slowdownTime, dt));

				// TODO: Maybe add a settling mechanic. Once we are really close, lock the destination and do not change it until it gets a certain minimum distance away from the agent
				// This would avoid the agent drifting without rotating to follow a destination that moves slowly.
				var dirToEnd = nextCorner2D - position2D;
				var distToEnd = math.length(dirToEnd);
				if (math.any(facingDir2D != 0)) {
					var desiredAngle = math.atan2(facingDir2D.y, facingDir2D.x);
					var maxRotationDelta = dt * math.radians(settings.maxRotationSpeed);
					output = new AnglePIDControlOutput2D {
						rotationDelta = math.clamp(AstarMath.DeltaAngle(rotation2D, desiredAngle), -maxRotationDelta, maxRotationDelta),
						// Convert back to a rotation convention where +Y is forward
						targetRotation = desiredAngle - Mathf.PI / 2,
						positionDelta = distToEnd > math.FLT_MIN_NORMAL ? dirToEnd * (dt * speed / distToEnd) : dirToEnd,
					};
				} else {
					output = new AnglePIDControlOutput2D {
						rotationDelta = 0,
						// Convert back to a rotation convention where +Y is forward
						targetRotation = rotation2D - Mathf.PI / 2,
						positionDelta = distToEnd > math.FLT_MIN_NORMAL ? dirToEnd * (dt * speed / distToEnd) : dirToEnd,
					};
				}
			} else {
				var modifiedFollowingStrength = settings.CurveFollowingStrength(signedDistToClearArea, desiredPositionClearance, controlParams.remainingDistance);
				var curveAngle = math.atan2(estimatedForward.y, estimatedForward.x);

				var minimumRotationSpeed = 0f;
				// If we are not perfectly facing our desired direction, we need to rotate to face it.
				// We try to ensure we will not hit any obstacles by checking for nearby obstacles
				// in the direction we are moving. If there are any obstacles, we can calculate
				// the approximate rotation speed we need to have to avoid them.
				//
				// If we are very close to our desired facing direction, we skip this check
				// to improve performance.
				if (math.abs(AstarMath.DeltaAngle(curveAngle, rotation2D)) > math.PI*0.001f) {
					math.sincos(rotation2D, out var sin, out var cos);
					var forward = new float2(cos, sin);
					var closestWithinWedge = SmallestDistanceWithinWedge(position2D, estimatedForward, forward, controlParams.agentRadius*0.1f, edges);

					if ((controlParams.debugFlags & DebugFlags.ForwardClearance) != 0 && float.IsFinite(closestWithinWedge)) {
						draw.xz.Arc(position2D, position2D + forward * closestWithinWedge, position2D + estimatedForward, Palette.Purple);
					}

					if (closestWithinWedge > 0.001f && closestWithinWedge*1.01f < controlParams.remainingDistance) {
						const float SAFETY_FACTOR = 2.0f;
						minimumRotationSpeed = math.rcp(closestWithinWedge) * SAFETY_FACTOR;
					}
				}

				MarkerPID.Begin();
				output = AnglePIDController.Control(
					ref settings,
					modifiedFollowingStrength,
					rotation2D,
					curveAngle + AstarMath.DeltaAngle(curveAngle, gammaAngle) * gammaAngleWeight,
					gamma,
					signedDist,
					controlParams.speed,
					controlParams.remainingDistance,
					minimumRotationSpeed,
					controlParams.speed < settings.speed*0.1f,
					dt
					);
				// Convert back to a rotation convention where +Y is forward
				output.targetRotation -= Mathf.PI / 2;
				MarkerPID.End();
			}
			if (controlParams.debugFlags != 0) draw.PopMatrix();
			return output;
		}
	}

	/// <summary>
	/// Implements a PID controller for the angular velocity of an agent following a curve.
	///
	/// The PID controller is formulated for small angles (see https://en.wikipedia.org/wiki/Small-angle_approximation), but extends well to large angles.
	/// For small angles, if y(t) is the curve/agent position, then y'(t) is the angle and y''(t) is the angular velocity.
	/// This controller outputs an angular velocity, meaning it controls y''(t).
	///
	/// See https://en.wikipedia.org/wiki/PID_controller
	/// </summary>
	public static class AnglePIDController {
		const float DampingRatio = 1.0f;

		/// <summary>
		/// An approximate turning radius the agent will have in an open space.
		///
		/// This is based on the PID controller in the <see cref="Control"/> method.
		/// </summary>
		public static float ApproximateTurningRadius (float followingStrength) {
			// With dampingRatio = 1, this will result in critical damping
			var alpha = followingStrength;
			var beta = 2 * math.sqrt(math.abs(alpha)) * DampingRatio;

			// Some sort of mean value
			// If a character turns around, the angleToCurveError will go from math.PI to 0.
			const float angleToCurveError = math.PI * 0.5f;

			return 1.0f/(beta * angleToCurveError);
		}

		/// <summary>
		/// Given a speed and a rotation speed, what is the approximate corresponding following strength.
		///
		/// This is based on the PID controller in the <see cref="Control"/> method.
		/// </summary>
		public static float RotationSpeedToFollowingStrength (float speed, float maxRotationSpeed) {
			// Using the following identity:
			// turningRadius = speed/rotationSpeed
			// and using the implementation for ApproximateTurningRadius, we can solve for the rotation speed
			// and we get the expression below.

			// Note that we use a different angleToCurveError here compared to in ApproximateTurningRadius.
			// This is because here we use the maximum angleToCurveError that could happen, while in ApproximateTurningRadius
			// we use an average value. This is reasonable because the input to this method is
			// the maximum rotation speed, not the average rotation speed.
			const float angleToCurveError = math.PI;
			var k = maxRotationSpeed / (2.0f * angleToCurveError * speed * DampingRatio);
			var alpha = k * k;
			return alpha;
		}

		public static float FollowingStrengthToRotationSpeed (float followingStrength) {
			return 1.0f / (ApproximateTurningRadius(followingStrength) * 0.5f);
		}

		/// <summary>
		/// How much to rotate and move in order to smoothly follow a given curve.
		///
		/// If the maximum rotation speed (settings.maxRotationSpeed) would be exceeded, the agent will slow down to avoid exceeding it (up to a point).
		///
		/// Returns: A control value that can be used to move the agent.
		/// </summary>
		/// <param name="settings">Various movement settings</param>
		/// <param name="followingStrength">The integral term of the PID controller. The higher this value is, the quicker the agent will try to align with the curve.</param>
		/// <param name="angle">The current direction of the agent, in radians.</param>
		/// <param name="curveAngle">The angle of the curve tangent at the nearest point, in radians.</param>
		/// <param name="curveCurvature">The curvature of the curve at the nearest point. Positive values means the curve is turning to the left, negative values means the curve is turning to the right.</param>
		/// <param name="curveDistanceSigned">The signed distance from the agent to the curve. Positive values means the agent is to the right of the curve, negative values means the agent is to the left of the curve.</param>
		/// <param name="speed">How quickly the agent should move. In meters/second.</param>
		/// <param name="remainingDistance">The remaining distance to where the agent should stop. In meters.</param>
		/// <param name="minRotationSpeed">The minimum rotation speed of the agent. In radians/second. Unless the agent does not desire to rotate at all, it will rotate at least this fast.</param>
		/// <param name="isStationary">Should be true if the agent is currently standing still (or close to it). This allows it to rotate in place.</param>
		/// <param name="dt">How long the current time-step is. In seconds.</param>
		public static AnglePIDControlOutput2D Control (ref PIDMovement settings, float followingStrength, float angle, float curveAngle, float curveCurvature, float curveDistanceSigned, float speed, float remainingDistance, float minRotationSpeed, bool isStationary, float dt) {
			Assert.IsTrue(math.isfinite(angle));
			Assert.IsTrue(math.isfinite(curveAngle));
			Assert.IsTrue(math.isfinite(curveDistanceSigned));
			Assert.IsTrue(math.isfinite(curveCurvature));
			Assert.IsTrue(minRotationSpeed >= 0);

			// With dampingRatio = 1, this will result in critical damping
			var alpha = followingStrength;
			var beta = 2 * math.sqrt(math.abs(alpha)) * DampingRatio;
			var gamma = 1.0f;
			var angleToCurveError = AstarMath.DeltaAngle(angle, curveAngle);
			var angleTowardsCurve = curveAngle + math.sign(curveDistanceSigned) * math.PI * 0.5f;
			var deltaAngleTowardsCurve = AstarMath.DeltaAngle(angle, angleTowardsCurve); // TODO: Divide by PI/2?

			// Desired primary rotation in radians per meter
			var alphaAngle = alpha * math.abs(curveDistanceSigned) * deltaAngleTowardsCurve;
			// Desired primary rotation during this timestep
			var alphaAngleDelta = alphaAngle * speed * dt;

			// Desired secondary rotation in radians per meter
			var betaAngle = beta * angleToCurveError;

			// Assuming that an agent is stationary, the rotation of the agent will reach a steady state after a short while (alphaAngle + betaAngle = 0).
			// This is the remaining angle we have left until we reach that steady state.
			var denominator = beta  + alpha * math.abs(curveDistanceSigned);
			var remainingAngle = denominator > math.FLT_MIN_NORMAL ? (betaAngle + alphaAngle)/denominator : 0;
			Assert.IsTrue(math.isfinite(remainingAngle));
			float.IsFinite(remainingAngle);

			// If the agent has to rotate *a lot* then stop moving and rotate in-place.
			// Once we are rotating in place, we should continue doing that until we are almost facing the desired direction.
			isStationary = settings.allowRotatingOnSpot && (math.abs(remainingAngle) > math.PI*0.6666f || (isStationary && math.abs(remainingAngle) > 0.1f));
			if (isStationary) {
				var newSpeed = settings.Accelerate(speed, settings.slowdownTimeWhenTurningOnSpot, -dt);
				var maxOnSpotRotationSpeed = math.radians(settings.maxOnSpotRotationSpeed);
				var canRotateInOneStep = maxOnSpotRotationSpeed*dt > math.abs(remainingAngle);
				if (newSpeed > 0 && !canRotateInOneStep) {
					// Slow down as quickly as possible
					return AnglePIDControlOutput2D.WithMovementAtEnd(
						currentRotation: angle,
						targetRotation: angle,
						rotationDelta: 0,
						moveDistance: newSpeed * dt
						);
				} else {
					// If we are rotating in place, rotate with the maximum rotation speed
					return AnglePIDControlOutput2D.WithMovementAtEnd(
						currentRotation: angle,
						targetRotation: angle + remainingAngle,
						rotationDelta: math.clamp(remainingAngle, -maxOnSpotRotationSpeed*dt, maxOnSpotRotationSpeed*dt),
						// Check if we can rotate in place in one time-step. If so, skip standing still for this time-step.
						moveDistance: canRotateInOneStep ? speed * dt : 0.0f
						);
				}
			}

			speed = math.min(settings.Speed(remainingDistance), settings.Accelerate(speed, settings.slowdownTime, dt));

			if (math.abs(angleToCurveError) > math.PI*0.5f) {
				// Ensures that if the agent is moving in the completely wrong direction, it will not continue doing that
				// because the alpha term tells it to move left, and the beta term tells it to move right, cancelling each other out.
				alphaAngleDelta = 0;
			}

			if (math.abs(betaAngle) > 0.0001f) {
				betaAngle = math.max(math.abs(betaAngle), minRotationSpeed) * math.sign(betaAngle);
			}

			var betaAngleDelta = betaAngle * speed * dt;
			// The weights are "how much we want to rotate this timestep, divided by the maximum amount of rotation that is allowed"
			// This is used to avoid overshooting when following strengths are very high or the fps is low.
			var alphaWeight = math.abs(alphaAngleDelta / deltaAngleTowardsCurve);
			var betaWeight = math.abs(betaAngleDelta / angleToCurveError);
			var gammaWeight = 1.0f;
			var directionComponentInCurveDirection = math.max(0, math.cos(angleToCurveError));
			var speedMultiplier = 1.0f;
			var moveDistance = speed * speedMultiplier * dt;
			var curvatureIntegral = curveCurvature * moveDistance;
			var gammaAngleDelta = gamma * curvatureIntegral * directionComponentInCurveDirection;
			// Don't allow individual contributions to contribute more than their limit (e.g. overshooting their rotation target).
			// But still keep the relative contribution proportions the same.
			var overflowWeight = math.max(1f, math.max(alphaWeight, math.max(betaWeight, gammaWeight)));
			var angleDelta = (gammaAngleDelta + betaAngleDelta + alphaAngleDelta) / overflowWeight;

			// If we would have rotated too quickly, slow down the agent
			var maxRotationSpeed = math.radians(settings.maxRotationSpeed);
			var rotationMultiplier = math.max(0.1f, math.min(1.0f, maxRotationSpeed*dt / math.abs(angleDelta)));

			Assert.IsTrue(math.isfinite(angle));
			Assert.IsTrue(math.isfinite(rotationMultiplier));
			Assert.IsTrue(math.isfinite(angleDelta));
			Assert.IsTrue(math.isfinite(moveDistance));

			return new AnglePIDControlOutput2D(
				currentRotation: angle,
				targetRotation: angle + remainingAngle,
				rotationDelta: angleDelta * rotationMultiplier,
				moveDistance: moveDistance * rotationMultiplier
				);
		}
	}
}
