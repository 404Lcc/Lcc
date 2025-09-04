//#define ASTARDEBUG   //"BBTree Debug" If enables, some queries to the tree will show debug lines. Turn off multithreading when using this since DrawLine calls cannot be called from a different thread

using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Profiling;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Pathfinding.Drawing;

namespace Pathfinding.Collections {
	using Pathfinding.Util;

	/// <summary>
	/// Axis Aligned Bounding Box Tree.
	/// Holds a bounding box tree of triangles.
	/// </summary>
	[BurstCompile]
	public struct BBTree : IDisposable {
		/// <summary>Holds all tree nodes</summary>
		UnsafeList<BBTreeBox> tree;
		UnsafeList<int> nodePermutation;

		const int MaximumLeafSize = 4;

		public IntRect Size => tree.Length == 0 ? default : tree[0].rect;

		// We need a stack while searching the tree.
		// We use a stack allocated array for this to avoid allocations.
		// A tile can at most contain NavmeshBase.VertexIndexMask triangles.
		// This works out to about a million. A perfectly balanced tree can fit this in log2(1000000/4) = 18 levels.
		// but we add a few more levels just to be safe, in case the tree is not perfectly balanced.
		const int MAX_TREE_HEIGHT = 26;

		public void Dispose () {
			nodePermutation.Dispose();
			tree.Dispose();
		}

		/// <summary>Build a BBTree from a list of triangles.</summary>
		/// <param name="triangles">The triangles. Each triplet of 3 indices represents a node. The triangles are assumed to be in clockwise order.</param>
		/// <param name="vertices">The vertices of the triangles.</param>
		public BBTree(UnsafeSpan<int> triangles, UnsafeSpan<Int3> vertices) {
			if (triangles.Length % 3 != 0) throw new ArgumentException("triangles must be a multiple of 3 in length");
			Build(ref triangles, ref vertices, out this);
		}

		[BurstCompile]
		static void Build (ref UnsafeSpan<int> triangles, ref UnsafeSpan<Int3> vertices, out BBTree bbTree) {
			var nodeCount = triangles.Length/3;
			// We will use approximately 2N tree nodes
			var tree = new UnsafeList<BBTreeBox>((int)(nodeCount * 2.1f), Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			// We will use approximately N node references
			var nodes = new UnsafeList<int>((int)(nodeCount * 1.1f), Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

			// This will store the order of the nodes while the tree is being built
			// It turns out that it is a lot faster to do this than to actually modify
			// the nodes and nodeBounds arrays (presumably since that involves shuffling
			// around 20 bytes of memory (sizeof(pointer) + sizeof(IntRect)) per node
			// instead of 4 bytes (sizeof(int)).
			// It also means we don't have to make a copy of the nodes array since
			// we do not modify it
			var permutation = new NativeArray<int>(nodeCount, Allocator.Temp);
			for (int i = 0; i < nodeCount; i++) {
				permutation[i] = i;
			}

			// Precalculate the bounds of the nodes in XZ space.
			// It turns out that calculating the bounds is a bottleneck and precalculating
			// the bounds makes it around 3 times faster to build a tree
			var nodeBounds = new NativeArray<IntRect>(nodeCount, Allocator.Temp);
			for (int i = 0; i < nodeCount; i++) {
				var v0 = ((int3)vertices[triangles[i*3+0]]).xz;
				var v1 = ((int3)vertices[triangles[i*3+1]]).xz;
				var v2 = ((int3)vertices[triangles[i*3+2]]).xz;
				var mn = math.min(v0, math.min(v1, v2));
				var mx = math.max(v0, math.max(v1, v2));
				nodeBounds[i] = new IntRect(mn.x, mn.y, mx.x, mx.y);
			}

			if (nodeCount > 0) BuildSubtree(permutation, nodeBounds, ref nodes, ref tree, 0, nodeCount, false, 0);
			nodeBounds.Dispose();
			permutation.Dispose();

			bbTree = new BBTree {
				tree = tree,
				nodePermutation = nodes,
			};
		}

		static int SplitByX (NativeArray<IntRect> nodesBounds, NativeArray<int> permutation, int from, int to, int divider) {
			int mx = to;

			for (int i = from; i < mx; i++) {
				var cr = nodesBounds[permutation[i]];
				var cx = (cr.xmin + cr.xmax)/2;
				if (cx > divider) {
					mx--;
					// Swap items i and mx
					var tmp = permutation[mx];
					permutation[mx] = permutation[i];
					permutation[i] = tmp;
					i--;
				}
			}
			return mx;
		}

		static int SplitByZ (NativeArray<IntRect> nodesBounds, NativeArray<int> permutation, int from, int to, int divider) {
			int mx = to;

			for (int i = from; i < mx; i++) {
				var cr = nodesBounds[permutation[i]];
				var cx = (cr.ymin + cr.ymax)/2;
				if (cx > divider) {
					mx--;
					// Swap items i and mx
					var tmp = permutation[mx];
					permutation[mx] = permutation[i];
					permutation[i] = tmp;
					i--;
				}
			}
			return mx;
		}

		static int BuildSubtree (NativeArray<int> permutation, NativeArray<IntRect> nodeBounds, ref UnsafeList<int> nodes, ref UnsafeList<BBTreeBox> tree, int from, int to, bool odd, int depth) {
			var rect = NodeBounds(permutation, nodeBounds, from, to);
			int boxId = tree.Length;
			tree.Add(new BBTreeBox(rect));

			if (to - from <= MaximumLeafSize) {
				if (depth > MAX_TREE_HEIGHT) {
					Debug.LogWarning($"Maximum tree height of {MAX_TREE_HEIGHT} exceeded (got depth of {depth}). Querying this tree may fail. Is the tree very unbalanced?");
				}
				var box = tree[boxId];
				var nodeOffset = box.nodeOffset = nodes.Length;
				tree[boxId] = box;
				nodes.Length += MaximumLeafSize;
				// Assign all nodes to the array. Note that we also need clear unused slots as the array from the pool may contain any information
				for (int i = 0; i < MaximumLeafSize; i++) {
					nodes[nodeOffset + i] = i < to - from ? permutation[from + i] : -1;
				}
				return boxId;
			} else {
				int splitIndex;
				if (odd) {
					// X
					int divider = (rect.xmin + rect.xmax)/2;
					splitIndex = SplitByX(nodeBounds, permutation, from, to, divider);
				} else {
					// Y/Z
					int divider = (rect.ymin + rect.ymax)/2;
					splitIndex = SplitByZ(nodeBounds, permutation, from, to, divider);
				}

				int margin = (to - from)/8;
				bool veryUneven = splitIndex <= from + margin || splitIndex >= to - margin;
				if (veryUneven) {
					// All nodes were on one side of the divider
					// Try to split along the other axis

					if (!odd) {
						// X
						int divider = (rect.xmin + rect.xmax)/2;
						splitIndex = SplitByX(nodeBounds, permutation, from, to, divider);
					} else {
						// Y/Z
						int divider = (rect.ymin + rect.ymax)/2;
						splitIndex = SplitByZ(nodeBounds, permutation, from, to, divider);
					}
					veryUneven = splitIndex <= from + margin || splitIndex >= to - margin;

					if (veryUneven) {
						// Almost all nodes were on one side of the divider
						// Just pick one half
						splitIndex = (from+to)/2;
					}
				}

				var left = BuildSubtree(permutation, nodeBounds, ref nodes, ref tree, from, splitIndex, !odd, depth+1);
				var right = BuildSubtree(permutation, nodeBounds, ref nodes, ref tree, splitIndex, to, !odd, depth+1);
				var box = tree[boxId];
				box.left = left;
				box.right = right;
				tree[boxId] = box;

				return boxId;
			}
		}

		/// <summary>Calculates the bounding box in XZ space of all nodes between from (inclusive) and to (exclusive)</summary>
		static IntRect NodeBounds (NativeArray<int> permutation, NativeArray<IntRect> nodeBounds, int from, int to) {
			var mn = nodeBounds[permutation[from]].Min.ToInt2();
			var mx = nodeBounds[permutation[from]].Max.ToInt2();

			for (int j = from + 1; j < to; j++) {
				var otherRect = nodeBounds[permutation[j]];
				var rmin = new int2(otherRect.xmin, otherRect.ymin);
				var rmax = new int2(otherRect.xmax, otherRect.ymax);
				mn = math.min(mn, rmin);
				mx = math.max(mx, rmax);
			}

			return new IntRect(mn.x, mn.y, mx.x, mx.y);
		}

		[BurstCompile]
		public readonly struct ProjectionParams {
			public readonly float2x3 planeProjection;
			public readonly float2 projectedUpNormalized;
			public readonly float3 projectionAxis;
			public readonly float distanceScaleAlongProjectionAxis;
			public readonly DistanceMetric distanceMetric;
			// bools are for some reason not blittable by the burst compiler, so we have to use a byte
			readonly byte alignedWithXZPlaneBacking;

			public bool alignedWithXZPlane => alignedWithXZPlaneBacking != 0;

			/// <summary>
			/// Calculates the squared distance from a point to a box when projected to 2D.
			///
			/// The input rectangle is assumed to be on the XZ plane, and to actually represent an infinitely tall box (along the Y axis).
			///
			/// The planeProjection matrix projects points from 3D to 2D. The box will also be projected.
			/// The upProjNormalized vector is the normalized direction orthogonal to the 2D projection.
			/// It is the direction pointing out of the plane from the projection's point of view.
			///
			/// In the special case that the projection just projects 3D coordinates onto the XZ plane, this is
			/// equivalent to the distance from a point to a rectangle in 2D.
			/// </summary>
			public float SquaredRectPointDistanceOnPlane (IntRect rect, float3 p) {
				return SquaredRectPointDistanceOnPlane(in this, ref rect, ref p);
			}

			[BurstCompile(FloatMode = FloatMode.Fast)][IgnoredByDeepProfiler]
			private static float SquaredRectPointDistanceOnPlane (in ProjectionParams projection, ref IntRect rect, ref float3 p) {
				if (projection.alignedWithXZPlane) {
					var p1 = new float2(rect.xmin, rect.ymin) * Int3.PrecisionFactor;
					var p4 = new float2(rect.xmax, rect.ymax) * Int3.PrecisionFactor;
					var closest = math.clamp(p.xz, p1, p4);
					return math.lengthsq(closest - p.xz);
				} else {
					var p1 = new float3(rect.xmin, 0, rect.ymin) * Int3.PrecisionFactor - p;
					var p4 = new float3(rect.xmax, 0, rect.ymax) * Int3.PrecisionFactor - p;
					var p2 = new float3(rect.xmin, 0, rect.ymax) * Int3.PrecisionFactor - p;
					var p3 = new float3(rect.xmax, 0, rect.ymin) * Int3.PrecisionFactor - p;
					var p1proj = math.mul(projection.planeProjection, p1);
					var p2proj = math.mul(projection.planeProjection, p2);
					var p3proj = math.mul(projection.planeProjection, p3);
					var p4proj = math.mul(projection.planeProjection, p4);
					var upNormal = new float2(projection.projectedUpNormalized.y, -projection.projectedUpNormalized.x);
					// Calculate the dot product of pNproj and upNormal for all N, this is the distance between p and pN
					// along the direction orthogonal to upProjNormalized.
					// The box is infinite along the up direction (since it is only a rect). When projected down to 2D
					// this results in an infinite line with a given thickness (a beam).
					// This is assuming the projection direction is not parallel to the world up direction, in which case we
					// would have entered the other branch of this if statement.
					// The minumum value and maximum value in dists gives us the signed distance to this beam
					// from the point p.
					var dists = math.mul(math.transpose(new float2x4(p1proj, p2proj, p3proj, p4proj)), upNormal);
					// Calculate the shortest distance to the beam (may be 0 if p is inside the beam).
					var dist = math.clamp(0, math.cmin(dists), math.cmax(dists));
					return dist*dist;
				}
			}

			public ProjectionParams(NNConstraint constraint, GraphTransform graphTransform) {
				const float MAX_ERROR_IN_RADIANS = 0.01f;

				// The normal of the plane we are projecting onto (if any).
				if (constraint != null && constraint.distanceMetric.projectionAxis != Vector3.zero) {
					// (inf,inf,inf) is a special value indicating to use the graph's natural up direction
					if (float.IsPositiveInfinity(constraint.distanceMetric.projectionAxis.x)) {
						projectionAxis = new float3(0, 1, 0);
					} else {
						projectionAxis = math.normalizesafe(graphTransform.InverseTransformVector(constraint.distanceMetric.projectionAxis));
					}

					if (projectionAxis.x*projectionAxis.x + projectionAxis.z*projectionAxis.z < MAX_ERROR_IN_RADIANS*MAX_ERROR_IN_RADIANS) {
						// We could let the code below handle this case, but since it is a common case we can optimize it a bit
						// by using a fast-path here.
						projectedUpNormalized = float2.zero;
						planeProjection = new float2x3(1, 0, 0, 0, 0, 1); // math.transpose(new float3x2(new float3(1, 0, 0), new float3(0, 0, 1)));
						distanceMetric = DistanceMetric.ScaledManhattan;
						alignedWithXZPlaneBacking = (byte)1;
						distanceScaleAlongProjectionAxis = math.max(constraint.distanceMetric.distanceScaleAlongProjectionDirection, 0);
						return;
					}

					// Find any two vectors which are perpendicular to the normal (and each other)
					var planeAxis1 = math.normalizesafe(math.cross(new float3(1, 0, 1), projectionAxis));

					if (math.all(planeAxis1 == 0)) planeAxis1 = math.normalizesafe(math.cross(new float3(-1, 0, 1), projectionAxis));
					var planeAxis2 = math.normalizesafe(math.cross(projectionAxis, planeAxis1));
					// Note: The inverse of an orthogonal matrix is its transpose, and the transpose is faster to compute
					planeProjection = math.transpose(new float3x2(planeAxis1, planeAxis2));
					// The projection of the (0,1,0) vector onto the plane.
					// This is important because the BBTree stores its rectangles in the XZ plane.
					// If the projection is close enough to the XZ plane, we snap to that because it allows us to use faster and more precise distance calculations.
					projectedUpNormalized = math.lengthsq(planeProjection.c1) <= MAX_ERROR_IN_RADIANS*MAX_ERROR_IN_RADIANS ? float2.zero : math.normalize(planeProjection.c1);
					distanceMetric = DistanceMetric.ScaledManhattan;
					alignedWithXZPlaneBacking = math.all(projectedUpNormalized == 0) ? (byte)1 : (byte)0;

					// The distance along the projection axis is scaled by a cost factor to make the distance
					// along the projection direction more or less important compared to the distance in the plane.
					// Usually the projection direction is less important.
					// For example, when an agent looks for the closest node, it is typically more interested in finding a point close
					// to it which is more or less directly below it, than it is in finding a point which is closer, but requires sideways movement.
					// Even if this value is zero we will use the distance along the projection axis to break ties.
					// Otherwise, when getting the nearest node in e.g. a tall building, it would not be well defined
					// which floor of the building was closest.
					distanceScaleAlongProjectionAxis = math.max(constraint.distanceMetric.distanceScaleAlongProjectionDirection, 0);
				} else {
					projectionAxis = float3.zero;
					planeProjection = default;
					projectedUpNormalized = default;
					distanceMetric = DistanceMetric.Euclidean;
					alignedWithXZPlaneBacking = 1;
					distanceScaleAlongProjectionAxis = 0;
				}
			}
		}

		public float DistanceSqrLowerBound (float3 p, in ProjectionParams projection) {
			if (tree.Length == 0) return float.PositiveInfinity;
			return projection.SquaredRectPointDistanceOnPlane(tree[0].rect, p);
		}

		/// <summary>
		/// Queries the tree for the closest node to p constrained by the NNConstraint trying to improve an existing solution.
		/// Note that this function will only fill in the constrained node.
		/// If you want a node not constrained by any NNConstraint, do an additional search with constraint = NNConstraint.None
		/// </summary>
		/// <param name="p">Point to search around</param>
		/// <param name="constraint">Optionally set to constrain which nodes to return</param>
		/// <param name="distanceSqr">The best squared distance for the previous solution. Will be updated with the best distance
		/// after this search. Supply positive infinity to start the search from scratch.</param>
		/// <param name="previous">This search will start from the previous NNInfo and improve it if possible. Will be updated with the new result.
		/// Even if the search fails on this call, the solution will never be worse than previous.</param>
		/// <param name="nodes">The nodes what this BBTree was built from</param>
		/// <param name="triangles">The triangles that this BBTree was built from</param>
		/// <param name="vertices">The vertices that this BBTree was built from</param>
		/// <param name="projection">Projection parameters derived from the constraint</param>
		public void QueryClosest (float3 p, NNConstraint constraint, in ProjectionParams projection, ref float distanceSqr, ref NNInfo previous, GraphNode[] nodes, UnsafeSpan<int> triangles, UnsafeSpan<Int3> vertices) {
			if (tree.Length == 0) return;

			UnsafeSpan<NearbyNodesIterator.BoxWithDist> stack;
			unsafe {
				NearbyNodesIterator.BoxWithDist* stackPtr = stackalloc NearbyNodesIterator.BoxWithDist[MAX_TREE_HEIGHT];
				stack = new UnsafeSpan<NearbyNodesIterator.BoxWithDist>(stackPtr, MAX_TREE_HEIGHT);
			}
			stack[0] = new NearbyNodesIterator.BoxWithDist {
				index = 0,
				distSqr = 0.0f,
			};
			var it = new NearbyNodesIterator {
				stack = stack,
				stackSize = 1,
				indexInLeaf = 0,
				point = p,
				projection = projection,
				distanceThresholdSqr = distanceSqr,
				tieBreakingDistanceThreshold = float.PositiveInfinity,
				tree = tree.AsUnsafeSpan(),
				nodes = nodePermutation.AsUnsafeSpan(),
				triangles = triangles,
				vertices = vertices,
			};

			// We use an iterator which searches through the tree and returns nodes closer than it.distanceThresholdSqr.
			// The iterator is compiled using burst for high performance, but when a new candidate node is found we need
			// to evaluate it in pure C# due to the NNConstraint being a C# class.
			// TODO: If constraint==null (or NNConstraint.None) we could run the whole thing in burst to improve perf even more.
			var result = previous;
			while (it.stackSize > 0 && it.MoveNext()) {
				var current = it.current;
				if (constraint == null || constraint.Suitable(nodes[current.node])) {
					it.distanceThresholdSqr = current.distanceSq;
					it.tieBreakingDistanceThreshold = current.tieBreakingDistance;
					result = new NNInfo(nodes[current.node], current.closestPointOnNode, current.distanceSq);
				}
			}
			distanceSqr = it.distanceThresholdSqr;
			previous = result;
		}

		struct CloseNode {
			public int node;
			public float distanceSq;
			public float tieBreakingDistance;
			public float3 closestPointOnNode;
		}

		public enum DistanceMetric: byte {
			Euclidean,
			ScaledManhattan,
		}

		[BurstCompile]
		struct NearbyNodesIterator : IEnumerator<CloseNode> {
			public UnsafeSpan<BoxWithDist> stack;
			public int stackSize;
			public UnsafeSpan<BBTreeBox> tree;
			public UnsafeSpan<int> nodes;
			public UnsafeSpan<int> triangles;
			public UnsafeSpan<Int3> vertices;
			public int indexInLeaf;
			public float3 point;
			public ProjectionParams projection;
			public float distanceThresholdSqr;
			public float tieBreakingDistanceThreshold;
			internal CloseNode current;

			public CloseNode Current => current;

			public struct BoxWithDist {
				public int index;
				public float distSqr;
			}

			public bool MoveNext () {
				return MoveNext(ref this);
			}

			void IDisposable.Dispose () {}

			void System.Collections.IEnumerator.Reset() => throw new NotSupportedException();
			object System.Collections.IEnumerator.Current => throw new NotSupportedException();

			// Note: Using FloatMode=Fast here can cause NaNs in rare cases.
			// I have not tracked down why, but it is not unreasonable given that FloatMode=Fast assumes that infinities do not happen.
			[BurstCompile(FloatMode = FloatMode.Default)]
			static bool MoveNext (ref NearbyNodesIterator it) {
				var distanceThresholdSqr = it.distanceThresholdSqr;
				while (true) {
					if (it.stackSize == 0) {
						return false;
					}

					// Pop the last element from the stack
					var boxRef = it.stack[it.stackSize-1];

					// If we cannot possibly find anything better than the current best solution in here, skip this box.
					// Allow the search when we can find an equally close node, because tie breaking
					// may cause this search to find a better node.
					if (boxRef.distSqr > distanceThresholdSqr) {
						it.stackSize--;
						// Setting this to zero shouldn't be necessary in theory, as a leaf will always (in theory) be searched completely.
						// However, in practice the distance to a node may be a tiny bit lower than the distance to the box containing the node, due to floating point errors.
						// and so the leaf's search may be terminated early if a point is found on a node exactly on the border of the box.
						// In that case it is important that we reset the iterator to the start of the next leaf.
						it.indexInLeaf = 0;
						continue;
					}

					BBTreeBox box = it.tree[boxRef.index];
					if (box.IsLeaf) {
						for (int i = it.indexInLeaf; i < MaximumLeafSize; i++) {
							var node = it.nodes[box.nodeOffset + i];
							if (node == -1) break;
							var ti1 = (uint)(node*3 + 0);
							var ti2 = (uint)(node*3 + 1);
							var ti3 = (uint)(node*3 + 2);
							if (ti3 >= it.triangles.length) throw new Exception("Invalid node index");
							Unity.Burst.CompilerServices.Hint.Assume(ti1 < it.triangles.length && ti2 < it.triangles.length && ti3 < it.triangles.length);
							var vi1 = it.vertices[it.triangles[ti1]];
							var vi2 = it.vertices[it.triangles[ti2]];
							var vi3 = it.vertices[it.triangles[ti3]];
							if (it.projection.distanceMetric == DistanceMetric.Euclidean) {
								var v1 = (float3)vi1;
								var v2 = (float3)vi2;
								var v3 = (float3)vi3;
								Polygon.ClosestPointOnTriangleByRef(in v1, in v2, in v3, in it.point, out var closest);
								var sqrDist = math.distancesq(closest, it.point);
								if (sqrDist < distanceThresholdSqr) {
									it.indexInLeaf = i + 1;
									it.current = new CloseNode {
										node = node,
										distanceSq = sqrDist,
										tieBreakingDistance = 0,
										closestPointOnNode = closest,
									};
									return true;
								}
							} else {
								Polygon.ClosestPointOnTriangleProjected(ref vi1, ref vi2, ref vi3, ref it.projection, ref it.point, out var closest, out var sqrDist, out var distAlongProjection);
								// Check if this point is better than the previously best point.
								// Handling ties here is important, in case the navmesh has multiple overlapping regions (e.g. a multi-story building).
								if (sqrDist < distanceThresholdSqr || (sqrDist == distanceThresholdSqr && distAlongProjection < it.tieBreakingDistanceThreshold)) {
									it.indexInLeaf = i + 1;
									it.current = new CloseNode {
										node = node,
										distanceSq = sqrDist,
										tieBreakingDistance = distAlongProjection,
										closestPointOnNode = closest,
									};
									return true;
								}
							}
						}
						it.indexInLeaf = 0;
						it.stackSize--;
					} else {
						it.stackSize--;

						int first = box.left, second = box.right;
						var firstDist = it.projection.SquaredRectPointDistanceOnPlane(it.tree[first].rect, it.point);
						var secondDist = it.projection.SquaredRectPointDistanceOnPlane(it.tree[second].rect, it.point);

						if (secondDist < firstDist) {
							// Swap
							Memory.Swap(ref first, ref second);
							Memory.Swap(ref firstDist, ref secondDist);
						}

						if (it.stackSize + 2 > it.stack.Length) {
							throw new InvalidOperationException("Tree is too deep. Overflowed the internal stack.");
						}

						// Push both children on the stack so that we can explore them later (if they are not too far away).
						// We push the one with the smallest distance last so that it will be popped first.
						if (secondDist <= distanceThresholdSqr) it.stack[it.stackSize++] = new BoxWithDist {
								index = second,
								distSqr = secondDist,
							};
						if (firstDist <= distanceThresholdSqr) it.stack[it.stackSize++] = new BoxWithDist {
								index = first,
								distSqr = firstDist,
							};
					}
				}
			}
		}

		struct BBTreeBox {
			public IntRect rect;

			public int nodeOffset;
			public int left, right;

			public bool IsLeaf => nodeOffset >= 0;

			public BBTreeBox (IntRect rect) {
				nodeOffset = -1;
				this.rect = rect;
				left = right = -1;
			}
		}

		public void DrawGizmos (CommandBuilder draw) {
			Gizmos.color = new Color(1, 1, 1, 0.5F);
			if (tree.Length == 0) return;
			DrawGizmos(ref draw, 0, 0);
		}

		void DrawGizmos (ref CommandBuilder draw, int boxi, int depth) {
			BBTreeBox box = tree[boxi];

			var min = (Vector3) new Int3(box.rect.xmin, 0, box.rect.ymin);
			var max = (Vector3) new Int3(box.rect.xmax, 0, box.rect.ymax);

			Vector3 center = (min+max)*0.5F;
			Vector3 size = max-min;

			size = new Vector3(size.x, 1, size.z);
			center.y += depth * 2;

			draw.xz.WireRectangle(center, new float2(size.x, size.z), AstarMath.IntToColor(depth, 1f));

			if (!box.IsLeaf) {
				DrawGizmos(ref draw, box.left, depth + 1);
				DrawGizmos(ref draw, box.right, depth + 1);
			}
		}
	}
}
