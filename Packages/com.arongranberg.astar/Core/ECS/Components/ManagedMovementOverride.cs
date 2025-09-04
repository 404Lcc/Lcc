#if MODULE_ENTITIES
using Unity.Entities;

namespace Pathfinding.ECS {
	using Unity.Transforms;

	public delegate void BeforeControlDelegate(Entity entity, float dt, ref LocalTransform localTransform, ref AgentCylinderShape shape, ref AgentMovementPlane movementPlane, ref DestinationPoint destination, ref MovementState movementState, ref MovementSettings movementSettings);
	public delegate void AfterControlDelegate(Entity entity, float dt, ref LocalTransform localTransform, ref AgentCylinderShape shape, ref AgentMovementPlane movementPlane, ref DestinationPoint destination, ref MovementState movementState, ref MovementSettings movementSettings, ref MovementControl movementControl);
	public delegate void BeforeMovementDelegate(Entity entity, float dt, ref LocalTransform localTransform, ref AgentCylinderShape shape, ref AgentMovementPlane movementPlane, ref DestinationPoint destination, ref MovementState movementState, ref MovementSettings movementSettings, ref MovementControl movementControl, ref ResolvedMovement resolvedMovement);

	/// <summary>
	/// Helper for adding and removing hooks to the FollowerEntity component.
	/// This is used to allow other systems to override the movement of the agent.
	///
	/// See: <see cref="FollowerEntity.movementOverrides"/>
	/// </summary>
	public ref struct ManagedMovementOverrides {
		Entity entity;
		World world;

		public ManagedMovementOverrides (Entity entity, World world) {
			this.entity = entity;
			this.world = world;
		}

		public void AddBeforeControlCallback (BeforeControlDelegate value) {
			AddCallback<ManagedMovementOverrideBeforeControl, BeforeControlDelegate>(value);
		}
		public void RemoveBeforeControlCallback (BeforeControlDelegate value) {
			RemoveCallback<ManagedMovementOverrideBeforeControl, BeforeControlDelegate>(value);
		}

		public void AddAfterControlCallback (AfterControlDelegate value) {
			AddCallback<ManagedMovementOverrideAfterControl, AfterControlDelegate>(value);
		}
		public void RemoveAfterControlCallback (AfterControlDelegate value) {
			RemoveCallback<ManagedMovementOverrideAfterControl, AfterControlDelegate>(value);
		}

		public void AddBeforeMovementCallback (BeforeMovementDelegate value) {
			AddCallback<ManagedMovementOverrideBeforeMovement, BeforeMovementDelegate>(value);
		}
		public void RemoveBeforeMovementCallback (BeforeMovementDelegate value) {
			RemoveCallback<ManagedMovementOverrideBeforeMovement, BeforeMovementDelegate>(value);
		}

		void AddCallback<C, T>(T callback) where T : System.Delegate where C : ManagedMovementOverride<T>, IComponentData, new() {
			if (callback == null) throw new System.ArgumentNullException(nameof(callback));
			if (world == null || !world.EntityManager.Exists(entity)) throw new System.InvalidOperationException("The entity does not exist. You can only set a callback when the FollowerEntity is active and has been enabled. If you are trying to set this during Awake or OnEnable, try setting it during Start instead.");
			if (!world.EntityManager.HasComponent<C>(entity)) world.EntityManager.AddComponentData(entity, new C());
			world.EntityManager.GetComponentData<C>(entity).AddCallback(callback);
		}

		void RemoveCallback<C, T>(T callback) where T : System.Delegate where C : ManagedMovementOverride<T>, IComponentData, new() {
			if (callback == null) throw new System.ArgumentNullException(nameof(callback));
			if (world == null || !world.EntityManager.Exists(entity)) return;
			if (!world.EntityManager.HasComponent<C>(entity)) return;

			var comp = world.EntityManager.GetComponentData<C>(entity);
			if (!comp.RemoveCallback(callback)) {
				world.EntityManager.RemoveComponent<C>(entity);
			}
		}
	}

	/// <summary>
	/// Stores a delegate that can be used to override movement control and movement settings for a specific entity.
	/// This is used by the FollowerEntity to allow other systems to override the movement of the entity.
	///
	/// See: <see cref="FollowerEntity.movementOverrides"/>
	/// </summary>
	public class ManagedMovementOverride<T> : IComponentData where T : class, System.Delegate {
		public T callback;

		public void AddCallback(T callback) => this.callback = (T)System.Delegate.Combine(this.callback, callback);
		public bool RemoveCallback(T callback) => (this.callback = (T)System.Delegate.Remove(this.callback, callback)) != null;
	}

	// IJobEntity does not support generic jobs yet, so we have to make concrete component types for each delegate type
	public class ManagedMovementOverrideBeforeControl : ManagedMovementOverride<BeforeControlDelegate>, System.ICloneable {
		// No fields in this class can be cloned safely
		public object Clone() => new ManagedMovementOverrideBeforeControl();
	}
	public class ManagedMovementOverrideAfterControl : ManagedMovementOverride<AfterControlDelegate> {
		public object Clone() => new ManagedMovementOverrideAfterControl();
	}
	public class ManagedMovementOverrideBeforeMovement : ManagedMovementOverride<BeforeMovementDelegate> {
		public object Clone() => new ManagedMovementOverrideBeforeMovement();
	}
}
#endif
