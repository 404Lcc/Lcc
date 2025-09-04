using UnityEngine;
using System.Collections.Generic;

namespace Pathfinding {
	using Pathfinding.Drawing;

	public class NodeLink3Node : PointNode {
		public NodeLink3 link;
		public Vector3 portalA;
		public Vector3 portalB;

		public NodeLink3Node (AstarPath astar) {
			astar.InitializeNode(this);
		}

		public override bool GetPortal (GraphNode other, out Vector3 left, out Vector3 right) {
			left = portalA;
			right = portalB;
			if (this.connections.Length < 2) return false;

			if (this.connections.Length != 2) throw new System.Exception("Invalid NodeLink3Node. Expected 2 connections, found " + this.connections.Length);

			return true;
		}

		public GraphNode GetOther (GraphNode a) {
			if (this.connections.Length < 2) return null;
			if (this.connections.Length != 2) throw new System.Exception("Invalid NodeLink3Node. Expected 2 connections, found " + this.connections.Length);

			return a == connections[0].node ? (connections[1].node as NodeLink3Node).GetOtherInternal(this) : (connections[0].node as NodeLink3Node).GetOtherInternal(this);
		}

		GraphNode GetOtherInternal (GraphNode a) {
			if (this.connections.Length < 2) return null;
			return a == connections[0].node ? connections[1].node : connections[0].node;
		}
	}

	/// <summary>
	/// Connects two TriangleMeshNodes (recast/navmesh graphs) as if they had shared an edge.
	/// Note: Usually you do not want to use this type of link, you want to use NodeLink2 or NodeLink (sorry for the not so descriptive names).
	/// </summary>
	[AddComponentMenu("Pathfinding/Link3")]
	[HelpURL("https://arongranberg.com/astar/documentation/stable/nodelink3.html")]
	public class NodeLink3 : GraphModifier {
		protected static Dictionary<GraphNode, NodeLink3> reference = new Dictionary<GraphNode, NodeLink3>();
		public static NodeLink3 GetNodeLink (GraphNode node) {
			reference.TryGetValue(node, out NodeLink3 v);
			return v;
		}

		/// <summary>End position of the link</summary>
		public Transform end;

		/// <summary>
		/// The connection will be this times harder/slower to traverse.
		/// Note that values lower than one will not always make the pathfinder choose this path instead of another path even though this one should
		/// lead to a lower total cost unless you also adjust the Heuristic Scale in A* Inspector -> Settings -> Pathfinding or disable the heuristic altogether.
		/// </summary>
		public float costFactor = 1.0f;

		public Transform StartTransform {
			get { return transform; }
		}

		public Transform EndTransform {
			get { return end; }
		}

		NodeLink3Node startNode;
		NodeLink3Node endNode;
		MeshNode connectedNode1, connectedNode2;
		Vector3 clamped1, clamped2;
		bool postScanCalled = false;

		public GraphNode StartNode {
			get { return startNode; }
		}

		public GraphNode EndNode {
			get { return endNode; }
		}

		public override void OnPostScan () {
			if (AstarPath.active.isScanning) {
				InternalOnPostScan();
			} else {
				AstarPath.active.AddWorkItem(new AstarWorkItem(_ => {
					InternalOnPostScan();
					return true;
				}));
			}
		}

		public void InternalOnPostScan () {
#if !ASTAR_NO_POINT_GRAPH
			if (AstarPath.active.data.pointGraph == null) {
				AstarPath.active.data.AddGraph(typeof(PointGraph));
			}

			//Get nearest nodes from the first point graph, assuming both start and end transforms are nodes
			startNode = AstarPath.active.data.pointGraph.AddNode(new NodeLink3Node(AstarPath.active), (Int3)StartTransform.position);
			startNode.link = this;
			endNode = AstarPath.active.data.pointGraph.AddNode(new NodeLink3Node(AstarPath.active), (Int3)EndTransform.position);
			endNode.link = this;
#else
			throw new System.Exception("Point graphs are not included. Check your A* Optimization settings.");
#endif
			connectedNode1 = null;
			connectedNode2 = null;

			if (startNode == null || endNode == null) {
				startNode = null;
				endNode = null;
				return;
			}

			postScanCalled = true;
			reference[startNode] = this;
			reference[endNode] = this;
			Apply(true);
		}

		public override void OnGraphsPostUpdateBeforeAreaRecalculation () {
			if (!AstarPath.active.isScanning) {
				if (connectedNode1 != null && connectedNode1.Destroyed) {
					connectedNode1 = null;
				}
				if (connectedNode2 != null && connectedNode2.Destroyed) {
					connectedNode2 = null;
				}

				if (!postScanCalled) {
					OnPostScan();
				} else {
					//OnPostScan will also call this method
					Apply(false);
				}
			}
		}

		protected override void OnEnable () {
			base.OnEnable();

#if !ASTAR_NO_POINT_GRAPH
			if (Application.isPlaying && AstarPath.active != null && AstarPath.active.data != null && AstarPath.active.data.pointGraph != null) {
				OnGraphsPostUpdate();
			}
#endif
		}

		protected override void OnDisable () {
			base.OnDisable();

			postScanCalled = false;

			if (startNode != null) reference.Remove(startNode);
			if (endNode != null) reference.Remove(endNode);

			if (startNode != null && endNode != null) {
				startNode.RemovePartialConnection(endNode);
				endNode.RemovePartialConnection(startNode);

				if (connectedNode1 != null && connectedNode2 != null) {
					startNode.RemovePartialConnection(connectedNode1);
					connectedNode1.RemovePartialConnection(startNode);

					endNode.RemovePartialConnection(connectedNode2);
					connectedNode2.RemovePartialConnection(endNode);
				}
			}
		}

		void RemoveConnections (GraphNode node) {
			//TODO, might be better to replace connection
			node.ClearConnections(true);
		}

		[ContextMenu("Recalculate neighbours")]
		void ContextApplyForce () {
			if (Application.isPlaying) {
				Apply(true);
			}
		}

		public void Apply (bool forceNewCheck) {
			//TODO
			//This function assumes that connections from the n1,n2 nodes never need to be removed in the future (e.g because the nodes move or something)
			NNConstraint nn = NNConstraint.None;

			nn.distanceMetric = DistanceMetric.ClosestAsSeenFromAboveSoft();
			int graph = (int)startNode.GraphIndex;

			//Search all graphs but the one which start and end nodes are on
			nn.graphMask = ~(1 << graph);

			bool same = true;

			{
				var info = AstarPath.active.GetNearest(StartTransform.position, nn);
				same &= info.node == connectedNode1 && info.node != null;
				connectedNode1 = info.node as MeshNode;
				clamped1 = info.position;
				if (connectedNode1 != null) Debug.DrawRay((Vector3)connectedNode1.position, Vector3.up*5, Color.red);
			}

			{
				var info = AstarPath.active.GetNearest(EndTransform.position, nn);
				same &= info.node == connectedNode2 && info.node != null;
				connectedNode2 = info.node as MeshNode;
				clamped2 = info.position;
				if (connectedNode2 != null) Debug.DrawRay((Vector3)connectedNode2.position, Vector3.up*5, Color.cyan);
			}

			if (connectedNode2 == null || connectedNode1 == null) return;

			startNode.position = (Int3)StartTransform.position;
			endNode.position = (Int3)EndTransform.position;

			if (same && !forceNewCheck) return;

			RemoveConnections(startNode);
			RemoveConnections(endNode);

			uint cost = (uint)Mathf.RoundToInt(((Int3)(StartTransform.position-EndTransform.position)).costMagnitude*costFactor);
			GraphNode.Connect(startNode, endNode, cost);

			Int3 dir = connectedNode2.position - connectedNode1.position;

			for (int a = 0; a < connectedNode1.GetVertexCount(); a++) {
				Int3 va1 = connectedNode1.GetVertex(a);
				Int3 va2 = connectedNode1.GetVertex((a+1) % connectedNode1.GetVertexCount());

				if (Int3.DotLong((va2-va1).Normal2D(), dir) > 0) continue;

				for (int b = 0; b < connectedNode2.GetVertexCount(); b++) {
					Int3 vb1 = connectedNode2.GetVertex(b);
					Int3 vb2 = connectedNode2.GetVertex((b+1) % connectedNode2.GetVertexCount());

					if (Int3.DotLong((vb2-vb1).Normal2D(), dir) < 0) continue;

					if (Int3.Angle((vb2-vb1), (va2-va1)) > (170.0/360.0f)*Mathf.PI*2) {
						float t1 = 0;
						float t2 = 1;

						t2 = System.Math.Min(t2, VectorMath.ClosestPointOnLineFactor(va1, va2, vb1));
						t1 = System.Math.Max(t1, VectorMath.ClosestPointOnLineFactor(va1, va2, vb2));

						if (t2 < t1) {
							Debug.LogError("Something went wrong! " + t1 + " " + t2 + " " + va1 + " " + va2 + " " + vb1 + " " + vb2+"\nTODO, how can this happen?");
						} else {
							Vector3 pa = (Vector3)(va2-va1)*t1 + (Vector3)va1;
							Vector3 pb = (Vector3)(va2-va1)*t2 + (Vector3)va1;

							startNode.portalA = pa;
							startNode.portalB = pb;

							endNode.portalA = pb;
							endNode.portalB = pa;

							//Add connections between nodes, or replace old connections if existing
							GraphNode.Connect(connectedNode1, startNode, (uint)Mathf.RoundToInt(((Int3)(clamped1 - StartTransform.position)).costMagnitude*costFactor));
							GraphNode.Connect(endNode, connectedNode2, (uint)Mathf.RoundToInt(((Int3)(clamped2 - EndTransform.position)).costMagnitude*costFactor));
							return;
						}
					}
				}
			}
		}

		private readonly static Color GizmosColor = new Color(206.0f/255.0f, 136.0f/255.0f, 48.0f/255.0f, 0.5f);
		private readonly static Color GizmosColorSelected = new Color(235.0f/255.0f, 123.0f/255.0f, 32.0f/255.0f, 1.0f);

		public override void DrawGizmos () {
			bool selected = GizmoContext.InActiveSelection(this);
			Color color = selected ? GizmosColorSelected : GizmosColor;

			if (StartTransform != null) {
				Draw.xz.Circle(StartTransform.position, 0.4f, color);
			}
			if (EndTransform != null) {
				Draw.xz.Circle(EndTransform.position, 0.4f, color);
			}

			if (StartTransform != null && EndTransform != null) {
				NodeLink.DrawArch(StartTransform.position, EndTransform.position, Vector3.up, color);
				if (selected) {
					Vector3 cross = Vector3.Cross(Vector3.up, (EndTransform.position-StartTransform.position)).normalized;
					NodeLink.DrawArch(StartTransform.position+cross*0.1f, EndTransform.position+cross*0.1f, Vector3.up, color);
					NodeLink.DrawArch(StartTransform.position-cross*0.1f, EndTransform.position-cross*0.1f, Vector3.up, color);
				}
			}
		}
	}
}
