#if MODULE_ENTITIES
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Pathfinding.ECS {
	using Pathfinding;
	using Pathfinding.Util;
	using Unity.Collections.LowLevel.Unsafe;
	using UnityEngine;

	/// <summary>
	/// Holds unmanaged information about an off-mesh link that the agent is currently traversing.
	/// This component is added to the agent when it starts traversing an off-mesh link.
	/// It is removed when the agent has finished traversing the link.
	///
	/// See: <see cref="ManagedAgentOffMeshLinkTraversal"/>
	/// </summary>
	public struct AgentOffMeshLinkTraversal : IComponentData {
		/// <summary>\copydocref{OffMeshLinks.OffMeshLinkTracer.relativeStart}</summary>
		public float3 relativeStart;

		/// <summary>\copydocref{OffMeshLinks.OffMeshLinkTracer.relativeEnd}</summary>
		public float3 relativeEnd;

		/// <summary>\copydocref{OffMeshLinks.OffMeshLinkTracer.relativeStart}. Deprecated: Use relativeStart instead</summary>
		[System.Obsolete("Use relativeStart instead")]
		public float3 firstPosition => relativeStart;

		/// <summary>\copydocref{OffMeshLinks.OffMeshLinkTracer.relativeEnd}. Deprecated: Use relativeEnd instead</summary>
		[System.Obsolete("Use relativeEnd instead")]
		public float3 secondPosition => relativeEnd;

		/// <summary>\copydocref{OffMeshLinks.OffMeshLinkTracer.isReverse}</summary>
		public bool isReverse;

		public AgentOffMeshLinkTraversal (OffMeshLinks.OffMeshLinkTracer linkInfo) {
			relativeStart = linkInfo.relativeStart;
			relativeEnd = linkInfo.relativeEnd;
			isReverse = linkInfo.isReverse;
		}
	}

	/// <summary>
	/// Holds managed information about an off-mesh link that the agent is currently traversing.
	/// This component is added to the agent when it starts traversing an off-mesh link.
	/// It is removed when the agent has finished traversing the link.
	///
	/// See: <see cref="AgentOffMeshLinkTraversal"/>
	/// </summary>
	public class ManagedAgentOffMeshLinkTraversal : IComponentData, System.ICloneable, ICleanupComponentData {
		/// <summary>Internal context used to pass component data to the coroutine</summary>
		public AgentOffMeshLinkTraversalContext context;

		/// <summary>Coroutine which is used to traverse the link</summary>
		public System.Collections.IEnumerator coroutine;
		public IOffMeshLinkHandler handler;
		public IOffMeshLinkStateMachine stateMachine;

		public ManagedAgentOffMeshLinkTraversal() {}

		public ManagedAgentOffMeshLinkTraversal (AgentOffMeshLinkTraversalContext context, IOffMeshLinkHandler handler) {
			this.context = context;
			this.handler = handler;
			this.coroutine = null;
			this.stateMachine = null;
		}

		public object Clone () {
			// This will set coroutine and stateMachine to null.
			// This is correct, as the coroutine cannot be cloned, and the state machine may be unique for a specific agent
			return new ManagedAgentOffMeshLinkTraversal((AgentOffMeshLinkTraversalContext)context.Clone(), handler);
		}
	}

	public struct MovementTarget {
		internal bool isReached;
		public bool reached => isReached;

		public MovementTarget (bool isReached) {
			this.isReached = isReached;
		}
	}

	/// <summary>
	/// Context with helpers for traversing an off-mesh link.
	///
	/// This will be passed to the code that is responsible for traversing the off-mesh link.
	///
	/// Warning: This context should never be accessed outside of an implementation of the <see cref="IOffMeshLinkStateMachine"/> interface.
	/// </summary>
	public class AgentOffMeshLinkTraversalContext : System.ICloneable {
		internal unsafe AgentOffMeshLinkTraversal* linkInfoPtr;
		internal unsafe MovementControl* movementControlPtr;
		internal unsafe MovementSettings* movementSettingsPtr;
		internal unsafe LocalTransform* transformPtr;
		internal unsafe AgentMovementPlane* movementPlanePtr;
		internal EnabledRefRW<AgentOffMeshLinkMovementDisabled> movementDisabled;

		/// <summary>The entity that is traversing the off-mesh link</summary>
		public Entity entity;

		/// <summary>Some internal state of the agent</summary>
		[Unity.Properties.DontCreateProperty]
		public ManagedState managedState;

		/// <summary>
		/// The off-mesh link that is being traversed.
		///
		/// See: <see cref="link"/>
		/// </summary>
		[Unity.Properties.DontCreateProperty]
		internal OffMeshLinks.OffMeshLinkConcrete concreteLink;

		protected bool disabledRVO;
		protected float backupRotationSmoothing = float.NaN;

		/// <summary>
		/// Delta time since the last link simulation.
		///
		/// During high time scales, the simulation may run multiple substeps per frame.
		///
		/// This is not the same as Time.deltaTime. Inside the link coroutine, you should always use this field instead of Time.deltaTime.
		/// </summary>
		public float deltaTime;

		protected GameObject gameObjectCache;

		/// <summary>
		/// GameObject associated with the agent.
		///
		/// In most cases, an agent is associated with an agent, but this is not always the case.
		/// For example, if you have created an entity without using the <see cref="FollowerEntity"/> component, this property may return null.
		///
		/// Note: When directly modifying the agent's transform during a link traversal, you should use the <see cref="transform"/> property instead of modifying the GameObject's transform.
		/// </summary>
		public virtual GameObject gameObject {
			get {
				if (gameObjectCache == null) {
					var follower = BatchedEvents.Find<FollowerEntity, Entity>(entity, (follower, entity) => follower.entity == entity);
					if (follower != null) gameObjectCache = follower.gameObject;
				}
				return gameObjectCache;
			}
		}

		/// <summary>ECS LocalTransform component attached to the agent</summary>
		public ref LocalTransform transform {
			get {
				unsafe {
					return ref *transformPtr;
				}
			}
		}

		/// <summary>The movement settings for the agent</summary>
		public ref MovementSettings movementSettings {
			get {
				unsafe {
					return ref *movementSettingsPtr;
				}
			}
		}

		/// <summary>
		/// How the agent should move.
		///
		/// The agent will move according to this data, every frame, if <see cref="enableBuiltInMovement"/> is enabled.
		///
		/// Note: <see cref="enableBuiltInMovement"/> needs to be enabled every tick to allow the agent to move.
		/// </summary>
		public ref MovementControl movementControl {
			get {
				unsafe {
					return ref *movementControlPtr;
				}
			}
		}

		/// <summary>Information about the off-mesh link that the agent is traversing</summary>
		public OffMeshLinks.OffMeshLinkTracer link {
			get {
				unsafe {
					return new OffMeshLinks.OffMeshLinkTracer(concreteLink, linkInfoPtr->relativeStart, linkInfoPtr->relativeEnd, linkInfoPtr->isReverse);
				}
			}
		}

		/// <summary>
		/// Information about the off-mesh link that the agent is traversing.
		///
		/// Deprecated: Use the <see cref="link"/> property instead
		/// </summary>
		[System.Obsolete("Use the link property instead")]
		public AgentOffMeshLinkTraversal linkInfo {
			get {
				unsafe {
					return *linkInfoPtr;
				}
			}
		}

		/// <summary>
		/// The plane in which the agent is moving.
		///
		/// In a 3D game, this will typically be the XZ plane, but in a 2D game
		/// it will typically be the XY plane. Games on spherical planets could have planes that are aligned with the surface of the planet.
		/// </summary>
		public ref NativeMovementPlane movementPlane {
			get {
				unsafe {
					return ref movementPlanePtr->value;
				}
			}
		}

		/// <summary>
		/// True if the agent's built-in movement logic should be enabled.
		///
		/// When traversing an off-mesh link, you typically want the agent's movement to be completely controlled by an animation, or some other code.
		/// However, sometimes you may want to use the built-in movement logic to move the agent.
		///
		/// Using the <see cref="MoveTowards"/> method will automatically enable the agent's movement logic during that frame.
		///
		/// Note: This will be reset to false every frame. Right before the off-mesh link traversal coroutine is executed.
		///
		/// See: <see cref="MoveTowards"/>
		/// </summary>
		public bool enableBuiltInMovement {
			get => movementDisabled.IsValid ? !movementDisabled.ValueRW : true;
			set {
				if (movementDisabled.IsValid) movementDisabled.ValueRW = !value;
			}
		}

		public AgentOffMeshLinkTraversalContext (OffMeshLinks.OffMeshLinkConcrete link) {
			this.concreteLink = link;
		}

		/// <summary>
		/// Internal method to set the data of the context.
		///
		/// This is used by the job system to set the data of the context.
		/// You should almost never need to use this.
		/// </summary>
		public virtual unsafe void SetInternalData (Entity entity, ref LocalTransform transform, ref AgentMovementPlane movementPlane, ref MovementControl movementControl, ref MovementSettings movementSettings, ref AgentOffMeshLinkTraversal linkInfo, EnabledRefRW<AgentOffMeshLinkMovementDisabled> movementDisabled, ManagedState state, float deltaTime) {
			this.linkInfoPtr = (AgentOffMeshLinkTraversal*)UnsafeUtility.AddressOf(ref linkInfo);
			this.movementControlPtr = (MovementControl*)UnsafeUtility.AddressOf(ref movementControl);
			this.movementSettingsPtr = (MovementSettings*)UnsafeUtility.AddressOf(ref movementSettings);
			this.transformPtr = (LocalTransform*)UnsafeUtility.AddressOf(ref transform);
			this.movementPlanePtr = (AgentMovementPlane*)UnsafeUtility.AddressOf(ref movementPlane);
			this.movementDisabled = movementDisabled;
			this.managedState = state;
			this.deltaTime = deltaTime;
			this.entity = entity;
		}

		/// <summary>
		/// Disables local avoidance for the agent.
		///
		/// Agents that traverse links are already marked as 'unstoppable' by the local avoidance system,
		/// but calling this method will make other agents ignore them completely while traversing the link.
		/// </summary>
		public void DisableLocalAvoidance () {
			if (managedState.enableLocalAvoidance) {
				disabledRVO = true;
				managedState.enableLocalAvoidance = false;
			}
		}

		/// <summary>
		/// Disables rotation smoothing for the agent.
		///
		/// This disables the effect of <see cref="MovementSettings.rotationSmoothing"/> while the agent is traversing the link.
		/// Having rotation smoothing enabled can make the agent rotate towards its target rotation more slowly,
		/// which is sometimes not desirable.
		///
		/// Rotation smoothing will automatically be restored when the agent finishes traversing the link (if it was enabled before).
		///
		/// The <see cref="MoveTowards"/> method automatically disables rotation smoothing when called.
		/// </summary>
		public void DisableRotationSmoothing () {
			if (float.IsNaN(backupRotationSmoothing) && movementSettings.rotationSmoothing > 0) {
				backupRotationSmoothing = movementSettings.rotationSmoothing;
				movementSettings.rotationSmoothing = 0;
			}
		}

		/// <summary>
		/// Restores the agent's settings to what it was before the link traversal started.
		///
		/// This undos the changes made by <see cref="DisableLocalAvoidance"/> and <see cref="DisableRotationSmoothing"/>.
		///
		/// This method is automatically called when the agent finishes traversing the link.
		/// </summary>
		public virtual void Restore () {
			if (disabledRVO) {
				managedState.enableLocalAvoidance = true;
				disabledRVO = false;
			}
			if (!float.IsNaN(backupRotationSmoothing)) {
				movementSettings.rotationSmoothing = backupRotationSmoothing;
				backupRotationSmoothing = float.NaN;
			}
		}

		/// <summary>Teleports the agent to the given position</summary>
		public virtual void Teleport (float3 position) {
			transform.Position = position;
		}

		/// <summary>
		/// Thrown when the off-mesh link traversal should be aborted.
		///
		/// See: <see cref="AgentOffMeshLinkTraversalContext.Abort"/>
		/// </summary>
		public class AbortOffMeshLinkTraversal : System.Exception {}

		/// <summary>
		/// Aborts traversing the off-mesh link.
		///
		/// This will immediately stop your off-mesh link traversal coroutine.
		///
		/// This is useful if your agent was traversing an off-mesh link, but you have detected that it cannot continue.
		/// Maybe the ladder it was climbing was destroyed, or the bridge it was walking on collapsed.
		///
		/// Note: If you instead want to immediately make the agent move to the end of the link, you can call <see cref="Teleport"/>, and then use 'yield break;' from your coroutine.
		/// </summary>
		/// <param name="teleportToStart">If true, the agent will be teleported back to the start of the link (from the perspective of the agent). Its rotation will remain unchanged.</param>
		public virtual void Abort (bool teleportToStart = true) {
			if (teleportToStart) Teleport(link.relativeStart);
			// Cancel the current path, as otherwise the agent will instantly try to traverse the off-mesh link again.
			managedState.pathTracer.RemoveAllButFirstNode(movementPlane, managedState.pathfindingSettings.traversalProvider);
			throw new AbortOffMeshLinkTraversal();
		}

		/// <summary>
		/// Move towards a point while ignoring the navmesh.
		/// This method should be called repeatedly until the returned <see cref="MovementTarget.reached"/> property is true.
		///
		/// Returns: A <see cref="MovementTarget"/> struct which can be used to check if the target has been reached.
		///
		/// Note: This method completely ignores the navmesh. It also overrides local avoidance, if enabled (other agents will still avoid it, but this agent will not avoid other agents).
		///
		/// TODO: The gravity property is not yet implemented. Gravity is always applied.
		///
		/// See: For more control, you can set <see cref="movementControl"/> directly.
		/// </summary>
		/// <param name="position">The position to move towards.</param>
		/// <param name="rotation">The rotation to rotate towards.</param>
		/// <param name="gravity">If true, gravity will be applied to the agent.</param>
		/// <param name="slowdown">If true, the agent will slow down as it approaches the target.</param>
		public virtual MovementTarget MoveTowards (float3 position, quaternion rotation, bool gravity, bool slowdown) {
			// If rotation smoothing was enabled, it could cause a very slow convergence to the target rotation.
			// Therefore, we disable it here.
			// The agent will try to remove its remaining rotation smoothing offset as quickly as possible.
			// After the off-mesh link is traversed, the rotation smoothing will be automatically restored.
			DisableRotationSmoothing();

			// Make sure the agent's movement logic is enabled.
			// This will reset every tick.
			enableBuiltInMovement = true;

			var dirInPlane = movementPlane.ToPlane(position - transform.Position);
			var remainingDistance = math.length(dirInPlane);
			var maxSpeed = movementSettings.follower.Speed(slowdown ? remainingDistance : float.PositiveInfinity);
			var speed = movementSettings.follower.Accelerate(movementControl.speed, movementSettings.follower.slowdownTime, deltaTime);
			speed = math.min(speed, maxSpeed);

			var targetRot = movementPlane.ToPlane(rotation);
			var currentRot = movementPlane.ToPlane(transform.Rotation);
			var remainingRot = Mathf.Abs(AstarMath.DeltaAngle(currentRot, targetRot));
			movementControl = new MovementControl {
				targetPoint = position,
				endOfPath = position,
				speed = speed,
				maxSpeed = speed * 1.1f,
				hierarchicalNodeIndex = -1,
				overrideLocalAvoidance = true,
				targetRotation = targetRot,
				targetRotationHint = targetRot,
				targetRotationOffset = 0,
				rotationSpeed = math.radians(movementSettings.follower.rotationSpeed),
			};

			return new MovementTarget {
					   isReached = remainingDistance <= (slowdown ? 0.01f : speed * (1/30f)) && remainingRot < math.radians(1),
			};
		}

		public virtual object Clone () {
			var clone = (AgentOffMeshLinkTraversalContext)MemberwiseClone();
			clone.entity = Entity.Null;
			clone.gameObjectCache = null;
			clone.managedState = null;
			clone.movementDisabled = default;
			unsafe {
				clone.linkInfoPtr = null;
				clone.movementControlPtr = null;
				clone.movementSettingsPtr = null;
				clone.transformPtr = null;
				clone.movementPlanePtr = null;
			}
			return clone;
		}
	}
}

// ctx.MoveTowards (position, rotation, rvo = Auto | Disabled | AutoUnstoppable, gravity = auto|disabled) -> { reached() }

// MovementTarget { ... }
// while (!movementTarget.reached) {
// 	ctx.SetMovementTarget(movementTarget);
// 	yield return null;
// }
// yield return ctx.MoveTo(position, rotation)
// ctx.TeleportTo(position, rotation)
#endif
