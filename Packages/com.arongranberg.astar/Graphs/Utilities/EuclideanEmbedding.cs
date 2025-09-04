#pragma warning disable 414
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Pathfinding.Pooling;
using Pathfinding.Collections;

namespace Pathfinding.Graphs.Util {
	using Pathfinding.Drawing;

	public enum HeuristicOptimizationMode {
		None,
		Random,
		RandomSpreadOut,
		Custom
	}

	/// <summary>
	/// Implements heuristic optimizations.
	///
	/// See: heuristic-opt
	/// See: Game AI Pro - Pathfinding Architecture Optimizations by Steve Rabin and Nathan R. Sturtevant
	/// </summary>
	[System.Serializable]
	public class EuclideanEmbedding {
		/// <summary>
		/// If heuristic optimization should be used and how to place the pivot points.
		/// See: heuristic-opt
		/// See: Game AI Pro - Pathfinding Architecture Optimizations by Steve Rabin and Nathan R. Sturtevant
		/// </summary>
		public HeuristicOptimizationMode mode;

		public int seed;

		/// <summary>All children of this transform will be used as pivot points</summary>
		public Transform pivotPointRoot;

		public int spreadOutCount = 10;

		[System.NonSerialized]
		public bool dirty;

		/// <summary>
		/// Costs laid out as n*[int],n*[int],n*[int] where n is the number of pivot points.
		/// Each node has n integers which is the cost from that node to the pivot node.
		/// They are at around the same place in the array for simplicity and for cache locality.
		///
		/// cost(nodeIndex, pivotIndex) = costs[nodeIndex*pivotCount+pivotIndex]
		/// </summary>
		public NativeArray<uint> costs { get; private set; }
		public int pivotCount { get; private set; }

		GraphNode[] pivots;

		/*
		 * Seed for random number generator.
		 * Must not be zero
		 */
		const uint ra = 12820163;

		/*
		 * Seed for random number generator.
		 * Must not be zero
		 */
		const uint rc = 1140671485;

		/*
		 * Parameter for random number generator.
		 */
		uint rval;

		/// <summary>
		/// Simple linear congruential generator.
		/// See: http://en.wikipedia.org/wiki/Linear_congruential_generator
		/// </summary>
		uint GetRandom () {
			rval = (ra*rval + rc);
			return rval;
		}
		public void OnDisable () {
			if (costs.IsCreated) costs.Dispose();
			costs = default;
			pivotCount = 0;
		}

		public static uint GetHeuristic (UnsafeSpan<uint> costs, uint pivotCount, uint nodeIndex1, uint nodeIndex2) {
			uint mx = 0;
			// TODO: Force pivotCount to be a multiple of 4 and use SIMD for performance
			if (nodeIndex1 < costs.Length && nodeIndex2 < costs.Length) {
				for (uint i = 0; i < pivotCount; i++) {
					var c1 = costs[nodeIndex1*pivotCount+i];
					var c2 = costs[nodeIndex2*pivotCount+i];
					// If either of the nodes have an unknown cost to the pivot point,
					// then we cannot use this pivot to calculate a heuristic
					if (c1 == uint.MaxValue || c2 == uint.MaxValue) continue;
					uint d = (uint)math.abs((int)c1 - (int)c2);
					if (d > mx) mx = d;
				}
			}
			return mx;
		}

		void GetClosestWalkableNodesToChildrenRecursively (Transform tr, List<GraphNode> nodes) {
			var nn = NNConstraint.Walkable;
			foreach (Transform ch in tr) {
				var info = AstarPath.active.GetNearest(ch.position, nn);
				if (info.node != null && info.node.Walkable) {
					nodes.Add(info.node);
				}

				GetClosestWalkableNodesToChildrenRecursively(ch, nodes);
			}
		}

		/// <summary>
		/// Pick N random walkable nodes from all nodes in all graphs and add them to the buffer.
		///
		/// Here we select N random nodes from a stream of nodes.
		/// Probability of choosing the first N nodes is 1
		/// Probability of choosing node i is min(N/i,1)
		/// A selected node will replace a random node of the previously
		/// selected ones.
		///
		/// See: https://en.wikipedia.org/wiki/Reservoir_sampling
		/// </summary>
		void PickNRandomNodes (int count, List<GraphNode> buffer) {
			int n = 0;

			var graphs = AstarPath.active.graphs;
			if (graphs == null) return;

			// Loop through all graphs
			// TODO: Probability to pick node should go up (more) if the area the node is in is larger
			for (int j = 0; j < graphs.Length; j++) {
				// Loop through all nodes in the graph
				if (graphs[j] != null) {
					graphs[j].GetNodes(node => {
						if (!node.Destroyed && node.Walkable) {
							n++;
							if ((GetRandom() % n) < count) {
								if (buffer.Count < count) {
									buffer.Add(node);
								} else {
									buffer[(int)(n%buffer.Count)] = node;
								}
							}
						}
					});
				}
			}
		}

		GraphNode PickAnyWalkableNode () {
			var graphs = AstarPath.active.graphs;
			GraphNode first = null;

			// Find any node in the graphs
			for (int j = 0; j < graphs.Length; j++) {
				if (graphs[j] != null) {
					graphs[j].GetNodes(node => {
						if (node != null && node.Walkable && first == null) {
							first = node;
						}
					});
				}
			}

			return first;
		}

		public void RecalculatePivots () {
			if (mode == HeuristicOptimizationMode.None) {
				pivotCount = 0;
				pivots = null;
				return;
			}

			// Reset the random number generator
			rval = (uint)seed;

			// Get a List<GraphNode> from a pool
			var pivotList = Pathfinding.Pooling.ListPool<GraphNode>.Claim();

			switch (mode) {
			case HeuristicOptimizationMode.Custom:
				if (pivotPointRoot == null) throw new System.Exception("heuristicOptimizationMode is HeuristicOptimizationMode.Custom, " +
					"but no 'customHeuristicOptimizationPivotsRoot' is set");

				GetClosestWalkableNodesToChildrenRecursively(pivotPointRoot, pivotList);
				break;
			case HeuristicOptimizationMode.Random:
				PickNRandomNodes(spreadOutCount, pivotList);
				break;
			case HeuristicOptimizationMode.RandomSpreadOut:
				if (pivotPointRoot != null) {
					GetClosestWalkableNodesToChildrenRecursively(pivotPointRoot, pivotList);
				}

				// If no pivot points were found, fall back to picking arbitrary nodes
				if (pivotList.Count == 0) {
					GraphNode first = PickAnyWalkableNode();

					if (first != null) {
						pivotList.Add(first);
					} else {
						Pathfinding.Pooling.ListPool<GraphNode>.Release(ref pivotList);
						pivots = new GraphNode[0];
						return;
					}
				}

				// Fill remaining slots with null
				int toFill = spreadOutCount - pivotList.Count;
				for (int i = 0; i < toFill; i++) pivotList.Add(null);
				break;
			default:
				throw new System.Exception("Invalid HeuristicOptimizationMode: " + mode);
			}

			pivots = pivotList.ToArray();

			Pathfinding.Pooling.ListPool<GraphNode>.Release(ref pivotList);
		}

		class EuclideanEmbeddingSearchPath : Path {
			public UnsafeSpan<uint> costs;
			public uint costIndexStride;
			public uint pivotIndex;
			public GraphNode startNode;
			public uint furthestNodeScore;
			public GraphNode furthestNode;

			public static EuclideanEmbeddingSearchPath Construct (UnsafeSpan<uint> costs, uint costIndexStride, uint pivotIndex, GraphNode startNode) {
				var p = PathPool.GetPath<EuclideanEmbeddingSearchPath>();
				p.costs = costs;
				p.costIndexStride = costIndexStride;
				p.pivotIndex = pivotIndex;
				p.startNode = startNode;
				p.furthestNodeScore = 0;
				p.furthestNode = null;
				return p;
			}

			protected override void OnFoundEndNode (uint pathNode, uint hScore, uint gScore) {
				throw new System.InvalidOperationException();
			}

			protected override void OnHeapExhausted () {
				CompleteState = PathCompleteState.Complete;
			}

			public override void OnVisitNode (uint pathNode, uint hScore, uint gScore) {
				if (!pathHandler.IsTemporaryNode(pathNode)) {
					// Get the node and then the node index from that.
					// This is because a triangle mesh node will have 3 path nodes,
					// but we want to collapse those to the same index as the original node.
					var node = pathHandler.GetNode(pathNode);
					uint baseIndex = node.NodeIndex*costIndexStride;
					// EnsureCapacity(idx);

					costs[baseIndex + pivotIndex] = math.min(costs[baseIndex + pivotIndex], gScore);

					// Find the minimum distance from the node to all existing pivot points
					uint mx = uint.MaxValue;
					for (int p = 0; p <= pivotIndex; p++) mx = math.min(mx, costs[baseIndex + (uint)p]);

					// Pick the node which has the largest minimum distance to the existing pivot points
					// (i.e pick the one furthest away from the existing ones)
					if (mx > furthestNodeScore || furthestNode == null) {
						furthestNodeScore = mx;
						furthestNode = node;
					}
				}
			}

			protected override void Prepare () {
				pathHandler.AddTemporaryNode(new TemporaryNode {
					associatedNode = startNode.NodeIndex,
					position = startNode.position,
					type = TemporaryNodeType.Start,
				});
				heuristicObjective = new HeuristicObjective(0, Heuristic.None, 0.0f);
				MarkNodesAdjacentToTemporaryEndNodes();
				AddStartNodesToHeap();
			}
		}

		public void RecalculateCosts () {
			if (pivots == null) RecalculatePivots();
			if (mode == HeuristicOptimizationMode.None) return;

			// Use a nested call to avoid allocating a delegate object
			// even when we just do an early return.
			RecalculateCostsInner();
		}

		void RecalculateCostsInner () {
			pivotCount = 0;

			for (int i = 0; i < pivots.Length; i++) {
				if (pivots[i] != null && (pivots[i].Destroyed || !pivots[i].Walkable)) {
					throw new System.Exception("Invalid pivot nodes (destroyed or unwalkable)");
				}
			}

			if (mode != HeuristicOptimizationMode.RandomSpreadOut)
				for (int i = 0; i < pivots.Length; i++)
					if (pivots[i] == null)
						throw new System.Exception("Invalid pivot nodes (null)");

			pivotCount = pivots.Length;

			System.Action<int> startCostCalculation = null;

			int numComplete = 0;

			var nodeCount = AstarPath.active.nodeStorage.nextNodeIndex;
			if (costs.IsCreated) costs.Dispose();
			// TODO: Quantize costs a bit to reduce memory usage?
			costs = new NativeArray<uint>((int)nodeCount * pivotCount, Allocator.Persistent);
			costs.AsUnsafeSpan().Fill(uint.MaxValue);

			startCostCalculation = (int pivotIndex) => {
				GraphNode pivot = pivots[pivotIndex];

				var path = EuclideanEmbeddingSearchPath.Construct(
					costs.AsUnsafeSpan(),
					(uint)pivotCount,
					(uint)pivotIndex,
					pivot
					);

				path.immediateCallback = (Path _) =>  {
					if (mode == HeuristicOptimizationMode.RandomSpreadOut && pivotIndex < pivots.Length-1) {
						// If the next pivot is null
						// then find the node which is furthest away from the earlier
						// pivot points
						if (pivots[pivotIndex+1] == null) {
							pivots[pivotIndex+1] = path.furthestNode;

							if (path.furthestNode == null) {
								Debug.LogError("Failed generating random pivot points for heuristic optimizations");
								return;
							}
						}

						// Start next path
						startCostCalculation(pivotIndex+1);
					}

					numComplete++;
					if (numComplete == pivotCount) {
						// Last completed path
						ApplyGridGraphEndpointSpecialCase();
					}
				};

				AstarPath.StartPath(path, true, true);
			};

			if (mode != HeuristicOptimizationMode.RandomSpreadOut) {
				// All calculated in parallel
				for (int i = 0; i < pivots.Length; i++) {
					startCostCalculation(i);
				}
			} else if (pivots.Length > 0) {
				// Recursive and serial
				startCostCalculation(0);
			}

			dirty = false;
		}

		/// <summary>
		/// Special case necessary for paths to unwalkable nodes right next to walkable nodes to be able to use good heuristics.
		///
		/// This will find all unwalkable nodes in all grid graphs with walkable nodes as neighbours
		/// and set the cost to reach them from each of the pivots as the minimum of the cost to
		/// reach the neighbours of each node.
		///
		/// See: ABPath.EndPointGridGraphSpecialCase
		/// </summary>
		void ApplyGridGraphEndpointSpecialCase () {
			var costs = this.costs.AsUnsafeSpan();
#if !ASTAR_NO_GRID_GRAPH
			var graphs = AstarPath.active.graphs;
			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] is GridGraph gg) {
					// Found a grid graph
					var nodes = gg.nodes;

					// Number of neighbours as an int
					int mxnum = gg.neighbours == NumNeighbours.Four ? 4 : (gg.neighbours == NumNeighbours.Eight ? 8 : 6);

					for (int z = 0; z < gg.depth; z++) {
						for (int x = 0; x < gg.width; x++) {
							var node = nodes[z*gg.width + x];
							if (!node.Walkable) {
								var pivotIndex = node.NodeIndex*(uint)pivotCount;
								// Set all costs to reach this node to maximum
								for (int piv = 0; piv < pivotCount; piv++) {
									costs[pivotIndex + (uint)piv] = uint.MaxValue;
								}

								// Loop through all potential neighbours of the node
								// and set the cost to reach it as the minimum
								// of the costs to reach any of the adjacent nodes
								for (int d = 0; d < mxnum; d++) {
									int nx, nz;
									if (gg.neighbours == NumNeighbours.Six) {
										// Hexagon graph
										nx = x + GridGraph.neighbourXOffsets[GridGraph.hexagonNeighbourIndices[d]];
										nz = z + GridGraph.neighbourZOffsets[GridGraph.hexagonNeighbourIndices[d]];
									} else {
										nx = x + GridGraph.neighbourXOffsets[d];
										nz = z + GridGraph.neighbourZOffsets[d];
									}

									// Check if the position is still inside the grid
									if (nx >= 0 && nz >= 0 && nx < gg.width && nz < gg.depth) {
										var adjacentNode = gg.nodes[nz*gg.width + nx];
										if (adjacentNode.Walkable) {
											for (uint piv = 0; piv < pivotCount; piv++) {
												uint cost = costs[adjacentNode.NodeIndex*(uint)pivotCount + piv] + gg.neighbourCosts[d];
												costs[pivotIndex + piv] = System.Math.Min(costs[pivotIndex + piv], cost);
												//Debug.DrawLine((Vector3)node.position, (Vector3)adjacentNode.position, Color.blue, 1);
											}
										}
									}
								}
							}
						}
					}
				}
			}
#endif
		}

		public void OnDrawGizmos () {
			if (pivots != null) {
				for (int i = 0; i < pivots.Length; i++) {
					if (pivots[i] != null && !pivots[i].Destroyed) {
						Draw.SolidBox((Vector3)pivots[i].position, Vector3.one, new Color(159/255.0f, 94/255.0f, 194/255.0f, 0.8f));
					}
				}
			}
		}
	}
}
