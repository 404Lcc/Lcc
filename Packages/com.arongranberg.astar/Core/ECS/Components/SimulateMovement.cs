#if MODULE_ENTITIES
using Unity.Entities;
using Unity.Mathematics;

namespace Pathfinding.ECS {
	using Pathfinding;

	/// <summary>
	/// Tag component to enable movement for an entity.
	/// Without this component, most systems will completely ignore the entity.
	///
	/// There are some more specific components that can be used to selectively enable/disable some jobs:
	/// - <see cref="SimulateMovementRepair"/>
	/// - <see cref="SimulateMovementControl"/>
	/// - <see cref="SimulateMovementFinalize"/>
	///
	/// Removing one of the above components can be useful if you want to override the movement of an agent in some way.
	/// </summary>
	public struct SimulateMovement : IComponentData {
	}

	/// <summary>
	/// Tag component to allow the agent to repair its path and recalculate various statistics.
	///
	/// Allows the <see cref="JobRepairPath"/> to run.
	///
	/// Note: <see cref="JobRepairPath"/> will, at the moment, run even if this component is not present, when manually setting some properties on the agent (e.g. ai.position, ai.destination, etc.).
	/// Create a forum post if you have a use case which isn't supported by this.
	/// </summary>
	public struct SimulateMovementRepair : IComponentData {
	}

	/// <summary>
	/// Tag component to allow the agent to calculate how it wants to move.
	///
	/// Allows the <see cref="JobControl"/> to run.
	/// </summary>
	public struct SimulateMovementControl : IComponentData {
	}

	/// <summary>
	/// Tag component to allow the agent to move according to its desired movement parameters.
	///
	/// Allows <see cref="AIMoveSystem"/> to run the <see cref="JobApplyGravity"/>, <see cref="JobAlignAgentWithMovementDirection"/> and <see cref="JobMoveAgent"/> jobs.
	/// </summary>
	public struct SimulateMovementFinalize : IComponentData {
	}
}
#endif
