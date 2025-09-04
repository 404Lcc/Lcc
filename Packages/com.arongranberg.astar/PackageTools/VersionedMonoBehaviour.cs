using Pathfinding.Serialization;
using UnityEngine;

namespace Pathfinding {
	/// <summary>Exposes internal methods from <see cref="Pathfinding.VersionedMonoBehaviour"/></summary>
	public interface IVersionedMonoBehaviourInternal {
		void UpgradeFromUnityThread();
	}

	namespace Util {
		/// <summary>Used by Pathfinding.Util.BatchedEvents</summary>
		public interface IEntityIndex {
			int EntityIndex { get; set; }
		}
	}

	/// <summary>Base class for all components in the package</summary>
	public abstract class VersionedMonoBehaviour :
#if MODULE_BURST && MODULE_MATHEMATICS && MODULE_COLLECTIONS
	Drawing.MonoBehaviourGizmos
#else
		MonoBehaviour
#endif
		, ISerializationCallbackReceiver, IVersionedMonoBehaviourInternal, Util.IEntityIndex {
		/// <summary>Version of the serialized data. Used for script upgrades.</summary>
		[SerializeField]
		[HideInInspector]
		int version = 0;

		/// <summary>Internal entity index used by <see cref="BatchedEvents"/>. Should never be modified by other scripts.</summary>
		int Util.IEntityIndex.EntityIndex { get; set; }

		protected virtual void Awake () {
			// Make sure the version field is up to date for components created during runtime.
			// Reset is not called when in play mode.
			// If the data had to be upgraded then OnAfterDeserialize would have been called earlier.
			if (Application.isPlaying) {
				if (version == 0) {
					// If version==0 then the component was created during runtime and has not been serialized previously.
					// We can mark all available migrations as finished.
					var m = new Migrations(int.MaxValue);
					OnUpgradeSerializedData(ref m, true);
					version = m.allMigrations;
				} else {
					// If version!=0 then the component may have to run migrations.
					(this as IVersionedMonoBehaviourInternal).UpgradeFromUnityThread();
				}
			}
		}

		/// <summary>Handle serialization backwards compatibility</summary>
		protected virtual void Reset () {
			// Set initial version when adding the component for the first time
			var m = new Migrations(int.MaxValue);
			OnUpgradeSerializedData(ref m, true);
			version = m.allMigrations;

			DisableGizmosIcon();
		}

		void DisableGizmosIcon () {
#if UNITY_EDITOR && UNITY_2022_1_OR_NEWER
			// Disable the icon in the scene view by default for all scripts.
			// There's no way to set the actual default, so we have to do this instead.
			// We store the list of scripts that have been reset in the editor prefs, for each project.
			// Unity stores its gizmo preferences in the Library folder. So it will be per-user and per-project.
			// We won't be able to detect if the user deletes and then rebuilds their library folder, though.
			// Note: Unity may return false for TryGetGizmoInfo the first time it is called for a script.
			// But the second time a user adds a component (and thus resets it), this should work.
			if (UnityEditor.GizmoUtility.TryGetGizmoInfo(GetType(), out var gizmoInfo) && gizmoInfo.hasIcon) {
				var resetPaths = UnityEditor.EditorPrefs.GetString("AstarPathfindingProject.HasResetShowIconGizmos", "");
				var splits = resetPaths.Split(',');
				var id = Application.productName.Replace(",", ";") + ":" + GetType().Name;
				if (System.Array.IndexOf(splits, id) == -1) {
					resetPaths += "," + id;
					UnityEditor.GizmoUtility.SetIconEnabled(GetType(), false);
					UnityEditor.EditorPrefs.SetString("AstarPathfindingProject.HasResetShowIconGizmos", resetPaths);
				}
			}
#endif
		}

		/// <summary>Handle serialization backwards compatibility</summary>
		void ISerializationCallbackReceiver.OnBeforeSerialize () {
		}

		/// <summary>Handle serialization backwards compatibility</summary>
		void ISerializationCallbackReceiver.OnAfterDeserialize() => UpgradeSerializedData(false);

		protected void UpgradeSerializedData (bool isUnityThread) {
			var m = new Migrations(version);
			OnUpgradeSerializedData(ref m, isUnityThread);
			if (m.ignore) return;
			if (m.IsLegacyFormat) throw new System.Exception("Failed to migrate from the legacy format");
			if ((m.finishedMigrations & ~m.allMigrations) != 0) throw new System.Exception("Run more migrations than there are migrations to run. Finished: " + m.finishedMigrations.ToString("X") + " all: " + m.allMigrations.ToString("X"));
			if (isUnityThread && ((m.allMigrations & ~m.finishedMigrations) != 0)) throw new System.Exception("Some migrations were registered, but they did not run. Finished: " + m.finishedMigrations.ToString("X") + " all: " + m.allMigrations.ToString("X"));
			this.version = m.finishedMigrations;
		}

		/// <summary>Handle serialization backwards compatibility</summary>
		protected virtual void OnUpgradeSerializedData (ref Migrations migrations, bool unityThread) {
			if (migrations.TryMigrateFromLegacyFormat(out var legacyVersion)) {
				if (legacyVersion > 1) throw new System.Exception("Reached base class without having migrated the legacy format, and the legacy version is not version 1.");
			}
		}

		void IVersionedMonoBehaviourInternal.UpgradeFromUnityThread() => UpgradeSerializedData(true);
	}
}
