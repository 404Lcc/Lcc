using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Compilation;
using UnityEditor.Scripting.ScriptCompilation;

namespace Pathfinding {
	internal class WelcomeScreen : UnityEditor.EditorWindow {
		[SerializeField]
		private VisualTreeAsset m_VisualTreeAsset = default;

		public bool isImportingSamples;
		private bool askedAboutQuitting;

		[InitializeOnLoadMethod]
		public static void TryCreate () {
			if (!PathfindingEditorSettings.instance.hasShownWelcomeScreen) {
				// Wait a bit before showing the window to avoid stuttering
				// as all the other windows in Unity load.
				// This makes the animation smoother.
				var delay = 0.5f;
				var t0 = Time.realtimeSinceStartup;
				EditorApplication.CallbackFunction create = null;
				create = () => {
					if (Time.realtimeSinceStartup - t0 > delay) {
						EditorApplication.update -= create;
						PathfindingEditorSettings.instance.hasShownWelcomeScreen = true;
						PathfindingEditorSettings.instance.Save();
						Create();
					}
				};
				EditorApplication.update += create;
			}
		}

		public static void Create () {
			var window = GetWindow<WelcomeScreen>(
				true,
				"A* Pathfinding Project",
				true
				);
			window.minSize = window.maxSize = new Vector2(400, 400*1.618f);
			window.ShowUtility();
		}


		public void CreateGUI () {
			VisualElement root = rootVisualElement;

			VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
			root.Add(labelFromUXML);

			var sampleButton = root.Query<Button>("importSamples").First();
			var samplesImportedIndicator = root.Query("samplesImported").First();
			samplesImportedIndicator.visible = GetSamples(out var sample) && sample.isImported;

			sampleButton.clicked += ImportSamples;
			root.Query<Button>("documentation").First().clicked += OpenDocumentation;
			root.Query<Button>("getStarted").First().clicked += OpenGetStarted;
			root.Query<Button>("changelog").First().clicked += OpenChangelog;
			root.Query<Label>("version").First().text = "Version " + AstarPath.Version.ToString();
			AnimateLogo(root.Query("logo").First());
		}

		static string FirstSceneToLoad = "Recast3D";

		public void OnEnable () {
			if (isImportingSamples) {
				// This will be after the domain reload that happened after the samples were imported
				OnPostImportedSamples();
			}
		}

		public void OnPostImportedSamples () {
			isImportingSamples = false;
			// Load the example scene
			var sample = UnityEditor.PackageManager.UI.Sample.FindByPackage("com.arongranberg.astar", "").First();
			if (sample.isImported) {
				var relativePath = "Assets/" + System.IO.Path.GetRelativePath(Application.dataPath, sample.importPath);
				Debug.Log(relativePath);
				var scenes = AssetDatabase.FindAssets("t:scene", new string[] { relativePath });
				string bestScene = null;
				for (int i = 0; i < scenes.Length; i++) {
					scenes[i] = AssetDatabase.GUIDToAssetPath(scenes[i]);
					if (scenes[i].Contains(FirstSceneToLoad)) {
						bestScene = scenes[i];
					}
				}
				if (bestScene == null) bestScene = scenes.FirstOrDefault();
				if (bestScene != null) {
					if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
						EditorSceneManager.OpenScene(bestScene);
					}
				}
			}
		}

		void AnimateLogo (VisualElement logo) {
			var t0 = Time.realtimeSinceStartup;
			EditorApplication.CallbackFunction introAnimation = null;
			int ticks = 0;
			introAnimation = () => {
				var t = Time.realtimeSinceStartup - t0;
				if (ticks == 1) {
					logo.RemoveFromClassList("largeIconEntry");
				}
				Repaint();
				ticks++;
				if (ticks > 1 && t > 5) {
					EditorApplication.update -= introAnimation;
				}
			};
			EditorApplication.update += introAnimation;
		}

		bool GetSamples (out UnityEditor.PackageManager.UI.Sample sample) {
			sample = default;

			System.Collections.Generic.IEnumerable<UnityEditor.PackageManager.UI.Sample> samples;
			try {
				samples = UnityEditor.PackageManager.UI.Sample.FindByPackage("com.arongranberg.astar", "");
				if (samples == null) {
					return false;
				}
			} catch (System.NullReferenceException) {
				// The package manager api is buggy, and will throw an exception if the package is not installed
				// via the package manager.
				// In any case, we can't import samples if the package is not installed via the package manager.
				return false;
			}

			var samplesArr = samples.ToArray();
			if (samplesArr.Length != 1) {
				Debug.LogError("Expected exactly 1 sample. Found " + samplesArr.Length + ". This should not happen");
				return false;
			}

			sample = samplesArr[0];
			return true;
		}

		private void ImportSamples () {
			if (!GetSamples(out var sample)) {
				Debug.LogError("The A* Pathfinding Project is not installed via the Unity package manager. Cannot import samples.");
				return;
			}

			if (sample.isImported) {
				// Show dialog box
				if (!EditorUtility.DisplayDialog("Import samples", "Samples are already imported. Do you want to reimport them?", "Reimport", "Cancel")) {
					return;
				}
			}

			isImportingSamples = true;

			CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;

			if (!sample.Import(UnityEditor.PackageManager.UI.Sample.ImportOptions.OverridePreviousImports)) {
				Debug.LogError("Failed to import samples");
				return;
			}

			OnPostImportedSamples();
		}

		void OnAssemblyCompilationFinished (string assembly, CompilerMessage[] message) {
			for (int i = 0; i < message.Length; i++) {
				// E.g.
				// error CS0006: Metadata file 'Assets/AstarPathfindingProject/Plugins/Clipper/Pathfinding.Clipper2Lib.dll' could not be found
				// error CS0006: Metadata file 'Assets/AstarPathfindingProject/Plugins/DotNetZip/Pathfinding.Ionic.Zip.Reduced.dll' could not be found
				// I believe this can happen if the user previously has had the package imported into the Assets folder (e.g. version 4),
				// and then it is imported via the package manager, and the samples imported.
				// Unity seems to miss that the dll files now have new locations, and gets confused.
				if (message[i].type == CompilerMessageType.Error && message[i].message.Contains("CS0006")) {
					Debug.LogError("Compilation failed due to a Unity bug. Asking user to restart Unity.");

					if (!askedAboutQuitting) {
						if (EditorUtility.DisplayDialog("Restart Unity", "Your version of Unity has a bug that, unfortunately, requires the editor to be restarted, after importing the samples.", "Quit Unity", "Cancel")) {
							askedAboutQuitting = true;
							EditorApplication.update += () => {
								if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
									EditorApplication.Exit(0);
								}
							};
						}
					}
				}
			}
		}

		private void OpenDocumentation () {
			Application.OpenURL(AstarUpdateChecker.GetURL("documentation"));
		}

		private void OpenGetStarted () {
			Application.OpenURL(AstarUpdateChecker.GetURL("documentation") + "getstarted.html");
		}

		private void OpenChangelog () {
			Application.OpenURL(AstarUpdateChecker.GetURL("changelog"));
		}
	}
}
