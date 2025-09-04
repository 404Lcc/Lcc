#if MODULE_ENTITIES
/// <summary>[followerEntity.onTraverseOffMeshLink]</summary>
using UnityEngine;
using Pathfinding;
using System.Collections;
using Pathfinding.ECS;

namespace Pathfinding.Examples {
	[HelpURL("https://arongranberg.com/astar/documentation/stable/followerjumplink.html")]
	public class FollowerJumpLink : MonoBehaviour, IOffMeshLinkHandler, IOffMeshLinkStateMachine {
		// Register this class as the handler for off-mesh links when the component is enabled
		void OnEnable() => GetComponent<NodeLink2>().onTraverseOffMeshLink = this;
		void OnDisable() => GetComponent<NodeLink2>().onTraverseOffMeshLink = null;

		IOffMeshLinkStateMachine IOffMeshLinkHandler.GetOffMeshLinkStateMachine(AgentOffMeshLinkTraversalContext context) => this;

		void IOffMeshLinkStateMachine.OnFinishTraversingOffMeshLink (AgentOffMeshLinkTraversalContext context) {
			Debug.Log("An agent finished traversing an off-mesh link");
		}

		void IOffMeshLinkStateMachine.OnAbortTraversingOffMeshLink () {
			Debug.Log("An agent aborted traversing an off-mesh link");
		}

		IEnumerable IOffMeshLinkStateMachine.OnTraverseOffMeshLink (AgentOffMeshLinkTraversalContext ctx) {
			var start = (Vector3)ctx.link.relativeStart;
			var end = (Vector3)ctx.link.relativeEnd;
			var dir = end - start;

			// Disable local avoidance while traversing the off-mesh link.
			// If it was enabled, it will be automatically re-enabled when the agent finishes traversing the link.
			ctx.DisableLocalAvoidance();

			// Move and rotate the agent to face the other side of the link.
			// When reaching the off-mesh link, the agent may be facing the wrong direction.
			while (!ctx.MoveTowards(
				position: start,
				rotation: Quaternion.LookRotation(dir, ctx.movementPlane.up),
				gravity: true,
				slowdown: true).reached) {
				yield return null;
			}

			var bezierP0 = start;
			var bezierP1 = start + Vector3.up*5;
			var bezierP2 = end + Vector3.up*5;
			var bezierP3 = end;
			var jumpDuration = 1.0f;

			// Animate the AI to jump from the start to the end of the link
			for (float t = 0; t < jumpDuration; t += ctx.deltaTime) {
				ctx.transform.Position = AstarSplines.CubicBezier(bezierP0, bezierP1, bezierP2, bezierP3, Mathf.SmoothStep(0, 1, t / jumpDuration));
				yield return null;
			}
		}
	}
}
/// <summary>[followerEntity.onTraverseOffMeshLink]</summary>
#else
using UnityEngine;
namespace Pathfinding.Examples {
	[HelpURL("https://arongranberg.com/astar/documentation/stable/followerjumplink.html")]
	public class FollowerJumpLink : MonoBehaviour {}
}
#endif
