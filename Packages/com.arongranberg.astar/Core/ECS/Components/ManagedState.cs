#if MODULE_ENTITIES
using Unity.Entities;

namespace Pathfinding.ECS {
	using Pathfinding;
	using Pathfinding.ECS.RVO;
	using UnityEngine.Serialization;

	/// <summary>
	/// Settings for agent movement that cannot be put anywhere else.
	///
	/// The Unity ECS in general wants everything in components to be unmanaged types.
	/// However, some things cannot be unmanaged types, for example delegates and interfaces.
	/// There are also other things like path references and node references which are not unmanaged types at the moment.
	///
	/// This component is used to store those things.
	///
	/// It can also be used for things that are not used often, and so are best kept out-of-band to avoid bloating the ECS chunks too much.
	/// </summary>
	[System.Serializable]
	public class ManagedState : IComponentData, System.IDisposable, System.ICloneable {
		/// <summary>
		/// Settings for when to recalculate the path.
		///
		/// Deprecated: Use <see cref="FollowerEntity.autoRepath"/>, or the <see cref="Pathfinding.ECS.AutoRepathPolicy"/> component instead.
		/// </summary>
		[System.Obsolete("Use FollowerEntity.autoRepath, or the Pathfinding.ECS.AutoRepathPolicy component instead", true)]
		public Pathfinding.AutoRepathPolicy autoRepath => null;

		/// <summary>Calculates in which direction to move to follow the path</summary>
		public PathTracer pathTracer;

		/// <summary>
		/// Local avoidance settings.
		///
		/// When the agent has local avoidance enabled, these settings will be copied into a <see cref="Pathfinding.ECS.RVO.RVOAgent"/> component which is attached to the agent.
		///
		/// See: <see cref="enableLocalAvoidance"/>
		/// </summary>
		[FormerlySerializedAs("rvoAgent")]
		public RVOAgent rvoSettings = RVOAgent.Default;

		/// <summary>Callback for when the agent starts to traverse an off-mesh link</summary>
		[System.NonSerialized]
		public IOffMeshLinkHandler onTraverseOffMeshLink;

		public PathRequestSettings pathfindingSettings = PathRequestSettings.Default;

		/// <summary>
		/// True if local avoidance is enabled for this agent.
		///
		/// Enabling this will automatically add a <see cref="Pathfinding.ECS.RVO.RVOAgent"/> component to the entity.
		///
		/// See: local-avoidance (view in online documentation for working links)
		/// </summary>
		[FormerlySerializedAs("rvoEnabled")]
		public bool enableLocalAvoidance;

		/// <summary>
		/// True if gravity is enabled for this agent.
		///
		/// The agent will always fall down according to its own movement plane.
		/// The gravity applied is Physics.gravity.y.
		///
		/// Enabling this will add the <see cref="GravityState"/> component to the entity.
		///
		/// This has no effect if the agent's orientation is set to YAxisForward (2D mode).
		/// Gravity does not really make sense for top-down 2D games. The gravity setting is also hidden from the inspector in this mode.
		/// </summary>
		public bool enableGravity = true;

		/// <summary>Path that is being calculated, if any</summary>
		// Do not create a property visitor for this field, as otherwise the ECS infrastructure will try to patch entities inside it, and get very confused.
		// I haven't been able to replicate this issue recently, but it has caused problems in the past.
		// [Unity.Properties.DontCreateProperty]
		public Path pendingPath { get; private set; }

		/// <summary>
		/// Path that is being followed, if any.
		///
		/// The agent may have moved away from this path since it was calculated. So it may not be up to date.
		/// </summary>
		// Do not create a property visitor for this field, as otherwise the ECS infrastructure will try to patch entities inside it, and get very confused.
		// [Unity.Properties.DontCreateProperty]
		public Path activePath { get; private set; }

		/// <summary>
		/// \copydocref{IAstarAI.SetPath}.
		///
		/// Warning: In almost all cases you should use <see cref="FollowerEntity.SetPath"/> instead of this method.
		/// </summary>
		public static void SetPath (Path path, ManagedState state, in AgentMovementPlane movementPlane, ref DestinationPoint destination) {
			if (path == null) {
				state.CancelCurrentPathRequest();
				state.ClearPath();
			} else if (path.PipelineState == PathState.Created) {
				// Path has not started calculation yet
				state.CancelCurrentPathRequest();
				state.pendingPath = path;
				path.Claim(state);
				AstarPath.StartPath(path);
			} else if (path.PipelineState >= PathState.ReturnQueue) {
				// Path has already been calculated

				if (state.pendingPath == path) {
					// The pending path is now obviously no longer pending
					state.pendingPath = null;
				} else {
					// We might be calculating another path at the same time, and we don't want that path to override this one. So cancel it.
					state.CancelCurrentPathRequest();

					// Increase the refcount on the path.
					// If the path was already our pending path, then the refcount will have already been incremented
					path.Claim(state);
				}

				var abPath = path as ABPath;
				if (abPath == null) throw new System.ArgumentException("This function only works with ABPaths, or paths inheriting from ABPath");

				if (!abPath.error) {
					try {
						state.pathTracer.SetPath(abPath, movementPlane.value);

						// Release the previous path back to the pool, to reduce GC pressure
						if (state.activePath != null) state.activePath.Release(state);

						state.activePath = abPath;
					} catch (System.Exception e) {
						// If the path was so invalid that the path tracer throws an exception, then we should not use it.
						abPath.Release(state);
						state.ClearPath();
						UnityEngine.Debug.LogException(e);
					}

					// If a RandomPath or MultiTargetPath have just been calculated, then we need
					// to patch our destination point, to ensure the agent continues to move towards the end of the path.
					// For these path types, the end point of the path is not known before the calculation starts.
					if (!abPath.endPointKnownBeforeCalculation) {
						destination = new DestinationPoint { destination = abPath.originalEndPoint, facingDirection = default };
					}

					// Right now, the pathTracer is almost fully up to date.
					// To make it fully up to date, we'd also have to call pathTracer.UpdateStart and pathTracer.UpdateEnd after this function.
					// During normal path recalculations, the JobRepairPath will be scheduled right after this function, and it will
					// call those functions. The incomplete state will not be observable outside the system.
					// When called from FollowerEntity, the SetPath method on that component will ensure that these methods are called.
				} else {
					abPath.Release(state);
				}
			} else {
				// Path calculation has been started, but it is not yet complete. Cannot really handle this.
				throw new System.ArgumentException("You must call the SetPath method with a path that either has been completely calculated or one whose path calculation has not been started at all. It looks like the path calculation for the path you tried to use has been started, but is not yet finished.");
			}
		}

		public void ClearPath () {
			pathTracer.Clear();
			if (activePath != null) {
				activePath.Release(this);
				activePath = null;
			}
		}

		public void CancelCurrentPathRequest () {
			if (pendingPath != null) {
				pendingPath.FailWithError("Canceled by script");
				pendingPath.Release(this);
				pendingPath = null;
			}
		}

		public void Dispose () {
			pathTracer.Dispose();
			if (pendingPath != null) {
				pendingPath.FailWithError("Canceled because entity was destroyed");
				pendingPath.Release(this);
				pendingPath = null;
			}
			if (activePath != null) {
				activePath.Release(this);
				activePath = null;
			}
		}

		/// <summary>
		/// Pops the current part, and the next part from the start of the path.
		///
		/// It is assumed that the agent is currently on a normal NodeSequence part, and that the next part in the path is an off-mesh link.
		/// </summary>
		public void PopNextLinkFromPath () {
			if (pathTracer.partCount < 2 && pathTracer.GetPartType(1) != Funnel.PartType.OffMeshLink) {
				throw new System.InvalidOperationException("The next part in the path is not an off-mesh link.");
			}
			pathTracer.PopParts(2, pathfindingSettings.traversalProvider, activePath);
		}

		/// <summary>
		/// Clones the managed state for when an entity is duplicated.
		///
		/// Some fields are cleared instead of being cloned, such as the pending path,
		/// which cannot reasonably be cloned.
		/// </summary>
		object System.ICloneable.Clone () {
			return new ManagedState {
					   pathTracer = pathTracer.Clone(),
					   rvoSettings = rvoSettings,
					   pathfindingSettings = new PathRequestSettings {
						   graphMask = pathfindingSettings.graphMask,
						   tagPenalties = pathfindingSettings.tagPenalties != null ? (int[])pathfindingSettings.tagPenalties.Clone() : null,
						   traversableTags = pathfindingSettings.traversableTags,
						   traversalProvider = null,  // Cannot be safely cloned or copied
					   },
					   enableLocalAvoidance = enableLocalAvoidance,
					   enableGravity = enableGravity,
					   onTraverseOffMeshLink = null,  // Cannot be safely cloned or copied
					   pendingPath = null,  // Cannot be safely cloned or copied
					   activePath = null,  // Cannot be safely cloned or copied
			};
		}
	}
}
#endif
