using UnityEngine;
using Pathfinding.Util;
#if MODULE_ENTITIES
using Unity.Entities;
#endif

namespace Pathfinding {
	/// <summary>
	/// Sets the destination of an AI to the position of a specified object.
	/// This component should be attached to a GameObject together with a movement script such as AIPath, RichAI or AILerp.
	/// This component will then make the AI move towards the <see cref="target"/> set on this component.
	///
	/// Essentially the only thing this component does is to set the <see cref="Pathfinding.IAstarAI.destination"/> property to the position of the target every frame.
	/// But there is some additional complexity to make sure that the destination is updated immediately before the AI searches for a path as well, in case the
	/// target moved since the last Update. There is also some complexity to reduce the performance impact, by using the <see cref="BatchedEvents"/> system to
	/// process all AIDestinationSetter components in a single batch.
	///
	/// When using ECS, this component is instead added as a managed component to the entity.
	/// The destination syncing is then handled by the <see cref="SyncDestinationTransformSystem"/> for better performance.
	///
	/// See: <see cref="Pathfinding.IAstarAI.destination"/>
	/// </summary>
	[UniqueComponent(tag = "ai.destination")]
	[AddComponentMenu("Pathfinding/AI/Behaviors/AIDestinationSetter")]
	[HelpURL("https://arongranberg.com/astar/documentation/stable/aidestinationsetter.html")]
	public class AIDestinationSetter : VersionedMonoBehaviour
#if MODULE_ENTITIES
		, IRuntimeBaker
#endif
	{
		/// <summary>The object that the AI should move to</summary>
		public Transform target;

		/// <summary>
		/// If true, the agent will try to align itself with the rotation of the <see cref="target"/>.
		///
		/// This can only be used together with the <see cref="FollowerEntity"/> movement script.
		/// Other movement scripts will ignore it.
		///
		/// [Open online documentation to see videos]
		///
		/// See: <see cref="FollowerEntity.SetDestination"/>
		/// </summary>
		public bool useRotation;

		IAstarAI ai;
#if MODULE_ENTITIES
		Entity entity;
		World world;
#endif

		void OnEnable () {
			ai = GetComponent<IAstarAI>();
#if MODULE_ENTITIES
			if (ai is FollowerEntity follower) {
				// This will call OnCreatedEntity on this component, if the entity has already been created.
				follower.RegisterRuntimeBaker(this);
			} else
#endif
			{
				// Update the destination right before searching for a path as well.
				// This is enough in theory, but this script will also update the destination every
				// frame as the destination is used for debugging and may be used for other things by other
				// scripts as well. So it makes sense that it is up to date every frame.
				if (ai != null) ai.onSearchPath += UpdateDestination;

				// Will make OnUpdate be called once every frame with all components.
				// This is significantly faster than letting Unity call the Update method
				// on each component.
				// See https://blog.unity.com/technology/1k-update-calls
				BatchedEvents.Add(this, BatchedEvents.Event.Update, OnUpdate, 0);
			}
		}

		void OnDisable () {
#if MODULE_ENTITIES
			if (world != null && world.IsCreated && world.EntityManager.Exists(entity)) {
				world.EntityManager.RemoveComponent<AIDestinationSetter>(entity);
			}
			if (ai != null && !(ai is FollowerEntity)) ai.onSearchPath -= UpdateDestination;
#else
			if (ai != null) ai.onSearchPath -= UpdateDestination;
#endif
			BatchedEvents.Remove(this);
		}

#if MODULE_ENTITIES
		void IRuntimeBaker.OnCreatedEntity (World world, Entity entity) {
			// Do nothing except add the component. Actual syncing is handled by the SyncDestinationTransformSystem.
			this.entity = entity;
			this.world = world;
			world.EntityManager.AddComponentObject(entity, this);
		}
#endif

		/// <summary>Updates the AI's destination every frame</summary>
		static void OnUpdate (AIDestinationSetter[] components, int count) {
			for (int i = 0; i < count; i++) {
				components[i].UpdateDestination();
			}
		}

		/// <summary>Updates the AI's destination immediately</summary>
		void UpdateDestination () {
			if (target != null && ai != null) ai.destination = target.position;
		}
	}
}
