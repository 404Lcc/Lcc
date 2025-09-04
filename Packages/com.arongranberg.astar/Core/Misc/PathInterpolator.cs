using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Pathfinding.Util {
	/// <summary>Interpolates along a sequence of points</summary>
	public class PathInterpolator {
		/// <summary>
		/// Represents a single point on the polyline represented by the <see cref="PathInterpolator"/>.
		/// The cursor is a lightweight structure which can be used to move backwards and forwards along a <see cref="PathInterpolator"/>.
		///
		/// If the <see cref="PathInterpolator"/> changes (e.g. has its path swapped out), then this cursor is invalidated and cannot be used anymore.
		/// </summary>
		public struct Cursor {
			private PathInterpolator interpolator;
			private int version;
			private float currentDistance;
			private float distanceToSegmentStart;
			private float currentSegmentLength;

			/// <summary>
			/// Current segment.
			/// The start and end points of the segment are path[value] and path[value+1].
			/// </summary>
			int segmentIndex { get; set; }

			public int segmentCount {
				get {
					AssertValid();
					return interpolator.path.Count - 1;
				}
			}

			/// <summary>Last point in the path</summary>
			public Vector3 endPoint {
				get {
					AssertValid();
					return interpolator.path[interpolator.path.Count-1];
				}
			}

			/// <summary>
			/// Fraction of the way along the current segment.
			/// 0 is at the start of the segment, 1 is at the end of the segment.
			/// </summary>
			public float fractionAlongCurrentSegment {
				get {
					return currentSegmentLength > 0 ? (currentDistance - distanceToSegmentStart) / currentSegmentLength : 1f;
				}
				set {
					currentDistance = distanceToSegmentStart + Mathf.Clamp01(value) * currentSegmentLength;
				}
			}

			/// <summary>A cursor at the start of the polyline represented by the interpolator</summary>
			public static Cursor StartOfPath (PathInterpolator interpolator) {
				if (!interpolator.valid) throw new System.InvalidOperationException("PathInterpolator has no path set");
				return new Cursor {
						   interpolator = interpolator,
						   version = interpolator.version,
						   segmentIndex = 0,
						   currentDistance = 0,
						   distanceToSegmentStart = 0,
						   currentSegmentLength = (interpolator.path[1] - interpolator.path[0]).magnitude,
				};
			}

			/// <summary>
			/// True if this instance has a path set.
			/// See: SetPath
			/// </summary>
			public bool valid {
				get {
					return interpolator != null && interpolator.version == version;
				}
			}

			/// <summary>
			/// Tangent of the curve at the current position.
			/// Not necessarily normalized.
			/// </summary>
			public Vector3 tangent {
				get {
					AssertValid();
					return interpolator.path[segmentIndex+1] - interpolator.path[segmentIndex];
				}
			}

			/// <summary>Remaining distance until the end of the path</summary>
			public float remainingDistance {
				get {
					AssertValid();
					return interpolator.totalDistance - distance;
				}
				set {
					AssertValid();
					distance = interpolator.totalDistance - value;
				}
			}

			/// <summary>Traversed distance from the start of the path</summary>
			public float distance {
				get {
					return currentDistance;
				}
				set {
					AssertValid();
					currentDistance = value;

					while (currentDistance < distanceToSegmentStart && segmentIndex > 0) PrevSegment();
					while (currentDistance > distanceToSegmentStart + currentSegmentLength && segmentIndex < interpolator.path.Count - 2) NextSegment();
				}
			}

			/// <summary>Current position</summary>
			public Vector3 position {
				get {
					AssertValid();
					float t = currentSegmentLength > 0.0001f ? (currentDistance - distanceToSegmentStart) / currentSegmentLength : 0f;
					return Vector3.Lerp(interpolator.path[segmentIndex], interpolator.path[segmentIndex+1], t);
				}
			}

			/// <summary>Appends the remaining path between <see cref="position"/> and <see cref="endPoint"/> to buffer</summary>
			public void GetRemainingPath (List<Vector3> buffer) {
				AssertValid();
				buffer.Add(position);
				for (int i = segmentIndex+1; i < interpolator.path.Count; i++) {
					buffer.Add(interpolator.path[i]);
				}
			}

			void AssertValid () {
				if (!this.valid) throw new System.InvalidOperationException("The cursor has been invalidated because SetPath has been called on the interpolator. Please create a new cursor.");
			}

			/// <summary>
			/// The tangent(s) of the curve at the current position.
			/// Not necessarily normalized.
			///
			/// Will output t1=<see cref="tangent"/>, t2=<see cref="tangent"/> if on a straight line segment.
			/// Will output the previous and next tangents for the adjacent line segments when on a corner.
			///
			/// This is similar to <see cref="tangent"/> but can output two tangents instead of one when on a corner.
			/// </summary>
			public void GetTangents (out Vector3 t1, out Vector3 t2) {
				AssertValid();
				var nearStart = currentDistance <= distanceToSegmentStart + 0.001f;
				var nearEnd = currentDistance >= distanceToSegmentStart + currentSegmentLength - 0.001f;
				if (nearStart || nearEnd) {
					int s1, s2;
					if (nearStart) {
						s1 = segmentIndex > 0 ? segmentIndex - 1 : segmentIndex;
						s2 = segmentIndex;
					} else {
						s1 = segmentIndex;
						s2 = segmentIndex < interpolator.path.Count - 2 ? segmentIndex + 1 : segmentIndex;
					}
					t1 = interpolator.path[s1+1] - interpolator.path[s1];
					t2 = interpolator.path[s2+1] - interpolator.path[s2];
				} else {
					t1 = tangent;
					t2 = t1;
				}
			}

			/// <summary>
			/// A vector parallel to the local curvature.
			///
			/// This will be zero on straight line segments, and in the same direction as the rotation axis when on a corner.
			///
			/// Since this interpolator follows a polyline, the curvature is always either 0 or infinite.
			/// Therefore the magnitude of this vector has no meaning when non-zero. Only the direction matters.
			/// </summary>
			public Vector3 curvatureDirection {
				get {
					GetTangents(out var t1, out var t2);
					var up = Vector3.Cross(t1, t2);
					return up.sqrMagnitude <= 0.000001f ? Vector3.zero : up;
				}
			}

			/// <summary>
			/// Moves the cursor to the next geometric corner in the path.
			///
			/// This is the next geometric corner.
			/// If the original path contained any zero-length segments, they will be skipped over.
			/// </summary>
			public void MoveToNextCorner () {
				AssertValid();
				var path = interpolator.path;
				// Skip zero-length segments
				while (currentDistance >= this.distanceToSegmentStart + this.currentSegmentLength && segmentIndex < path.Count - 2) NextSegment();
				// Skip parallel segements
				while (segmentIndex < path.Count - 2 && VectorMath.IsColinear(path[segmentIndex], path[segmentIndex+1], path[segmentIndex+2])) NextSegment();
				// Move to end of current segment
				currentDistance = distanceToSegmentStart + currentSegmentLength;
			}

			/// <summary>
			/// Moves to the closest intersection of the line segment (origin + direction*range.x, origin + direction*range.y).
			/// The closest intersection as measured by the distance along the path is returned.
			///
			/// If no intersection is found, false will be returned and the cursor remains unchanged.
			///
			/// The intersection is calculated in XZ space.
			/// </summary>
			/// <param name="origin">A point on the line</param>
			/// <param name="direction">The direction of the line. Need not be normalized.</param>
			/// <param name="range">The range of the line segment along the line. The segment is (origin + direction*range.x, origin + direction*range.y). May be (-inf, +inf) to consider an infinite line.</param>
			public bool MoveToClosestIntersectionWithLineSegment (Vector3 origin, Vector3 direction, Vector2 range) {
				AssertValid();
				var closestIntersection = float.PositiveInfinity;
				var closestDist = float.PositiveInfinity;
				var d = 0f;
				for (int i = 0; i < interpolator.path.Count - 1; i++) {
					var p1 = interpolator.path[i];
					var p2 = interpolator.path[i+1];
					var segmentLength = (p2 - p1).magnitude;
					if (
						VectorMath.LineLineIntersectionFactors(((float3)p1).xz, ((float3)(p2 - p1)).xz, ((float3)origin).xz, ((float3)direction).xz, out float t1, out float t2)
						&& t1 >= 0.0f && t1 <= 1.0f
						&& t2 >= range.x && t2 <= range.y
						) {
						var intersection = d + t1 * segmentLength;
						var dist = Mathf.Abs(intersection - this.currentDistance);
						if (dist < closestDist) {
							closestIntersection = intersection;
							closestDist = dist;
						}
					}
					d += segmentLength;
				}
				if (closestDist != float.PositiveInfinity) {
					this.distance = closestIntersection;
					return true;
				}
				return false;
			}

			/// <summary>Move to the specified segment and move a fraction of the way to the next segment</summary>
			void MoveToSegment (int index, float fractionAlongSegment) {
				AssertValid();
				if (index < 0 || index >= interpolator.path.Count - 1) throw new System.ArgumentOutOfRangeException("index");
				while (segmentIndex > index) PrevSegment();
				while (segmentIndex < index) NextSegment();
				currentDistance = distanceToSegmentStart + Mathf.Clamp01(fractionAlongSegment) * currentSegmentLength;
			}

			/// <summary>Move as close as possible to the specified point</summary>
			public void MoveToClosestPoint (Vector3 point) {
				AssertValid();

				float bestDist = float.PositiveInfinity;
				float bestFactor = 0f;
				int bestIndex = 0;

				var path = interpolator.path;

				for (int i = 0; i < path.Count-1; i++) {
					float factor = VectorMath.ClosestPointOnLineFactor(path[i], path[i+1], point);
					Vector3 closest = Vector3.Lerp(path[i], path[i+1], factor);
					float dist = (point - closest).sqrMagnitude;

					if (dist < bestDist) {
						bestDist = dist;
						bestFactor = factor;
						bestIndex = i;
					}
				}

				MoveToSegment(bestIndex, bestFactor);
			}

			public void MoveToLocallyClosestPoint (Vector3 point, bool allowForwards = true, bool allowBackwards = true) {
				AssertValid();

				var path = interpolator.path;

				segmentIndex = Mathf.Min(segmentIndex, path.Count - 2);
				while (true) {
					int currentSegment = segmentIndex;
					var factor = VectorMath.ClosestPointOnLineFactor(path[currentSegment], path[currentSegment+1], point);
					if (factor > 1.0f && allowForwards && segmentIndex < path.Count - 2) {
						NextSegment();
						allowBackwards = false;
					} else if (factor < 0.0f && allowBackwards && segmentIndex > 0) {
						PrevSegment();
						allowForwards = false;
					} else {
						if (factor > 0.5f && segmentIndex < path.Count - 2) {
							NextSegment();
						}
						break;
					}
				}

				// Check the distances to the two segments extending from the vertex path[segmentIndex]
				// and pick the position on those segments that is closest to the #point parameter.
				float factor1 = 0, d1 = float.PositiveInfinity;

				if (segmentIndex > 0) {
					var s1 = segmentIndex - 1;
					factor1 = VectorMath.ClosestPointOnLineFactor(path[s1], path[s1+1], point);
					d1 = (Vector3.Lerp(path[s1], path[s1+1], factor1) - point).sqrMagnitude;
				}

				var factor2 = VectorMath.ClosestPointOnLineFactor(path[segmentIndex], path[segmentIndex+1], point);
				var d2 = (Vector3.Lerp(path[segmentIndex], path[segmentIndex+1], factor2) - point).sqrMagnitude;

				if (d1 < d2) MoveToSegment(segmentIndex - 1, factor1);
				else MoveToSegment(segmentIndex, factor2);
			}

			public void MoveToCircleIntersection2D<T>(Vector3 circleCenter3D, float radius, T transform) where T : IMovementPlane {
				AssertValid();

				var path = interpolator.path;

				// Move forwards as long as we are getting closer to circleCenter3D
				while (segmentIndex < path.Count - 2 && VectorMath.ClosestPointOnLineFactor(path[segmentIndex], path[segmentIndex+1], circleCenter3D) > 1) {
					NextSegment();
				}

				var circleCenter = transform.ToPlane(circleCenter3D);

				// Move forwards as long as the current segment endpoint is within the circle
				while (segmentIndex < path.Count - 2 && (transform.ToPlane(path[segmentIndex+1]) - circleCenter).sqrMagnitude <= radius*radius) {
					NextSegment();
				}

				// Calculate the intersection with the circle. This involves some math.
				var factor = VectorMath.LineCircleIntersectionFactor(circleCenter, transform.ToPlane(path[segmentIndex]), transform.ToPlane(path[segmentIndex+1]), radius);

				// Move to the intersection point
				MoveToSegment(segmentIndex, factor);
			}

			/// <summary>
			/// Integrates exp(-|x|/smoothingDistance)/(2*smoothingDistance) from a to b.
			/// The integral from -inf to +inf is 1.
			/// </summary>
			static float IntegrateSmoothingKernel (float a, float b, float smoothingDistance) {
				if (smoothingDistance <= 0) return a <= 0 && b > 0 ? 1 : 0;
				var iA = a < 0 ? Mathf.Exp(a / smoothingDistance) : 2.0f - Mathf.Exp(-a / smoothingDistance);
				var iB = b < 0 ? Mathf.Exp(b / smoothingDistance) : 2.0f - Mathf.Exp(-b / smoothingDistance);
				return 0.5f * (iB - iA);
			}

			/// <summary>Integrates (x - a)*exp(-x/smoothingDistance)/(2*smoothingDistance) from a to b.</summary>
			static float IntegrateSmoothingKernel2 (float a, float b, float smoothingDistance) {
				if (smoothingDistance <= 0) return 0f;
				var iA = -Mathf.Exp(-a / smoothingDistance) * (smoothingDistance);
				var iB = -Mathf.Exp(-b / smoothingDistance) * (smoothingDistance + b - a);
				return 0.5f * (iB - iA);
			}

			static Vector3 IntegrateSmoothTangent (Vector3 p1, Vector3 p2, ref Vector3 tangent, ref float distance, float expectedRadius, float smoothingDistance) {
				var segment = p2 - p1;
				var segmentLength = segment.magnitude;
				if (segmentLength <= 0.00001f) return Vector3.zero;
				var nextTangent = segment  * (1.0f / segmentLength);
				var deltaAngle = Vector3.Angle(tangent, nextTangent) * Mathf.Deg2Rad;
				var arcLength = expectedRadius*Mathf.Abs(deltaAngle);
				// We try to approximate
				// integrate kernel(x) * tangent(x); where * denotes convolution

				var integratedTangent = Vector3.zero;
				if (arcLength > float.Epsilon) {
					// Arc
					// integrate kernel(x) * (tangent + (nextTangent - tangent) * x/arcLength) dx
					var convolution = tangent * IntegrateSmoothingKernel(distance, distance + arcLength, smoothingDistance) +
									  (nextTangent - tangent) * IntegrateSmoothingKernel2(distance, distance + arcLength, smoothingDistance) / arcLength;
					integratedTangent += convolution;
					distance += arcLength;
				}

				// Straight line
				// integrate kernel(x) * nextTangent dx = nextTangent * integrate kernel(x) dx
				integratedTangent += nextTangent * IntegrateSmoothingKernel(distance, distance + segmentLength, smoothingDistance);
				tangent = nextTangent;
				distance += segmentLength;
				return integratedTangent;
			}

			public Vector3 EstimateSmoothTangent (Vector3 normalizedTangent, float smoothingDistance, float expectedRadius, Vector3 beforePathStartContribution, bool forward = true, bool backward = true) {
				AssertValid();
				if (expectedRadius <= float.Epsilon || smoothingDistance <= 0f) return normalizedTangent;

				var path = interpolator.path;
				var estimatedTangent = Vector3.zero;
				// Avoid zero-length segments at the start
				while (currentDistance >= this.distanceToSegmentStart + this.currentSegmentLength && segmentIndex < interpolator.path.Count - 2) NextSegment();
				if (forward) {
					var d = 0f;
					var prev = position;
					var prevTangent = normalizedTangent;
					for (int i = segmentIndex + 1; i < path.Count; i++) {
						estimatedTangent += IntegrateSmoothTangent(prev, path[i], ref prevTangent, ref d, expectedRadius, smoothingDistance);
						prev = path[i];
					}
				}
				if (backward) {
					var d = 0f;
					var prevTangent = -normalizedTangent;
					var prev = position;
					for (int i = segmentIndex; i >= 0; i--) {
						estimatedTangent -= IntegrateSmoothTangent(prev, path[i], ref prevTangent, ref d, expectedRadius, smoothingDistance);
						prev = path[i];
					}
					estimatedTangent += beforePathStartContribution * IntegrateSmoothingKernel(float.NegativeInfinity, -currentDistance, smoothingDistance);
				}

				return estimatedTangent;
			}

			public Vector3 EstimateSmoothCurvature (Vector3 tangent, float smoothingDistance, float expectedRadius) {
				AssertValid();
				if (expectedRadius <= float.Epsilon) return Vector3.zero;

				var path = interpolator.path;
				tangent = tangent.normalized;
				var curvature = Vector3.zero;
				// Avoid zero-length segments at the start
				while (currentDistance >= this.distanceToSegmentStart + this.currentSegmentLength && segmentIndex < interpolator.path.Count - 2) NextSegment();
				var d = 0f;
				var prev = position;
				var currentTangent = tangent.normalized;
				for (int i = segmentIndex + 1; i < path.Count; i++) {
					var segment = path[i] - prev;
					var t = segment.normalized;
					var deltaAngle = Vector3.Angle(currentTangent, t) * Mathf.Deg2Rad;
					var c = Vector3.Cross(currentTangent, t).normalized;
					var angleDerivative = 1.0f / expectedRadius;
					var arcLength = expectedRadius*Mathf.Abs(deltaAngle);
					// d/dx(f * angle(x)) = f * angle'(x); where * denotes convolution
					var convolutionDerivative = angleDerivative * IntegrateSmoothingKernel(d, d + arcLength, smoothingDistance);
					curvature -= convolutionDerivative * c;
					currentTangent = t;
					d += arcLength;
					d += segment.magnitude;
					prev = path[i];
				}
				// Do another integral in the backwards direction.
				// Ensures that if smoothingDistance is 0, the smoothing kernel will only be sampled once at x=0.
				d = float.Epsilon;
				currentTangent = -tangent.normalized;
				prev = position;
				for (int i = segmentIndex; i >= 0; i--) {
					var segment = path[i] - prev;
					if (segment == Vector3.zero) continue;

					var t = segment.normalized;
					var deltaAngle = Vector3.Angle(currentTangent, t) * Mathf.Deg2Rad;
					var c = Vector3.Cross(currentTangent, t).normalized;
					var angleDerivative = 1.0f / expectedRadius;
					var arcLength = expectedRadius*Mathf.Abs(deltaAngle);
					// d/dx(f * angle(x)) = f * angle'(x); where * denotes convolution
					var convolutionDerivative = angleDerivative * IntegrateSmoothingKernel(d, d + arcLength, smoothingDistance);
					curvature += convolutionDerivative * c;
					currentTangent = t;
					d += arcLength;
					d += segment.magnitude;
					prev = path[i];
				}
				return curvature;
			}

			/// <summary>
			/// Moves the agent along the path, stopping to rotate on the spot when the path changes direction.
			///
			/// Note: The cursor state does not include the rotation of the agent. So if an agent stops in the middle of a rotation, the final state of this struct will be as if the agent completed its rotation.
			///       If you want to preserve the rotation state as well, keep track of the output tangent, and pass it along to the next call to this function.
			/// </summary>
			/// <param name="time">The number of seconds to move forwards or backwards (if negative).</param>
			/// <param name="speed">Speed in meters/second.</param>
			/// <param name="turningSpeed">Turning speed in radians/second.</param>
			/// <param name="tangent">The current forwards direction of the agent. May be set to the #tangent property if you have no other needs.
			///               If set to something other than #tangent, the agent will start by rotating to face the #tangent direction.
			///               This will be replaced with the forwards direction of the agent after moving.
			///               It will be smoothly interpolated as the agent rotates from one segment to the next.
			///               It is more precise than the #tangent property after this call, which does not take rotation into account.
			///               This value is not necessarily normalized.</param>
			public void MoveWithTurningSpeed (float time, float speed, float turningSpeed, ref Vector3 tangent) {
				if (turningSpeed <= 0) throw new System.ArgumentException("turningSpeed must be greater than zero");
				if (speed <= 0) throw new System.ArgumentException("speed must be greater than zero");
				AssertValid();
				var radiansToMeters = speed / turningSpeed;
				var remainingOffset = time * speed;
				int its = 0;
				// Make sure we don't start by rotating unnecessarily
				while (remainingOffset > 0 && currentDistance >= this.distanceToSegmentStart + this.currentSegmentLength && segmentIndex < interpolator.path.Count - 2) NextSegment();
				while (remainingOffset < 0 && currentDistance <= this.distanceToSegmentStart && segmentIndex > 0) PrevSegment();
				while (remainingOffset != 0f) {
					its++;
					if (its > 100) throw new System.Exception("Infinite Loop " + remainingOffset + " " + time);
					var desiredTangent = this.tangent;
					if (tangent != desiredTangent && currentSegmentLength > 0) {
						// Rotate to face the desired tangent
						var angle = Vector3.Angle(tangent, desiredTangent) * Mathf.Deg2Rad;
						var arcLength = angle * radiansToMeters;
						if (Mathf.Abs(remainingOffset) > arcLength) {
							remainingOffset -= arcLength * Mathf.Sign(remainingOffset);
							tangent = desiredTangent;
						} else {
							tangent = Vector3.Slerp(tangent, desiredTangent, Mathf.Abs(remainingOffset) / arcLength);
							return;
						}
					}

					if (remainingOffset > 0) {
						// Move forward along the segment
						var remainingOnCurrentSegment = this.currentSegmentLength - (this.currentDistance - this.distanceToSegmentStart);
						if (remainingOffset >= remainingOnCurrentSegment) {
							remainingOffset -= remainingOnCurrentSegment;
							if (segmentIndex + 1 >= this.interpolator.path.Count - 1) {
								MoveToSegment(segmentIndex, 1.0f);
								return;
							} else {
								MoveToSegment(segmentIndex + 1, 0.0f);
							}
						} else {
							this.currentDistance += remainingOffset;
							return;
						}
					} else {
						// Move backward along the segment
						var remainingOnCurrentSegment = this.currentDistance - this.distanceToSegmentStart;
						if (-remainingOffset > remainingOnCurrentSegment) {
							remainingOffset += remainingOnCurrentSegment;
							if (segmentIndex - 1 < 0) {
								MoveToSegment(segmentIndex, 0.0f);
								return;
							} else {
								MoveToSegment(segmentIndex - 1, 1.0f);
							}
						} else {
							this.currentDistance += remainingOffset;
							return;
						}
					}
				}
			}

			void PrevSegment () {
				segmentIndex--;
				currentSegmentLength = (interpolator.path[segmentIndex+1] - interpolator.path[segmentIndex]).magnitude;
				distanceToSegmentStart -= currentSegmentLength;
			}

			void NextSegment () {
				segmentIndex++;
				distanceToSegmentStart += currentSegmentLength;
				currentSegmentLength = (interpolator.path[segmentIndex+1] - interpolator.path[segmentIndex]).magnitude;
			}
		}

		List<Vector3> path;
		int version = 1;
		float totalDistance;

		/// <summary>
		/// True if this instance has a path set.
		/// See: SetPath
		/// </summary>
		public bool valid {
			get {
				return path != null;
			}
		}

		public Cursor start {
			get {
				return Cursor.StartOfPath(this);
			}
		}

		public Cursor AtDistanceFromStart (float distance) {
			var cursor = start;

			cursor.distance = distance;
			return cursor;
		}

		/// <summary>
		/// Set the path to interpolate along.
		/// This will invalidate all existing cursors.
		/// </summary>
		public void SetPath (List<Vector3> path) {
			this.version++;
			if (this.path == null) this.path = new List<Vector3>();
			this.path.Clear();

			if (path == null) {
				totalDistance = float.PositiveInfinity;
				return;
			}

			if (path.Count < 2) throw new System.ArgumentException("Path must have a length of at least 2");

			var prev = path[0];

			totalDistance = 0;
			this.path.Capacity = Mathf.Max(this.path.Capacity, path.Count);
			this.path.Add(path[0]);
			for (int i = 1; i < path.Count; i++) {
				var current = path[i];
				// Avoid degenerate segments
				if (current != prev) {
					totalDistance += (current - prev).magnitude;
					this.path.Add(current);
					prev = current;
				}
			}
			if (this.path.Count < 2) this.path.Add(path[0]);
			if (float.IsNaN(totalDistance)) throw new System.ArgumentException("Path contains NaN values");
		}
	}
}
