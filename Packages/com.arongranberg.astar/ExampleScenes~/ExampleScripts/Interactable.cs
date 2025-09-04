using System.Collections;
using System.Collections.Generic;
using Pathfinding.ECS;
using UnityEngine;

namespace Pathfinding.Examples {
	/// <summary>
	/// Example script for handling interactable objects in the example scenes.
	///
	/// It implements a very simple and lightweight state machine.
	///
	/// Note: This is an example script intended for the A* Pathfinding Project's example scenes.
	/// If you need a proper state machine for your game, you may be better served by other state machine solutions on the Unity Asset Store.
	///
	/// It works by keeping a linear list of states, each with an associated action.
	/// When an agent iteracts with this object, it immediately does the first action in the list.
	/// Once that action is done, it will do the next action and so on.
	///
	/// Some actions may cancel the whole interaction. For example the MoveTo action will cancel the interaction if the agent
	/// suddenly had its destination to something else. Presumably because the agent was interrupted by something.
	///
	/// If this component is added to the same GameObject as a <see cref="NodeLink2"/> component, the interactable will automatically trigger when the agent traverses the link.
	/// Some components behave differently when used during an off-mesh link component.
	/// For example the <see cref="MoveToAction"/> will move the agent without taking the navmesh into account (becoming a thin wrapper for <see cref="AgentOffMeshLinkTraversalContext.MoveTowards"/>).
	/// </summary>
	[HelpURL("https://arongranberg.com/astar/documentation/stable/interactable.html")]
	public class Interactable : VersionedMonoBehaviour, IOffMeshLinkHandler, IOffMeshLinkStateMachine {
		public enum CoroutineAction {
			Tick,
			Cancel,
		}

		[System.Serializable]
		public abstract class InteractableAction {
			public virtual IEnumerator<CoroutineAction> Execute (IAstarAI ai) {
				return Execute();
			}

#if MODULE_ENTITIES
			public virtual IEnumerator<CoroutineAction> Execute (Pathfinding.ECS.AgentOffMeshLinkTraversalContext context) {
				return Execute();
			}
#endif

			public virtual IEnumerator<CoroutineAction> Execute () {
				throw new System.NotImplementedException("This action has no implementation");
			}
		}


		[System.Serializable]
		public class AnimatorPlay : InteractableAction {
			public string stateName;
			public float normalizedTime = 0;
			public Animator animator;

			public override IEnumerator<CoroutineAction> Execute () {
				animator.Play(stateName, -1, normalizedTime);
				yield break;
			}
		}

		[System.Serializable]
		public class AnimatorSetBoolAction : InteractableAction {
			public string propertyName;
			public bool value;
			public Animator animator;

			public override IEnumerator<CoroutineAction> Execute () {
				animator.SetBool(propertyName, value);
				yield break;
			}
		}

		[System.Serializable]
		public class ActivateParticleSystem : InteractableAction {
			public ParticleSystem particleSystem;

			public override IEnumerator<CoroutineAction> Execute () {
				particleSystem.Play();
				yield break;
			}
		}

		[System.Serializable]
		public class DelayAction : InteractableAction {
			public float delay;

			public override IEnumerator<CoroutineAction> Execute () {
				float time = Time.time + delay;
				while (Time.time < time) yield return CoroutineAction.Tick;
				yield break;
			}
		}

		[System.Serializable]
		public class SetObjectActiveAction : InteractableAction {
			public GameObject target;
			public bool active;

			public override IEnumerator<CoroutineAction> Execute () {
				target.SetActive(active);
				yield break;
			}
		}

		[System.Serializable]
		public class InstantiatePrefab : InteractableAction {
			public GameObject prefab;
			public Transform position;

			public override IEnumerator<CoroutineAction> Execute () {
				if (prefab != null && position != null) {
					GameObject.Instantiate(prefab, position.position, position.rotation);
				}
				yield break;
			}
		}

		[System.Serializable]
		public class CallFunction : InteractableAction {
			public UnityEngine.Events.UnityEvent function;

			public override IEnumerator<CoroutineAction> Execute () {
				function.Invoke();
				yield break;
			}
		}

		[System.Serializable]
		public class TeleportAgentAction : InteractableAction {
			public Transform destination;

			public override IEnumerator<CoroutineAction> Execute (IAstarAI ai) {
				ai.Teleport(destination.position);
				yield break;
			}

#if MODULE_ENTITIES
			public override IEnumerator<CoroutineAction> Execute (AgentOffMeshLinkTraversalContext context) {
				context.Teleport(destination.position);
				yield break;
			}
#endif
		}

		[System.Serializable]
		public class TeleportAgentOnLinkAction : InteractableAction {
			public enum Destination {
				/// <summary>The side of the link that the agent starts traversing it from</summary>
				RelativeStartOfLink,
				/// <summary>The side of the link that is opposite the one the agent starts traversing it from</summary>
				RelativeEndOfLink,
			}

			public Destination destination = Destination.RelativeEndOfLink;

			public override IEnumerator<CoroutineAction> Execute() => throw new System.NotImplementedException("This action only works for agents traversing off-mesh links.");

#if MODULE_ENTITIES
			public override IEnumerator<CoroutineAction> Execute (AgentOffMeshLinkTraversalContext context) {
				context.Teleport(destination == Destination.RelativeStartOfLink ? context.link.relativeStart : context.link.relativeEnd);
				yield break;
			}
#endif
		}

		[System.Serializable]
		public class SetTransformAction : InteractableAction {
			public Transform transform;
			public Transform source;
			public bool setPosition = true;
			public bool setRotation;
			public bool setScale;

			public override IEnumerator<CoroutineAction> Execute () {
				if (setPosition) transform.position = source.position;
				if (setRotation) transform.rotation = source.rotation;
				if (setScale) transform.localScale = source.localScale;
				yield break;
			}
		}

		[System.Serializable]
		public class MoveToAction : InteractableAction {
			public Transform destination;
			public bool useRotation;
			public bool waitUntilReached;

			public override IEnumerator<CoroutineAction> Execute (IAstarAI ai) {
				var dest = destination.position;
#if MODULE_ENTITIES
				if (useRotation && ai is FollowerEntity follower) {
					follower.SetDestination(dest, destination.rotation * Vector3.forward);
				} else
#endif
				{
					if (useRotation) Debug.LogError("useRotation is only supported for FollowerEntity agents", ai as MonoBehaviour);
					ai.destination = dest;
				}

				if (waitUntilReached) {
					if (ai is AIBase || ai is AILerp) {
						// Only the FollowerEntity component is good enough to set the reachedDestination property to false immediately.
						// The other movement scripts need to wait for the new path to be available, which is somewhat annoying.
						ai.SearchPath();
						while (ai.pathPending) yield return CoroutineAction.Tick;
					}

					while (!ai.reachedDestination) {
						if (ai.destination != dest) {
							// Something else must have changed the destination
							yield return CoroutineAction.Cancel;
						}
						if (ai.reachedEndOfPath) {
							// We have reached the end of the path, but not the destination
							// This must mean that we cannot get any closer
							// TODO: More accurate 'cannot move forwards' check
							yield return CoroutineAction.Cancel;
						}
						yield return CoroutineAction.Tick;
					}
				}
			}

#if MODULE_ENTITIES
			public override IEnumerator<CoroutineAction> Execute (AgentOffMeshLinkTraversalContext context) {
				while (!context.MoveTowards(destination.position, destination.rotation, true, true).reached) {
					yield return CoroutineAction.Tick;
				}
				yield break;
			}
#endif
		}

		[System.Serializable]
		public class InteractAction : InteractableAction {
			public Interactable interactable;

			public override IEnumerator<CoroutineAction> Execute (IAstarAI ai) {
				var it = interactable.InteractCoroutine(ai);
				while (it.MoveNext()) {
					yield return it.Current;
				}
			}
		}

		[SerializeReference]
		public List<InteractableAction> actions;

		public void Interact (IAstarAI ai) {
			StartCoroutine(InteractCoroutine(ai));
		}

#if MODULE_ENTITIES
		IOffMeshLinkStateMachine IOffMeshLinkHandler.GetOffMeshLinkStateMachine(AgentOffMeshLinkTraversalContext context) => this;

		IEnumerable IOffMeshLinkStateMachine.OnTraverseOffMeshLink (AgentOffMeshLinkTraversalContext context) {
			var it = InteractCoroutine(context);
			while (it.MoveNext()) {
				yield return null;
			}
		}

		public IEnumerator<CoroutineAction> InteractCoroutine (Pathfinding.ECS.AgentOffMeshLinkTraversalContext context) {
			if (actions.Count == 0) {
				Debug.LogWarning("No actions have been set up for this interactable", this);
				yield break;
			}

			var actionIndex = 0;
			while (actionIndex < actions.Count) {
				var action = actions[actionIndex];
				if (action == null) {
					actionIndex++;
					continue;
				}

				var enumerator = action.Execute(context);
				while (enumerator.MoveNext()) {
					yield return enumerator.Current;
					if (enumerator.Current == CoroutineAction.Cancel) yield break;
				}

				actionIndex++;
			}
		}
#endif

		public IEnumerator<CoroutineAction> InteractCoroutine (IAstarAI ai) {
			if (actions.Count == 0) {
				Debug.LogWarning("No actions have been set up for this interactable", this);
				yield break;
			}

			var actionIndex = 0;
			while (actionIndex < actions.Count) {
				var action = actions[actionIndex];
				if (action == null) {
					actionIndex++;
					continue;
				}

				var enumerator = action.Execute(ai);
				while (enumerator.MoveNext()) {
					yield return enumerator.Current;
					if (enumerator.Current == CoroutineAction.Cancel) yield break;
				}

				actionIndex++;
			}
		}

		void OnEnable () {
			// Allow the interactable to be triggered by an agent traversing an off-mesh link
			if (TryGetComponent<NodeLink2>(out var link)) link.onTraverseOffMeshLink = this;
		}

		void OnDisable () {
			if (TryGetComponent<NodeLink2>(out var link) && link.onTraverseOffMeshLink == (IOffMeshLinkHandler)this) link.onTraverseOffMeshLink = null;
		}
	}
}
