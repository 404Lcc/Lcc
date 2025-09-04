using UnityEngine;

namespace Pathfinding {
	using Pathfinding.Util;
	using Pathfinding.Drawing;
	using Pathfinding.Pooling;

	/// <summary>
	/// Interface for handling off-mesh links.
	///
	/// If a component implements this interface, and is attached to the same GameObject as a NodeLink2 component,
	/// then the OnTraverseOffMeshLink method will be called when an agent traverses the off-mesh link.
	///
	/// This only works with the <see cref="FollowerEntity"/> component.
	///
	/// Note: <see cref="FollowerEntity.onTraverseOffMeshLink"/> overrides this callback, if set.
	///
	/// The <see cref="Interactable"/> component implements this interface, and allows a small state-machine to run when the agent traverses the link.
	///
	/// See: <see cref="FollowerEntity.onTraverseOffMeshLink"/>
	/// See: offmeshlinks (view in online documentation for working links)
	/// </summary>
	public interface IOffMeshLinkHandler {
		/// <summary>
		/// Name of the handler.
		/// This is used to identify the handler in the inspector.
		/// </summary>
		public string name => null;

#if MODULE_ENTITIES
		/// <summary>
		/// Called when an agent starts traversing an off-mesh link.
		///
		/// This can be used to perform any setup that is necessary for the traversal.
		///
		/// Returns: An object which will be used to control the agent for the full duration of the link traversal.
		///
		/// For simple cases, you can often implement <see cref="IOffMeshLinkHandler"/> and <see cref="IOffMeshLinkStateMachine"/> in the same class, and then
		/// just make this method return this.
		/// For more complex cases, you may want to keep track of the agent's identity, and thus want to return a new object for each agent that traverses the link.
		/// </summary>
		/// <param name="context">Context for the traversal. Provides information about the link and the agent, as well as some helper methods for movement.
		/// This context is only valid for this method call. Do not store it and use it later.</param>
		public IOffMeshLinkStateMachine GetOffMeshLinkStateMachine(ECS.AgentOffMeshLinkTraversalContext context);
#endif
	}

	public interface IOffMeshLinkStateMachine {
#if MODULE_ENTITIES
		/// <summary>
		/// Called when an agent traverses an off-mesh link.
		/// This method should be a coroutine (i.e return an IEnumerable) which will be iterated over until it finishes, or the agent is destroyed.
		/// The coroutine should yield null every frame until the agent has finished traversing the link.
		///
		/// When the coroutine completes, the agent will be assumed to have reached the end of the link and control
		/// will be returned to the normal movement code.
		///
		/// The coroutine typically moves the agent to the end of the link over some time, and perform any other actions that are necessary.
		/// For example, it could play an animation, or move the agent in a specific way.
		/// </summary>
		/// <param name="context">Context for the traversal. Provides information about the link and the agent, as well as some helper methods for movement.
		/// This context is only valid when this coroutine steps forward. Do not store it and use it elsewhere.</param>
		System.Collections.IEnumerable OnTraverseOffMeshLink(ECS.AgentOffMeshLinkTraversalContext context) => ECS.JobStartOffMeshLinkTransition.DefaultOnTraverseOffMeshLink(context);

		/// <summary>
		/// Called when an agent finishes traversing an off-mesh link.
		///
		/// This can be used to perform any cleanup that is necessary after the traversal.
		///
		/// Either <see cref="OnFinishTraversingOffMeshLink"/> or <see cref="OnAbortTraversingOffMeshLink"/> will be called, but not both.
		/// </summary>
		/// <param name="context">Context for the traversal. Provides information about the link and the agent, as well as some helper methods for movement.
		/// This context is only valid for this method call. Do not store it and use it later.</param>
		void OnFinishTraversingOffMeshLink (ECS.AgentOffMeshLinkTraversalContext context) {}
#endif

		/// <summary>
		/// Called when an agent fails to finish traversing an off-mesh link.
		///
		/// This can be used to perform any cleanup that is necessary after the traversal.
		///
		/// An abort can happen if the agent was destroyed while it was traversing the link. It can also happen if the agent was teleported somewhere else while traversing the link.
		///
		/// Either <see cref="OnFinishTraversingOffMeshLink"/> or <see cref="OnAbortTraversingOffMeshLink"/> will be called, but not both.
		///
		/// Warning: When this is called, the agent may already be destroyed. The handler component itself could also be destroyed at this point.
		/// </summary>
		void OnAbortTraversingOffMeshLink () {}
	}

	/// <summary>
	/// Connects two nodes using an off-mesh link.
	/// In contrast to the <see cref="NodeLink"/> component, this link type will not connect the nodes directly
	/// instead it will create two link nodes at the start and end position of this link and connect
	/// through those nodes.
	///
	/// If the closest node to this object is called A and the closest node to the end transform is called
	/// D, then it will create one link node at this object's position (call it B) and one link node at
	/// the position of the end transform (call it C), it will then connect A to B, B to C and C to D.
	///
	/// This link type is possible to detect while following since it has these special link nodes in the middle.
	/// The link corresponding to one of those intermediate nodes can be retrieved using the <see cref="GetNodeLink"/> method
	/// which can be of great use if you want to, for example, play a link specific animation when reaching the link.
	///
	/// \inspectorField{End, NodeLink2.end}
	/// \inspectorField{Cost Factor, NodeLink2.costFactor}
	/// \inspectorField{One Way, NodeLink2.oneWay}
	/// \inspectorField{Pathfinding Tag, NodeLink2.pathfindingTag}
	/// \inspectorField{Graph Mask, NodeLink2.graphMask}
	///
	/// See: offmeshlinks (view in online documentation for working links)
	/// See: The example scene RecastExample2 contains a few links which you can take a look at to see how they are used.
	///
	/// Note: If you make any modifications to the node link's settings after it has been created, you need to call the <see cref="Apply"/> method in order to apply the changes to the graph.
	/// </summary>
	[AddComponentMenu("Pathfinding/Link2")]
	[HelpURL("https://arongranberg.com/astar/documentation/stable/nodelink2.html")]
	public class NodeLink2 : GraphModifier {
		/// <summary>End position of the link</summary>
		public Transform end;

		/// <summary>
		/// The connection will be this times harder/slower to traverse.
		///
		/// A cost factor of 1 means that the link is equally expensive as moving the same distance on the normal navmesh. But a cost factor greater than 1 means that it is proportionally more expensive.
		///
		/// You should not use a cost factor less than 1 unless you also change the <see cref="AstarPath.heuristicScale"/> field (A* Inspector -> Settings -> Pathfinding) to at most the minimum cost factor that you use anywhere in the scene (or disable the heuristic altogether).
		/// This is because the pathfinding algorithm assumes that paths are at least as costly as walking just the straight line distance to the target, and if you use a cost factor less than 1, that assumption is no longer true.
		/// What then happens is that the pathfinding search may ignore some links because it doesn't even think to search in that direction, even if they would have lead to a lower path cost.
		///
		/// Warning: Reducing the heuristic scale or disabling the heuristic can significantly increase the cpu cost for pathfinding, especially for large graphs.
		///
		/// Read more about this at https://en.wikipedia.org/wiki/Admissible_heuristic.
		/// </summary>
		public float costFactor = 1.0f;

		/// <summary>Make a one-way connection</summary>
		public bool oneWay = false;

		/// <summary>
		/// The tag to apply to the link.
		///
		/// This can be used to exclude certain agents from using the link, or make it more expensive to use.
		///
		/// See: tags (view in online documentation for working links)
		/// </summary>
		public PathfindingTag pathfindingTag = 0;

		/// <summary>
		/// Which graphs this link is allowed to connect.
		///
		/// The link will always connect the nodes closest to the start and end points on the graphs that it is allowed to connect.
		/// </summary>
		public GraphMask graphMask = -1;

		public Transform StartTransform => transform;

		public Transform EndTransform => end;

		protected OffMeshLinks.OffMeshLinkSource linkSource;

		/// <summary>
		/// Returns the link component associated with the specified node.
		/// Returns: The link associated with the node or null if the node is not a link node, or it is not associated with a <see cref="NodeLink2"/> component.
		/// </summary>
		/// <param name="node">The node to get the link for.</param>
		public static NodeLink2 GetNodeLink(GraphNode node) => node is LinkNode linkNode ? linkNode.linkSource.component as NodeLink2 : null;

		/// <summary>
		/// True if the link is connected to the graph.
		///
		/// This will be true if the link has been successfully connected to the graph, and false if it either has failed, or if the component/gameobject is disabled.
		///
		/// When the component is enabled, the link will be scheduled to be added to the graph, it will not take effect immediately.
		/// This means that this property will return false until the next time graph updates are processed (usually later this frame, or next frame).
		/// To ensure the link is refreshed immediately, you can call <see cref="AstarPath.active.FlushWorkItems"/>.
		/// </summary>
		internal bool isActive => linkSource != null && (linkSource.status & OffMeshLinks.OffMeshLinkStatus.Active) != 0;

		IOffMeshLinkHandler onTraverseOffMeshLinkHandler;

		/// <summary>
		/// Callback to be called when an agent starts traversing an off-mesh link.
		///
		/// The handler will be called when the agent starts traversing an off-mesh link.
		/// It allows you to to control the agent for the full duration of the link traversal.
		///
		/// Use the passed context struct to get information about the link and to control the agent.
		///
		/// <code>
		/// using UnityEngine;
		/// using Pathfinding;
		/// using System.Collections;
		/// using Pathfinding.ECS;
		///
		/// namespace Pathfinding.Examples {
		///     public class FollowerJumpLink : MonoBehaviour, IOffMeshLinkHandler, IOffMeshLinkStateMachine {
		///         // Register this class as the handler for off-mesh links when the component is enabled
		///         void OnEnable() => GetComponent<NodeLink2>().onTraverseOffMeshLink = this;
		///         void OnDisable() => GetComponent<NodeLink2>().onTraverseOffMeshLink = null;
		///
		///         IOffMeshLinkStateMachine IOffMeshLinkHandler.GetOffMeshLinkStateMachine(AgentOffMeshLinkTraversalContext context) => this;
		///
		///         void IOffMeshLinkStateMachine.OnFinishTraversingOffMeshLink (AgentOffMeshLinkTraversalContext context) {
		///             Debug.Log("An agent finished traversing an off-mesh link");
		///         }
		///
		///         void IOffMeshLinkStateMachine.OnAbortTraversingOffMeshLink () {
		///             Debug.Log("An agent aborted traversing an off-mesh link");
		///         }
		///
		///         IEnumerable IOffMeshLinkStateMachine.OnTraverseOffMeshLink (AgentOffMeshLinkTraversalContext ctx) {
		///             var start = (Vector3)ctx.link.relativeStart;
		///             var end = (Vector3)ctx.link.relativeEnd;
		///             var dir = end - start;
		///
		///             // Disable local avoidance while traversing the off-mesh link.
		///             // If it was enabled, it will be automatically re-enabled when the agent finishes traversing the link.
		///             ctx.DisableLocalAvoidance();
		///
		///             // Move and rotate the agent to face the other side of the link.
		///             // When reaching the off-mesh link, the agent may be facing the wrong direction.
		///             while (!ctx.MoveTowards(
		///                 position: start,
		///                 rotation: Quaternion.LookRotation(dir, ctx.movementPlane.up),
		///                 gravity: true,
		///                 slowdown: true).reached) {
		///                 yield return null;
		///             }
		///
		///             var bezierP0 = start;
		///             var bezierP1 = start + Vector3.up*5;
		///             var bezierP2 = end + Vector3.up*5;
		///             var bezierP3 = end;
		///             var jumpDuration = 1.0f;
		///
		///             // Animate the AI to jump from the start to the end of the link
		///             for (float t = 0; t < jumpDuration; t += ctx.deltaTime) {
		///                 ctx.transform.Position = AstarSplines.CubicBezier(bezierP0, bezierP1, bezierP2, bezierP3, Mathf.SmoothStep(0, 1, t / jumpDuration));
		///                 yield return null;
		///             }
		///         }
		///     }
		/// }
		/// </code>
		///
		/// Warning: Off-mesh links can be destroyed or disabled at any moment. The built-in code will attempt to make the agent continue following the link even if it is destroyed,
		/// but if you write your own traversal code, you should be aware of this.
		///
		/// You can alternatively set the corresponding property property on the agent (<see cref="FollowerEntity.onTraverseOffMeshLink"/>) to specify a callback for a all off-mesh links.
		///
		/// Note: The agent's off-mesh link handler takes precedence over the link's off-mesh link handler, if both are set.
		///
		/// Warning: This property only works with the <see cref="FollowerEntity"/> component. Use <see cref="RichAI.onTraverseOffMeshLink"/> if you are using the <see cref="RichAI"/> movement script.
		///
		/// See: offmeshlinks (view in online documentation for working links) for more details and example code
		/// </summary>
		public IOffMeshLinkHandler onTraverseOffMeshLink {
			get => onTraverseOffMeshLinkHandler;
			set {
				onTraverseOffMeshLinkHandler = value;
				if (linkSource != null) linkSource.handler = value;
			}
		}

		public override void OnPostScan () {
			TryAddLink();
		}

		protected override void OnEnable () {
			base.OnEnable();
			if (Application.isPlaying && !BatchedEvents.Has(this)) BatchedEvents.Add(this, BatchedEvents.Event.Update, OnUpdate);
			TryAddLink();
		}

		static void OnUpdate (NodeLink2[] components, int count) {
			// Only check for moved links every N frames, for performance
			if ((Time.frameCount % 16) != 0) return;

			for (int i = 0; i < count; i++) {
				var comp = components[i];
				var start = comp.StartTransform;
				var end = comp.EndTransform;
				var added = comp.linkSource != null;
				if ((start != null && end != null) != added || (added && (start.hasChanged || end.hasChanged))) {
					if (start != null) start.hasChanged = false;
					if (end != null) end.hasChanged = false;
					comp.RemoveLink();
					comp.TryAddLink();
				}
			}
		}

		void TryAddLink () {
			// In case the AstarPath component has been destroyed (destroying the link).
			// But do not clear it if the link is inactive because it failed to become enabled
			if (linkSource != null && (linkSource.status == OffMeshLinks.OffMeshLinkStatus.Inactive || (linkSource.status & OffMeshLinks.OffMeshLinkStatus.PendingRemoval) != 0)) linkSource = null;

			if (linkSource == null && AstarPath.active != null && EndTransform != null) {
				StartTransform.hasChanged = false;
				EndTransform.hasChanged = false;

				linkSource = new OffMeshLinks.OffMeshLinkSource {
					start = new OffMeshLinks.Anchor {
						center = StartTransform.position,
						rotation = StartTransform.rotation,
						width = 0f,
					},
					end = new OffMeshLinks.Anchor {
						center = EndTransform.position,
						rotation = EndTransform.rotation,
						width = 0f,
					},
					directionality = oneWay ? OffMeshLinks.Directionality.OneWay : OffMeshLinks.Directionality.TwoWay,
					tag = pathfindingTag,
					costFactor = costFactor,
					graphMask = graphMask,
					maxSnappingDistance = 1, // TODO
					component = this,
					handler = onTraverseOffMeshLink,
				};
				AstarPath.active.offMeshLinks.Add(linkSource);
			}
		}

		void RemoveLink () {
			if (AstarPath.active != null && linkSource != null) AstarPath.active.offMeshLinks.Remove(linkSource);
			linkSource = null;
		}

		protected override void OnDisable () {
			base.OnDisable();
			BatchedEvents.Remove(this);
			RemoveLink();
		}

		[ContextMenu("Recalculate neighbours")]
		void ContextApplyForce () {
			Apply();
		}

		/// <summary>
		/// Disconnects and then reconnects the link to the graph.
		///
		/// If you have moved the link or otherwise modified it you need to call this method to apply those changes.
		/// </summary>
		public virtual void Apply () {
			RemoveLink();
			TryAddLink();
		}

		private readonly static Color GizmosColor = new Color(206.0f/255.0f, 136.0f/255.0f, 48.0f/255.0f, 0.5f);
		private readonly static Color GizmosColorSelected = new Color(235.0f/255.0f, 123.0f/255.0f, 32.0f/255.0f, 1.0f);

		public override void DrawGizmos () {
			if (StartTransform == null || EndTransform == null) return;

			var startPos = StartTransform.position;
			var endPos = EndTransform.position;
			if (linkSource != null && (Time.renderedFrameCount % 16) == 0 && Application.isEditor) {
				// Check if the link has moved
				// During runtime, this will be done by the OnUpdate method instead
				if (linkSource.start.center != startPos || linkSource.end.center != endPos || linkSource.directionality != (oneWay ? OffMeshLinks.Directionality.OneWay : OffMeshLinks.Directionality.TwoWay) || linkSource.costFactor != costFactor || linkSource.graphMask != graphMask || linkSource.tag != pathfindingTag) {
					Apply();
				}
			}

			bool selected = GizmoContext.InActiveSelection(this);
			var graphs = linkSource != null && AstarPath.active != null? AstarPath.active.offMeshLinks.ConnectedGraphs(linkSource) : null;
			var up = Vector3.up;

			// Find the natural up direction of the connected graphs, so that we can orient the gizmos appropriately
			if (graphs != null) {
				for (int i = 0; i < graphs.Count; i++) {
					var graph = graphs[i];
					if (graph != null) {
						if (graph is NavmeshBase navmesh) {
							up = navmesh.transform.WorldUpAtGraphPosition(Vector3.zero);
							break;
						} else if (graph is GridGraph grid) {
							up = grid.transform.WorldUpAtGraphPosition(Vector3.zero);
							break;
						}
					}
				}
				ListPool<NavGraph>.Release(ref graphs);
			}

			var active = linkSource != null && linkSource.status == OffMeshLinks.OffMeshLinkStatus.Active;
			Color color = selected ? GizmosColorSelected : GizmosColor;
			if (active) color = Color.green;

			Draw.Circle(startPos, up, 0.4f, linkSource != null && linkSource.status.HasFlag(OffMeshLinks.OffMeshLinkStatus.FailedToConnectStart) ? Color.red : color);
			Draw.Circle(endPos, up, 0.4f, linkSource != null && linkSource.status.HasFlag(OffMeshLinks.OffMeshLinkStatus.FailedToConnectEnd) ? Color.red : color);

			NodeLink.DrawArch(startPos, endPos, up, color);
			if (selected) {
				Vector3 cross = Vector3.Cross(up, endPos-startPos).normalized;
				using (Draw.WithLineWidth(2)) {
					NodeLink.DrawArch(startPos+cross*0.0f, endPos+cross*0.0f, up, color);
				}
				// NodeLink.DrawArch(startPos-cross*0.1f, endPos-cross*0.1f, color);
			}
		}
	}
}
