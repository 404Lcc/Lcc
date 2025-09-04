#if MODULE_ENTITIES
using Unity.Entities;
using Unity.Mathematics;

namespace Pathfinding.ECS {
	using Pathfinding;
	using Pathfinding.Util;

	/// <summary>
	/// Desired movement for an agent.
	/// This data will be fed to the local avoidance system to calculate the final movement of the agent.
	/// If no local avoidance is used, it will be directly copied to <see cref="ResolvedMovement"/>.
	///
	/// See: <see cref="ResolvedMovement"/>
	/// </summary>
	public struct MovementControl : IComponentData {
		/// <summary>The point the agent should move towards</summary>
		public float3 targetPoint;

		/// <summary>
		/// The end of the current path.
		///
		/// This informs the local avoidance system about the final desired destination for the agent.
		/// This is used to make agents stop if the destination is crowded and it cannot reach its destination.
		///
		/// If this is not set, agents will often move forever around a crowded destination, always trying to find
		/// some way to get closer, but never finding it.
		/// </summary>
		public float3 endOfPath;

		/// <summary>The speed at which the agent should move towards <see cref="targetPoint"/>, in meters per second</summary>
		public float speed;

		/// <summary>
		/// The maximum speed at which the agent may move, in meters per second.
		///
		/// It is recommended to keep this slightly above <see cref="speed"/>, to allow the local avoidance system to move agents around more efficiently when necessary.
		/// </summary>
		public float maxSpeed;

		/// <summary>
		/// The index of the hierarchical node that the agent is currently in.
		/// Will be -1 if the hierarchical node index is not known.
		/// See: <see cref="HierarchicalGraph"/>
		///
		/// See: <see cref="MovementState.hierarchicalNodeIndex"/>
		/// </summary>
		public int hierarchicalNodeIndex;

		/// <summary>
		/// The desired rotation of the agent, in radians, relative to the current movement plane.
		/// See: <see cref="NativeMovementPlane.ToWorldRotation"/>
		/// </summary>
		public float targetRotation;

		/// <summary>
		/// The desired rotation of the agent, in radians, over a longer time horizon, relative to the current movement plane.
		///
		/// The <see cref="targetRotation"/> is usually only over a very short time-horizon, usually a single simulation time step.
		/// This variable is used to provide a hint of where the agent wants to rotate to over a slightly longer time scale (on the order of a second or so).
		/// It is not used to control movement directly, but it may be used to guide animations, or rotation smoothing.
		///
		/// If no better hint is available, this should be set to the same value as <see cref="targetRotation"/>.
		///
		/// See: <see cref="NativeMovementPlane.ToWorldRotation"/>
		/// </summary>
		public float targetRotationHint;

		/// <summary>
		/// Additive modifier to <see cref="targetRotation"/>, in radians.
		/// This is used by the local avoidance system to rotate the agent, without this causing a feedback loop.
		/// This extra rotation will be ignored by the control system which decides how the agent *wants* to move.
		/// It will instead be directly applied to the agent.
		/// </summary>
		public float targetRotationOffset;

		/// <summary>The speed at which the agent should rotate towards <see cref="targetRotation"/> + <see cref="targetRotationOffset"/>, in radians per second</summary>
		public float rotationSpeed;

		/// <summary>
		/// If true, this agent will ignore other agents during local avoidance, but other agents will still avoid this one.
		/// This is useful for example for a player character which should not avoid other agents, but other agents should avoid the player.
		/// </summary>
		public bool overrideLocalAvoidance;
	}
}
#endif
