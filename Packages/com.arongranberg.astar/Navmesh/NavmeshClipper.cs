namespace Pathfinding {
	using Pathfinding.Util;
	using Pathfinding.Collections;
	using UnityEngine;
	using System.Collections.Generic;

	/// <summary>Base class for the <see cref="NavmeshCut"/> and <see cref="NavmeshAdd"/> components</summary>
	[ExecuteAlways]
	public abstract class NavmeshClipper : VersionedMonoBehaviour {
		/// <summary>Called every time a NavmeshCut/NavmeshAdd component is enabled.</summary>
		static System.Action<NavmeshClipper> OnEnableCallback;

		/// <summary>Called every time a NavmeshCut/NavmeshAdd component is disabled.</summary>
		static System.Action<NavmeshClipper> OnDisableCallback;

		static readonly List<NavmeshClipper> all = new List<NavmeshClipper>();
		int listIndex = -1;

		/// <summary>
		/// Which graphs that are affected by this component.
		///
		/// You can use this to make a graph ignore a particular navmesh cut altogether.
		///
		/// Note that navmesh cuts can only affect navmesh/recast graphs.
		///
		/// If you change this field during runtime you must disable the component and enable it again for the changes to be detected.
		///
		/// See: <see cref="NavmeshBase.enableNavmeshCutting"/>
		/// </summary>
		public GraphMask graphMask = GraphMask.everything;

		/// <summary>
		/// Ensures that the list of enabled clippers is up to date.
		///
		/// This is useful when loading the scene, and some components may be enabled, but Unity has not yet called their OnEnable method.
		///
		/// See: <see cref="allEnabled"/>
		/// </summary>
		internal static void RefreshEnabledList () {
			var allModifiers = UnityCompatibility.FindObjectsByTypeUnsorted<NavmeshClipper>();

			for (int i = 0; i < allModifiers.Length; i++) {
				if (allModifiers[i].enabled && allModifiers[i].listIndex == -1) {
					// The modifier is not yet registered. Presumably it is enabled,
					// but unity hasn't had time to call OnEnable yet.
					// Disabling it and enabling it will force unity to call OnEnable immediately.
					// We don't want to call it ourselves, because then Unity won't know that it has been called,
					// which could cause issues for lifecycle management.
					// For example, if we called OnEnable manually (before Unity did), and then the object was destroyed
					// before Unity had a chance to call OnEnable, then Unity would not call OnDisable.
					// Warning: This may cause Unity to call OnEnable more than once.
					allModifiers[i].enabled = false;
					allModifiers[i].enabled = true;
				}
			}
		}

		public static void AddEnableCallback (System.Action<NavmeshClipper> onEnable,  System.Action<NavmeshClipper> onDisable) {
			OnEnableCallback += onEnable;
			OnDisableCallback += onDisable;
		}

		public static void RemoveEnableCallback (System.Action<NavmeshClipper> onEnable,  System.Action<NavmeshClipper> onDisable) {
			OnEnableCallback -= onEnable;
			OnDisableCallback -= onDisable;
		}

		/// <summary>
		/// All navmesh clipper components in the scene.
		/// Not ordered in any particular way.
		/// Warning: Do not modify this list
		/// </summary>
		public static List<NavmeshClipper> allEnabled { get { return all; } }

		protected virtual void OnEnable () {
			if (listIndex != -1) {
				// Unity is terrible and can actually call OnEnable more than once in some rare situations.
				// So we have to guard for this.
				// Specifically:
				// 1. At the start of the game, the cutter may have .enabled=true, but OnEnable might not have been called yet.
				// 2. If you from another OnEnable function call 'cutter.enabled = false; cutter.enabled = true', then OnEnable will
				//    get called.
				// 3. Unity may call cutter.OnEnable later in the same frame, even though it was already done.
				// This may get triggered by the RefreshEnabledList method.
				return;
			}

			if (OnEnableCallback != null) OnEnableCallback(this);
			listIndex = all.Count;
			all.Add(this);
		}

		protected virtual void OnDisable () {
			if (listIndex == -1) return;

			// Efficient removal (the list doesn't need to be ordered).
			// Move the last item in the list to the slot occupied by this item
			// and then remove the last slot.
			all[listIndex] = all[all.Count-1];
			all[listIndex].listIndex = listIndex;
			all.RemoveAt(all.Count-1);
			listIndex = -1;
			if (OnDisableCallback != null) OnDisableCallback(this);
		}

		public abstract void NotifyUpdated(GridLookup<NavmeshClipper>.Root previousState);
		public abstract Rect GetBounds(GraphTransform transform, float radiusMargin);
		public abstract bool RequiresUpdate(GridLookup<NavmeshClipper>.Root previousState);
		public abstract void ForceUpdate();
	}
}
