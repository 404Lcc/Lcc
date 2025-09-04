using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Pathfinding.Graphs.Util;
using Pathfinding.Util;

namespace Pathfinding {
	[CustomEditor(typeof(AstarPath))]
	public class AstarPathEditor : Editor {
		/// <summary>List of all graph editors available (e.g GridGraphEditor)</summary>
		static Dictionary<string, CustomGraphEditorAttribute> graphEditorTypes = new Dictionary<string, CustomGraphEditorAttribute>();

		/// <summary>
		/// Holds node counts for each graph to avoid calculating it every frame.
		/// Only used for visualization purposes
		/// </summary>
		static Dictionary<NavGraph, (float, int, int)> graphNodeCounts;

		/// <summary>List of all graph editors for the graphs. May be larger than script.data.graphs.Length</summary>
		GraphEditor[] graphEditors;

		System.Type[] graphTypes => AstarData.graphTypes;

		static int lastUndoGroup = -1000;

		/// <summary>Used to make sure correct behaviour when handling undos</summary>
		static uint ignoredChecksum;

		const string scriptsFolder = "Assets/AstarPathfindingProject";

		#region SectionFlags

		static bool showSettings, showCustomAreaColors, showTagNames;

		FadeArea settingsArea, colorSettingsArea, editorSettingsArea, aboutArea, optimizationSettingsArea, serializationSettingsArea;
		FadeArea tagsArea, graphsArea, addGraphsArea, alwaysVisibleArea;

		/// <summary>Graph editor which has its 'name' field focused</summary>
		GraphEditor graphNameFocused;

		#endregion

		/// <summary>AstarPath instance that is being inspected</summary>
		public AstarPath script { get; private set; }
		public bool isPrefab { get; private set; }

		#region Styles

		static bool stylesLoaded;
		public static GUISkin astarSkin { get; private set; }

		static GUIStyle level0AreaStyle, level0LabelStyle;
		static GUIStyle level1AreaStyle, level1LabelStyle;

		static GUIStyle graphDeleteButtonStyle, graphInfoButtonStyle, graphGizmoButtonStyle, graphEditNameButtonStyle, graphDuplicateButtonStyle;

		public static GUIStyle helpBox  { get; private set; }
		public static GUIStyle thinHelpBox  { get; private set; }

		#endregion

		/// <summary>Holds defines found in script files, used for optimizations.</summary>
		List<OptimizationHandler.DefineDefinition> defines;

		/// <summary>Enables editor stuff. Loads graphs, reads settings and sets everything up</summary>
		public void OnEnable () {
			script = target as AstarPath;
			isPrefab = PrefabUtility.IsPartOfPrefabAsset(script);

			// Make sure all references are set up to avoid NullReferenceExceptions
			script.colorSettings.PushToStatic();

			if (!isPrefab) HideToolsWhileActive();

			Undo.undoRedoPerformed += OnUndoRedoPerformed;

			FindGraphTypes();
			GetAstarEditorSettings();
			LoadStyles();

			// Load graphs only when not playing, or in extreme cases, when data.graphs is null
			if ((!Application.isPlaying && (script.data.graphs == null || script.data.graphs.Length == 0)) || script.data.graphs == null) {
				DeserializeGraphs();
			}

			CreateFadeAreas();
		}

		/// <summary>
		/// Hide position/rotation/scale tools for the AstarPath object. Instead, OnSceneGUI will draw position tools for each graph.
		///
		/// We cannot rely on the inspector's OnEnable/OnDisable events, because they are tied to the lifetime of the inspector,
		/// which does not necessarily follow which object is selected. In particular if there are multiple inspector windows, or
		/// an inspector window is locked.
		/// </summary>
		void HideToolsWhileActive () {
			EditorApplication.CallbackFunction toolsCheck = null;
			var activelyHidden = true;
			Tools.hidden = true;

			AssemblyReloadEvents.AssemblyReloadCallback onAssemblyReload = () => {
				if (activelyHidden) {
					Tools.hidden = false;
					activelyHidden = false;
				}
			};
			// Ensure that the tools become visible when Unity reloads scripts.
			// To avoid it getting stuck in the hidden state.
			AssemblyReloadEvents.beforeAssemblyReload += onAssemblyReload;
			toolsCheck = () => {
				// This will trigger if the inspector is disabled
				if (script == null) {
					EditorApplication.update -= toolsCheck;
					AssemblyReloadEvents.beforeAssemblyReload -= onAssemblyReload;
					if (activelyHidden) {
						Tools.hidden = false;
						activelyHidden = false;
					}
					return;
				}
				if (Selection.activeGameObject == script.gameObject) {
					Tools.hidden = true;
					activelyHidden = true;
				} else if (activelyHidden) {
					Tools.hidden = false;
					activelyHidden = false;
				}
			};
			EditorApplication.update += toolsCheck;
		}

		void CreateFadeAreas () {
			if (settingsArea == null) {
				aboutArea                 = new FadeArea(false, this, level0AreaStyle, level0LabelStyle);
				optimizationSettingsArea  = new FadeArea(false, this, level0AreaStyle, level0LabelStyle);
				graphsArea                = new FadeArea(script.showGraphs, this, level0AreaStyle, level0LabelStyle);
				serializationSettingsArea = new FadeArea(false, this, level0AreaStyle, level0LabelStyle);
				settingsArea              = new FadeArea(showSettings, this, level0AreaStyle, level0LabelStyle);

				addGraphsArea             = new FadeArea(false, this, level1AreaStyle, level1LabelStyle);
				colorSettingsArea         = new FadeArea(false, this, level1AreaStyle, level1LabelStyle);
				editorSettingsArea        = new FadeArea(false, this, level1AreaStyle, level1LabelStyle);
				alwaysVisibleArea         = new FadeArea(true, this, level1AreaStyle, level1LabelStyle);
				tagsArea                  = new FadeArea(showTagNames, this, level1AreaStyle, level1LabelStyle);
			}
		}

		/// <summary>Cleans up editor stuff</summary>
		public void OnDisable () {
			Undo.undoRedoPerformed -= OnUndoRedoPerformed;
			SetAstarEditorSettings();
			script = null;
		}

		/// <summary>Reads settings frome EditorPrefs</summary>
		void GetAstarEditorSettings () {
			FadeArea.fancyEffects = EditorPrefs.GetBool("EditorGUILayoutx.fancyEffects", true);
		}

		void SetAstarEditorSettings () {
			EditorPrefs.SetBool("EditorGUILayoutx.fancyEffects", FadeArea.fancyEffects);
		}

		void RepaintSceneView () {
			if (!Application.isPlaying || EditorApplication.isPaused) SceneView.RepaintAll();
		}

		/// <summary>Tell Unity that we want to use the whole inspector width</summary>
		public override bool UseDefaultMargins () {
			return false;
		}

		public override void OnInspectorGUI () {
			if (!LoadStyles()) {
				EditorGUILayout.HelpBox("The GUISkin 'AstarEditorSkin.guiskin' in the folder "+EditorResourceHelper.editorAssets+"/ was not found or some custom styles in it does not exist.\n"+
					"This file is required for the A* Pathfinding Project editor.\n\n"+
					"If you are trying to add A* to a new project, please do not copy the files outside Unity, "+
					"export them as a UnityPackage and import them to this project or download the package from the Asset Store"+
					"or the 'scripts only' package from the A* Pathfinding Project website.\n\n\n"+
					"Skin loading is done in the AstarPathEditor.cs --> LoadStyles method", MessageType.Error);
				return;
			}

#if ASTAR_ATAVISM
			EditorGUILayout.HelpBox("This is a special version of the A* Pathfinding Project for Atavism. This version only supports scanning recast graphs and exporting them, but no pathfinding during runtime.", MessageType.Info);
#endif

			EditorGUI.BeginChangeCheck();

			Undo.RecordObject(script, "A* inspector");

			CheckGraphEditors();

			EditorGUI.indentLevel = 1;

			// Apparently these can sometimes get eaten by unity components
			// so I catch them here for later use
			EventType storedEventType = Event.current.type;
			string storedEventCommand = Event.current.commandName;

			DrawMainArea();

			GUILayout.Space(5);

			if (isPrefab) {
				EditorGUI.BeginDisabledGroup(true);
				GUILayout.Button(new GUIContent("Scan", "Cannot recalculate graphs on prefabs"));
				EditorGUI.EndDisabledGroup();
			} else if (GUILayout.Button(new GUIContent("Scan", "Recalculate all graphs. Shortcut cmd+alt+s ( ctrl+alt+s on windows )"))) {
				RunTask(MenuScan);
			}


			// Handle undo
			SaveGraphsAndUndo(storedEventType, storedEventCommand);


			if (EditorGUI.EndChangeCheck()) {
				RepaintSceneView();
				EditorUtility.SetDirty(script);
			}
		}

		/// <summary>
		/// Loads GUISkin and sets up styles.
		/// See: EditorResourceHelper.LocateEditorAssets
		/// Returns: True if all styles were found, false if there was an error somewhere
		/// </summary>
		public static bool LoadStyles () {
			if (stylesLoaded) return true;

			// Dummy styles in case the loading fails
			var inspectorSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);

			if (!EditorResourceHelper.LocateEditorAssets()) {
				return false;
			}

			var skinPath = EditorResourceHelper.editorAssets + "/AstarEditorSkin" + (EditorGUIUtility.isProSkin ? "Dark" : "Light") + ".guiskin";
			astarSkin = AssetDatabase.LoadAssetAtPath(skinPath, typeof(GUISkin)) as GUISkin;

			if (astarSkin != null) {
				astarSkin.button = inspectorSkin.button;
			} else {
				Debug.LogWarning("Could not load editor skin at '" + skinPath + "'");
				return false;
			}

			level0AreaStyle = astarSkin.FindStyle("PixelBox");

			// If the first style is null, then the rest are likely corrupted as well
			// Probably due to the user not copying meta files
			if (level0AreaStyle == null) {
				return false;
			}

			level1LabelStyle = astarSkin.FindStyle("BoxHeader");
			level0LabelStyle = astarSkin.FindStyle("TopBoxHeader");

			level1AreaStyle = astarSkin.FindStyle("PixelBox3");
			graphDeleteButtonStyle = astarSkin.FindStyle("PixelButton");
			graphInfoButtonStyle = astarSkin.FindStyle("InfoButton");
			graphGizmoButtonStyle = astarSkin.FindStyle("GizmoButton");
			graphEditNameButtonStyle = astarSkin.FindStyle("EditButton");
			graphDuplicateButtonStyle = astarSkin.FindStyle("DuplicateButton");

			helpBox = inspectorSkin.FindStyle("HelpBox") ?? inspectorSkin.box;
			thinHelpBox = astarSkin.FindStyle("Banner");

			stylesLoaded = true;
			return true;
		}

		/// <summary>Draws the main area in the inspector</summary>
		void DrawMainArea () {
			CheckGraphEditors();

			graphsArea.Begin();
			graphsArea.Header("Graphs", ref script.showGraphs);

			if (graphsArea.BeginFade()) {
				bool anyNonNull = false;
				for (int i = 0; i < script.graphs.Length; i++) {
					if (script.graphs[i] != null && script.graphs[i].showInInspector) {
						anyNonNull = true;
						DrawGraph(graphEditors[i]);
					}
				}

				// Draw the Add Graph button
				addGraphsArea.Begin();
				addGraphsArea.open |= !anyNonNull;
				addGraphsArea.Header("Add New Graph");

				if (addGraphsArea.BeginFade()) {
					script.data.FindGraphTypes();
					for (int i = 0; i < graphTypes.Length; i++) {
						if (graphEditorTypes.ContainsKey(graphTypes[i].Name)) {
							if (GUILayout.Button(graphEditorTypes[graphTypes[i].Name].displayName)) {
								addGraphsArea.open = false;
								AddGraph(graphTypes[i]);
							}
						} else if (!graphTypes[i].Name.Contains("Base") && graphTypes[i] != typeof(LinkGraph)) {
							EditorGUI.BeginDisabledGroup(true);
							GUILayout.Label(graphTypes[i].Name + " (no editor found)", "Button");
							EditorGUI.EndDisabledGroup();
						}
					}
				}
				addGraphsArea.End();
			}

			graphsArea.End();

			DrawSettings();
			DrawSerializationSettings();
			DrawOptimizationSettings();
			DrawAboutArea();

			bool showNavGraphs = EditorGUILayout.Toggle("Show Graphs", script.showNavGraphs);
			if (script.showNavGraphs != showNavGraphs) {
				script.showNavGraphs = showNavGraphs;
				RepaintSceneView();
			}
		}

		/// <summary>Draws optimizations settings.</summary>
		void DrawOptimizationSettings () {
			optimizationSettingsArea.Begin();
			optimizationSettingsArea.Header("Optimization");

			if (optimizationSettingsArea.BeginFade()) {
				defines = defines ?? OptimizationHandler.FindDefines();

				EditorGUILayout.HelpBox("Using C# pre-processor directives, performance and memory usage can be improved by disabling features that you don't use in the project.\n" +
					"Every change to these settings requires recompiling the scripts", MessageType.Info);

				foreach (var define in defines) {
					EditorGUILayout.Separator();

					var label = new GUIContent(ObjectNames.NicifyVariableName(define.name), define.description);
					define.enabled = EditorGUILayout.Toggle(label, define.enabled);
					EditorGUILayout.HelpBox(define.description, MessageType.None);

					if (!define.consistent) {
						GUIUtilityx.PushTint(Color.red);
						EditorGUILayout.HelpBox("This define is not consistent for all build targets, some have it enabled enabled some have it disabled. Press Apply to change them to the same value", MessageType.Error);
						GUIUtilityx.PopTint();
					}
				}

				EditorGUILayout.Separator();
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Apply", GUILayout.Width(150))) {
					RunTask(() => {
						if (EditorUtility.DisplayDialog("Apply Optimizations", "Applying optimizations requires (in case anything changed) a recompilation of the scripts. The inspector also has to be reloaded. Do you want to continue?", "Ok", "Cancel")) {
							OptimizationHandler.ApplyDefines(defines);
							AssetDatabase.Refresh();
							defines = null;
						}
					});
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}

			optimizationSettingsArea.End();
		}

		/// <summary>
		/// Returns a version with all fields fully defined.
		/// This is used because by default new Version(3,0,0) > new Version(3,0).
		/// This is not the desired behaviour so we make sure that all fields are defined here
		/// </summary>
		public static System.Version FullyDefinedVersion (System.Version v) {
			return new System.Version(Mathf.Max(v.Major, 0), Mathf.Max(v.Minor, 0), Mathf.Max(v.Build, 0), Mathf.Max(v.Revision, 0));
		}

		void DrawAboutArea () {
			aboutArea.Begin();

			GUILayout.BeginHorizontal();

			if (GUILayout.Button("About", level0LabelStyle)) {
				aboutArea.open = !aboutArea.open;
				GUI.changed = true;
			}

#if !ASTAR_ATAVISM
			System.Version newVersion = AstarUpdateChecker.latestVersion;
			bool beta = false;

			// Check if either the latest release version or the latest beta version is newer than this version
			if (FullyDefinedVersion(AstarUpdateChecker.latestVersion) > FullyDefinedVersion(AstarPath.Version) || FullyDefinedVersion(AstarUpdateChecker.latestBetaVersion) > FullyDefinedVersion(AstarPath.Version)) {
				if (FullyDefinedVersion(AstarUpdateChecker.latestVersion) <= FullyDefinedVersion(AstarPath.Version)) {
					newVersion = AstarUpdateChecker.latestBetaVersion;
					beta = true;
				}
			}

			// Check if the latest version is newer than this version
			if (FullyDefinedVersion(newVersion) > FullyDefinedVersion(AstarPath.Version)
				) {
				GUIUtilityx.PushTint(Color.green);
				if (GUILayout.Button((beta ? "Beta" : "New") + " version available! "+newVersion, thinHelpBox)) {
					Application.OpenURL(AstarUpdateChecker.GetURL("download"));
				}
				GUIUtilityx.PopTint();
				GUILayout.Space(20);
			}
#endif

			GUILayout.EndHorizontal();

			if (aboutArea.BeginFade()) {
				GUILayout.Label("The A* Pathfinding Project was made by Aron Granberg\nYour current version is "+AstarPath.Version);

#if !ASTAR_ATAVISM
				if (FullyDefinedVersion(newVersion) > FullyDefinedVersion(AstarPath.Version)) {
					EditorGUILayout.HelpBox("A new "+(beta ? "beta " : "")+"version of the A* Pathfinding Project is available, the new version is "+
						newVersion, MessageType.Info);

					if (GUILayout.Button("What's new?")) {
						Application.OpenURL(AstarUpdateChecker.GetURL(beta ? "beta_changelog" : "changelog"));
					}

					if (GUILayout.Button("Click here to find out more")) {
						Application.OpenURL(AstarUpdateChecker.GetURL("findoutmore"));
					}

					GUIUtilityx.PushTint(new Color(0.3F, 0.9F, 0.3F));

					if (GUILayout.Button("Download new version")) {
						Application.OpenURL(AstarUpdateChecker.GetURL("download"));
					}

					GUIUtilityx.PopTint();
				}
#endif

				if (GUILayout.Button(new GUIContent("Documentation", "Open the documentation for the A* Pathfinding Project"))) {
					Application.OpenURL(AstarUpdateChecker.GetURL("documentation"));
				}

				if (GUILayout.Button(new GUIContent("Project Homepage", "Open the homepage for the A* Pathfinding Project"))) {
					Application.OpenURL(AstarUpdateChecker.GetURL("homepage"));
				}
			}

			aboutArea.End();
		}

		void DrawGraphHeader (GraphEditor graphEditor) {
			var graph = graphEditor.target;

			// Graph guid, just used to get a unique value
			string graphGUIDString = graph.guid.ToString();

			GUILayout.BeginHorizontal();

			if (graphNameFocused == graphEditor) {
				GUI.SetNextControlName(graphGUIDString);
				graph.name = GUILayout.TextField(graph.name ?? "", level1LabelStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));

				// Mark the name field as deselected when it has been deselected or when the user presses Return or Escape
				if ((Event.current.type == EventType.Repaint && GUI.GetNameOfFocusedControl() != graphGUIDString) || (Event.current.type == EventType.KeyUp && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Escape))) {
					if (Event.current.type == EventType.KeyUp) Event.current.Use();
					graphNameFocused = null;
				}
			} else {
				// If the graph name text field is not focused and the graph name is empty, then fill it in
				if (graph.name == null || graph.name == "") graph.name = graphEditorTypes[graph.GetType().Name].displayName;

				if (GUILayout.Button(graph.name, level1LabelStyle)) {
					graphEditor.fadeArea.open = graph.open = !graph.open;
					if (!graph.open) {
						graph.infoScreenOpen = false;
					}
					RepaintSceneView();
				}
			}

			// The OnInspectorGUI method ensures that the scene view is repainted when gizmos are toggled on or off by checking for EndChangeCheck
			graph.drawGizmos = GUILayout.Toggle(graph.drawGizmos, new GUIContent("Draw Gizmos", "Draw Gizmos"), graphGizmoButtonStyle);

			if (GUILayout.Button(new GUIContent("", "Edit Name"), graphEditNameButtonStyle)) {
				graphNameFocused = graphEditor;
				GUI.FocusControl(graphGUIDString);
			}

			if (GUILayout.Toggle(graph.infoScreenOpen, new GUIContent("Info", "Info"), graphInfoButtonStyle)) {
				if (!graph.infoScreenOpen) {
					graphEditor.infoFadeArea.open = graph.infoScreenOpen = true;
					graphEditor.fadeArea.open = graph.open = true;
				}
			} else {
				graphEditor.infoFadeArea.open = graph.infoScreenOpen = false;
			}

			if (GUILayout.Button(new GUIContent("Duplicate", "Duplicate"), graphDuplicateButtonStyle)) {
				DuplidateGraph(graph);
			}

			if (GUILayout.Button(new GUIContent("Delete", "Delete"), graphDeleteButtonStyle)) {
				RemoveGraph(graph);
			}
			GUILayout.EndHorizontal();
		}

		void DrawGraphInfoArea (GraphEditor graphEditor) {
			graphEditor.infoFadeArea.Begin();

			if (graphEditor.infoFadeArea.BeginFade()) {
				int total = 0;
				int numWalkable = 0;

				// Calculate number of nodes in the graph
				(float, int, int)pair;
				graphNodeCounts = graphNodeCounts ?? new Dictionary<NavGraph, (float, int, int)>();

				if (!graphNodeCounts.TryGetValue(graphEditor.target, out pair) || (Time.realtimeSinceStartup-pair.Item1) > 2) {
					graphEditor.target.GetNodes(node => {
						// Guard against bad user-implemented graphs
						if (node != null) {
							total++;
							if (node.Walkable) numWalkable++;
						}
					});
					pair = (Time.realtimeSinceStartup, total, numWalkable);
					graphNodeCounts[graphEditor.target] = pair;
				}

				total = pair.Item2;
				numWalkable = pair.Item3;

				EditorGUI.indentLevel++;

				EditorGUILayout.LabelField("Nodes", total.ToString());
				EditorGUILayout.LabelField("Walkable", numWalkable.ToString());
				EditorGUILayout.LabelField("Unwalkable", (total-numWalkable).ToString());
				if (!graphEditor.target.isScanned) EditorGUILayout.HelpBox("The graph is not scanned", MessageType.Info);

				EditorGUI.indentLevel--;
			}

			graphEditor.infoFadeArea.End();
		}

		/// <summary>Draws the inspector for the given graph with the given graph editor</summary>
		void DrawGraph (GraphEditor graphEditor) {
			graphEditor.fadeArea.Begin();
			DrawGraphHeader(graphEditor);

			if (graphEditor.fadeArea.BeginFade()) {
				DrawGraphInfoArea(graphEditor);
				graphEditor.OnInspectorGUI(graphEditor.target);
				graphEditor.OnBaseInspectorGUI(graphEditor.target);
			}

			graphEditor.fadeArea.End();
		}

		public void OnSceneGUI () {
			script = target as AstarPath;

			DrawSceneGUISettings();

			// OnSceneGUI may be called from EditorUtility.DisplayProgressBar
			// which is called repeatedly while the graphs are scanned in the
			// editor. However running the OnSceneGUI method while the graphs
			// are being scanned is a bad idea since it can interfere with
			// scanning, especially by serializing changes
			if (script.isScanning) {
				return;
			}

			script.colorSettings.PushToStatic();
			EditorGUI.BeginChangeCheck();

			if (!LoadStyles()) return;

			// Some GUI controls might change this to Used, so we need to grab it here
			EventType et = Event.current.type;

			CheckGraphEditors();
			for (int i = 0; i < script.graphs.Length; i++) {
				NavGraph graph = script.graphs[i];
				if (graph != null && graphEditors[i] != null) {
					graphEditors[i].OnSceneGUI(graph);
				}
			}

			SaveGraphsAndUndo(et);

			if (EditorGUI.EndChangeCheck()) {
				EditorUtility.SetDirty(target);
			}
		}

		void DrawSceneGUISettings () {
			var darkSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);

			Handles.BeginGUI();
			float width = 180;
			float height = 76;
			float margin = 10;

			var origWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 144;

			GUILayout.BeginArea(new Rect(Camera.current.pixelWidth/EditorGUIUtility.pixelsPerPoint - width, Camera.current.pixelHeight/EditorGUIUtility.pixelsPerPoint - height, width - margin, height - margin), "Graph Display", astarSkin.FindStyle("SceneBoxDark"));
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Show Graphs", darkSkin.toggle, astarSkin.FindStyle("ScenePrefixLabel"));
			script.showNavGraphs = EditorGUILayout.Toggle(script.showNavGraphs, darkSkin.toggle);
			EditorGUILayout.EndHorizontal();

			if (GUILayout.Button("Scan", darkSkin.button)) {
				RunTask(MenuScan);
			}

			// Invisible button to capture clicks. This prevents a click inside the box from causing some other GameObject to be selected.
			GUI.Button(new Rect(0, 0, width - margin, height - margin), "", GUIStyle.none);
			GUILayout.EndArea();

			EditorGUIUtility.labelWidth = origWidth;
			Handles.EndGUI();
		}


		TextAsset SaveGraphData (byte[] bytes, TextAsset target = null) {
			string projectPath = System.IO.Path.GetDirectoryName(Application.dataPath) + "/";

			string path;

			if (target != null) {
				path = AssetDatabase.GetAssetPath(target);
			} else {
				// Find a valid file name
				int i = 0;
				do {
					path = "Assets/GraphCaches/GraphCache" + (i == 0 ? "" : i.ToString()) + ".bytes";
					i++;
				} while (System.IO.File.Exists(projectPath+path));
			}

			string fullPath = projectPath + path;
			System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fullPath));
			var fileInfo = new System.IO.FileInfo(fullPath);
			// Make sure we can write to the file
			if (fileInfo.Exists && fileInfo.IsReadOnly)
				fileInfo.IsReadOnly = false;
			System.IO.File.WriteAllBytes(fullPath, bytes);

			AssetDatabase.Refresh();
			return AssetDatabase.LoadAssetAtPath<TextAsset>(path);
		}

		void DrawSerializationSettings () {
			serializationSettingsArea.Begin();
			GUILayout.BeginHorizontal();

			if (GUILayout.Button("Save & Load", level0LabelStyle)) {
				serializationSettingsArea.open = !serializationSettingsArea.open;
			}

			if (script.data.cacheStartup && script.data.file_cachedStartup != null) {
				GUIUtilityx.PushTint(Color.yellow);
				GUILayout.Label("Startup cached", thinHelpBox);
				GUILayout.Space(20);
				GUIUtilityx.PopTint();
			}

			GUILayout.EndHorizontal();

			// This displays the serialization settings
			if (serializationSettingsArea.BeginFade()) {
				script.data.cacheStartup = EditorGUILayout.Toggle(new GUIContent("Cache startup", "If enabled, will cache the graphs so they don't have to be scanned at startup"), script.data.cacheStartup);

				script.data.file_cachedStartup = EditorGUILayout.ObjectField(script.data.file_cachedStartup, typeof(TextAsset), false) as TextAsset;

				if (script.data.cacheStartup && script.data.file_cachedStartup == null) {
					EditorGUILayout.HelpBox("No cache has been generated", MessageType.Error);
				}

				if (script.data.cacheStartup && script.data.file_cachedStartup != null) {
					EditorGUILayout.HelpBox("All graph settings will be replaced with the ones from the cache when the game starts", MessageType.Info);
				}

				GUILayout.BeginHorizontal();

				if (GUILayout.Button("Generate cache")) {
					RunTask(() => {
						var serializationSettings = new Pathfinding.Serialization.SerializeSettings();

						if (isPrefab) {
							if (!EditorUtility.DisplayDialog("Can only save settings", "Only graph settings can be saved when the AstarPath object is a prefab. Instantiate the prefab in a scene to be able to save node data as well.", "Save settings", "Cancel")) {
								return;
							}
						} else {
							serializationSettings.nodes = true;

							if (EditorUtility.DisplayDialog("Scan before generating cache?", "Do you want to scan the graphs before saving the cache.\n" +
								"If the graphs have not been scanned then the cache may not contain node data and then the graphs will have to be scanned at startup anyway.", "Scan", "Don't scan")) {
								MenuScan();
							}
						}

						// Save graphs
						var bytes = script.data.SerializeGraphs(serializationSettings);

						// Store it in a file
						script.data.file_cachedStartup = SaveGraphData(bytes, script.data.file_cachedStartup);
						script.data.cacheStartup = true;
					});
				}

				if (GUILayout.Button("Load from cache")) {
					RunTask(() => {
						if (EditorUtility.DisplayDialog("Are you sure you want to load from cache?", "Are you sure you want to load graphs from the cache, this will replace your current graphs?", "Yes", "Cancel")) {
							script.data.LoadFromCache();
						}
					});
				}

				GUILayout.EndHorizontal();

				GUILayout.Space(5);

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Save to file")) {
					RunTask(() => {
						string path = EditorUtility.SaveFilePanel("Save Graphs", "", "graph.bytes", "bytes");

						if (path != "") {
							var serializationSettings = Pathfinding.Serialization.SerializeSettings.Settings;
							if (isPrefab) {
								if (!EditorUtility.DisplayDialog("Can only save settings", "Only graph settings can be saved when the AstarPath object is a prefab. Instantiate the prefab in a scene to be able to save node data as well.", "Save settings", "Cancel")) {
									return;
								}
							} else {
								if (EditorUtility.DisplayDialog("Include node data?", "Do you want to include node data in the save file. " +
									"If node data is included the graph can be restored completely without having to scan it first.", "Include node data", "Only settings")) {
									serializationSettings.nodes = true;
								}
							}

							if (serializationSettings.nodes && EditorUtility.DisplayDialog("Scan before saving?", "Do you want to scan the graphs before saving? " +
								"\nNot scanning can cause node data to be omitted from the file if the graph is not yet scanned.", "Scan", "Don't scan")) {
								MenuScan();
							}

							uint checksum;
							var bytes = SerializeGraphs(serializationSettings, out checksum);
							Pathfinding.Serialization.AstarSerializer.SaveToFile(path, bytes);

							EditorUtility.DisplayDialog("Done Saving", "Done saving graph data.", "Ok");
						}
					});
				}

				if (GUILayout.Button("Load from file")) {
					RunTask(() => {
						string path = EditorUtility.OpenFilePanel("Load Graphs", "", "");

						if (path != "") {
							try {
								byte[] bytes = Pathfinding.Serialization.AstarSerializer.LoadFromFile(path);
								DeserializeGraphs(bytes);
							} catch (System.Exception e) {
								Debug.LogError("Could not load from file at '"+path+"'\n"+e);
							}
						}
					});
				}

				GUILayout.EndHorizontal();
			}

			serializationSettingsArea.End();
		}

		public void RunTask (System.Action action) {
			EditorApplication.CallbackFunction wrapper = null;
			wrapper = () => {
				EditorApplication.update -= wrapper;
				// Run the callback only if the editor has not been disabled since the task was scheduled
				if (script != null) action();
			};
			EditorApplication.update += wrapper;
		}

		void DrawSettings () {
			settingsArea.Begin();
			settingsArea.Header("Settings", ref showSettings);

			if (settingsArea.BeginFade()) {
				DrawPathfindingSettings();
				DrawDebugSettings();
				DrawColorSettings();
				DrawTagSettings();
				DrawEditorSettings();
			}

			settingsArea.End();
		}

		void DrawPathfindingSettings () {
			alwaysVisibleArea.Begin();
			alwaysVisibleArea.HeaderLabel("Pathfinding");
			alwaysVisibleArea.BeginFade();

#if !ASTAR_ATAVISM
			EditorGUI.BeginDisabledGroup(Application.isPlaying);

			script.threadCount = (ThreadCount)EditorGUILayout.EnumPopup(new GUIContent("Thread Count", "Number of threads to run the pathfinding in (if any). More threads " +
				"can boost performance on multi core systems. \n" +
				"Use None for debugging or if you dont use pathfinding that much.\n " +
				"See docs for more info"), script.threadCount);

			EditorGUI.EndDisabledGroup();

			int threads = AstarPath.CalculateThreadCount(script.threadCount);
			if (threads > 0) EditorGUILayout.HelpBox("Using " + threads +" thread(s)" + (script.threadCount < 0 ? " on your machine" : ""), MessageType.None);
			else EditorGUILayout.HelpBox("Using a single coroutine (no threads)" + (script.threadCount < 0 ? " on your machine" : ""), MessageType.None);
			if (threads > SystemInfo.processorCount) EditorGUILayout.HelpBox("Using more threads than there are CPU cores may not have a positive effect on performance", MessageType.Warning);

			if (script.threadCount == ThreadCount.None) {
				script.maxFrameTime = EditorGUILayout.FloatField(new GUIContent("Max Frame Time", "Max number of milliseconds to use for path calculation per frame"), script.maxFrameTime);
			} else {
				script.maxFrameTime = 10;
			}

			script.maxNearestNodeDistance = EditorGUILayout.FloatField(new GUIContent("Max Nearest Node Distance",
				"Normally, if the nearest node to e.g the start point of a path was not walkable" +
				" a search will be done for the nearest node which is walkble. This is the maximum distance (world units) which it will search"),
				script.maxNearestNodeDistance);

			script.heuristic = (Heuristic)EditorGUILayout.EnumPopup("Heuristic", script.heuristic);

			if (script.heuristic == Heuristic.Manhattan || script.heuristic == Heuristic.Euclidean || script.heuristic == Heuristic.DiagonalManhattan) {
				EditorGUI.indentLevel++;
				script.heuristicScale = EditorGUILayout.FloatField("Heuristic Scale", script.heuristicScale);
				script.heuristicScale = Mathf.Clamp01(script.heuristicScale);
				EditorGUI.indentLevel--;
			}

			GUILayout.Label(new GUIContent("Advanced"), EditorStyles.boldLabel);

			DrawHeuristicOptimizationSettings();

			script.batchGraphUpdates = EditorGUILayout.Toggle(new GUIContent("Batch Graph Updates", "Limit graph updates to only run every x seconds. Can have positive impact on performance if many graph updates are done"), script.batchGraphUpdates);

			if (script.batchGraphUpdates) {
				EditorGUI.indentLevel++;
				script.graphUpdateBatchingInterval = EditorGUILayout.FloatField(new GUIContent("Update Interval (s)", "Minimum number of seconds between each batch of graph updates"), script.graphUpdateBatchingInterval);
				EditorGUI.indentLevel--;
			}

			// Only show if there is actually a navmesh/recast graph in the scene
			// to help reduce clutter for other users.
			if (script.data.FindGraphWhichInheritsFrom(typeof(NavmeshBase)) != null) {
				script.navmeshUpdates.updateInterval = EditorGUILayout.FloatField(new GUIContent("Navmesh Cutting Update Interval (s)", "How often to check if any navmesh cut has changed."), script.navmeshUpdates.updateInterval);
			}
#endif
			script.scanOnStartup = EditorGUILayout.Toggle(new GUIContent("Scan on Awake", "Scan all graphs on Awake. If this is false, you must call AstarPath.active.Scan () yourself. Useful if you want to make changes to the graphs with code."), script.scanOnStartup);

			alwaysVisibleArea.End();
		}

		readonly string[] heuristicOptimizationOptions = new [] {
			"None",
			"Random (low quality)",
			"RandomSpreadOut (high quality)",
			"Custom"
		};

		void DrawHeuristicOptimizationSettings () {
			script.euclideanEmbedding.mode = (HeuristicOptimizationMode)EditorGUILayout.Popup(new GUIContent("Heuristic Optimization"), (int)script.euclideanEmbedding.mode, heuristicOptimizationOptions);

			EditorGUI.indentLevel++;
			if (script.euclideanEmbedding.mode == HeuristicOptimizationMode.Random) {
				script.euclideanEmbedding.spreadOutCount = EditorGUILayout.IntField(new GUIContent("Count", "Number of optimization points, higher numbers give better heuristics and could make it faster, " +
					"but too many could make the overhead too great and slow it down. Try to find the optimal value for your map. Recommended value < 100"), script.euclideanEmbedding.spreadOutCount);
			} else if (script.euclideanEmbedding.mode == HeuristicOptimizationMode.Custom) {
				script.euclideanEmbedding.pivotPointRoot = EditorGUILayout.ObjectField(new GUIContent("Pivot point root",
					"All children of this transform are going to be used as pivot points. " +
					"Recommended count < 100"), script.euclideanEmbedding.pivotPointRoot, typeof(Transform), true) as Transform;
				if (script.euclideanEmbedding.pivotPointRoot == null) {
					EditorGUILayout.HelpBox("Please assign an object", MessageType.Error);
				}
			} else if (script.euclideanEmbedding.mode == HeuristicOptimizationMode.RandomSpreadOut) {
				script.euclideanEmbedding.pivotPointRoot = EditorGUILayout.ObjectField(new GUIContent("Pivot point root",
					"All children of this transform are going to be used as pivot points. " +
					"They will seed the calculation of more pivot points. " +
					"Recommended count < 100"), script.euclideanEmbedding.pivotPointRoot, typeof(Transform), true) as Transform;

				if (script.euclideanEmbedding.pivotPointRoot == null) {
					EditorGUILayout.HelpBox("No root is assigned. A random node will be choosen as the seed.", MessageType.Info);
				}

				script.euclideanEmbedding.spreadOutCount = EditorGUILayout.IntField(new GUIContent("Count", "Number of optimization points, higher numbers give better heuristics and could make it faster, " +
					"but too many could make the overhead too great and slow it down. Try to find the optimal value for your map. Recommended value < 100"), script.euclideanEmbedding.spreadOutCount);
			}

			if (script.euclideanEmbedding.mode != HeuristicOptimizationMode.None) {
				EditorGUILayout.HelpBox("Heuristic optimization assumes the graph remains static. No graph updates, dynamic obstacles or similar should be applied to the graph " +
					"when using heuristic optimization.", MessageType.Info);
			}

			EditorGUI.indentLevel--;
		}

		/// <summary>Opens the A* Inspector and shows the section for editing tags</summary>
		public static void EditTags () {
			AstarPath astar = UnityCompatibility.FindAnyObjectByType<AstarPath>();

			if (astar != null) {
				showTagNames = true;
				showSettings = true;
				Selection.activeGameObject = astar.gameObject;
			} else {
				Debug.LogWarning("No AstarPath component in the scene");
			}
		}

		void DrawTagSettings () {
			tagsArea.Begin();
			tagsArea.Header("Tag Names", ref showTagNames);

			if (tagsArea.BeginFade()) {
				string[] tagNames = script.GetTagNames();

				for (int i = 0; i < tagNames.Length; i++) {
					tagNames[i] = EditorGUILayout.TextField(new GUIContent("Tag "+i, "Name for tag "+i), tagNames[i]);
					if (tagNames[i] == "") tagNames[i] = ""+i;
				}
			}

			tagsArea.End();
		}

		void DrawEditorSettings () {
			editorSettingsArea.Begin();
			editorSettingsArea.Header("Editor");

			if (editorSettingsArea.BeginFade()) {
				FadeArea.fancyEffects = EditorGUILayout.Toggle("Smooth Transitions", FadeArea.fancyEffects);
			}

			editorSettingsArea.End();
		}

		static void DrawColorSlider (ref float left, ref float right, bool editable) {
			GUILayout.BeginHorizontal();
			GUILayout.Space(20);
			GUILayout.BeginVertical();

			GUILayout.Box("", astarSkin.GetStyle("ColorInterpolationBox"));
			GUILayout.BeginHorizontal();
			if (editable) {
				left = EditorGUILayout.IntField((int)left);
			} else {
				GUILayout.Label(left.ToString("0"));
			}
			GUILayout.FlexibleSpace();
			if (editable) {
				right = EditorGUILayout.IntField((int)right);
			} else {
				GUILayout.Label(right.ToString("0"));
			}
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();
			GUILayout.Space(4);
			GUILayout.EndHorizontal();
		}

		void DrawDebugSettings () {
			alwaysVisibleArea.Begin();
			alwaysVisibleArea.HeaderLabel("Debug");
			alwaysVisibleArea.BeginFade();

			script.logPathResults = (PathLog)EditorGUILayout.EnumPopup("Path Logging", script.logPathResults);
			script.debugMode = (GraphDebugMode)EditorGUILayout.EnumPopup("Graph Coloring", script.debugMode);

			if (script.debugMode == GraphDebugMode.SolidColor) {
				EditorGUI.BeginChangeCheck();
				script.colorSettings._SolidColor = EditorGUILayout.ColorField(new GUIContent("Color", "Color used for the graph when 'Graph Coloring'='Solid Color'"), script.colorSettings._SolidColor);
				if (EditorGUI.EndChangeCheck()) {
					script.colorSettings.PushToStatic();
				}
			}

			if (script.debugMode == GraphDebugMode.G || script.debugMode == GraphDebugMode.H || script.debugMode == GraphDebugMode.F || script.debugMode == GraphDebugMode.Penalty) {
				script.manualDebugFloorRoof = !EditorGUILayout.Toggle("Automatic Limits", !script.manualDebugFloorRoof);
				DrawColorSlider(ref script.debugFloor, ref script.debugRoof, script.manualDebugFloorRoof);
			}

			script.showSearchTree = EditorGUILayout.Toggle("Show Search Tree", script.showSearchTree);
			if (script.showSearchTree) {
				EditorGUILayout.HelpBox("Show Search Tree is enabled, you may see rendering glitches in the graph rendering" +
					" while the game is running. This is nothing to worry about and is simply due to the paths being calculated at the same time as the gizmos" +
					" are being rendered. You can pause the game to see an accurate rendering.", MessageType.Info);
			}
			script.showUnwalkableNodes = EditorGUILayout.Toggle("Show Unwalkable Nodes", script.showUnwalkableNodes);

			if (script.showUnwalkableNodes) {
				EditorGUI.indentLevel++;
				script.unwalkableNodeDebugSize = EditorGUILayout.FloatField("Size", script.unwalkableNodeDebugSize);
				EditorGUI.indentLevel--;
			}

			alwaysVisibleArea.End();
		}

		void DrawColorSettings () {
			colorSettingsArea.Begin();
			colorSettingsArea.Header("Colors");

			if (colorSettingsArea.BeginFade()) {
				// Make sure the object is not null
				AstarColor colors = script.colorSettings = script.colorSettings ?? new AstarColor();

				colors._SolidColor = EditorGUILayout.ColorField(new GUIContent("Solid Color", "Color used for the graph when 'Graph Coloring'='Solid Color'"), colors._SolidColor);
				colors._UnwalkableNode = EditorGUILayout.ColorField("Unwalkable Node", colors._UnwalkableNode);
				colors._BoundsHandles = EditorGUILayout.ColorField("Bounds Handles", colors._BoundsHandles);

				colors._ConnectionLowLerp = EditorGUILayout.ColorField("Connection Gradient (low)", colors._ConnectionLowLerp);
				colors._ConnectionHighLerp = EditorGUILayout.ColorField("Connection Gradient (high)", colors._ConnectionHighLerp);

				colors._MeshEdgeColor = EditorGUILayout.ColorField("Mesh Edge", colors._MeshEdgeColor);

				if (EditorResourceHelper.GizmoSurfaceMaterial != null && EditorResourceHelper.GizmoLineMaterial != null) {
					EditorGUI.BeginChangeCheck();
					var col1 = EditorResourceHelper.GizmoSurfaceMaterial.color;
					col1.a = EditorGUILayout.Slider("Navmesh Surface Opacity", col1.a, 0, 1);

					var col2 = EditorResourceHelper.GizmoLineMaterial.color;
					col2.a = EditorGUILayout.Slider("Navmesh Outline Opacity", col2.a, 0, 1);

					var fade = EditorResourceHelper.GizmoSurfaceMaterial.GetColor("_FadeColor");
					fade.a = EditorGUILayout.Slider("Opacity Behind Objects", fade.a, 0, 1);

					if (EditorGUI.EndChangeCheck()) {
						Undo.RecordObjects(new [] { EditorResourceHelper.GizmoSurfaceMaterial, EditorResourceHelper.GizmoLineMaterial }, "Change navmesh transparency");
						EditorResourceHelper.GizmoSurfaceMaterial.color = col1;
						EditorResourceHelper.GizmoLineMaterial.color = col2;
						EditorResourceHelper.GizmoSurfaceMaterial.SetColor("_FadeColor", fade);
						EditorResourceHelper.GizmoLineMaterial.SetColor("_FadeColor", fade * new Color(1, 1, 1, 0.7f));
					}
				}

				colors._AreaColors = colors._AreaColors ?? new Color[0];

				// Custom Area Colors
				showCustomAreaColors = EditorGUILayout.Foldout(showCustomAreaColors, "Custom Area Colors");
				if (showCustomAreaColors) {
					EditorGUI.indentLevel += 2;

					for (int i = 0; i < colors._AreaColors.Length; i++) {
						GUILayout.BeginHorizontal();
						colors._AreaColors[i] = EditorGUILayout.ColorField("Area "+i+(i == 0 ? " (not used usually)" : ""), colors._AreaColors[i]);
						if (GUILayout.Button(new GUIContent("", "Reset to the default color"), astarSkin.FindStyle("SmallReset"), GUILayout.Width(20))) {
							colors._AreaColors[i] = AstarMath.IntToColor(i, 1F);
						}
						GUILayout.EndHorizontal();
					}

					GUILayout.BeginHorizontal();
					EditorGUI.BeginDisabledGroup(colors._AreaColors.Length > 255);

					if (GUILayout.Button("Add New")) {
						Memory.Realloc(ref colors._AreaColors, colors._AreaColors.Length+1);
						colors._AreaColors[colors._AreaColors.Length-1] = AstarMath.IntToColor(colors._AreaColors.Length-1, 1F);
					}

					EditorGUI.EndDisabledGroup();
					EditorGUI.BeginDisabledGroup(colors._AreaColors.Length == 0);

					if (GUILayout.Button("Remove last") && colors._AreaColors.Length > 0) {
						colors._AreaColors = Memory.ShrinkArray(colors._AreaColors, colors._AreaColors.Length-1);
					}

					EditorGUI.EndDisabledGroup();
					GUILayout.EndHorizontal();

					EditorGUI.indentLevel -= 2;
				}

				if (GUI.changed) {
					colors.PushToStatic();
				}
			}

			colorSettingsArea.End();
		}

		/// <summary>Make sure every graph has a graph editor</summary>
		void CheckGraphEditors () {
			var data = script.data;
			data.graphs = data.graphs ?? new NavGraph[0];
			// Ensure graphEditors.Length >= data.graphs.Length
			Memory.Realloc(ref graphEditors, data.graphs.Length);

			for (int i = 0; i < script.graphs.Length; i++) {
				var graph = script.graphs[i];

				if (graph != null && graph.guid == new Pathfinding.Util.Guid()) {
					graph.guid = Pathfinding.Util.Guid.NewGuid();
				}

				if (graph == null || !graph.showInInspector) {
					graphEditors[i] = null;
					continue;
				}

				if (graphEditors[i] == null || graphEditors[i].target != graph) {
					graphEditors[i] = CreateGraphEditor(graph);
				}
			}
		}

		void RemoveGraph (NavGraph graph) {
			script.data.RemoveGraph(graph);
			CheckGraphEditors();
			GUI.changed = true;
			Repaint();
		}

		void DuplidateGraph (NavGraph graph) {
			script.data.DuplicateGraph(graph);
			CheckGraphEditors();
			GUI.changed = true;
			Repaint();
		}

		void AddGraph (System.Type type) {
			script.data.AddGraph(type);
			CheckGraphEditors();
			GUI.changed = true;
		}

		/// <summary>Creates a GraphEditor for a graph</summary>
		GraphEditor CreateGraphEditor (NavGraph graph) {
			var graphType = graph.GetType().Name;
			GraphEditor result;

			if (graphEditorTypes.TryGetValue(graphType, out var graphEditorTypeAttr)) {
				var graphEditorType = graphEditorTypeAttr.editorType;
				result = System.Activator.CreateInstance(graphEditorType) as GraphEditor;

				// Deserialize editor settings
				var editorData = (graph as IGraphInternals).SerializedEditorSettings;
				if (editorData != null) Pathfinding.Serialization.TinyJsonDeserializer.Deserialize(editorData, graphEditorType, result, script.gameObject);
			} else {
				Debug.LogError("Couldn't find an editor for the graph type '" + graphType + "'. There are " + graphEditorTypes.Count + " available graph editors");
				result = new GraphEditor();
				graphEditorTypes[graphType] = new CustomGraphEditorAttribute(graph.GetType(), graphType) {
					editorType = typeof(GraphEditor)
				};
			}

			result.editor = this;
			result.fadeArea = new FadeArea(graph.open, this, level1AreaStyle, level1LabelStyle);
			result.infoFadeArea = new FadeArea(graph.infoScreenOpen, this, null, null);
			result.target = graph;

			result.OnEnable();
			return result;
		}

		void HandleUndo () {
			// The user has tried to undo something, apply that
			DeserializeGraphs();
		}

		void SerializeIfDataChanged () {
			byte[] bytes = SerializeGraphs(out var checksum);

			uint byteHash = Checksum.GetChecksum(bytes);
			uint dataHash = Checksum.GetChecksum(script.data.GetData());
			// Check if the data is different than the previous data, use checksums
			bool isDifferent = checksum != ignoredChecksum && dataHash != byteHash;

			// nly save undo if the data was different from the last saved undo
			if (isDifferent) {
				Undo.RegisterCompleteObjectUndo(script, "A* Graph Settings");
				Undo.IncrementCurrentGroup();
				// Assign the new data
				script.data.SetData(bytes);
				EditorUtility.SetDirty(script);
			}
		}

		/// <summary>Called when an undo or redo operation has been performed</summary>
		void OnUndoRedoPerformed () {
			if (!this) return;

			byte[] bytes = SerializeGraphs(out var checksum);

			// Check if the data is different than the previous data, use checksums
			bool isDifferent = Checksum.GetChecksum(script.data.GetData()) != Checksum.GetChecksum(bytes);

			if (isDifferent) {
				HandleUndo();
			}

			CheckGraphEditors();
			// Deserializing a graph does not necessarily yield the same hash as the data loaded from
			// this is (probably) because editor settings are not saved all the time
			// so we explicitly ignore the new hash
			SerializeGraphs(out checksum);
			ignoredChecksum = checksum;
		}

		public void SaveGraphsAndUndo (EventType et = EventType.Used, string eventCommand = "") {
			// Serialize the settings of the graphs

			// Dont process undo events in editor, we don't want to reset graphs
			// Also don't do this if the graph is being updated as serializing the graph
			// might interfere with that (in particular it might unblock the path queue).
			// Also don't do this if the AstarPath object is not the active one, since serialization uses the singleton in some ways.
			if (Application.isPlaying || script.isScanning || script.IsAnyWorkItemInProgress) {
				return;
			}

			if ((Undo.GetCurrentGroup() != lastUndoGroup || et == EventType.MouseUp) && eventCommand != "UndoRedoPerformed") {
				SerializeIfDataChanged();

				lastUndoGroup = Undo.GetCurrentGroup();
			}

			if (Event.current == null || script.data.GetData() == null) {
				SerializeIfDataChanged();
			}
		}

		public byte[] SerializeGraphs (out uint checksum) {
			return SerializeGraphs(Pathfinding.Serialization.SerializeSettings.Settings, out checksum);
		}

		public byte[] SerializeGraphs (Pathfinding.Serialization.SerializeSettings settings, out uint checksum) {
			CheckGraphEditors();
			// Serialize all graph editors
			var output = new System.Text.StringBuilder();
			for (int i = 0; i < graphEditors.Length; i++) {
				if (graphEditors[i] == null) continue;
				output.Length = 0;
				Pathfinding.Serialization.TinyJsonSerializer.Serialize(graphEditors[i], output);
				(graphEditors[i].target as IGraphInternals).SerializedEditorSettings = output.ToString();
			}
			// Serialize all graphs (including serialized editor data)
			return script.data.SerializeGraphs(settings, out checksum);
		}

		void DeserializeGraphs () {
			// User has cleared the data field. Revert this.
			if (script.data.GetData() == null) script.data.SetData(new byte[0]);
			DeserializeGraphs(script.data.GetData());
		}

		void DeserializeGraphs (byte[] bytes) {
			try {
				if (bytes == null || bytes.Length == 0) {
					script.data.graphs = new NavGraph[0];
				} else {
					script.data.DeserializeGraphs(bytes);
				}
				// Make sure every graph has a graph editor
				CheckGraphEditors();
			} catch (System.Exception e) {
				Debug.LogError("Failed to deserialize graphs");
				Debug.LogException(e);
				script.data.SetData(null);
			}
		}

		[MenuItem("Edit/Pathfinding/Scan All Graphs %&s")]
		public static void MenuScan () {
			if (AstarPath.active == null) {
				AstarPath.FindAstarPath();
				if (AstarPath.active == null) {
					return;
				}
			}

			try {
				var lastMessageTime = Time.realtimeSinceStartup;
				foreach (var p in AstarPath.active.ScanAsync()) {
					// Displaying the progress bar is pretty slow, so don't do it too often
					if (Time.realtimeSinceStartup - lastMessageTime > 0.2f) {
						// Display a progress bar of the scan
						UnityEditor.EditorUtility.DisplayProgressBar("Scanning", p.ToString(), p.progress);
						lastMessageTime = Time.realtimeSinceStartup;
					}
				}

				// Repaint the game view in addition to just the scene view.
				// In case the user only has the game view open it's nice to refresh it so they can see the graph.
				UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
			} catch (System.Exception e) {
				Debug.LogError("There was an error generating the graphs:\n"+e+"\n\nIf you think this is a bug, please contact me on forum.arongranberg.com (post a new thread)\n");
				EditorUtility.DisplayDialog("Error Generating Graphs", "There was an error when generating graphs, check the console for more info", "Ok");
				throw e;
			} finally {
				EditorUtility.ClearProgressBar();
			}
		}

		/// <summary>Searches in the current assembly for GraphEditor and NavGraph types</summary>
		void FindGraphTypes () {
			if (graphEditorTypes.Count > 0) return;

			graphEditorTypes = new Dictionary<string, CustomGraphEditorAttribute>();

			var editorTypes = AssemblySearcher.FindTypesInheritingFrom<GraphEditor>();
			foreach (var type in editorTypes) {
				// Loop through the attributes for the CustomGraphEditorAttribute attribute
				foreach (var attribute in type.GetCustomAttributes(false)) {
					if (attribute is CustomGraphEditorAttribute cge && !System.Type.Equals(cge.graphType, null)) {
						cge.editorType = type;
						graphEditorTypes.Add(cge.graphType.Name, cge);
					}
				}
			}

			// Make sure graph types (not graph editor types) are also up to date
			script.data.FindGraphTypes();
		}
	}
}
