using System.Collections.Generic;
using Pathfinding.Pooling;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace Pathfinding {
	/// <summary>
	/// Simplifies a path on a grid graph using a string pulling algorithm.
	/// This is based on a paper called "Toward a String-Pulling Approach to Path Smoothing on Grid Graphs",
	/// with some optimizations as well as fixes for some edge cases that the paper didn't handle.
	///
	/// The result is conceptually similar to the well known funnel string pulling algorithm for navmesh graphs
	/// but it uses a different algorithm.
	///
	/// This class is used by the <see cref="FunnelModifier"/> on grid graphs.
	///
	/// See: <see cref="Funnel"/>
	/// See: <see cref="FunnelModifier"/>
	/// See: article: https://ojs.aaai.org/index.php/SOCS/article/view/18541
	/// </summary>
	public static class GridStringPulling {
		/// <summary>
		///         Z
		///         |
		///         |
		///
		///      3     2
		///       \ | /
		/// --    - X -    ----- X
		///       / | \
		///      0     1
		///
		///         |
		///         |
		/// </summary>
		static int2[] directionToCorners = new int2[] {
			new int2(0, 0),
			new int2(FixedPrecisionScale, 0),
			new int2(FixedPrecisionScale, FixedPrecisionScale),
			new int2(0, FixedPrecisionScale),
		};

		static long Cross (int2 lhs, int2 rhs) {
			return (long)lhs.x*(long)rhs.y - (long)lhs.y*(long)rhs.x;
		}

		static long Dot (int2 a, int2 b) {
			return (long)a.x*(long)b.x + (long)a.y*(long)b.y;
		}

		static bool RightOrColinear (int2 a, int2 b, int2 p) {
			return (long)(b.x - a.x) * (long)(p.y - a.y) - (long)(p.x - a.x) * (long)(b.y - a.y) <= 0;
		}

		static int2 Perpendicular (int2 v) {
			return new int2(-v.y, v.x);
		}

		struct TriangleBounds {
			int2 d1, d2, d3;
			long t1, t2, t3;

			public TriangleBounds(int2 p1, int2 p2, int2 p3) {
				if (RightOrColinear(p1, p2, p3)) {
					var tmp = p3;
					p3 = p1;
					p1 = tmp;
				}
				d1 = Perpendicular(p2 - p1);
				d2 = Perpendicular(p3 - p2);
				d3 = Perpendicular(p1 - p3);
				t1 = Dot(d1, p1);
				t2 = Dot(d2, p2);
				t3 = Dot(d3, p3);
			}

			public bool Contains (int2 p) {
				return Dot(d1, p) >= t1 && Dot(d2, p) >= t2 && Dot(d3, p) >= t3;
			}
		}

		const int FixedPrecisionScale = 1024;

		static int2 ToFixedPrecision (Vector2 p) {
			return new int2(math.round(new float2(p)*FixedPrecisionScale));
		}

		static Vector2 FromFixedPrecision (int2 p) {
			return (Vector2)(((float2)p) * (1.0f/FixedPrecisionScale));
		}

		/// <summary>Returns which side of the line a - b that p lies on</summary>
		static Side Side2D (int2 a, int2 b, int2 p) {
			var s = Cross(b-a, p-a);

			return s > 0 ? Side.Left : (s < 0 ? Side.Right : Side.Colinear);
		}

		static Unity.Profiling.ProfilerMarker marker1 = new Unity.Profiling.ProfilerMarker("Linecast hit");
		static Unity.Profiling.ProfilerMarker marker2 = new Unity.Profiling.ProfilerMarker("Linecast success");
		static Unity.Profiling.ProfilerMarker marker3 = new Unity.Profiling.ProfilerMarker("Trace");
		static Unity.Profiling.ProfilerMarker marker4 = new Unity.Profiling.ProfilerMarker("Neighbours");
		static Unity.Profiling.ProfilerMarker marker5 = new Unity.Profiling.ProfilerMarker("Re-evaluate linecast");
		static Unity.Profiling.ProfilerMarker marker6 = new Unity.Profiling.ProfilerMarker("Init");
		static Unity.Profiling.ProfilerMarker marker7 = new Unity.Profiling.ProfilerMarker("Initloop");

		/// <summary>
		/// Intersection length of the given segment with a square of size Int3.Precision centered at nodeCenter.
		/// The return value is between 0 and sqrt(2).
		/// </summary>
		public static float IntersectionLength (int2 nodeCenter, int2 segmentStart, int2 segmentEnd) {
			// TODO: Calculations can be hoisted
			var invNormal = math.rcp((float2)(segmentEnd - segmentStart));
			var normalMagn = math.length((float2)(segmentEnd - segmentStart));

			float tmin = float.NegativeInfinity, tmax = float.PositiveInfinity;

			var normal = segmentEnd - segmentStart;
			var bmin = nodeCenter; // - new int2(Int3.Precision/2, Int3.Precision/2);
			var bmax = nodeCenter + new int2(FixedPrecisionScale, FixedPrecisionScale);

			if (normal.x != 0.0) {
				float tx1 = (bmin.x - segmentStart.x)*invNormal.x;
				float tx2 = (bmax.x - segmentStart.x)*invNormal.x;

				tmin = math.max(tmin, math.min(tx1, tx2));
				tmax = math.min(tmax, math.max(tx1, tx2));
			} else if (segmentStart.x < bmin.x || segmentStart.x > bmax.x) {
				return 0.0f;
			}

			if (normal.y != 0.0) {
				float ty1 = (bmin.y - segmentStart.y)*invNormal.y;
				float ty2 = (bmax.y - segmentStart.y)*invNormal.y;

				tmin = math.max(tmin, math.min(ty1, ty2));
				tmax = math.min(tmax, math.max(ty1, ty2));
			} else if (segmentStart.y < bmin.y || segmentStart.y > bmax.y) {
				return 0.0f;
			}

			tmin = math.max(0, tmin);
			tmax = math.min(1, tmax);
			return math.max(tmax - tmin, 0)*normalMagn*(1.0f/FixedPrecisionScale);
		}

		internal static void TestIntersectionLength () {
			var s = FixedPrecisionScale;

			UnityEngine.Assertions.Assert.AreApproximatelyEqual(math.sqrt(2), IntersectionLength(new int2(1*s, 1*s), new int2(0, 0), new int2(2*s, 2*s)));
			UnityEngine.Assertions.Assert.AreApproximatelyEqual(0.0f, IntersectionLength(new int2(1*s, 1*s), new int2(0, 0), new int2(0, 0)));
			UnityEngine.Assertions.Assert.AreApproximatelyEqual(1.0f, IntersectionLength(new int2(1*s, 1*s), new int2(-1*s, s+1), new int2(2*s, s+1)));
			UnityEngine.Assertions.Assert.AreApproximatelyEqual(1.0f, IntersectionLength(new int2(1*s, 1*s), new int2(2*s, s), new int2(-1*s, s)));

			// All sides of the square should be included
			UnityEngine.Assertions.Assert.AreApproximatelyEqual(1.0f, IntersectionLength(new int2(1*s, 1*s), new int2(s, s), new int2(s+s, s)));
			UnityEngine.Assertions.Assert.AreApproximatelyEqual(1.0f, IntersectionLength(new int2(1*s, 1*s), new int2(s+s, s), new int2(s+s, s+s)));
			UnityEngine.Assertions.Assert.AreApproximatelyEqual(1.0f, IntersectionLength(new int2(1*s, 1*s), new int2(s+s, s+s), new int2(s, s+s)));
			UnityEngine.Assertions.Assert.AreApproximatelyEqual(1.0f, IntersectionLength(new int2(1*s, 1*s), new int2(s, s+s), new int2(s, s)));
		}

		/// <summary>
		/// Cost of moving across all the nodes in the list, along the given segment.
		/// It is assumed that the segment intersects the nodes. Any potentially intersecting nodes that are not part of the list will be ignored.
		/// </summary>
		static uint LinecastCost (List<GraphNode> trace, int2 segmentStart, int2 segmentEnd, GridGraph gg, System.Func<GraphNode, uint> traversalCost) {
			// Check the cost of the segment compared to not taking this "shortcut"
			uint cost = 0;

			for (int k = 0; k < trace.Count; k++) {
				var node = trace[k] as GridNodeBase;
				// Note: this assumes the default grid connection costs are used. Which is relatively reasonable
				// since they require changing the code to modify.
				cost += (uint)(((float)traversalCost(node) + gg.nodeSize*Int3.Precision) * IntersectionLength(new int2(node.XCoordinateInGrid, node.ZCoordinateInGrid)*FixedPrecisionScale, segmentStart, segmentEnd));
			}
			return cost;
		}

		enum PredicateFailMode {
			Undefined,
			Turn,
			LinecastObstacle,
			LinecastCost,
			ReachedEnd,
		}

		/// <summary>
		/// Simplifies a path on a grid graph using a string pulling algorithm.
		/// See the class documentation for more details.
		/// </summary>
		/// <param name="pathNodes">A list of input nodes. Only the slice of nodes from nodeStartIndex to nodeEndIndex (inclusive) will be used. These must all be of type GridNodeBase and must form a path (i.e. each node must be a neighbor to the next one in the list).</param>
		/// <param name="nodeStartIndex">The index in pathNodes to start from.</param>
		/// <param name="nodeEndIndex">The last index in pathNodes that is used.</param>
		/// <param name="startPoint">A more exact start point for the path. This should be a point inside the first node (if not, it will be clamped to the node's surface).</param>
		/// <param name="endPoint">A more exact end point for the path. This should be a point inside the first node (if not, it will be clamped to the node's surface).</param>
		/// <param name="traversalCost">Can be used to specify how much it costs to traverse each node. If this is null, node penalties and tag penalties will be completely ignored.</param>
		/// <param name="filter">Can be used to filter out additional nodes that should be treated as unwalkable. It is assumed that all nodes in pathNodes pass this filter.</param>
		/// <param name="maxCorners">If you only need the first N points of the result, you can specify that here, to avoid unnecessary work.</param>
		public static List<Vector3> Calculate (List<GraphNode> pathNodes, int nodeStartIndex, int nodeEndIndex, Vector3 startPoint, Vector3 endPoint, System.Func<GraphNode, uint> traversalCost = null, System.Func<GraphNode, bool> filter = null, int maxCorners = int.MaxValue) {
			Profiler.BeginSample("Funnel");
			marker6.Begin();
			// A list of indices into the arrays defined below.
			// Each index represents a point. But it's more convenient to use indices here and keep all the data separately (also probably faster).
			var outputPath = ListPool<int>.Claim();
			outputPath.Add(0);

			var numInputNodes = nodeEndIndex - nodeStartIndex + 1;
			var gg = pathNodes[nodeStartIndex].Graph as GridGraph;
			var trace = ListPool<GraphNode>.Claim();
			var turn = Side.Colinear;
			int counter = 0;

			// We will add two points, see comments inside the loop.
			// We may end up adding even more points later, therefore we get arrays that are a bit larger than we need for the initial path.
			numInputNodes += 2;
			int numPoints = numInputNodes;
			var nodes = ArrayPool<GridNodeBase>.Claim(numPoints*2);
			var points = ArrayPool<int2>.Claim(numPoints*2);
			var normalizedPoints = ArrayPool<int2>.Claim(numPoints*2);
			var costs = ArrayPool<uint>.Claim(numPoints*2);

			marker7.Begin();
			uint costSoFar = 0;
			// Go through all points and store all relevant data we need about them
			for (int j = 0; j < numInputNodes; j++) {
				// After the start-end modifier has adjusted the endpoints of the path, the line from the start point to the center of the second node in the path
				// might not actually have line of sight.
				// Assume the path starts at N1 with a diagonal move to node N2.
				// The start-end modifier adjusts the start point of the path to point S.
				// This causes the path to cut the corner to the unwalkable node in the bottom right.
				// ┌─────────┬────────┐
				// │         │        │
				// │   N2    │        │
				// │     \   │        │
				// │      \  │        │
				// ├───────\─┼────────┤
				// │########\│        │
				// │#########│S  N1   │
				// │#########│        │
				// │#########│        │
				// └─────────┴────────┘
				// We can solve this case by making the path go from S to N1 and then to N2 instead of directly from S to N2.
				// We also do the same thing for the end of the path.
				// The clamping and indexing here is essentially equivalent to one insert at the beginning of the arrays and one at the end.
				var node = nodes[j] = pathNodes[math.clamp(nodeStartIndex + j-1, nodeStartIndex, nodeEndIndex)] as GridNodeBase;
				var gridCoordinates = new int2(node.XCoordinateInGrid, node.ZCoordinateInGrid);
				var point = gridCoordinates * FixedPrecisionScale;
				int2 normalized;
				if (j == 0) {
					normalized = ToFixedPrecision(node.NormalizePoint(startPoint));
					normalized = math.clamp(normalized, int2.zero, new int2(FixedPrecisionScale, FixedPrecisionScale));
				} else if (j == numInputNodes - 1) {
					normalized = ToFixedPrecision(node.NormalizePoint(endPoint));
					normalized = math.clamp(normalized, int2.zero, new int2(FixedPrecisionScale, FixedPrecisionScale));
				} else {
					normalized = new int2(FixedPrecisionScale/2, FixedPrecisionScale/2);
				}
				points[j] = point + normalized;
				normalizedPoints[j] = normalized;
				if (j > 0 && traversalCost != null) {
					// Calculate the cost of moving along the original path
					costSoFar += (uint)(((float)traversalCost(nodes[j-1]) + gg.nodeSize*Int3.Precision) * IntersectionLength(new int2(nodes[j-1].XCoordinateInGrid, nodes[j-1].ZCoordinateInGrid)*FixedPrecisionScale, points[j-1], points[j]));
					costSoFar += (uint)(((float)traversalCost(nodes[j]) + gg.nodeSize*Int3.Precision) * IntersectionLength(gridCoordinates*FixedPrecisionScale, points[j-1], points[j]));
				}
				costs[j] = costSoFar;
			}
			marker7.End();

			// We know that there is line of sight from the first point to the second point in the path.
			var lastSuccessfulStart = 0;
			var lastSuccessfulEnd = 1;
			marker6.End();

			int i = 1;
			while (true) {
				if (i >= numInputNodes) {
					// We are done, add the last point
					outputPath.Add(numInputNodes-1);
					break;
				}
				if (outputPath.Count >= maxCorners) {
					// We are done with the partial result
					break;
				}

				counter++;
				if (counter > 10000) {
					Debug.LogError("Inf loop");
					break;
				}

				// In the paper, they just use a straight forward loop over the input path.
				// However, it is better for performance to use a binary search to figure out the next time we need to do something.
				// We only need an 'i' which succeeds and 'i+1' which fails.
				// The success in this case is defined by the predicate below. We only need to do stuff if that returns true.
				var last = outputPath[outputPath.Count-1];
				var normalizedLast = normalizedPoints[last];
				var prev = outputPath.Count > 1 ? outputPath[outputPath.Count-2] : -1;
				var nodeLast = nodes[last];
				var upperBound = numInputNodes - i - 1;

				// Lower and upper bounds for the binary search
				int mn = 0;
				// It is reasonable that most paths can be simplified at least a bit. Assume that seeing 4 or more nodes ahead is common.
				int mx = math.min(4, upperBound);
				var mxFailMode = PredicateFailMode.Undefined;
				uint mxLinecastCost = 0;

				// The calculations are not perfectly accurate. Allow the shortcut's cost to be a tiny bit higher.
				const uint COST_FUDGE = 5;

				GridHitInfo hit;
				// First fire off linecasts to nodes exponentially further away until the predicate returns true.
				while (true) {
					var idx = i + mx;

					var turnPredicate = outputPath.Count > 1 && Side2D(points[prev], points[last], points[idx]) != turn;
					if (turnPredicate) {
						mxFailMode = PredicateFailMode.Turn;
						break;
					} else {
						trace.Clear();
						if (gg.Linecast(nodeLast, normalizedLast, nodes[idx], normalizedPoints[idx], out hit, trace, filter)) {
							mxFailMode = PredicateFailMode.LinecastObstacle;
							break;
						} else if (traversalCost != null) {
							var cost = LinecastCost(trace, points[last], points[idx], gg, traversalCost);
							if (cost > costs[idx] - costs[last] + COST_FUDGE) {
								// The "shortcut" had such a high penalty that it's not worth taking it
								mxFailMode = PredicateFailMode.LinecastCost;
								mxLinecastCost = cost;
								break;
							}
						}
					}

					if (mx < upperBound) {
						mn = mx;
						mx = math.min(mx*2, upperBound);
					} else {
						mxFailMode = PredicateFailMode.ReachedEnd;
						break;
					}
				}

				if (mxFailMode == PredicateFailMode.ReachedEnd) {
					// Reached final node without any hits, we can stop here
					outputPath.Add(numInputNodes-1);
					break;
				}

				// Run a standard binary search
				while (mx > mn + 1) {
					int mid = (mn+mx)/2;
					int idx = i + mid;

					var turnPredicate = outputPath.Count > 1 && Side2D(points[prev], points[last], points[idx]) != turn;
					bool pred = turnPredicate;
					if (turnPredicate) {
						mxFailMode = PredicateFailMode.Turn;
					} else {
						trace.Clear();
						if (gg.Linecast(nodeLast, normalizedLast, nodes[idx], normalizedPoints[idx], out hit, trace, filter)) {
							mxFailMode = PredicateFailMode.LinecastObstacle;
							pred = true;
						} else if (traversalCost != null) {
							var cost = LinecastCost(trace, points[last], points[idx], gg, traversalCost);
							if (cost > costs[idx] - costs[last] + COST_FUDGE) {
								// The "shortcut" had such a high penalty that it's not worth taking it
								mxFailMode = PredicateFailMode.LinecastCost;
								mxLinecastCost = cost;
								pred = true;
							}
						}
					}

					if (pred) {
						mx = mid;
					} else {
						mn = mid;
					}
				}

				// i+mn is now a succeeding index, and i+mn+1 (or i+mx) is a failing index
				if (mn > 0) {
					lastSuccessfulStart = last;
					lastSuccessfulEnd = i+mn;
				} else {
					// We are not actually completely sure that i+mn is a succeeding index if mn=0
					// So double check it.
					// TODO: This is a lot of code duplication. Tidy this up.
					var turnPredicate = outputPath.Count > 1 && Side2D(points[prev], points[last], points[i+mn]) != turn;
					bool pred = turnPredicate;
					if (turnPredicate) {
					} else {
						trace.Clear();
						if (gg.Linecast(nodeLast, normalizedLast, nodes[i+mn], normalizedPoints[i+mn], out hit, trace, filter)) {
							pred = true;
						} else if (traversalCost != null) {
							var cost = LinecastCost(trace, points[last], points[i+mn], gg, traversalCost);
							if (cost > costs[i+mn] - costs[last] + COST_FUDGE) {
								// The "shortcut" had such a high penalty that it's not worth taking it
								mxLinecastCost = cost;
								pred = true;
							}
						}
					}

					if (!pred) {
						// Success!
						lastSuccessfulStart = last;
						lastSuccessfulEnd = i+mn;
					}
				}

				// Move to the failing index
				i += mx;
				UnityEngine.Assertions.Assert.AreNotEqual(mxFailMode, PredicateFailMode.Undefined);

				marker5.Begin();
				trace.Clear();
				trace.Clear();
				if (mxFailMode == PredicateFailMode.LinecastCost) {
					outputPath.Add(lastSuccessfulEnd);
					turn = Side2D(points[last], points[lastSuccessfulEnd], points[i]);
					// It is guaranteed that there is line of sight from lastSuccessfulStart to lastSuccessfulEnd
					lastSuccessfulStart = lastSuccessfulEnd;
					i--;
					marker5.End();
					continue;
				} else if (mxFailMode == PredicateFailMode.LinecastObstacle) {
					marker5.End();
					// Draw.Line(nodes[last].UnNormalizePoint(FromFixedPrecision(normalizedPoints[last])), toNode.UnNormalizePoint(FromFixedPrecision(normalizedTo)), Color.red);
					marker1.Begin();
					marker3.Begin();
					// Re-run a previously successfully linecast to get all nodes it traversed.
					trace.Clear();
					int chosenCorner;
					if (gg.Linecast(nodes[lastSuccessfulStart], normalizedPoints[lastSuccessfulStart], nodes[lastSuccessfulEnd], normalizedPoints[lastSuccessfulEnd], out hit, trace, filter)) {
						// Weird! This linecast should have succeeded.
						// Maybe the path crosses some unwalkable nodes it shouldn't cross (the graph could have changed).
						// Or possibly the linecast implementation doesn't handle some edge case (there are so many!)
						// In any case, we fall back to just assuming there is a valid line of sight.
						chosenCorner = lastSuccessfulEnd;
						Debug.LogError("Inconsistent linecasts");
					} else {
						trace.Add(nodes[i]);
						marker3.End();
						marker4.Begin();

						GridNodeBase candidateNode = null;
						var candidateNormalizedPoint = new int2();
						uint candidateCost = 0;
						var dirToCandidateCorner = new int2();
						var lastSuccessfulStartPoint = points[lastSuccessfulStart];
						var lastSuccessfulEndPoint = points[lastSuccessfulEnd];
						var dir = lastSuccessfulEndPoint - lastSuccessfulStartPoint;
						var bounds = new TriangleBounds(
							lastSuccessfulStartPoint,
							lastSuccessfulEndPoint,
							points[i]
							);

						var desiredSide = System.Math.Sign(Cross(dir, points[i] - lastSuccessfulStartPoint));
						var candidateCostSoFar = costs[lastSuccessfulStart];
						for (int j = 0; j < trace.Count; j++) {
							var node = trace[j] as GridNodeBase;
							var nodeGridPos = new int2(node.XCoordinateInGrid, node.ZCoordinateInGrid);
							var nodeCenter = nodeGridPos * FixedPrecisionScale;
							if (traversalCost != null) {
								// Not perfectly accurate as it doesn't measure the cost to the exact corner
								candidateCostSoFar += (uint)(((float)traversalCost(node) + gg.nodeSize*Int3.Precision) * IntersectionLength(nodeCenter, lastSuccessfulStartPoint, lastSuccessfulEndPoint));
							}
							for (int d = 0; d < 4; d++) {
								if (!node.HasConnectionInDirection(d) || (filter != null && !filter(node.GetNeighbourAlongDirection(d)))) {
									for (int q = 0; q < 2; q++) {
										var ncorner = directionToCorners[(d+q)&0x3];
										var corner = nodeCenter + ncorner;

										if (!bounds.Contains(corner)) {
											continue;
										}

										var dirToCorner = corner - lastSuccessfulStartPoint;
										// We shouldn't pick corners at our current position
										if (math.all(dirToCorner == 0)) continue;
										if (math.all(corner == lastSuccessfulEndPoint)) continue;

										var side = Cross(dirToCorner, dirToCandidateCorner);
										if (candidateNode == null || System.Math.Sign(side) == desiredSide || (side == 0 && math.lengthsq(dirToCorner) > math.lengthsq(dirToCandidateCorner))) {
											dirToCandidateCorner = dirToCorner;
											candidateNode = node;
											candidateNormalizedPoint = ncorner;
											candidateCost = candidateCostSoFar;
										}
									}
								}
							}
						}
						marker4.End();

						if (candidateNode == null) {
							// Fall back to adding the lastSuccessfulEnd node. We know there's line of sight to that one.
							chosenCorner = lastSuccessfulEnd;
						} else {
							chosenCorner = numPoints;
							// TODO: Reallocate
							nodes[numPoints] = candidateNode;
							normalizedPoints[numPoints] = candidateNormalizedPoint;
							var gridCoordinates = new int2(candidateNode.XCoordinateInGrid, candidateNode.ZCoordinateInGrid);
							points[numPoints] = gridCoordinates * FixedPrecisionScale + candidateNormalizedPoint;
							costs[numPoints] = candidateCost;
							numPoints++;
						}
					}

					outputPath.Add(chosenCorner);
					turn = Side2D(points[last], points[chosenCorner], points[i]);
					// It is guaranteed that there is line of sight from lastSuccessfulStart to chosenCorner because of how we choose the corner.
					lastSuccessfulStart = chosenCorner;
					i--;
					marker1.End();
					continue;
				} else {
					marker5.End();
					marker2.Begin();
					lastSuccessfulStart = last;
					lastSuccessfulEnd = i;
					// Draw.Line(nodes[last].UnNormalizePoint(FromFixedPrecision(normalizedPoints[last])), toNode.UnNormalizePoint(FromFixedPrecision(normalizedTo)), Color.green);
					if (outputPath.Count > 1) {
						var spPrev = outputPath[outputPath.Count-2];
						var nextTurn = Side2D(points[spPrev], points[last], points[i]);
						// Check if the string is no longer taut. If it is not we can remove a previous point.
						if (turn != nextTurn) {
							// Draw.SphereOutline(nodes[pts[pts.Count-1]].UnNormalizePoint(FromFixedPrecision(normalizedPoints[pts[pts.Count-1]])), 0.05f, Color.black);

							lastSuccessfulStart = outputPath[outputPath.Count-2];
							lastSuccessfulEnd = outputPath[outputPath.Count-1];

							outputPath.RemoveAt(outputPath.Count-1);
							if (outputPath.Count > 1) {
								last = spPrev;
								spPrev = outputPath[outputPath.Count-2];
								turn = Side2D(points[spPrev], points[last], points[i]);
							} else {
								// TODO: Should be separate value
								turn = Side.Colinear;
							}
							i--;
							marker2.End();
							continue;
						}
					}
					marker2.End();
				}
			}

			Profiler.EndSample();

			var result = ListPool<Vector3>.Claim(outputPath.Count);
			for (int j = 0; j < outputPath.Count; j++) {
				var idx = outputPath[j];
				result.Add(nodes[idx].UnNormalizePoint(FromFixedPrecision(normalizedPoints[idx])));
			}

			ArrayPool<GridNodeBase>.Release(ref nodes);
			ArrayPool<int2>.Release(ref points);
			ArrayPool<int2>.Release(ref normalizedPoints);
			ArrayPool<uint>.Release(ref costs);
			ListPool<int>.Release(ref outputPath);
			ListPool<GraphNode>.Release(ref trace);
			return result;
		}
	}
}
