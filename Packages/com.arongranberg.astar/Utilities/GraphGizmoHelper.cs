using UnityEngine;

namespace Pathfinding.Util {
	using Pathfinding.Drawing;
	using Pathfinding.Collections;
	using Pathfinding.Pooling;

	/// <summary>Combines hashes into a single hash value</summary>
	public struct NodeHasher {
		readonly bool includePathSearchInfo;
		readonly bool includeAreaInfo;
		readonly bool includeHierarchicalNodeInfo;
		readonly PathHandler debugData;
		public DrawingData.Hasher hasher;

		public NodeHasher(AstarPath active) {
			hasher = default;
			this.debugData = active.debugPathData;
			includePathSearchInfo = debugData != null && (active.debugMode == GraphDebugMode.F || active.debugMode == GraphDebugMode.G || active.debugMode == GraphDebugMode.H || active.showSearchTree);
			includeAreaInfo = active.debugMode == GraphDebugMode.Areas;
			includeHierarchicalNodeInfo = active.debugMode == GraphDebugMode.HierarchicalNode;
			hasher.Add(active.debugMode);
			hasher.Add(active.debugFloor);
			hasher.Add(active.debugRoof);
			hasher.Add(active.showSearchTree);
			hasher.Add(AstarColor.ColorHash());
		}

		public void HashNode (GraphNode node) {
			hasher.Add(node.GetGizmoHashCode());
			if (includeAreaInfo) hasher.Add((int)node.Area);
			if (includeHierarchicalNodeInfo) hasher.Add(node.HierarchicalNodeIndex);

			if (includePathSearchInfo) {
				var pathNode = debugData.pathNodes[node.NodeIndex];
				hasher.Add(pathNode.pathID);
				hasher.Add(pathNode.pathID == debugData.PathID);
				// hasher.Add(pathNode.F);
			}
		}

		public void Add<T>(T v) {
			hasher.Add(v);
		}

		public static implicit operator DrawingData.Hasher(NodeHasher hasher) {
			return hasher.hasher;
		}
	}

	public class GraphGizmoHelper : IAstarPooledObject, System.IDisposable {
		public DrawingData.Hasher hasher { get; private set; }
		PathHandler debugData;
		ushort debugPathID;
		GraphDebugMode debugMode;
		public bool showSearchTree;
		float debugFloor;
		float debugRoof;
		public CommandBuilder builder;
		Vector3 drawConnectionStart;
		Color drawConnectionColor;
		readonly System.Action<GraphNode> drawConnection;
#if UNITY_EDITOR
		UnsafeSpan<GlobalNodeStorage.DebugPathNode> debugPathNodes;
#endif
		GlobalNodeStorage nodeStorage;

		public GraphGizmoHelper () {
			// Cache a delegate to avoid allocating memory for it every time
			drawConnection = DrawConnection;
		}

		public static GraphGizmoHelper GetSingleFrameGizmoHelper (DrawingData gizmos, AstarPath active, RedrawScope redrawScope) {
			return GetGizmoHelper(gizmos, active, DrawingData.Hasher.NotSupplied, redrawScope);
		}

		public static GraphGizmoHelper GetGizmoHelper (DrawingData gizmos, AstarPath active, DrawingData.Hasher hasher, RedrawScope redrawScope) {
			var helper = ObjectPool<GraphGizmoHelper>.Claim();

			helper.Init(active, hasher, gizmos, redrawScope);
			return helper;
		}

		public void Init (AstarPath active, DrawingData.Hasher hasher, DrawingData gizmos, RedrawScope redrawScope) {
			if (active != null) {
				debugData = active.debugPathData;
				debugPathID = active.debugPathID;
				debugMode = active.debugMode;
				debugFloor = active.debugFloor;
				debugRoof = active.debugRoof;
				nodeStorage = active.nodeStorage;
#if UNITY_EDITOR
				if (debugData != null && debugData.threadID < active.nodeStorage.pathfindingThreadData.Length) debugPathNodes = active.nodeStorage.pathfindingThreadData[debugData.threadID].debugPathNodes;
				else debugPathNodes = default;
				showSearchTree = active.showSearchTree && debugPathNodes.Length > 0;
#else
				showSearchTree = false;
#endif
			}
			this.hasher = hasher;
			builder = gizmos.GetBuilder(hasher, redrawScope);
		}

		public void OnEnterPool () {
			builder.Dispose();
			debugData = null;
		}

		public void DrawConnections (GraphNode node) {
			if (showSearchTree) {
#if UNITY_EDITOR
				if (debugPathNodes.Length > 0) {
					var nodeIndex = node.NodeIndex;
					var variants = (uint)node.PathNodeVariants;
					for (uint i = 0; i < variants; i++) {
						var pnode = debugPathNodes[nodeIndex + i];
						if (pnode.pathID == debugPathID) {
							if (pnode.parentIndex != 0 && debugPathNodes[pnode.parentIndex].pathID == debugPathID) {
								var parent = nodeStorage.GetNode(pnode.parentIndex);
								if (parent != null) {
									var nodePos = node.DecodeVariantPosition(nodeIndex + i, pnode.fractionAlongEdge);
									var parentPos = parent.DecodeVariantPosition(pnode.parentIndex, debugPathNodes[pnode.parentIndex].fractionAlongEdge);
									builder.Line((Vector3)parentPos, (Vector3)nodePos, NodeColor(node));
								}
							}
						}
					}
				}
#endif
			} else {
				// Calculate which color to use for drawing the node
				// based on the settings specified in the editor
				drawConnectionColor = NodeColor(node);
				// Get the node position
				// Cast it here to avoid doing it for every neighbour
				drawConnectionStart = (Vector3)node.position;
				node.GetConnections(drawConnection);
			}
		}

		void DrawConnection (GraphNode other) {
			builder.Line(drawConnectionStart, ((Vector3)other.position + drawConnectionStart)*0.5f, drawConnectionColor);
		}

		/// <summary>
		/// Color to use for gizmos.
		/// Returns a color to be used for the specified node with the current debug settings (editor only).
		///
		/// Version: Since 3.6.1 this method will not handle null nodes
		/// </summary>
		public Color NodeColor (GraphNode node) {
#if UNITY_EDITOR
			if (showSearchTree && !InSearchTree(node, debugPathNodes, debugPathID)) return Color.clear;
#endif

			Color color;

			if (node.Walkable) {
				switch (debugMode) {
				case GraphDebugMode.Areas:
					color = AstarColor.GetAreaColor(node.Area);
					break;
				case GraphDebugMode.HierarchicalNode:
				case GraphDebugMode.NavmeshBorderObstacles:
					color = AstarColor.GetTagColor((uint)node.HierarchicalNodeIndex);
					break;
				case GraphDebugMode.Penalty:
					color = Color.Lerp(AstarColor.ConnectionLowLerp, AstarColor.ConnectionHighLerp, ((float)node.Penalty-debugFloor) / (debugRoof-debugFloor));
					break;
				case GraphDebugMode.Tags:
					color = AstarColor.GetTagColor(node.Tag);
					break;
				case GraphDebugMode.SolidColor:
					color = AstarColor.SolidColor;
					break;
				default:
#if UNITY_EDITOR
					if (debugPathNodes.Length == 0) {
						color = AstarColor.SolidColor;
						break;
					}

					var variants = (uint)node.PathNodeVariants;
					float value1 = float.PositiveInfinity;
					float value2 = float.PositiveInfinity;
					for (uint i = 0; i < variants; i++) {
						var pathNode = debugPathNodes[node.NodeIndex + i];
						float v;
						if (debugMode == GraphDebugMode.G) {
							v = pathNode.g;
						} else if (debugMode == GraphDebugMode.H) {
							v = pathNode.h;
						} else {
							// mode == F
							v = pathNode.g + pathNode.h;
						}
						if (pathNode.pathID == debugPathID) {
							value1 = System.Math.Min(value1, v);
						} else {
							value2 = System.Math.Min(value2, v);
						}
					}

					// Pick the minimum of only the variants searched by the current path if any, otherwise take minimum of all variants.
					// For graphs without multiple variants per node (all graphs except recast graphs), this will always just pick the value for the single node variant.
					float value = float.IsPositiveInfinity(value1) ? value2 : value1;

					color = Color.Lerp(AstarColor.ConnectionLowLerp, AstarColor.ConnectionHighLerp, (value-debugFloor) / (debugRoof-debugFloor));
#else
					color = AstarColor.SolidColor;
#endif
					break;
				}
			} else {
				color = AstarColor.UnwalkableNode;
			}

			return color;
		}

#if UNITY_EDITOR
		/// <summary>
		/// Returns if the node is in the search tree of the path.
		/// Only guaranteed to be correct if path is the latest path calculated.
		/// Use for gizmo drawing only.
		/// </summary>
		internal static bool InSearchTree (GraphNode node, UnsafeSpan<GlobalNodeStorage.DebugPathNode> debugPathNodes, ushort pathID) {
			if (debugPathNodes.Length > 0) {
				for (uint i = 0; i < node.PathNodeVariants; i++) {
					if (debugPathNodes[node.NodeIndex + i].pathID == pathID) {
						return true;
					}
				}
			}
			return false;
		}
#endif

		public void DrawWireTriangle (Vector3 a, Vector3 b, Vector3 c, Color color) {
			builder.Line(a, b, color);
			builder.Line(b, c, color);
			builder.Line(c, a, color);
		}

		public void DrawTriangles (Vector3[] vertices, Color[] colors, int numTriangles) {
			var triangles = ArrayPool<int>.Claim(numTriangles*3);

			for (int i = 0; i < numTriangles*3; i++) triangles[i] = i;
			builder.SolidMesh(vertices, triangles, colors, numTriangles*3, numTriangles*3);
			ArrayPool<int>.Release(ref triangles);
		}

		public void DrawWireTriangles (Vector3[] vertices, Color[] colors, int numTriangles) {
			for (int i = 0; i < numTriangles; i++) {
				DrawWireTriangle(vertices[i*3+0], vertices[i*3+1], vertices[i*3+2], colors[i*3+0]);
			}
		}

		void System.IDisposable.Dispose () {
			var tmp = this;

			ObjectPool<GraphGizmoHelper>.Release(ref tmp);
		}
	}
}
