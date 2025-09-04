#if MODULE_ENTITIES
using Unity.Entities;
using UnityEngine;
using System;

namespace Pathfinding.ECS {
	/// <summary>
	/// The physics scene to use for raycasting.
	///
	/// For most games, this will be the default physics scene. But if your game has more than one physics scene, you can use this component to specify which one to use.
	///
	/// The <see cref="FollowerEntity"/> will use PhysicsSceneExtensions.GetPhysicsScene to get the physics scene from the GameObject that the FollowerEntity component is attached to.
	/// </summary>
	public struct PhysicsSceneRef : ISharedComponentData, IQueryTypeParameter, IEquatable<PhysicsSceneRef> {
		public PhysicsScene physicsScene;

		public bool Equals (PhysicsSceneRef other) {
			return physicsScene == other.physicsScene;
		}

		override public int GetHashCode () {
			return physicsScene.GetHashCode();
		}
	}
}
#endif
