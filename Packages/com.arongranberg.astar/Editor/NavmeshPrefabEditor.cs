using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Pathfinding {
	using Pathfinding.Sync;

	/// <summary>Editor for the <see cref="NavmeshPrefab"/> component</summary>
	[CustomEditor(typeof(NavmeshPrefab), true)]
	[CanEditMultipleObjects]
	public class NavmeshPrefabEditor : EditorBase {
		protected override void OnEnable () {
			base.OnEnable();
			AstarPathEditor.LoadStyles();
			EditorApplication.update += OnUpdate;
		}

		protected override void OnDisable () {
			base.OnDisable();
			EditorApplication.update -= OnUpdate;
		}

		void OnUpdate () {
		}

		static int pendingScanProgressId;

		static int PendingScan (Promise<NavmeshPrefab.SerializedOutput>[] pendingScanProgress, NavmeshPrefab[] pendingScanTargets) {
			var progressId = UnityEditor.Progress.Start("Scanning Navmesh Prefab", "Scanning Navmesh Prefab", UnityEditor.Progress.Options.None, -1);
			EditorApplication.CallbackFunction cb = null;
			cb = () => {
				if (UnityEditor.Progress.Exists(progressId)) {
					bool allDone = true;
					var avg = 0.0f;
					for (int i = 0; i < pendingScanProgress.Length; i++) {
						if (pendingScanTargets[i] == null) {
							avg += 1.0f;
						} else {
							avg += pendingScanProgress[i].Progress;

							if (pendingScanProgress[i].IsCompleted) {
								var data = pendingScanProgress[i].Complete().data;
								// Data can be null if some exception has been thrown during the scan
								if (data != null) {
									pendingScanTargets[i].SaveToFile(data);
								}
								pendingScanProgress[i].Dispose();
								pendingScanTargets[i] = null;
								pendingScanProgress[i] = default;
							} else {
								allDone = false;
							}
						}
					}
					avg /= pendingScanProgress.Length;
					UnityEditor.Progress.Report(progressId, avg);

					if (allDone) {
						UnityEditor.Progress.Finish(progressId);
					}
				} else {
					EditorApplication.update -= cb;
				}
			};
			EditorApplication.update += cb;
			return progressId;
		}

		protected override void Inspector () {
			AstarPath.FindAstarPath();
			Section("Shape");
			BoundsField("bounds");

			bool needsRounding = false;
			RecastGraph graph = null;
			if (AstarPath.active != null && AstarPath.active.data.recastGraph != null) {
				graph = AstarPath.active.data.recastGraph;
			}
			bool isPrefab = EditorUtility.IsPersistent(this.target);

			if (graph != null && !isPrefab) {
				if (!graph.useTiles) {
					EditorGUILayout.HelpBox("The recast graph in the scene doesn't use tiles. It needs to use tiles to be used with this component.", MessageType.Warning);
					if (GUILayout.Button("Enable tiling on recast graph")) {
						graph.useTiles = true;
					}
				}

				var roundedTiles = this.targets.Cast<NavmeshPrefab>().Select(target => {
					var navmeshPrefab = target as NavmeshPrefab;
					var bounds = navmeshPrefab.bounds;
					var desiredBounds = NavmeshPrefab.SnapSizeToClosestTileMultiple(graph, bounds);
					return new {
						needsRounding = !Mathf.Approximately(bounds.extents.x, desiredBounds.extents.x) || !Mathf.Approximately(bounds.extents.z, desiredBounds.extents.z),
						desiredBounds = desiredBounds
					};
				}).ToArray();
				needsRounding = roundedTiles.Any(x => x.needsRounding);

				if (needsRounding) {
					EditorGUILayout.HelpBox("Bounds size is not a multiple of the recast graph's tile size (" + (graph.editorTileSize * graph.cellSize).ToString("0.0") + ").\nThe tile size is voxel size * tile size (voxels) (set in recast graph settings)", MessageType.Warning);
					if (GUILayout.Button("Round to nearest multiple")) {
						UnityEditor.Undo.RecordObjects(targets, "Snap to nearest tile multiple");
						for (int i = 0; i < targets.Length; i++) {
							(targets[i] as NavmeshPrefab).bounds = roundedTiles[i].desiredBounds;
							EditorUtility.SetDirty(targets[i]);
						}
					}
				}

				if (GUILayout.Button("Snap position to nearest tile")) {
					for (int i = 0; i < targets.Length; i++) {
						var navmeshPrefab = targets[i] as NavmeshPrefab;
						navmeshPrefab.SnapToClosestTileAlignment();
					}
				}
			}
			Section("Settings");
			PropertyField("applyOnStart");
			PropertyField("removeTilesWhenDisabled");
			Section("Serialized Data");
			EditorGUILayout.BeginHorizontal();
			PropertyField("serializedNavmesh");

#if UNITY_2021_3_OR_NEWER
			EditorGUI.showMixedValue = targets.Length > 1;
			var target = this.target as NavmeshPrefab;
			GUILayout.Label(EditorUtility.FormatBytes(target.serializedNavmesh != null ? target.serializedNavmesh.dataSize : 0), GUILayout.ExpandWidth(false));
			EditorGUI.showMixedValue = false;
#endif
			EditorGUILayout.EndHorizontal();

			if (UnityEditor.Progress.Exists(pendingScanProgressId)) {
				var r = EditorGUILayout.GetControlRect();
				EditorGUI.ProgressBar(r, UnityEditor.Progress.GetProgress(pendingScanProgressId), "Scanning...");
				Repaint();
			} else {
				EditorGUI.BeginDisabledGroup(needsRounding || isPrefab || graph == null);
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("Scan & Save")) {
					var pendingScanProgress = new Promise<NavmeshPrefab.SerializedOutput>[targets.Length];
					var pendingScanTargets = new NavmeshPrefab[targets.Length];
					for (int i = 0; i < targets.Length; i++) {
						var navmeshPrefab = targets[i] as NavmeshPrefab;
						pendingScanProgress[i] = navmeshPrefab.ScanAsync(graph);
						// this.pendingScanProgress[i].Complete();
						pendingScanTargets[i] = navmeshPrefab;
					}

					pendingScanProgressId = PendingScan(pendingScanProgress, pendingScanTargets);
				}
				EditorGUI.EndDisabledGroup();
				EditorGUI.BeginDisabledGroup(isPrefab || graph == null);
				if (GUILayout.Button("Edit graph", GUILayout.ExpandWidth(false))) {
					Selection.activeGameObject = AstarPath.active.gameObject;
					AstarPath.active.showGraphs = true;
				}
				EditorGUI.EndDisabledGroup();
				EditorGUILayout.EndHorizontal();
				if (isPrefab) {
					EditorGUILayout.HelpBox("Open the prefab or add it to a scene to scan it.", MessageType.Info);
				} else if (graph == null) {
					EditorGUILayout.HelpBox("No recast graph was found in the scene. Add one if you want to scan this navmesh prefab.", MessageType.Info);
				}
			}
		}
	}
}
