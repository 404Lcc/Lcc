using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Assertions;

namespace Pathfinding {
	using Pathfinding.Pooling;
	using Pathfinding.Collections;
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Mathematics;
	using UnityEngine.Profiling;

	/// <summary>
	/// Implements the funnel algorithm as well as various related methods.
	/// See: http://digestingduck.blogspot.se/2010/03/simple-stupid-funnel-algorithm.html
	/// See: Usually you do not use this class directly. Instead use the <see cref="FunnelModifier"/> component.
	///
	/// <code>
	/// using UnityEngine;
	/// using Pathfinding;
	/// using Pathfinding.Drawing;
	///
	/// public class FunnelExample : MonoBehaviour {
	///     public Transform target = null;
	///
	///     void Update () {
	///         var path = ABPath.Construct(transform.position, target.position);
	///
	///         AstarPath.StartPath(path);
	///         path.BlockUntilCalculated();
	///
	///         // Apply some default adjustments to the path
	///         // not necessary if you are using the Seeker component
	///         new StartEndModifier().Apply(path);
	///
	///         // Split the path into segments and links
	///         var parts = Funnel.SplitIntoParts(path);
	///         // Optionally simplify the path to make it straighter
	///         var nodes = path.path;
	///         Funnel.Simplify(parts, ref nodes);
	///
	///         using (Draw.WithLineWidth(2)) {
	///             // Go through all the parts and draw them in the scene view
	///             for (int i = 0; i < parts.Count; i++) {
	///                 var part = parts[i];
	///                 if (part.type == Funnel.PartType.OffMeshLink) {
	///                     // Draw off-mesh links as a single line
	///                     Draw.Line(part.startPoint, part.endPoint, Color.cyan);
	///                 } else {
	///                     // Calculate the shortest path through the funnel
	///                     var portals = Funnel.ConstructFunnelPortals(nodes, part);
	///                     var pathThroghPortals = Funnel.Calculate(portals, splitAtEveryPortal: false);
	///                     Draw.Polyline(pathThroghPortals, Color.black);
	///                 }
	///             }
	///         }
	///     }
	/// }
	/// </code>
	///
	/// In the image you can see the output from the code example above. The cyan lines represent off-mesh links.
	///
	/// [Open online documentation to see images]
	/// </summary>
	[BurstCompile]
	public static class Funnel {
		/// <summary>Funnel in which the path to the target will be</summary>
		public struct FunnelPortals {
			public List<Vector3> left;
			public List<Vector3> right;
		}

		/// <summary>The type of a <see cref="PathPart"/></summary>
		public enum PartType {
			/// <summary>An off-mesh link between two nodes in the same or different graphs</summary>
			OffMeshLink,
			/// <summary>A sequence of adjacent nodes in the same graph</summary>
			NodeSequence,
		}

		/// <summary>
		/// Part of a path.
		/// This is either a sequence of adjacent triangles
		/// or a link.
		/// See: NodeLink2
		/// </summary>
		public struct PathPart {
			/// <summary>Index of the first node in this part</summary>
			public int startIndex;
			/// <summary>Index of the last node in this part</summary>
			public int endIndex;
			/// <summary>Exact start-point of this part or off-mesh link</summary>
			public Vector3 startPoint;
			/// <summary>Exact end-point of this part or off-mesh link</summary>
			public Vector3 endPoint;
			/// <summary>If this is an off-mesh link or a sequence of nodes in a single graph</summary>
			public PartType type;
		}

		/// <summary>Splits the path into a sequence of parts which are either off-mesh links or sequences of adjacent triangles</summary>

		public static List<PathPart> SplitIntoParts (Path path) {
			var nodes = path.path;

			var result = ListPool<PathPart>.Claim();

			if (nodes == null || nodes.Count == 0) {
				return result;
			}

			// Loop through the path and split it into
			// parts joined by links
			for (int i = 0; i < nodes.Count; i++) {
				var node = nodes[i];
				if (node is TriangleMeshNode || node is GridNodeBase) {
					var startIndex = i;
					uint currentGraphIndex = node.GraphIndex;

					// Loop up until we find a node in another graph
					// Ignore NodeLink3 nodes
					while (i < nodes.Count && (nodes[i].GraphIndex == currentGraphIndex || nodes[i] is NodeLink3Node)) i++;

					i--;
					var endIndex = i;
					result.Add(new PathPart {
						type = PartType.NodeSequence,
						startIndex = startIndex,
						endIndex = endIndex,
						// If this is the first part in the path, use the exact start point
						// otherwise use the position of the node right before the start of this
						// part which is likely the end of the link to this part
						startPoint = startIndex == 0 ? path.vectorPath[0] : (Vector3)nodes[startIndex-1].position,
						endPoint = endIndex == nodes.Count-1 ? path.vectorPath[path.vectorPath.Count-1] : (Vector3)nodes[endIndex+1].position,
					});
				} else if (node is LinkNode) {
					var startIndex = i;
					var currentGraphIndex = node.GraphIndex;

					while (i < nodes.Count && nodes[i].GraphIndex == currentGraphIndex) i++;
					i--;

					if (i - startIndex == 0) {
						// The link is a single node.
						// Just ignore it. It can happen in very rare circumstances with some path types.
						// For example, a RandomPath can stop at the first node of a node link, without including the other end of the link.

						if (startIndex > 0 && startIndex + 1 < nodes.Count && nodes[startIndex - 1] == nodes[startIndex + 1]) {
							// We can also move to a node link node and then immediately move back to the previous node in rare circumstances.
							// Since triangle nodes are represented as 3 nodes during pathfinding, this is a possibility.
							// (TODO: How can this happen in practice? It has been empirically observed on a standard graph, but the edge costs must be kinda weird for it to happen?)

							// [A, LinkNode, A] => [A]
							nodes.RemoveRange(startIndex, 2);
							i--;
							throw new System.Exception("Link node connected back to the previous node in the path. This should not happen.");
						} else {
							// [A, LinkNode] => [A]
							// [LinkNode, A] => [A]
							Assert.IsTrue(startIndex == 0 || startIndex == nodes.Count - 1);
							nodes.RemoveAt(startIndex);
							i--;
						}

						continue;
					} else if (i - startIndex != 1) {
						throw new System.Exception("Off mesh link included more than two nodes: " + (i - startIndex + 1));
					}

					result.Add(new PathPart {
						type = PartType.OffMeshLink,
						startIndex = startIndex,
						endIndex = i,
						startPoint = (Vector3)nodes[startIndex].position,
						endPoint = (Vector3)nodes[i].position,
					});
				} else {
					throw new System.Exception("Unsupported node type or null node");
				}
			}

			// The path should always start and stop on regular nodes
			if (result[0].type == PartType.OffMeshLink) {
				result.RemoveAt(0);
			}
			if (result[result.Count - 1].type == PartType.OffMeshLink) {
				result.RemoveAt(result.Count - 1);
			}

			Assert.IsTrue(result.Count > 0);
			Assert.AreEqual(result[0].startIndex, 0);
			Assert.AreEqual(result[0].type, PartType.NodeSequence);
			Assert.AreEqual(result[result.Count-1].type, PartType.NodeSequence);
			Assert.AreEqual(result[result.Count-1].endIndex, nodes.Count - 1);

			return result;
		}


		public static void Simplify (List<PathPart> parts, ref List<GraphNode> nodes, System.Func<GraphNode, bool> filter = null) {
			List<GraphNode> resultNodes = ListPool<GraphNode>.Claim();

			for (int i = 0; i < parts.Count; i++) {
				var part = parts[i];

				// We are changing the nodes list, so indices may change
				var newPart = part;
				newPart.startIndex = resultNodes.Count;

				if (part.type == PartType.NodeSequence) {
					if (nodes[part.startIndex].Graph is IRaycastableGraph graph) {
						Simplify(part, graph, nodes, resultNodes, Path.ZeroTagPenalties, -1, filter);
						newPart.endIndex = resultNodes.Count - 1;
						parts[i] = newPart;
						continue;
					}
				}

				for (int j = part.startIndex; j <= part.endIndex; j++) {
					resultNodes.Add(nodes[j]);
				}
				newPart.endIndex = resultNodes.Count - 1;
				parts[i] = newPart;
			}

			ListPool<GraphNode>.Release(ref nodes);
			nodes = resultNodes;
		}

		/// <summary>
		/// Simplifies a funnel path using linecasting.
		/// Running time is roughly O(n^2 log n) in the worst case (where n = end-start)
		/// Actually it depends on how the graph looks, so in theory the actual upper limit on the worst case running time is O(n*m log n) (where n = end-start and m = nodes in the graph)
		/// but O(n^2 log n) is a much more realistic worst case limit.
		///
		/// Requires graph to implement IRaycastableGraph
		/// </summary>
		public static void Simplify (PathPart part, IRaycastableGraph graph, List<GraphNode> nodes, List<GraphNode> result, int[] tagPenalties, int traversableTags, System.Func<GraphNode, bool> filter = null) {
			var start = part.startIndex;
			var end = part.endIndex;
			var startPoint = part.startPoint;
			var endPoint = part.endPoint;

			if (graph == null) throw new System.ArgumentNullException(nameof(graph));

			if (start > end) {
				throw new System.ArgumentException("start > end");
			}

			// Do a straight line of sight check to see if the path can be simplified to a single line
			{
				if (!graph.Linecast(startPoint, endPoint, out GraphHitInfo hit) && hit.node == nodes[end]) {
					graph.Linecast(startPoint, endPoint, out hit, result);

					long penaltySum = 0;
					long penaltySum2 = 0;
					for (int i = start; i <= end; i++) {
						penaltySum += nodes[i].Penalty + tagPenalties[nodes[i].Tag];
					}

					bool walkable = true;
					for (int i = 0; i < result.Count; i++) {
						penaltySum2 += result[i].Penalty + tagPenalties[result[i].Tag];
						walkable &= ((traversableTags >> (int)result[i].Tag) & 1) == 1;
						walkable &= filter == null || filter(result[i]);
					}

					// Allow 40% more penalty on average per node
					if (!walkable || (penaltySum*1.4*result.Count) < (penaltySum2*(end-start+1))) {
						// The straight line penalties are much higher than the original path.
						// Revert the simplification
						result.Clear();
					} else {
						// The straight line simplification looks good.
						// We are done here.
						return;
					}
				}
			}

			int ostart = start;

			int count = 0;
			while (true) {
				if (count++ > 1000) {
					Debug.LogError("Was the path really long or have we got cought in an infinite loop?");
					break;
				}

				if (start == end) {
					result.Add(nodes[end]);
					RemoveBacktracking(result, ostart, result.Count-2);
					return;
				}

				int resCount = result.Count;

				// Run a binary search to find the furthest node that we have a clear line of sight to
				int mx = end+1;
				int mn = start+1;
				bool anySucceded = false;
				while (mx > mn+1) {
					int mid = (mx+mn)/2;

					Vector3 sp = start == ostart ? startPoint : (Vector3)nodes[start].position;
					Vector3 ep = mid == end ? endPoint : (Vector3)nodes[mid].position;

					// Check if there is an obstacle between these points, or if there is no obstacle, but we didn't end up at the right node.
					// The second case can happen for example in buildings with multiple floors.
					if (graph.Linecast(sp, ep, out GraphHitInfo hit) || hit.node != nodes[mid]) {
						mx = mid;
					} else {
						anySucceded = true;
						mn = mid;
					}
				}

				if (!anySucceded) {
					result.Add(nodes[start]);
					RemoveBacktracking(result, ostart, result.Count-2);

					// It is guaranteed that mn = start+1
					start = mn;
				} else {
					// Replace a part of the path with the straight path to the furthest node we had line of sight to.
					// Need to redo the linecast to get the trace (i.e. list of nodes along the line of sight).
					Vector3 sp = start == ostart ? startPoint : (Vector3)nodes[start].position;
					Vector3 ep = mn == end ? endPoint : (Vector3)nodes[mn].position;
					graph.Linecast(sp, ep, out _, result);

					long penaltySum = 0;
					long penaltySum2 = 0;
					for (int i = start; i <= mn; i++) {
						penaltySum += nodes[i].Penalty + tagPenalties[nodes[i].Tag];
					}

					bool walkable = true;
					for (int i = resCount; i < result.Count; i++) {
						penaltySum2 += result[i].Penalty + tagPenalties[result[i].Tag];
						walkable &= ((traversableTags >> (int)result[i].Tag) & 1) == 1;
					}

					// Allow 40% more penalty on average per node
					if (!walkable || (penaltySum*1.4*(result.Count-resCount)) < (penaltySum2*(mn-start+1)) || result[result.Count-1] != nodes[mn]) {
						// Linecast hit the wrong node or it is a lot more expensive than the original path
						result.RemoveRange(resCount, result.Count-resCount);

						result.Add(nodes[start]);
						start++;
					} else {
						// In some rare cases, doing the raycast simplification may cause it to backtrack so that the path goes:
						// A -> B -> C -> B -> D
						// While this is technically allowed, it will cause a weird and suboptimal path. So we should try to avoid it.
						RemoveBacktracking(result, ostart, resCount);

						// Remove nodes[end], otherwise we will get a duplicate node when the next raycast happens
						result.RemoveAt(result.Count-1);
						start = mn;
					}
				}
			}
		}

		/// <summary>
		/// Removes backtracking in the path.
		/// This can happen when the path goes A -> B -> C -> B -> D.
		/// This method will replace B -> C -> B with just B, when passed aroundIndex=C.
		/// </summary>
		static void RemoveBacktracking (List<GraphNode> nodes, int listStartIndex, int aroundIndex) {
			while (aroundIndex - 1 > listStartIndex && aroundIndex + 1 < nodes.Count && nodes[aroundIndex-1] == nodes[aroundIndex+1]) {
				nodes.RemoveRange(aroundIndex, 2);
				aroundIndex--;
			}
		}

		public static FunnelPortals ConstructFunnelPortals (List<GraphNode> nodes, PathPart part) {
			if (nodes == null || nodes.Count == 0) {
				return new FunnelPortals { left = ListPool<Vector3>.Claim(0), right = ListPool<Vector3>.Claim(0) };
			}

			if (part.endIndex < part.startIndex || part.startIndex < 0 || part.endIndex > nodes.Count) throw new System.ArgumentOutOfRangeException();

			// Claim temporary lists and try to find lists with a high capacity
			var left = ListPool<Vector3>.Claim(nodes.Count+1);
			var right = ListPool<Vector3>.Claim(nodes.Count+1);

			// Add start point
			left.Add(part.startPoint);
			right.Add(part.startPoint);

			// Loop through all nodes in the path (except the last one)
			for (int i = part.startIndex; i < part.endIndex; i++) {
				// Get the portal between path[i] and path[i+1] and add it to the left and right lists
				if (nodes[i].GetPortal(nodes[i+1], out var lp, out var rp)) {
					left.Add(lp);
					right.Add(rp);
				} else {
					// Fallback, just use the positions of the nodes
					left.Add((Vector3)nodes[i].position);
					right.Add((Vector3)nodes[i].position);

					left.Add((Vector3)nodes[i+1].position);
					right.Add((Vector3)nodes[i+1].position);
				}
			}

			// Add end point
			left.Add(part.endPoint);
			right.Add(part.endPoint);

			return new FunnelPortals { left = left, right = right };
		}

		[BurstCompile]
		public struct FunnelState {
			/// <summary>Left side of the funnel</summary>
			public NativeCircularBuffer<float3> leftFunnel;
			/// <summary>Right side of the funnel</summary>
			public NativeCircularBuffer<float3> rightFunnel;
			/// <summary>
			/// Unwrapped version of the funnel portals in 2D space.
			///
			/// The input is a funnel like in the image below. It may be rotated and twisted.
			/// [Open online documentation to see images]
			/// The output will be a funnel in 2D space like in the image below. All twists and bends will have been straightened out.
			/// [Open online documentation to see images]
			///
			/// This array is used as a cache and the unwrapped portals are calculated on demand. Thus it may not contain all portals.
			/// </summary>
			public NativeCircularBuffer<float4> unwrappedPortals;

			/// <summary>
			/// If set to anything other than (0,0,0), then all portals will be projected on a plane with this normal.
			///
			/// This is used to make the funnel fit a rotated graph better.
			/// It is ideally used for grid graphs, but navmesh/recast graphs are probably better off with it set to zero.
			///
			/// The vector should be normalized (unless zero), in world space, and should never be changed after the first portal has been added (unless the funnel is cleared first).
			/// </summary>
			public float3 projectionAxis;


			public FunnelState(int initialCapacity, Allocator allocator) {
				leftFunnel = new NativeCircularBuffer<float3>(initialCapacity, allocator);
				rightFunnel = new NativeCircularBuffer<float3>(initialCapacity, allocator);
				unwrappedPortals = new NativeCircularBuffer<float4>(initialCapacity, allocator);
				projectionAxis = float3.zero;
			}

			public FunnelState(FunnelPortals portals, Allocator allocator) : this(portals.left.Count, allocator) {
				if (portals.left.Count != portals.right.Count) throw new System.ArgumentException("portals.left.Count != portals.right.Count");
				for (int i = 0; i < portals.left.Count; i++) {
					PushEnd(portals.left[i], portals.right[i]);
				}
			}

			public FunnelState Clone () {
				return new FunnelState {
						   leftFunnel = leftFunnel.Clone(),
						   rightFunnel = rightFunnel.Clone(),
						   unwrappedPortals = unwrappedPortals.Clone(),
						   projectionAxis = projectionAxis,
				};
			}

			public void Clear () {
				leftFunnel.Clear();
				rightFunnel.Clear();
				unwrappedPortals.Clear();
				projectionAxis = float3.zero;
			}

			public void PopStart () {
				leftFunnel.PopStart();
				rightFunnel.PopStart();
				if (unwrappedPortals.Length > 0) unwrappedPortals.PopStart();
			}

			public void PopEnd () {
				leftFunnel.PopEnd();
				rightFunnel.PopEnd();
				unwrappedPortals.TrimTo(leftFunnel.Length);
			}

			public void Pop (bool fromStart) {
				if (fromStart) PopStart();
				else PopEnd();
			}

			public void PushStart (float3 newLeftPortal, float3 newRightPortal) {
				PushStart(ref leftFunnel, ref rightFunnel, ref unwrappedPortals, ref newLeftPortal, ref newRightPortal, ref projectionAxis);
			}

			/// <summary>True if a and b lie on different sides of the infinite line that passes through start and end</summary>
			static bool DifferentSidesOfLine (float3 start, float3 end, float3 a, float3 b) {
				var portal = math.normalizesafe(end - start);
				var d1 = a - start;
				var d2 = b - start;
				d1 -= portal * math.dot(d1, portal);
				d2 -= portal * math.dot(d2, portal);
				return math.dot(d1, d2) < 0;
			}

			/// <summary>
			/// True if it is reasonable that the given start point has passed the first portal in the funnel.
			///
			/// If this is true, it is most likely better to pop the start/end portal of the funnel first.
			///
			/// This can be used as a heuristic to determine if the agent has passed a portal and we should pop it,
			/// in those cases when node information is not available (e.g. because the path has been invalidated).
			/// </summary>
			public bool IsReasonableToPopStart (float3 startPoint, float3 endPoint) {
				if (leftFunnel.Length == 0) return false;

				var reference = 1;
				while (reference < leftFunnel.Length && VectorMath.IsColinear(leftFunnel.First, rightFunnel.First, leftFunnel[reference])) {
					reference++;
				}
				return !DifferentSidesOfLine(leftFunnel.First, rightFunnel.First, startPoint, reference < leftFunnel.Length ? leftFunnel[reference] : endPoint);
			}

			/// <summary>Like <see cref="IsReasonableToPopStart"/> but for the end of the funnel</summary>
			public bool IsReasonableToPopEnd (float3 startPoint, float3 endPoint) {
				if (leftFunnel.Length == 0) return false;

				var reference = leftFunnel.Length - 1;
				while (reference >= 0 && VectorMath.IsColinear(leftFunnel.Last, rightFunnel.Last, leftFunnel[reference])) {
					reference--;
				}
				return !DifferentSidesOfLine(leftFunnel.Last, rightFunnel.Last, endPoint, reference >= 0 ? leftFunnel[reference] : startPoint);
			}

			[BurstCompile]
			static void PushStart (ref NativeCircularBuffer<float3> leftPortals, ref NativeCircularBuffer<float3> rightPortals, ref NativeCircularBuffer<float4> unwrappedPortals, ref float3 newLeftPortal, ref float3 newRightPortal, ref float3 projectionAxis) {
				if (unwrappedPortals.Length == 0) {
					leftPortals.PushStart(newLeftPortal);
					rightPortals.PushStart(newRightPortal);
					return;
				}

				var firstUnwrapped = unwrappedPortals.First;
				var unwrappedRight = Unwrap(leftPortals.First, rightPortals.First, firstUnwrapped.xy, firstUnwrapped.zw, newRightPortal, -1, projectionAxis);
				var unwrappedLeft = Unwrap(leftPortals.First, newRightPortal, firstUnwrapped.xy, unwrappedRight, newLeftPortal, -1, projectionAxis);
				leftPortals.PushStart(newLeftPortal);
				rightPortals.PushStart(newRightPortal);
				unwrappedPortals.PushStart(new float4(unwrappedLeft, unwrappedRight));
			}

			public void Splice (int startIndex, int toRemove, List<float3> newLeftPortal, List<float3> newRightPortal) {
				this.leftFunnel.Splice(startIndex, toRemove, newLeftPortal);
				this.rightFunnel.Splice(startIndex, toRemove, newRightPortal);
				unwrappedPortals.TrimTo(startIndex);
			}

			public void PushEnd (Vector3 newLeftPortal, Vector3 newRightPortal) {
				leftFunnel.PushEnd(newLeftPortal);
				rightFunnel.PushEnd(newRightPortal);
			}

			public void Push (bool toStart, Vector3 newLeftPortal, Vector3 newRightPortal) {
				if (toStart) PushStart(newLeftPortal, newRightPortal);
				else PushEnd(newLeftPortal, newRightPortal);
			}

			public void Dispose () {
				leftFunnel.Dispose();
				rightFunnel.Dispose();
				unwrappedPortals.Dispose();
			}

			/// <summary>
			/// Calculate the shortest path through the funnel.
			///
			/// Returns: The number of corners added to the result array.
			///
			/// See: http://digestingduck.blogspot.se/2010/03/simple-stupid-funnel-algorithm.html
			/// </summary>
			/// <param name="maxCorners">The maximum number of corners to add to the result array. Should be positive.</param>
			/// <param name="result">Output indices. Contains an index as well as possibly the \reflink{RightSideBit} set. Corresponds to an index of the left or right portals, depending on if \reflink{RightSideBit} is set. This must point to an array which is at least maxCorners long.</param>
			/// <param name="startPoint">Start point of the funnel. The agent will move from here to the best point along the first portal.</param>
			/// <param name="endPoint">End point of the funnel.</param>
			/// <param name="lastCorner">True if the final corner of the path was reached. If true, then the return value is guaranteed to be at most maxCorners - 1 (unless maxCorners = 0).</param>
			public int CalculateNextCornerIndices (int maxCorners, NativeArray<int> result, float3 startPoint, float3 endPoint, out bool lastCorner) {
				Assert.AreEqual(leftFunnel.Length, rightFunnel.Length);
				Assert.IsTrue(unwrappedPortals.Length <= leftFunnel.Length);
				if (result.Length < math.min(maxCorners, leftFunnel.Length)) throw new System.ArgumentException("result array may not be large enough to hold all corners");

				unsafe {
					// TODO: Pass this as ref instead?
					var resultsSpan = result.AsUnsafeSpan();
					return Calculate(ref unwrappedPortals, ref leftFunnel, ref rightFunnel, ref startPoint, ref endPoint, ref resultsSpan, maxCorners, ref projectionAxis, out lastCorner);
				}
			}

			public void CalculateNextCorners (int maxCorners, bool splitAtEveryPortal, float3 startPoint, float3 endPoint, NativeList<float3> result) {
				var indices = new NativeArray<int>(math.min(maxCorners, leftFunnel.Length), Allocator.Temp);
				var numCorners = CalculateNextCornerIndices(maxCorners, indices, startPoint, endPoint, out bool lastCorner);
				ConvertCornerIndicesToPath(indices, numCorners, splitAtEveryPortal, startPoint, endPoint, lastCorner, result);
				indices.Dispose();
			}

			public void ConvertCornerIndicesToPath (NativeArray<int> indices, int numCorners, bool splitAtEveryPortal, float3 startPoint, float3 endPoint, bool lastCorner, NativeList<float3> result) {
				if (result.Capacity < numCorners) result.Capacity = numCorners;

				Assert.IsTrue(numCorners == 0 || (indices[numCorners-1] & FunnelPortalIndexMask) < unwrappedPortals.Length);
				result.Add(startPoint);
				if (leftFunnel.Length == 0) {
					if (lastCorner) result.Add(endPoint);
					return;
				}

				if (splitAtEveryPortal) {
					float2 prev2D = Unwrap(leftFunnel[0], rightFunnel[0], unwrappedPortals[0].xy, unwrappedPortals[0].zw, startPoint, -1, projectionAxis);
					var prevIdx = 0;
					for (int i = 0; i < numCorners; i++) {
						var idx = indices[i] & FunnelPortalIndexMask;
						var rightSide = (indices[i] & RightSideBit) != 0;
						// Check intersections with every portal segment
						float2 next2D = rightSide ? unwrappedPortals[idx].zw : unwrappedPortals[idx].xy;
						CalculatePortalIntersections(prevIdx + 1, idx - 1, leftFunnel, rightFunnel, unwrappedPortals, prev2D, next2D, result);
						prevIdx = math.abs(idx);
						prev2D = next2D;

						result.Add(rightSide ? rightFunnel[idx] : leftFunnel[idx]);
					}
					if (lastCorner) {
						var next2D = Unwrap(leftFunnel.Last, rightFunnel.Last, unwrappedPortals.Last.xy, unwrappedPortals.Last.zw, endPoint, 1, projectionAxis);
						CalculatePortalIntersections(prevIdx + 1, unwrappedPortals.Length - 1, leftFunnel, rightFunnel, unwrappedPortals, prev2D, next2D, result);
						result.Add(endPoint);
					}
				} else {
					for (int i = 0; i < numCorners; i++) {
						var idx = indices[i];
						result.Add((idx & RightSideBit) != 0 ? rightFunnel[idx & FunnelPortalIndexMask] : leftFunnel[idx & FunnelPortalIndexMask]);
					}
					if (lastCorner) result.Add(endPoint);
				}
			}

			public void ConvertCornerIndicesToPathProjected (UnsafeSpan<int> indices, bool splitAtEveryPortal, float3 startPoint, float3 endPoint, bool lastCorner, NativeList<float3> result, float3 up) {
				var resultCount = indices.Length + 1 + (lastCorner ? 1 : 0);
				if (result.Capacity < resultCount) result.Capacity = resultCount;
				result.ResizeUninitialized(resultCount);
				var resultSpan = result.AsUnsafeSpan();
				ConvertCornerIndicesToPathProjected(ref this, ref indices, splitAtEveryPortal, in startPoint, in endPoint, lastCorner, in projectionAxis, ref resultSpan, in up);
			}

			public float4x3 UnwrappedPortalsToWorldMatrix (float3 up) {
				int startIndex = 0;
				while (startIndex < unwrappedPortals.Length && math.lengthsq(unwrappedPortals[startIndex].xy - unwrappedPortals[startIndex].zw) <= 0.00001f) startIndex++;
				if (startIndex >= unwrappedPortals.Length) return new float4x3(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1);
				var left2D = unwrappedPortals[startIndex].xy;
				var right2D = unwrappedPortals[startIndex].zw;
				var left3D = leftFunnel[startIndex];
				var right3D = rightFunnel[startIndex];
				var portal2D = right2D - left2D;
				var portal3D = right3D - left3D;
				var portal2DInv = portal2D * math.rcp(math.lengthsq(portal2D));
				// Matrix to rotate unwrapped portals so that portal2D maps to the x-axis (1,0)
				var mr = new float2x2(
					new float2(portal2DInv.x, -portal2DInv.y),
					new float2(portal2DInv.y, portal2DInv.x)
					);

				// Matrix to transform points in unwrapped-portal-space so left2D maps to (0,0) and right2D maps to (1,0)
				var offset = math.mul(mr, -left2D);
				var m1 = new float4x3(
					new float4(mr.c0.x, 0, mr.c0.y, 0),
					new float4(mr.c1.x, 0, mr.c1.y, 0),
					new float4(offset.x, 0, offset.y, 1)
					);

				// Matrix that maps (0,0,0) to left3D and (1,0,0) to right3D, as well as (0,1,0) to up.
				var m2 = new float4x4(
					new float4(portal3D, 0),
					new float4(up, 0),
					new float4(math.cross(portal3D, up), 0),
					new float4(left3D, 1)
					);

				// Matrix to transform points in unwrapped-portal-space to 3D space. Such that left2D maps to left3D.
				return math.mul(m2, m1);
			}

			[BurstCompile]
			public static void ConvertCornerIndicesToPathProjected (ref FunnelState funnelState, ref UnsafeSpan<int> indices, bool splitAtEveryPortal, in float3 startPoint, in float3 endPoint, bool lastCorner, in float3 projectionAxis, ref UnsafeSpan<float3> result, in float3 up) {
				Assert.IsTrue(indices.Length == 0 || (indices[indices.Length-1] & FunnelPortalIndexMask) < funnelState.unwrappedPortals.Length);
				int resultIndex = 0;
				result[resultIndex++] = startPoint;
				if (funnelState.leftFunnel.Length == 0) {
					if (lastCorner) result[resultIndex++] = endPoint;
					Assert.AreEqual(resultIndex, result.Length);
					return;
				}

				var unwrappedToWorld = funnelState.UnwrappedPortalsToWorldMatrix(up);

				if (splitAtEveryPortal) {
					throw new System.NotImplementedException();
				} else {
					for (int i = 0; i < indices.Length; i++) {
						var idx = indices[i];
						var corner = (idx & RightSideBit) != 0 ? funnelState.unwrappedPortals[idx & FunnelPortalIndexMask].zw : funnelState.unwrappedPortals[idx & FunnelPortalIndexMask].xy;
						result[resultIndex++] = math.mul(unwrappedToWorld, new float3(corner, 1)).xyz;
					}
					if (lastCorner) {
						float2 endPoint2D = Unwrap(funnelState.leftFunnel.Last, funnelState.rightFunnel.Last, funnelState.unwrappedPortals.Last.xy, funnelState.unwrappedPortals.Last.zw, endPoint, 1, projectionAxis);
						result[resultIndex++] = math.mul(unwrappedToWorld, new float3(endPoint2D, 1)).xyz;
					}
				}
				Assert.AreEqual(resultIndex, result.Length);
			}

			static void CalculatePortalIntersections (int startIndex, int endIndex, NativeCircularBuffer<float3> leftPortals, NativeCircularBuffer<float3> rightPortals, NativeCircularBuffer<float4> unwrappedPortals, float2 from, float2 to, NativeList<float3> result) {
				for (int j = startIndex; j < endIndex; j++) {
					var portal = unwrappedPortals[j];
					var left = portal.xy;
					var right = portal.zw;
					if (!VectorMath.LineLineIntersectionFactor(left, right - left, from, to - from, out float factor)) {
						// This really shouldn't happen
						factor = 0.5f;
					}
					result.Add(math.lerp(leftPortals[j], rightPortals[j], factor));
				}
			}
		}

		private static float2 Unwrap (float3 leftPortal, float3 rightPortal, float2 leftUnwrappedPortal, float2 rightUnwrappedPortal, float3 point, float sideMultiplier, float3 projectionAxis) {
			// TODO: On grid graphs this is kind of a weird way to do it.
			// We project all points onto a plane and then unwrap them.
			// It would be faster (and possibly more numerically accurate) to transform the points to graph space and then just use the xz coordinates.
			// This branch is extremely well predicted, since it will always be true for grid graphs, and always false for other graphs.
			if (!math.all(projectionAxis == 0)) {
				leftPortal -= projectionAxis * math.dot(leftPortal, projectionAxis);
				rightPortal -= projectionAxis * math.dot(rightPortal, projectionAxis);
				point -= projectionAxis * math.dot(point, projectionAxis);
			}

			var portal = rightPortal - leftPortal;
			var portalLengthInvSq = 1.0f / math.lengthsq(portal);
			if (float.IsPositiveInfinity(portalLengthInvSq)) {
				return leftUnwrappedPortal + new float2(-math.length(point - leftPortal), 0);
			}
			var distance = math.length(math.cross(point - leftPortal, portal)) * portalLengthInvSq;
			var projection = math.dot(point - leftPortal, portal) * portalLengthInvSq;

			// Weld corner vertices if they are close enough.
			// This is important for grid graphs, as if the unwrapped portals are not quite identical in the corners,
			// the grid simplification may fail to remove inner corners. This is because it will detect 2+ almost identical corners in each turn, instead of 1.
			// TODO: Unwrap grid portals in a different way. It really can be done much faster and more numerically robustly.
			// We should not use graph space directly, though, as grid graphs can move around (ProceduralGraphMover).
			if (distance < 0.002f) {
				if (math.abs(projection) < 0.002f) {
					return leftUnwrappedPortal;
				} else if (math.abs(projection - 1) < 0.002f) {
					return rightUnwrappedPortal;
				}
			}

			var unwrappedPortal = rightUnwrappedPortal - leftUnwrappedPortal;
			var unwrappedNormal = new float2(-unwrappedPortal.y, unwrappedPortal.x);
			return leftUnwrappedPortal + math.mad(unwrappedPortal, projection, unwrappedNormal * (distance * sideMultiplier));
		}

		/// <summary>True if b is to the right of or on the line from (0,0) to a</summary>
		private static bool RightOrColinear (Vector2 a, Vector2 b) {
			return (a.x*b.y - b.x*a.y) <= 0;
		}

		/// <summary>True if b is to the left of or on the line from (0,0) to a</summary>
		private static bool LeftOrColinear (Vector2 a, Vector2 b) {
			return (a.x*b.y - b.x*a.y) >= 0;
		}

		/// <summary>
		/// Calculate the shortest path through the funnel.
		///
		/// The path will be unwrapped into 2D space before the funnel algorithm runs.
		/// This makes it possible to support the funnel algorithm in XY space as well as in more complicated cases, such as on curved worlds.
		/// [Open online documentation to see images]
		///
		/// [Open online documentation to see images]
		///
		/// See: Unwrap
		/// </summary>
		/// <param name="funnel">The portals of the funnel. The first and last vertices portals must be single points (so for example left[0] == right[0]).</param>
		/// <param name="splitAtEveryPortal">If true, then a vertex will be inserted every time the path crosses a portal
		///  instead of only at the corners of the path. The result will have exactly one vertex per portal if this is enabled.
		///  This may introduce vertices with the same position in the output (esp. in corners where many portals meet).</param>
		public static List<Vector3> Calculate (FunnelPortals funnel, bool splitAtEveryPortal) {
			var state = new FunnelState(funnel, Allocator.Temp);
			var startPoint = state.leftFunnel.First;
			var endPoint = state.leftFunnel.Last;
			state.PopStart();
			state.PopEnd();
			var nativeResult = new NativeList<float3>(Allocator.Temp);
			state.CalculateNextCorners(int.MaxValue, splitAtEveryPortal, startPoint, endPoint, nativeResult);
			state.Dispose();
			var result = ListPool<Vector3>.Claim(nativeResult.Length);
			for (int i = 0; i < nativeResult.Length; i++) result.Add((Vector3)nativeResult[i]);
			nativeResult.Dispose();
			return result;
		}

		public const int RightSideBit = 1 << 30;
		public const int FunnelPortalIndexMask = RightSideBit - 1;

		/// <summary>
		/// Calculate the shortest path through the funnel.
		///
		/// Returns: The number of corners added to the funnelPath array.
		///
		/// See: http://digestingduck.blogspot.se/2010/03/simple-stupid-funnel-algorithm.html
		/// </summary>
		/// <param name="leftPortals">Left side of the funnel. Should not contain the start point.</param>
		/// <param name="rightPortals">Right side of the funnel. Should not contain the end point.</param>
		/// <param name="unwrappedPortals">Cache of unwrapped portal segments. This may be empty, but it will be filled with unwrapped portals and next time you run the algorithm it will be faster.</param>
		/// <param name="startPoint">Start point of the funnel. The agent will move from here to the best point between leftPortals[0] and rightPortals[0].</param>
		/// <param name="endPoint">End point of the funnel.</param>
		/// <param name="funnelPath">Output indices. Contains an index as well as possibly the \reflink{RightSideBit} set. Corresponds to an index into leftPortals or rightPortals depending on if \reflink{RightSideBit} is set. This must point to an array which is at least maxCorners long.</param>
		/// <param name="lastCorner">True if the final corner of the path was reached. If true, then the return value is guaranteed to be at most maxCorners - 1 (unless maxCorners = 0).</param>
		/// <param name="maxCorners">The first N corners of the optimized path will be calculated. Calculating fewer corners is faster. Pass int.MaxValue if you want to calculate all corners.</param>
		/// <param name="projectionAxis">If set to anything other than (0,0,0), then all portals will be projected on a plane with this normal.</param>
		[BurstCompile]
		static unsafe int Calculate (ref NativeCircularBuffer<float4> unwrappedPortals, ref NativeCircularBuffer<float3> leftPortals, ref NativeCircularBuffer<float3> rightPortals, ref float3 startPoint, ref float3 endPoint, ref UnsafeSpan<int> funnelPath, int maxCorners, ref float3 projectionAxis, out bool lastCorner) {
			lastCorner = false;
			if (leftPortals.Length <= 0) {
				lastCorner = true;
				return 0;
			}
			if (maxCorners <= 0) return 0;

			int apexIndex = 0;
			int rightIndex = 0;
			int leftIndex = 0;

			int outputCount = 0;

			if (unwrappedPortals.Length == 0) {
				unwrappedPortals.PushEnd(new float4(new float2(0, 0), new float2(math.length(rightPortals[0] - leftPortals[0]))));
			}

			float2 portalApex = Unwrap(leftPortals[0], rightPortals[0], unwrappedPortals[0].xy, unwrappedPortals[0].zw, startPoint, -1, projectionAxis);
			float2 portalLeft = float2.zero;
			float2 portalRight = float2.zero;

			for (int i = 0; i <= leftPortals.Length; i++) {
				// Unwrap the funnel on the fly as needed
				float2 rLeft, rRight;
				if (i == unwrappedPortals.Length) {
					if (i == leftPortals.Length) {
						// The end point of the path
						rLeft = rRight = Unwrap(leftPortals[i-1], rightPortals[i-1], unwrappedPortals[i-1].xy, unwrappedPortals[i-1].zw, endPoint, 1, projectionAxis) - portalApex;
					} else {
						// The funnel needs unwrapping
						var unwrappedLeft = Unwrap(leftPortals[i-1], rightPortals[i-1], unwrappedPortals[i-1].xy, unwrappedPortals[i-1].zw, leftPortals[i], 1, projectionAxis);
						var unwrappedRight = Unwrap(leftPortals[i], rightPortals[i-1], unwrappedLeft, unwrappedPortals[i-1].zw, rightPortals[i], 1, projectionAxis);
						unwrappedPortals.PushEnd(new float4(unwrappedLeft, unwrappedRight));
						rLeft = unwrappedLeft - portalApex;
						rRight = unwrappedRight - portalApex;
					}
				} else {
					// Common case
					rLeft = unwrappedPortals[i].xy - portalApex;
					rRight = unwrappedPortals[i].zw - portalApex;
				}

				if (LeftOrColinear(portalRight, rRight)) {
					if (RightOrColinear(portalLeft, rRight)) {
						portalRight = rRight;
						rightIndex = i;
					} else {
						portalRight = portalLeft = float2.zero;
						i = apexIndex = rightIndex = leftIndex;
						portalApex = unwrappedPortals[i].xy;

						funnelPath[outputCount++] = apexIndex;
						if (outputCount >= maxCorners) return outputCount;
						continue;
					}
				}

				if (RightOrColinear(portalLeft, rLeft)) {
					if (LeftOrColinear(portalRight, rLeft)) {
						portalLeft = rLeft;
						leftIndex = i;
					} else {
						portalRight = portalLeft = float2.zero;
						i = apexIndex = leftIndex = rightIndex;
						portalApex = unwrappedPortals[i].zw;

						funnelPath[outputCount++] = apexIndex | RightSideBit;
						if (outputCount >= maxCorners) return outputCount;
						continue;
					}
				}
			}

			lastCorner = true;
			return outputCount;
		}
	}
}
