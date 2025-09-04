using UnityEngine;
using System.Collections.Generic;
using Pathfinding.Util;

namespace Pathfinding {
	/// <summary>
	/// GraphModifier is used for modifying graphs or processing graph data based on events.
	/// This class is a simple container for a number of events.
	///
	/// \borderlessimage{graph_events.png}
	///
	/// Warning: Some events will be called both in play mode <b>and in editor mode</b> (at least the scan events).
	/// So make sure your code handles both cases well. You may choose to ignore editor events.
	/// See: Application.IsPlaying
	///
	/// Warning: Events may be received before Awake and OnEnable has been called on the component. This is because
	/// graphs are typically scanned during Awake on the AstarPath component, which may happen before Awake on the graph modifier itself.
	/// </summary>
	[ExecuteInEditMode]
	public abstract class GraphModifier : VersionedMonoBehaviour {
		/// <summary>All active graph modifiers</summary>
		private static GraphModifier root;

		private GraphModifier prev;
		private GraphModifier next;

		/// <summary>Unique persistent ID for this component, used for serialization</summary>
		[SerializeField]
		[HideInInspector]
		protected ulong uniqueID;

		/// <summary>Maps persistent IDs to the component that uses it</summary>
		protected static Dictionary<ulong, GraphModifier> usedIDs = new Dictionary<ulong, GraphModifier>();

		protected static List<T> GetModifiersOfType<T>() where T : GraphModifier {
			var current = root;
			var result = new List<T>();

			while (current != null) {
				var cast = current as T;
				if (cast != null) result.Add(cast);
				current = current.next;
			}
			return result;
		}

		public static void FindAllModifiers () {
			var allModifiers = UnityCompatibility.FindObjectsByTypeSorted<GraphModifier>();

			for (int i = 0; i < allModifiers.Length; i++) {
				if (allModifiers[i].enabled) {
					if (allModifiers[i].next == null) {
						// The modifier is not yet registered. Presumably it is enabled,
						// but unity hasn't had time to call OnEnable yet.
						// Disabling it and enabling it will force unity to call OnEnable immediately.
						// We don't want to call it ourselves, because then Unity won't know that it has been called,
						// which could cause issues for lifecycle management.
						// For example, if we called OnEnable manually (before Unity did), and then the object was destroyed
						// before Unity had a chance to call OnEnable, then Unity would not call OnDisable.
						allModifiers[i].enabled = false;
						allModifiers[i].enabled = true;
					}
				}
			}
		}

		/// <summary>GraphModifier event type</summary>
		public enum EventType {
			PostScan = 1 << 0,
			PreScan = 1 << 1,
			LatePostScan = 1 << 2,
			PreUpdate = 1 << 3,
			PostUpdate = 1 << 4,
			PostCacheLoad = 1 << 5,
			PostUpdateBeforeAreaRecalculation = 1 << 6,
			PostGraphLoad = 1 << 7,
		}

		/// <summary>Triggers an event for all active graph modifiers</summary>
		public static void TriggerEvent (GraphModifier.EventType type) {
			if (!Application.isPlaying) {
				FindAllModifiers();
			}

			try {
				GraphModifier c = root;
				switch (type) {
				case EventType.PreScan:
					while (c != null) { c.OnPreScan(); c = c.next; }
					break;
				case EventType.PostScan:
					while (c != null) { c.OnPostScan(); c = c.next; }
					break;
				case EventType.LatePostScan:
					while (c != null) { c.OnLatePostScan(); c = c.next; }
					break;
				case EventType.PreUpdate:
					while (c != null) { c.OnGraphsPreUpdate(); c = c.next; }
					break;
				case EventType.PostUpdate:
					while (c != null) { c.OnGraphsPostUpdate(); c = c.next; }
					break;
				case EventType.PostUpdateBeforeAreaRecalculation:
					while (c != null) { c.OnGraphsPostUpdateBeforeAreaRecalculation(); c = c.next; }
					break;
				case EventType.PostCacheLoad:
					while (c != null) { c.OnPostCacheLoad(); c = c.next; }
					break;
				case EventType.PostGraphLoad:
					while (c != null) { c.OnPostGraphLoad(); c = c.next; }
					break;
				}
			} catch (System.Exception e) {
				Debug.LogException(e);
			}
		}

		/// <summary>Adds this modifier to list of active modifiers</summary>
		protected virtual void OnEnable () {
			RemoveFromLinkedList();
			AddToLinkedList();
			ConfigureUniqueID();
		}

		/// <summary>Removes this modifier from list of active modifiers</summary>
		protected virtual void OnDisable () {
			RemoveFromLinkedList();
		}

		protected override void Awake () {
			base.Awake();
			ConfigureUniqueID();
		}

		void ConfigureUniqueID () {
			// Check if any other object is using the same uniqueID
			// In that case this object may have been duplicated
			GraphModifier usedBy;

			if (usedIDs.TryGetValue(uniqueID, out usedBy) && usedBy != this) {
				Reset();
			}

			usedIDs[uniqueID] = this;
		}

		void AddToLinkedList () {
			if (root == null) {
				root = this;
			} else {
				next = root;
				root.prev = this;
				root = this;
			}
		}

		void RemoveFromLinkedList () {
			if (root == this) {
				root = next;
				if (root != null) root.prev = null;
			} else {
				if (prev != null) prev.next = next;
				if (next != null) next.prev = prev;
			}
			prev = null;
			next = null;
		}

		protected virtual void OnDestroy () {
			usedIDs.Remove(uniqueID);
		}

		/// <summary>
		/// Called right after all graphs have been scanned.
		///
		/// Note: Area information (see <see cref="Pathfinding.HierarchicalGraph)"/> may not be up to date when this event is sent.
		/// This means some methods like <see cref="Pathfinding.PathUtilities.IsPathPossible"/> may return incorrect results.
		/// Use <see cref="OnLatePostScan"/> if you need that info to be up to date.
		///
		/// See: OnLatePostScan
		/// </summary>
		public virtual void OnPostScan () {}

		/// <summary>
		/// Called right before graphs are going to be scanned.
		///
		/// See: OnLatePostScan
		/// </summary>
		public virtual void OnPreScan () {}

		/// <summary>
		/// Called at the end of the scanning procedure.
		/// This is the absolute last thing done by Scan.
		/// </summary>
		public virtual void OnLatePostScan () {}

		/// <summary>
		/// Called after cached graphs have been loaded.
		/// When using cached startup, this event is analogous to OnLatePostScan and implementing scripts
		/// should do roughly the same thing for both events.
		/// </summary>
		public virtual void OnPostCacheLoad () {}

		/// <summary>
		/// Called after a graph has been deserialized and loaded.
		/// Note: The graph may not have had any valid node data, it might just contain the graph settings.
		///
		/// This will be called often outside of play mode. Make sure to check Application.isPlaying if appropriate.
		/// </summary>
		public virtual void OnPostGraphLoad () {}

		/// <summary>Called before graphs are updated using GraphUpdateObjects</summary>
		public virtual void OnGraphsPreUpdate () {}

		/// <summary>
		/// Called after graphs have been updated using GraphUpdateObjects or navmesh cutting.
		///
		/// This is among other times called after graphs have been scanned, updated using GraphUpdateObjects, navmesh cuts, or GraphUpdateScene components.
		///
		/// Area recalculations (see <see cref="Pathfinding.HierarchicalGraph"/>) have been done at this stage so things like PathUtilities.IsPathPossible will work.
		///
		/// Use <see cref="OnGraphsPostUpdateBeforeAreaRecalculation"/> instead if you are modifying the graph in any way, especially connections and walkability.
		/// This is because if you do this then area recalculations
		/// </summary>
		public virtual void OnGraphsPostUpdate () {}

		/// <summary>
		/// Called after graphs have been updated.
		///
		/// This is among other times called after graphs have been scanned, updated using GraphUpdateObjects, navmesh cuts, or GraphUpdateScene components.
		///
		/// Note: Area information (see <see cref="Pathfinding.HierarchicalGraph)"/> may not be up to date when this event is sent.
		/// This means some methods like <see cref="Pathfinding.PathUtilities.IsPathPossible"/> may return incorrect results.
		/// Use <see cref="OnLatePostScan"/> if you need that info to be up to date.
		///
		/// Use this if you are modifying any graph connections or walkability.
		///
		/// See: <see cref="OnGraphsPostUpdate"/>
		/// </summary>
		public virtual void OnGraphsPostUpdateBeforeAreaRecalculation () {}

		protected override void Reset () {
			base.Reset();
			// Create a new random 64 bit value (62 bit actually because we skip negative numbers, but that's still enough by a huge margin)
			var rnd1 = (ulong)Random.Range(0, int.MaxValue);
			var rnd2 = ((ulong)Random.Range(0, int.MaxValue) << 32);

			uniqueID = rnd1 | rnd2;
			usedIDs[uniqueID] = this;
		}
	}
}
