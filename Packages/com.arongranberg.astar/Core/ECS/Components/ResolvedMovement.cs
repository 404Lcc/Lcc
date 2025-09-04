#if MODULE_ENTITIES
using Unity.Entities;
using Unity.Mathematics;

namespace Pathfinding.ECS {
	/// <summary>
	/// Holds the final movement data for an entity.
	/// This is the data that is used by the movement system to move the entity.
	/// </summary>
	public struct ResolvedMovement : IComponentData {
		/// <summary>\copydocref{MovementControl.targetPoint}</summary>
		public float3 targetPoint;

		/// <summary>\copydocref{MovementControl.speed}</summary>
		public float speed;

		public float turningRadiusMultiplier;

		/// <summary>\copydocref{MovementControl.targetRotation}</summary>
		public float targetRotation;

		/// <summary>\copydocref{MovementControl.targetRotationHint}</summary>
		public float targetRotationHint;

		/// <summary>\copydocref{MovementControl.targetRotationOffset}</summary>
		public float targetRotationOffset;

		/// <summary>\copydocref{MovementControl.rotationSpeed}</summary>
		public float rotationSpeed;
	}
}
#endif
