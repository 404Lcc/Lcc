// #define CHECK_INVARIANTS
using System.Collections.Generic;
using Pathfinding.Util;
using UnityEngine;
using Unity.Jobs;
using UnityEngine.Profiling;
using UnityEngine.Assertions;

namespace Pathfinding {
	using System.Runtime.InteropServices;
	using Pathfinding.Drawing;
	using Pathfinding.Jobs;
	using Pathfinding.Collections;
	using Pathfinding.Sync;
	using Pathfinding.Pooling;
	using Unity.Collections;
	using Unity.Collections.LowLevel.Unsafe;

	/// <summary>
	/// Holds a hierarchical graph to speed up certain pathfinding queries.
	///
	/// A common type of query that needs to be very fast is on the form 'is this node reachable from this other node'.
	/// This is for example used when picking the end node of a path. The end node is determined as the closest node to the end point
	/// that can be reached from the start node.
	///
	/// This data structure's primary purpose is to keep track of which connected component each node is contained in, in order to make such queries fast.
	///
	/// See: https://en.wikipedia.org/wiki/Connected_component_(graph_theory)
	///
	/// A connected component is a set of nodes such that there is a valid path between every pair of nodes in that set.
	/// Thus the query above can simply be answered by checking if they are in the same connected component.
	/// The connected component is exposed on nodes as the <see cref="Pathfinding.GraphNode.Area"/> property and on this class using the <see cref="GetConnectedComponent"/> method.
	///
	/// Note: This class does not calculate strictly connected components. In case of one-way connections, it will still consider the nodes to be in the same connected component.
	///
	/// In the image below (showing a 200x200 grid graph) each connected component is colored using a separate color.
	/// The actual color doesn't signify anything in particular however, only that they are different.
	/// [Open online documentation to see images]
	///
	/// Prior to version 4.2 the connected components were just a number stored on each node, and when a graph was updated
	/// the connected components were completely recalculated. This can be done relatively efficiently using a flood filling
	/// algorithm (see https://en.wikipedia.org/wiki/Flood_fill) however it still requires a pass through every single node
	/// which can be quite costly on larger graphs.
	///
	/// This class instead builds a much smaller graph that still respects the same connectivity as the original graph.
	/// Each node in this hierarchical graph represents a larger number of real nodes that are one single connected component.
	/// Take a look at the image below for an example. In the image each color is a separate hierarchical node, and the black connections go between the center of each hierarchical node.
	///
	/// [Open online documentation to see images]
	///
	/// With the hierarchical graph, the connected components can be calculated by flood filling the hierarchical graph instead of the real graph.
	/// Then when we need to know which connected component a node belongs to, we look up the connected component of the hierarchical node the node belongs to.
	///
	/// The benefit is not immediately obvious. The above is just a bit more complicated way to accomplish the same thing. However the real benefit comes when updating the graph.
	/// When the graph is updated, all hierarchical nodes which contain any node that was affected by the update is removed completely and then once all have been removed new hierarchical nodes are recalculated in their place.
	/// Once this is done the connected components of the whole graph can be updated by flood filling only the hierarchical graph. Since the hierarchical graph is vastly smaller than the real graph, this is significantly faster.
	///
	/// [Open online documentation to see videos]
	///
	/// So finally using all of this, the connected components of the graph can be recalculated very quickly as the graph is updated.
	/// The effect of this grows larger the larger the graph is, and the smaller the graph update is. Making a small update to a 1000x1000 grid graph is on the order of 40 times faster with these optimizations.
	/// When scanning a graph or making updates to the whole graph at the same time there is however no speed boost. In fact due to the extra complexity it is a bit slower, however after profiling the extra time seems to be mostly insignificant compared to the rest of the cost of scanning the graph.
	///
	/// [Open online documentation to see videos]
	///
	/// See: <see cref="Pathfinding.PathUtilities.IsPathPossible"/>
	/// See: <see cref="Pathfinding.NNConstraint"/>
	/// See: <see cref="Pathfinding.GraphNode.Area"/>
	/// </summary>
	public class HierarchicalGraph {
		const int Tiling = 16;
		const int MaxChildrenPerNode = Tiling * Tiling;
		const int MinChildrenPerNode = MaxChildrenPerNode/2;

		GlobalNodeStorage nodeStorage;
		internal List<GraphNode>[] children;
		internal NativeList<int> connectionAllocations;
		internal SlabAllocator<int> connectionAllocator;
		NativeList<int> dirtiedHierarchicalNodes;
		int[] areas;
		byte[] dirty;
		int[] versions;
		internal NativeList<Bounds> bounds;
		/// <summary>Holds areas.Length as a burst-accessible reference</summary>
		NativeReference<int> numHierarchicalNodes;
		internal GCHandle gcHandle;

		public int version { get; private set; }
		public NavmeshEdges navmeshEdges;

		Queue<GraphNode> temporaryQueue = new Queue<GraphNode>();
		List<int> currentConnections = new List<int>();
		Stack<int> temporaryStack = new Stack<int>();

		HierarchicalBitset dirtyNodes;

		CircularBuffer<int> freeNodeIndices;

		int gizmoVersion = 0;

		RWLock rwLock = new RWLock();

		/// <summary>
		/// Disposes of all unmanaged data and clears managed data.
		///
		/// If you want to use this instance again, you must call <see cref="OnEnable"/>.
		/// </summary>
		internal void OnDisable () {
			rwLock.WriteSync().Unlock();
			navmeshEdges.Dispose();
			if (gcHandle.IsAllocated) gcHandle.Free();
			if (connectionAllocator.IsCreated) {
				numHierarchicalNodes.Dispose();
				connectionAllocator.Dispose();
				connectionAllocations.Dispose();
				bounds.Dispose();
				dirtiedHierarchicalNodes.Dispose();
				dirtyNodes.Dispose();

				children = null;
				areas = null;
				dirty = null;
				versions = null;
				freeNodeIndices.Clear();
			}
		}

		// Make methods internal
		public int GetHierarchicalNodeVersion (int index) {
			return (index * 71237) ^ versions[index];
		}

		/// <summary>Burst-accessible data about the hierarhical nodes</summary>
		public struct HierarhicalNodeData {
			[Unity.Collections.ReadOnly]
			public SlabAllocator<int> connectionAllocator;
			[Unity.Collections.ReadOnly]
			public NativeList<int> connectionAllocations;
			[Unity.Collections.ReadOnly]
			public NativeList<Bounds> bounds;
		}

		/// <summary>
		/// Data about the hierarhical nodes.
		///
		/// Can be accessed in burst jobs.
		/// </summary>
		public HierarhicalNodeData GetHierarhicalNodeData (out RWLock.ReadLockAsync readLock) {
			readLock = rwLock.Read();
			return new HierarhicalNodeData {
					   connectionAllocator = connectionAllocator,
					   connectionAllocations = connectionAllocations,
					   bounds = bounds,
			};
		}

		internal HierarchicalGraph (GlobalNodeStorage nodeStorage) {
			this.nodeStorage = nodeStorage;
			navmeshEdges = new NavmeshEdges();
			navmeshEdges.hierarchicalGraph = this;
		}

		/// <summary>
		/// Initializes the HierarchicalGraph data.
		/// It is safe to call this multiple times even if it has already been enabled before.
		/// </summary>
		public void OnEnable () {
			if (!connectionAllocator.IsCreated) {
				gcHandle = GCHandle.Alloc(this);

				connectionAllocator = new SlabAllocator<int>(1024, Allocator.Persistent);
				connectionAllocations = new NativeList<int>(0, Allocator.Persistent);
				bounds = new NativeList<Bounds>(0, Allocator.Persistent);
				numHierarchicalNodes = new NativeReference<int>(0, Allocator.Persistent);
				dirtiedHierarchicalNodes = new NativeList<int>(0, Allocator.Persistent);
				dirtyNodes = new HierarchicalBitset(1024, Allocator.Persistent);

				children = new List<GraphNode>[1] { new List<GraphNode>() };
				areas = new int[1];
				dirty = new byte[1];
				versions = new int[1];
				freeNodeIndices.Clear();
			}
		}

		internal void OnCreatedNode (GraphNode node) {
			AddDirtyNode(node);
		}

		internal void OnDestroyedNode (GraphNode node) {
			dirty[node.HierarchicalNodeIndex] = 1;
			dirtyNodes.Reset((int)node.NodeIndex);
			node.IsHierarchicalNodeDirty = false;
		}

		/// <summary>
		/// Marks this node as dirty because it's connectivity or walkability has changed.
		/// This must be called by node classes after any connectivity/walkability changes have been made to them.
		///
		/// See: <see cref="GraphNode.SetConnectivityDirty"/>
		/// </summary>
		public void AddDirtyNode (GraphNode node) {
			if (!node.IsHierarchicalNodeDirty) {
				// We may be calling this when shutting down
				if (!dirtyNodes.IsCreated || node.Destroyed) return;

				dirtyNodes.Set((int)node.NodeIndex);

				// Mark the associated hierarchical node as dirty to ensure it is recalculated or removed later.
				// Nodes that have been unwalkable since the last update, will have HierarchicalNodeIndex=0, which is a dummy hierarchical node that is never used.
				dirty[node.HierarchicalNodeIndex] = 1;
				node.IsHierarchicalNodeDirty = true;
			}
		}

		public void ReserveNodeIndices (uint nodeIndexCount) {
			dirtyNodes.Capacity = Mathf.Max(dirtyNodes.Capacity, (int)nodeIndexCount);
		}

		public int NumConnectedComponents { get; private set; }

		/// <summary>Get the connected component index of a hierarchical node</summary>
		public uint GetConnectedComponent (int hierarchicalNodeIndex) {
			return (uint)areas[hierarchicalNodeIndex];
		}

		struct JobRecalculateComponents : IJob {
			public System.Runtime.InteropServices.GCHandle hGraphGC;
			public NativeList<int> connectionAllocations;
			public NativeList<Bounds> bounds;
			public NativeList<int> dirtiedHierarchicalNodes;
			public NativeReference<int> numHierarchicalNodes;

			void Grow (HierarchicalGraph graph) {
				var newChildren = new List<GraphNode>[System.Math.Max(64, graph.children.Length*2)];
				var newAreas = new int[newChildren.Length];
				var newDirty = new byte[newChildren.Length];
				var newVersions = new int[newChildren.Length];
				numHierarchicalNodes.Value = newChildren.Length;

				graph.children.CopyTo(newChildren, 0);
				graph.areas.CopyTo(newAreas, 0);
				graph.dirty.CopyTo(newDirty, 0);
				graph.versions.CopyTo(newVersions, 0);
				bounds.Resize(newChildren.Length, NativeArrayOptions.UninitializedMemory);
				connectionAllocations.Resize(newChildren.Length, NativeArrayOptions.ClearMemory);

				// Create all necessary lists for the new nodes
				// Iterate in reverse so that when popping from the freeNodeIndices
				// stack we get numbers from smallest to largest (this is not
				// necessary, but it makes the starting colors be a bit nicer when
				// visualized in the scene view).
				for (int i = newChildren.Length - 1; i >= graph.children.Length; i--) {
					newChildren[i] = ListPool<GraphNode>.Claim(MaxChildrenPerNode);
					connectionAllocations[i] = SlabAllocator<int>.InvalidAllocation;
					if (i > 0) graph.freeNodeIndices.PushEnd(i);
				}
				connectionAllocations[0] = SlabAllocator<int>.InvalidAllocation;

				graph.children = newChildren;
				graph.areas = newAreas;
				graph.dirty = newDirty;
				graph.versions = newVersions;
			}

			int GetHierarchicalNodeIndex (HierarchicalGraph graph) {
				if (graph.freeNodeIndices.Length == 0) Grow(graph);
				return graph.freeNodeIndices.PopEnd();
			}

			void RemoveHierarchicalNode (HierarchicalGraph hGraph, int hierarchicalNode, bool removeAdjacentSmallNodes) {
				Assert.AreNotEqual(hierarchicalNode, 0);
				hGraph.freeNodeIndices.PushEnd(hierarchicalNode);
				hGraph.versions[hierarchicalNode]++;
				var connAllocation = connectionAllocations[hierarchicalNode];
				var conns = hGraph.connectionAllocator.GetSpan(connAllocation);

				for (int i = 0; i < conns.Length; i++) {
					var adjacentHierarchicalNode = conns[i];
					// If dirty, this hierarchical node will be removed later anyway, so don't bother doing anything with it.
					if (hGraph.dirty[adjacentHierarchicalNode] != 0) continue;

					if (removeAdjacentSmallNodes && hGraph.children[adjacentHierarchicalNode].Count < MinChildrenPerNode) {
						hGraph.dirty[adjacentHierarchicalNode] = 2;
						RemoveHierarchicalNode(hGraph, adjacentHierarchicalNode, false);

						// The connection list may have been reallocated, so we need to get it again
						conns = hGraph.connectionAllocator.GetSpan(connAllocation);
					} else {
						// Remove the connection from the other node to this node as we are removing this node.
						var otherConnections = hGraph.connectionAllocator.GetList(connectionAllocations[adjacentHierarchicalNode]);
						otherConnections.Remove(hierarchicalNode);
						// Update the allocation index of the list, in case it was reallocated
						connectionAllocations[adjacentHierarchicalNode] = otherConnections.allocationIndex;
					}
				}
				Assert.AreEqual(connectionAllocations[hierarchicalNode], connAllocation);

				hGraph.connectionAllocator.Free(connAllocation);
				connectionAllocations[hierarchicalNode] = SlabAllocator<int>.InvalidAllocation;

				var nodeChildren = hGraph.children[hierarchicalNode];

				// Ensure all children of dirty hierarchical nodes are included in the recalculation
				var preDirty = hGraph.dirty[hierarchicalNode];
				for (int i = 0; i < nodeChildren.Count; i++) {
					if (!nodeChildren[i].Destroyed) hGraph.AddDirtyNode(nodeChildren[i]);
				}
				// Put the dirty flag back to what it was before, as it might have been set to 1 by the AddDirtyNode call
				hGraph.dirty[hierarchicalNode] = preDirty;

				nodeChildren.ClearFast();
			}

			[System.Diagnostics.Conditional("CHECK_INVARIANTS")]
			void CheckConnectionInvariants () {
				var hGraph = (HierarchicalGraph)hGraphGC.Target;
				if (connectionAllocations.Length > 0) Assert.AreEqual(connectionAllocations[0], SlabAllocator<int>.InvalidAllocation);
				for (int i = 0; i < connectionAllocations.Length; i++) {
					if (connectionAllocations[i] != SlabAllocator<int>.InvalidAllocation) {
						var conns = hGraph.connectionAllocator.GetSpan(connectionAllocations[i]);
						for (int j = 0; j < conns.Length; j++) {
							Assert.IsFalse(connectionAllocations[conns[j]] == SlabAllocator<int>.InvalidAllocation, "Invalid connection allocation");
							var otherConns = hGraph.connectionAllocator.GetSpan(connectionAllocations[conns[j]]);
							if (!otherConns.Contains(i)) {
								throw new System.Exception("Connections are not bidirectional");
							}
						}
					}
				}
			}

			[System.Diagnostics.Conditional("CHECK_INVARIANTS")]
			void CheckPreUpdateInvariants () {
				var hGraph = (HierarchicalGraph)hGraphGC.Target;

				if (connectionAllocations.Length > 0) Assert.AreEqual(connectionAllocations[0], SlabAllocator<int>.InvalidAllocation);
				for (int i = 0; i < connectionAllocations.Length; i++) {
					if (connectionAllocations[i] != SlabAllocator<int>.InvalidAllocation) {
						var children = hGraph.children[i];
						for (int j = 0; j < children.Count; j++) {
							if (!children[j].Destroyed) {
								Assert.AreEqual(children[j].HierarchicalNodeIndex, i);
							}
						}
					}
				}
			}

			[System.Diagnostics.Conditional("CHECK_INVARIANTS")]
			void CheckChildInvariants () {
				var hGraph = (HierarchicalGraph)hGraphGC.Target;

				if (connectionAllocations.Length > 0) Assert.AreEqual(connectionAllocations[0], SlabAllocator<int>.InvalidAllocation);
				for (int i = 0; i < connectionAllocations.Length; i++) {
					if (connectionAllocations[i] != SlabAllocator<int>.InvalidAllocation) {
						var children = hGraph.children[i];
						for (int j = 0; j < children.Count; j++) {
							Assert.IsFalse(children[j].Destroyed);
							Assert.AreEqual(children[j].HierarchicalNodeIndex, i);
						}
					}
				}
			}

			struct Context {
				public List<GraphNode> children;
				public int hierarchicalNodeIndex;
				public List<int> connections;
				public uint graphindex;
				public Queue<GraphNode> queue;
			}


			/// <summary>Run a BFS out from a start node and assign up to MaxChildrenPerNode nodes to the specified hierarchical node which are not already assigned to another hierarchical node</summary>
			void FindHierarchicalNodeChildren (HierarchicalGraph hGraph, int hierarchicalNode, GraphNode startNode) {
				Assert.AreNotEqual(hierarchicalNode, 0);
				Assert.AreEqual(hGraph.children[hierarchicalNode].Count, 0);
				hGraph.versions[hierarchicalNode]++;

				// We create a context and pass that by reference to the GetConnections method.
				// This allows us to pass a non-capturing delegate, which does not require a heap allocation.
				var queue = hGraph.temporaryQueue;
				var context = new Context {
					children =  hGraph.children[hierarchicalNode],
					hierarchicalNodeIndex = hierarchicalNode,
					connections = hGraph.currentConnections,
					graphindex = startNode.GraphIndex,
					queue = queue,
				};
				context.connections.Clear();
				context.children.Add(startNode);
				context.queue.Enqueue(startNode);
				startNode.HierarchicalNodeIndex = hierarchicalNode;

				GraphNode.GetConnectionsWithData<Context> visitConnection = (GraphNode neighbour, ref Context context) => {
					if (neighbour.Destroyed) {
						throw new System.InvalidOperationException("A node in a " + AstarPath.active.graphs[context.graphindex].GetType().Name + " contained a connection to a destroyed " + neighbour.GetType().Name + ".");
					}
					var hIndex = neighbour.HierarchicalNodeIndex;
					if (hIndex == 0) {
						if (context.children.Count < MaxChildrenPerNode && neighbour.Walkable && neighbour.GraphIndex == context.graphindex /* && (((GridNode)currentChildren[0]).XCoordinateInGrid/Tiling == ((GridNode)neighbour).XCoordinateInGrid/Tiling) && (((GridNode)currentChildren[0]).ZCoordinateInGrid/Tiling == ((GridNode)neighbour).ZCoordinateInGrid/Tiling)*/) {
							neighbour.HierarchicalNodeIndex = context.hierarchicalNodeIndex;
							context.queue.Enqueue(neighbour);
							context.children.Add(neighbour);
						}
					} else if (hIndex != context.hierarchicalNodeIndex && !context.connections.Contains(hIndex)) {
						// The Contains call can in theory be very slow as an hierarchical node may be adjacent to an arbitrary number of nodes.
						// However in practice due to how the hierarchical nodes are constructed they will only be adjacent to a smallish (≈4-6) number of other nodes.
						// So a Contains call will be much faster than say a Set lookup.
						context.connections.Add(hIndex);
					}
				};

				while (queue.Count > 0) queue.Dequeue().GetConnections(visitConnection, ref context, Connection.IncomingConnection | Connection.OutgoingConnection);

				if (hGraph.currentConnections.Count > SlabAllocator<int>.MaxAllocationSize) {
					throw new System.Exception("Too many connections for a single hierarchical node. Do you have thousands of off-mesh links in a single location?");
				}

				for (int i = 0; i < hGraph.currentConnections.Count; i++) {
					var otherHierarchicalNode = hGraph.currentConnections[i];
					Assert.AreNotEqual(otherHierarchicalNode, 0);
					var otherAllocationIndex = connectionAllocations[otherHierarchicalNode];
					Assert.AreNotEqual(otherAllocationIndex, SlabAllocator<int>.InvalidAllocation);
					var otherConnections = hGraph.connectionAllocator.GetList(otherAllocationIndex);
					otherConnections.Add(hierarchicalNode);
					// Update the allocation index in case the list was reallocated
					connectionAllocations[otherHierarchicalNode] = otherConnections.allocationIndex;
				}

				connectionAllocations[hierarchicalNode] = hGraph.connectionAllocator.Allocate(hGraph.currentConnections);
				queue.Clear();
			}

			/// <summary>Flood fills the graph of hierarchical nodes and assigns the same area ID to all hierarchical nodes that are in the same connected component</summary>
			void FloodFill (HierarchicalGraph hGraph) {
				var areas = hGraph.areas;
				for (int i = 0; i < areas.Length; i++) areas[i] = 0;

				Stack<int> stack = hGraph.temporaryStack;
				int currentArea = 0;
				for (int i = 1; i < areas.Length; i++) {
					// Already taken care of, or does not exist
					if (areas[i] != 0 || connectionAllocations[i] == SlabAllocator<int>.InvalidAllocation) continue;

					currentArea++;
					areas[i] = currentArea;
					stack.Push(i);
					while (stack.Count > 0) {
						int node = stack.Pop();
						var conns = hGraph.connectionAllocator.GetSpan(connectionAllocations[node]);
						for (int j = conns.Length - 1; j >= 0; j--) {
							var otherNode = conns[j];
							// Note: slightly important that this is != currentArea and not != 0 in case there are some connected, but not stongly connected components in the graph (this will happen in only veeery few types of games)
							if (areas[otherNode] != currentArea) {
								areas[otherNode] = currentArea;
								stack.Push(otherNode);
							}
						}
					}
				}

				hGraph.NumConnectedComponents = System.Math.Max(1, currentArea + 1);
				hGraph.version++;
			}

			public void Execute () {
				var hGraph = hGraphGC.Target as HierarchicalGraph;
				CheckPreUpdateInvariants();
				Profiler.BeginSample("Recalculate Connected Components");
				var dirty = hGraph.dirty;
				CheckConnectionInvariants();

				Profiler.BeginSample("Remove");
				// Remove all hierarchical nodes and then build new hierarchical nodes in their place
				// which take into account the new graph data.
				var initialFreeLength = hGraph.freeNodeIndices.Length;
				for (int i = 1; i < dirty.Length; i++) {
					if (dirty[i] == 1) RemoveHierarchicalNode(hGraph, i, true);
				}

				// Reset the dirty flag on all hierarchical nodes
				for (int i = 1; i < dirty.Length; i++) dirty[i] = 0;

				// Reset the dirty flag on all nodes, and make sure they don't refer to their new destroyed hierarchical nodes
				var buffer = new NativeArray<int>(512, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
				var nodeStorage = hGraph.nodeStorage;
				foreach (var span in hGraph.dirtyNodes.GetIterator(buffer.AsUnsafeSpan())) {
					for (int i = 0; i < span.Length; i++) {
						var node = nodeStorage.GetNode((uint)span[i]);
						node.IsHierarchicalNodeDirty = false;
						node.HierarchicalNodeIndex = 0;
					}
				}

				Profiler.EndSample();
				CheckConnectionInvariants();

				Profiler.BeginSample("Find");
				dirtiedHierarchicalNodes.Clear();
				foreach (var span in hGraph.dirtyNodes.GetIterator(buffer.AsUnsafeSpan())) {
					for (int i = 0; i < span.Length; i++) {
						var node = nodeStorage.GetNode((uint)span[i]);

						Assert.IsFalse(node.Destroyed);
						if (!node.Destroyed && node.HierarchicalNodeIndex == 0 && node.Walkable) {
							var hNode = GetHierarchicalNodeIndex(hGraph);
							Profiler.BeginSample("FindChildren");
							FindHierarchicalNodeChildren(hGraph, hNode, node);
							Profiler.EndSample();
							dirtiedHierarchicalNodes.Add(hNode);
						}
					}
				}

				// These are hierarchical node indices that were pushed to the free id stack, and we did not immediately reuse them.
				// This means that they have been destroyed, and we should notify the NavmeshEdges class about this.
				for (int i = initialFreeLength; i < hGraph.freeNodeIndices.Length; i++) {
					dirtiedHierarchicalNodes.Add(hGraph.freeNodeIndices[i]);
				}

				hGraph.dirtyNodes.Clear();

				// Recalculate the connected components of the hierarchical nodes
				// This is usually very quick compared to the code above
				FloodFill(hGraph);
				Profiler.EndSample();
				hGraph.gizmoVersion++;
				CheckConnectionInvariants();
				CheckChildInvariants();
			}
		}

		/// <summary>Recalculate the hierarchical graph and the connected components if any nodes have been marked as dirty</summary>
		public void RecalculateIfNecessary () {
			// We need to complete both jobs here, because after this method
			// the graph may change in arbitrary ways. The RecalculateObstacles job reads from the graph.
			JobRecalculateIfNecessary().Complete();
		}

		/// <summary>
		/// Schedule a job to recalculate the hierarchical graph and the connected components if any nodes have been marked as dirty.
		/// Returns dependsOn if nothing has to be done.
		///
		/// Note: Assumes the graph is unchanged until the returned dependency is completed.
		/// </summary>
		public JobHandle JobRecalculateIfNecessary (JobHandle dependsOn = default) {
			if (!connectionAllocator.IsCreated) throw new System.InvalidOperationException("The hierarchical graph has not been initialized. Please call OnEnable before using it.");

			if (!dirtyNodes.IsEmpty) {
				var writeLock = rwLock.Write();
				var lastJob = new JobRecalculateComponents {
					hGraphGC = gcHandle,
					connectionAllocations = connectionAllocations,
					bounds = bounds,
					dirtiedHierarchicalNodes = dirtiedHierarchicalNodes,
					numHierarchicalNodes = numHierarchicalNodes,
				}.Schedule(JobHandle.CombineDependencies(writeLock.dependency, dependsOn));
				// We need to output both jobs as dependencies.
				// Firstly they use some internal data (e.g. dirtiedHierarchicalNodes), so we need to set lastJob.
				// Secondly, they read from the graph. And the graph data is only read-only until this returned dependency is completed.
				lastJob = navmeshEdges.RecalculateObstacles(dirtiedHierarchicalNodes, numHierarchicalNodes, lastJob);
				writeLock.UnlockAfter(lastJob);
				return lastJob;
			} else {
				return dependsOn;
			}
		}

		/// <summary>
		/// Recalculate everything from scratch.
		/// This is primarily to be used for legacy code for compatibility reasons, not for any new code.
		///
		/// See: <see cref="RecalculateIfNecessary"/>
		/// </summary>
		public void RecalculateAll () {
			var writeLock = rwLock.WriteSync();
			AstarPath.active.data.GetNodes(AddDirtyNode);
			writeLock.Unlock();
			RecalculateIfNecessary();
		}

		public void OnDrawGizmos (DrawingData gizmos, RedrawScope redrawScope) {
			var hasher = new NodeHasher(AstarPath.active);

			hasher.Add(gizmoVersion);

			if (!gizmos.Draw(hasher, redrawScope)) {
				var readLock = rwLock.ReadSync();
				try {
					using (var builder = gizmos.GetBuilder(hasher, redrawScope)) {
						for (int i = 0; i < areas.Length; i++) {
							if (children[i].Count > 0) {
								builder.WireBox(bounds[i].center, bounds[i].size);
								var conns = connectionAllocator.GetSpan(connectionAllocations[i]);
								for (int j = 0; j < conns.Length; j++) {
									if (conns[j] > i) {
										builder.Line(bounds[i].center, bounds[conns[j]].center, Color.black);
									}
								}
							}
						}
					}
				} finally {
					readLock.Unlock();
				}
			}
		}
	}
}
