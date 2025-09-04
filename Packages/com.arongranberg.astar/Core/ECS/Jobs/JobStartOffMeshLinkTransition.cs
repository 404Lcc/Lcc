#pragma warning disable 0282 // Allows the 'partial' keyword without warnings
#if MODULE_ENTITIES
using Unity.Entities;
using Unity.Mathematics;

namespace Pathfinding.ECS {
	public partial struct JobStartOffMeshLinkTransition {
		public EntityCommandBuffer commandBuffer;

		/// <summary>
		/// This is a fallback for traversing off-mesh links in case the user has not specified a custom traversal method.
		/// It is a coroutine which will move the agent from the start point of the link to the end point of the link.
		/// It will also disable RVO for the agent while traversing the link.
		/// </summary>
		public static System.Collections.Generic.IEnumerable<object> DefaultOnTraverseOffMeshLink (AgentOffMeshLinkTraversalContext ctx) {
			var linkInfo = ctx.link;
			var up = ctx.movementPlane.ToWorld(float2.zero, 1);
			var dirInPlane = ctx.movementPlane.ToWorld(ctx.movementPlane.ToPlane(linkInfo.relativeEnd - linkInfo.relativeStart), 0);
			var rot = quaternion.LookRotationSafe(dirInPlane, up);

			while (!ctx.MoveTowards(linkInfo.relativeStart, rot, true, false).reached) yield return null;

			ctx.DisableLocalAvoidance();

			while (!ctx.MoveTowards(linkInfo.relativeEnd, rot, true, false).reached) yield return null;

			// ctx.Teleport(linkInfo.endPoint);

			yield break;
		}
	}
}
#endif
