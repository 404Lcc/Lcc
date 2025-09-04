#if MODULE_ENTITIES
using Unity.Entities;
using Unity.Mathematics;

namespace Pathfinding.ECS {
	using Pathfinding;

	/// <summary>
	/// Tag component to enable syncing between an agent's Transform and the agent entity's position.
	///
	/// See: <see cref="FollowerEntity.updatePosition"/>
	/// </summary>
	public struct SyncPositionWithTransform : IComponentData {
	}

	/// <summary>
	/// Tag component to enable syncing between an agent's Transform and the agent entity's rotation.
	///
	/// See: <see cref="FollowerEntity.updateRotation"/>
	/// </summary>
	public struct SyncRotationWithTransform : IComponentData {
	}

	/// <summary>
	/// Tag component to indicate that the agent's forward direction is along the Y axis.
	///
	/// This is used to convert between the forward direction of the GameObject and the internal forward direction, which always uses +Z as forward.
	///
	/// See: <see cref="FollowerEntity.orientation"/>
	/// See: <see cref="OrientationMode"/>
	/// </summary>
	public struct OrientationYAxisForward : IComponentData {
	}
}
#endif
