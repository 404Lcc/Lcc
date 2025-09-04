// TODO: Check HDRP custom pass support, and log a warning if it is disabled
#pragma warning disable 649 // Field `Drawing.GizmoContext.activeTransform' is never assigned to, and will always have its default value `null'. Not used outside of the unity editor.
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine.Rendering;
using Unity.Profiling;
#if MODULE_RENDER_PIPELINES_UNIVERSAL
using UnityEngine.Rendering.Universal;
#endif
#if MODULE_RENDER_PIPELINES_HIGH_DEFINITION
using UnityEngine.Rendering.HighDefinition;
#endif

namespace Pathfinding.Drawing {
	/// <summary>Info about the current selection in the editor</summary>
	public static class GizmoContext {
#if UNITY_EDITOR
		static Transform activeTransform;
#endif

		static HashSet<Transform> selectedTransforms = new HashSet<Transform>();

		static internal bool drawingGizmos;
		static internal bool dirty;
		private static int selectionSizeInternal;

		/// <summary>Number of top-level transforms that are selected</summary>
		public static int selectionSize {
			get {
				Refresh();
				return selectionSizeInternal;
			}
			private set {
				selectionSizeInternal = value;
			}
		}

		internal static void SetDirty () {
			dirty = true;
		}

		private static void Refresh () {
#if UNITY_EDITOR
			if (!drawingGizmos) throw new System.Exception("Can only be used inside the ALINE library's gizmo drawing functions.");
			if (dirty) {
				dirty = false;
				DrawingManager.MarkerRefreshSelectionCache.Begin();
				activeTransform = Selection.activeTransform;
				selectedTransforms.Clear();
				// Optimization to avoid allocating an empty array when calling Selection.transforms in many cases
				if (Selection.count > 0) {
					var topLevel = Selection.transforms;
					for (int i = 0; i < topLevel.Length; i++) selectedTransforms.Add(topLevel[i]);
					selectionSize = topLevel.Length;
				} else {
					selectionSize = 0;
				}
				DrawingManager.MarkerRefreshSelectionCache.End();
			}
#endif
		}

		/// <summary>
		/// True if the component is selected.
		/// This is a deep selection: even children of selected transforms are considered to be selected.
		/// </summary>
		public static bool InSelection (Component c) {
			return InSelection(c.transform);
		}

		/// <summary>
		/// True if the transform is selected.
		/// This is a deep selection: even children of selected transforms are considered to be selected.
		/// </summary>
		public static bool InSelection (Transform tr) {
			Refresh();
			var leaf = tr;
			while (tr != null) {
				if (selectedTransforms.Contains(tr)) {
					selectedTransforms.Add(leaf);
					return true;
				}
				tr = tr.parent;
			}
			return false;
		}

		/// <summary>
		/// True if the component is shown in the inspector.
		/// The active selection is the GameObject that is currently visible in the inspector.
		/// </summary>
		public static bool InActiveSelection (Component c) {
			return InActiveSelection(c.transform);
		}

		/// <summary>
		/// True if the transform is shown in the inspector.
		/// The active selection is the GameObject that is currently visible in the inspector.
		/// </summary>
		public static bool InActiveSelection (Transform tr) {
#if UNITY_EDITOR
			Refresh();
			return tr.transform == activeTransform;
#else
			return false;
#endif
		}
	}

	/// <summary>
	/// Every object that wants to draw gizmos should implement this interface.
	/// See: <see cref="Drawing.MonoBehaviourGizmos"/>
	/// </summary>
	public interface IDrawGizmos {
		void DrawGizmos();
	}

	public enum DetectedRenderPipeline {
		BuiltInOrCustom,
		HDRP,
		URP
	}

	/// <summary>
	/// Global script which draws debug items and gizmos.
	/// If a Draw.* method has been used or if any script inheriting from the <see cref="Drawing.MonoBehaviourGizmos"/> class is in the scene then an instance of this script
	/// will be created and put on a hidden GameObject.
	///
	/// It will inject drawing logic into any cameras that are rendered.
	///
	/// Usually you never have to interact with this class.
	/// </summary>
	[ExecuteAlways]
	[AddComponentMenu("")]
	public class DrawingManager : MonoBehaviour {
		public DrawingData gizmos;
		static List<GizmoDrawerGroup> gizmoDrawers = new List<GizmoDrawerGroup>();
		static Dictionary<System.Type, int> gizmoDrawerIndices = new Dictionary<System.Type, int>();
#if UNITY_EDITOR
		static float lastGizmoInfoRefresh = float.NegativeInfinity;
#endif
		static bool ignoreAllDrawing;
		static DrawingManager _instance;
		bool framePassed;
		int lastFrameCount = int.MinValue;
		float lastFrameTime = float.NegativeInfinity;
		int lastFilterFrame;
#if UNITY_EDITOR
		bool builtGizmos;
#endif

		struct GizmoDrawerGroup {
			public System.Type type;
			public ProfilerMarker profilerMarker;
			public List<IDrawGizmos> drawers;
			public bool enabled;
		}

		/// <summary>True if OnEnable has been called on this instance and OnDisable has not</summary>
		[SerializeField]
		bool actuallyEnabled;

		RedrawScope previousFrameRedrawScope;

		/// <summary>
		/// Allow rendering to cameras that render to RenderTextures.
		/// By default cameras which render to render textures are never rendered to.
		/// You may enable this if you wish.
		///
		/// See: <see cref="Drawing.CommandBuilder.cameraTargets"/>
		/// See: advanced (view in online documentation for working links)
		/// </summary>
		public static bool allowRenderToRenderTextures = false;
		public static bool drawToAllCameras = false;

		/// <summary>
		/// Multiply all line widths by this value.
		/// This can be used to make lines thicker or thinner.
		///
		/// This is primarily useful when generating screenshots, and you want to render at a higher resolution before scaling down the image.
		///
		/// It is only read when a camera is being rendered. So it cannot be used to change line thickness on a per-item basis.
		/// Use <see cref="Draw.WithLineWidth"/> for that.
		/// </summary>
		public static float lineWidthMultiplier = 1.0f;

		CommandBuffer commandBuffer;

		[System.NonSerialized]
		DetectedRenderPipeline detectedRenderPipeline = DetectedRenderPipeline.BuiltInOrCustom;

#if MODULE_RENDER_PIPELINES_HIGH_DEFINITION_16_0_0_OR_NEWER
		CustomPass hdrpGlobalPass;
#endif

#if MODULE_RENDER_PIPELINES_UNIVERSAL
		HashSet<ScriptableRenderer> scriptableRenderersWithPass = new HashSet<ScriptableRenderer>();
		AlineURPRenderPassFeature renderPassFeature;
#endif

		private static readonly ProfilerMarker MarkerALINE = new ProfilerMarker("ALINE");
		private static readonly ProfilerMarker MarkerCommandBuffer = new ProfilerMarker("Executing command buffer");
		private static readonly ProfilerMarker MarkerFrameTick = new ProfilerMarker("Frame Tick");
		private static readonly ProfilerMarker MarkerFilterDestroyedObjects = new ProfilerMarker("Filter destroyed objects");
		internal static readonly ProfilerMarker MarkerRefreshSelectionCache = new ProfilerMarker("Refresh Selection Cache");
		private static readonly ProfilerMarker MarkerGizmosAllowed = new ProfilerMarker("GizmosAllowed");
		private static readonly ProfilerMarker MarkerDrawGizmos = new ProfilerMarker("DrawGizmos");
		private static readonly ProfilerMarker MarkerSubmitGizmos = new ProfilerMarker("Submit Gizmos");

		public static DrawingManager instance {
			get {
				if (_instance == null) Init();
				return _instance;
			}
		}

#if UNITY_EDITOR
		[InitializeOnLoadMethod]
#endif
		public static void Init () {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
			if (Unity.Jobs.LowLevel.Unsafe.JobsUtility.IsExecutingJob) throw new System.Exception("Draw.* methods cannot be called from inside a job. See the documentation for info about how to use drawing functions from the Unity Job System.");
#endif
			if (_instance != null) return;

			// Here one might try to look for existing instances of the class that haven't yet been enabled.
			// However, this turns out to be tricky.
			// Resources.FindObjectsOfTypeAll<T>() is the only call that includes HideInInspector GameObjects.
			// But it is hard to distinguish between objects that are internal ones which will never be enabled and objects that will be enabled.
			// Checking .gameObject.scene.isLoaded doesn't work reliably (object may be enabled and working even if isLoaded is false)
			// Checking .gameObject.scene.isValid doesn't work reliably (object may be enabled and working even if isValid is false)

			// So instead we just always create a new instance. This is not a particularly heavy operation and it only happens once per game, so why not.
			// The OnEnable call will clean up duplicate managers if there are any.

			var go = new GameObject("RetainedGizmos") {
				hideFlags = HideFlags.DontSave | HideFlags.NotEditable | HideFlags.HideInInspector | HideFlags.HideInHierarchy
			};
			_instance = go.AddComponent<DrawingManager>();
			if (Application.isPlaying) DontDestroyOnLoad(go);

			if (Application.isBatchMode) {
				// In batch mode, we never want to draw anything.
				// See https://forum.arongranberg.com/t/drawingmanager-holds-on-to-memory-in-batch-mode/17765
				ignoreAllDrawing = true;
				gizmoDrawers.Clear();
				gizmoDrawerIndices.Clear();
			}
		}

		/// <summary>Detects which render pipeline is being used and configures them for rendering</summary>
		void RefreshRenderPipelineMode () {
			var pipelineType = RenderPipelineManager.currentPipeline != null? RenderPipelineManager.currentPipeline.GetType() : null;

#if MODULE_RENDER_PIPELINES_HIGH_DEFINITION
			if (pipelineType == typeof(HDRenderPipeline)) {
				if (detectedRenderPipeline != DetectedRenderPipeline.HDRP) {
					detectedRenderPipeline = DetectedRenderPipeline.HDRP;
#if MODULE_RENDER_PIPELINES_HIGH_DEFINITION_16_0_0_OR_NEWER
					UnityEngine.Assertions.Assert.IsNull(hdrpGlobalPass);
					hdrpGlobalPass = new AlineHDRPCustomPass();
					CustomPassVolume.RegisterGlobalCustomPass(CustomPassInjectionPoint.AfterPostProcess, hdrpGlobalPass);
#else
					if (!_instance.gameObject.TryGetComponent<CustomPassVolume>(out CustomPassVolume volume)) {
						volume = _instance.gameObject.AddComponent<CustomPassVolume>();
						volume.isGlobal = true;
						volume.injectionPoint = CustomPassInjectionPoint.AfterPostProcess;
						volume.customPasses.Add(new AlineHDRPCustomPass());
					}
#endif

					var asset = GraphicsSettings.defaultRenderPipeline as HDRenderPipelineAsset;
					if (asset != null) {
						if (!asset.currentPlatformRenderPipelineSettings.supportCustomPass) {
							Debug.LogWarning("A*: The current render pipeline has custom pass support disabled. The A* Pathfinding Project will not be able to render anything. Please enable custom pass support on your HDRenderPipelineAsset.", asset);
						}
					}
				}

#if UNITY_ASSERTIONS && MODULE_RENDER_PIPELINES_HIGH_DEFINITION_16_0_0_OR_NEWER
				var globalPasses = CustomPassVolume.GetGlobalCustomPasses(CustomPassInjectionPoint.AfterPostProcess);
				bool found = false;
				for (int i = 0; i < globalPasses.Count; i++) found |= globalPasses[i].instance == hdrpGlobalPass;
				UnityEngine.Assertions.Assert.IsTrue(found, "Custom pass for gizmos is not registered. Have the custom passes been forcefully removed by another script?");
#endif
				return;
			}
#if MODULE_RENDER_PIPELINES_HIGH_DEFINITION_16_0_0_OR_NEWER
			if (hdrpGlobalPass != null) {
				CustomPassVolume.UnregisterGlobalCustomPass(CustomPassInjectionPoint.AfterPostProcess, hdrpGlobalPass);
				hdrpGlobalPass = null;
			}
#endif
#endif

#if MODULE_RENDER_PIPELINES_UNIVERSAL
			if (pipelineType == typeof(UniversalRenderPipeline)) {
				detectedRenderPipeline = DetectedRenderPipeline.URP;
				return;
			}
#endif
			detectedRenderPipeline = DetectedRenderPipeline.BuiltInOrCustom;
		}

#if UNITY_EDITOR
		void DelayedDestroy () {
			EditorApplication.update -= DelayedDestroy;
			// Check if the object still exists (it might have been destroyed in some other way already).
			if (gameObject) GameObject.DestroyImmediate(gameObject);
		}

		void OnPlayModeStateChanged (PlayModeStateChange change) {
			if (change == PlayModeStateChange.ExitingEditMode || change == PlayModeStateChange.ExitingPlayMode) {
				gizmos.OnChangingPlayMode();
			}
		}
#endif

		void OnEnable () {
			if (_instance == null) _instance = this;

			// Ensure we don't have duplicate managers
			if (_instance != this) {
				// We cannot destroy the object while it is being enabled, so we need to delay it a bit
#if UNITY_EDITOR
				// This is only important in the editor to avoid a build-up of old managers.
				// In an actual game at most 1 (though in practice zero) old managers will be laying around.
				// It would be nice to use a coroutine for this instead, but unfortunately they do not work for objects marked with HideAndDontSave.
				EditorApplication.update += DelayedDestroy;
#endif
				return;
			}

			actuallyEnabled = true;
			if (gizmos == null) gizmos = new DrawingData();
			gizmos.frameRedrawScope = new RedrawScope(gizmos);
			Draw.builder = gizmos.GetBuiltInBuilder(false);
			Draw.ingame_builder = gizmos.GetBuiltInBuilder(true);
			commandBuffer = new CommandBuffer();
			commandBuffer.name = "ALINE Gizmos";

			detectedRenderPipeline = DetectedRenderPipeline.BuiltInOrCustom;

			// Callback when rendering with the built-in render pipeline
			Camera.onPostRender += PostRender;
			// Callback when rendering with a scriptable render pipeline
#if UNITY_2021_1_OR_NEWER
			UnityEngine.Rendering.RenderPipelineManager.beginContextRendering += BeginContextRendering;
#else
			UnityEngine.Rendering.RenderPipelineManager.beginFrameRendering += BeginFrameRendering;
#endif
			UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
			UnityEngine.Rendering.RenderPipelineManager.endCameraRendering += EndCameraRendering;
#if UNITY_EDITOR
			EditorApplication.update += OnEditorUpdate;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
		}

		void BeginContextRendering (ScriptableRenderContext context, List<Camera> cameras) {
			RefreshRenderPipelineMode();
		}

		void BeginFrameRendering (ScriptableRenderContext context, Camera[] cameras) {
			RefreshRenderPipelineMode();
		}

		void BeginCameraRendering (ScriptableRenderContext context, Camera camera) {
#if MODULE_RENDER_PIPELINES_UNIVERSAL
			if (detectedRenderPipeline == DetectedRenderPipeline.URP) {
				var data = camera.GetUniversalAdditionalCameraData();
				if (data != null) {
					var renderer = data.scriptableRenderer;
					if (renderPassFeature == null) {
						renderPassFeature = ScriptableObject.CreateInstance<AlineURPRenderPassFeature>();
					}
					renderPassFeature.AddRenderPasses(renderer);
				}
			}
#endif
		}

		void OnDisable () {
			if (!actuallyEnabled) return;
			actuallyEnabled = false;
			commandBuffer.Dispose();
			commandBuffer = null;
			Camera.onPostRender -= PostRender;
#if UNITY_2021_1_OR_NEWER
			UnityEngine.Rendering.RenderPipelineManager.beginContextRendering -= BeginContextRendering;
#else
			UnityEngine.Rendering.RenderPipelineManager.beginFrameRendering -= BeginFrameRendering;
#endif
			UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
			UnityEngine.Rendering.RenderPipelineManager.endCameraRendering -= EndCameraRendering;
#if UNITY_EDITOR
			EditorApplication.update -= OnEditorUpdate;
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
			// Gizmos can be null here if this GameObject was duplicated by a user in the hierarchy.
			if (gizmos != null) {
				Draw.builder.DiscardAndDisposeInternal();
				Draw.ingame_builder.DiscardAndDisposeInternal();
				gizmos.ClearData();
			}
#if MODULE_RENDER_PIPELINES_UNIVERSAL
			if (renderPassFeature != null) {
				ScriptableObject.DestroyImmediate(renderPassFeature);
				renderPassFeature = null;
			}
#endif
#if MODULE_RENDER_PIPELINES_HIGH_DEFINITION_16_0_0_OR_NEWER
			if (hdrpGlobalPass != null) {
				CustomPassVolume.UnregisterGlobalCustomPass(CustomPassInjectionPoint.AfterPostProcess, hdrpGlobalPass);
				hdrpGlobalPass = null;
			}
#endif
		}

		// When enter play mode = reload scene & reload domain
		//	editor => play mode: OnDisable -> OnEnable (same object)
		//  play mode => editor: OnApplicationQuit (note: no OnDisable/OnEnable)
		// When enter play mode = reload scene & !reload domain
		//	editor => play mode: Nothing
		//  play mode => editor: OnApplicationQuit
		// When enter play mode = !reload scene & !reload domain
		//	editor => play mode: Nothing
		//  play mode => editor: OnApplicationQuit
		// OnDestroy is never really called for this object (unless Unity or the game quits I quess)

		// TODO: Should run in OnDestroy. OnApplicationQuit runs BEFORE OnDestroy (which we do not want)
		// private void OnApplicationQuit () {
		// Debug.Log("OnApplicationQuit");
		// Draw.builder.DiscardAndDisposeInternal();
		// Draw.ingame_builder.DiscardAndDisposeInternal();
		// gizmos.ClearData();
		// Draw.builder = gizmos.GetBuiltInBuilder(false);
		// Draw.ingame_builder = gizmos.GetBuiltInBuilder(true);
		// }

		const float NO_DRAWING_TIMEOUT_SECS = 10;

		void OnEditorUpdate () {
			framePassed = true;
			CleanupIfNoCameraRendered();
		}

		void Update () {
			if (actuallyEnabled) CleanupIfNoCameraRendered();
		}

		void CleanupIfNoCameraRendered () {
			if (Time.frameCount > lastFrameCount + 1) {
				// More than one frame old
				// It is possible no camera is being rendered at all.
				// Ensure we don't get any memory leaks from drawing items being queued every frame.
				CheckFrameTicking();
				gizmos.PostRenderCleanup();

				// Note: We do not always want to call the above method here
				// because it is nicer to call it right after the cameras have been rendered.
				// Otherwise drawing items queued before Update/OnEditorUpdate or after Update/OnEditorUpdate may end up
				// in different frames (for the purposes of rendering gizmos)
			}

			if (Time.realtimeSinceStartup - lastFrameTime > NO_DRAWING_TIMEOUT_SECS) {
				// More than NO_DRAWING_TIMEOUT_SECS seconds since we drew the last frame.
				// In the editor some script could be queuing drawing commands in e.g. EditorWindow.Update without the scene
				// view or any game view being re-rendered. We discard these commands if nothing has been rendered for a long time.
				Draw.builder.DiscardAndDisposeInternal();
				Draw.ingame_builder.DiscardAndDisposeInternal();
				Draw.builder = gizmos.GetBuiltInBuilder(false);
				Draw.ingame_builder = gizmos.GetBuiltInBuilder(true);
				lastFrameTime = Time.realtimeSinceStartup;
				RemoveDestroyedGizmoDrawers();
			}

			// Avoid potential memory leak if gizmos are not being drawn
			if (lastFilterFrame - Time.frameCount > 5) {
				lastFilterFrame = Time.frameCount;
				RemoveDestroyedGizmoDrawers();
			}
		}

		internal void ExecuteCustomRenderPass (ScriptableRenderContext context, Camera camera) {
			MarkerALINE.Begin();
			commandBuffer.Clear();
			SubmitFrame(camera, new DrawingData.CommandBufferWrapper { cmd = commandBuffer }, true);
			context.ExecuteCommandBuffer(commandBuffer);
			MarkerALINE.End();
		}

#if MODULE_RENDER_PIPELINES_UNIVERSAL
		internal void ExecuteCustomRenderGraphPass (DrawingData.CommandBufferWrapper cmd, Camera camera) {
			MarkerALINE.Begin();
			SubmitFrame(camera, cmd, true);
			MarkerALINE.End();
		}
#endif

		private void EndCameraRendering (ScriptableRenderContext context, Camera camera) {
			if (detectedRenderPipeline == DetectedRenderPipeline.BuiltInOrCustom) {
				// Execute the custom render pass after the camera has finished rendering.
				// For the HDRP and URP the render pass will already have been executed.
				// However for a custom render pipline we execute the rendering code here.
				// This is only best effort. It's impossible to be compatible with all custom render pipelines.
				// However it should work for most simple ones.
				// For Unity's built-in render pipeline the EndCameraRendering method will never be called.
				ExecuteCustomRenderPass(context, camera);
			}
		}

		void PostRender (Camera camera) {
			// This method is only called when using Unity's built-in render pipeline
			commandBuffer.Clear();
			SubmitFrame(camera, new DrawingData.CommandBufferWrapper { cmd = commandBuffer }, false);
			MarkerCommandBuffer.Begin();
			Graphics.ExecuteCommandBuffer(commandBuffer);
			MarkerCommandBuffer.End();
		}

		void CheckFrameTicking () {
			MarkerFrameTick.Begin();
			if (Time.frameCount != lastFrameCount) {
				framePassed = true;
				lastFrameCount = Time.frameCount;
				lastFrameTime = Time.realtimeSinceStartup;
				previousFrameRedrawScope = gizmos.frameRedrawScope;
				gizmos.frameRedrawScope = new RedrawScope(gizmos);
				Draw.builder.DisposeInternal();
				Draw.ingame_builder.DisposeInternal();
				Draw.builder = gizmos.GetBuiltInBuilder(false);
				Draw.ingame_builder = gizmos.GetBuiltInBuilder(true);
			} else if (framePassed && Application.isPlaying) {
				// Rendered frame passed without a game frame passing!
				// This might mean the game is paused.
				// Redraw gizmos while the game is paused.
				// It might also just mean that we are rendering with multiple cameras.
				previousFrameRedrawScope.Draw();
			}

			if (framePassed) {
				gizmos.TickFramePreRender();
#if UNITY_EDITOR
				builtGizmos = false;
#endif
				framePassed = false;
			}
			MarkerFrameTick.End();
		}

		internal void SubmitFrame (Camera camera, DrawingData.CommandBufferWrapper cmd, bool usingRenderPipeline) {
#if UNITY_EDITOR
			bool isSceneViewCamera = SceneView.currentDrawingSceneView != null && SceneView.currentDrawingSceneView.camera == camera;
#else
			bool isSceneViewCamera = false;
#endif
			// Do not include when rendering to a texture unless this is a scene view camera
			bool allowCameraDefault = allowRenderToRenderTextures || drawToAllCameras || camera.targetTexture == null || isSceneViewCamera;

			CheckFrameTicking();

			Submit(camera, cmd, usingRenderPipeline, allowCameraDefault);

			gizmos.PostRenderCleanup();
		}

#if UNITY_EDITOR
		static readonly System.Reflection.MethodInfo IsGizmosAllowedForObject = typeof(UnityEditor.EditorGUIUtility).GetMethod("IsGizmosAllowedForObject", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
		readonly System.Object[] cachedObjectParameterArray = new System.Object[1];
#endif

		bool ShouldDrawGizmos (UnityEngine.Object obj) {
#if UNITY_EDITOR
			// Use reflection to call EditorGUIUtility.IsGizmosAllowedForObject which is an internal method.
			// It is exactly the information we want though.
			// In case Unity has changed its API or something so that the method can no longer be found then just return true
			cachedObjectParameterArray[0] = obj;
			return IsGizmosAllowedForObject == null || (bool)IsGizmosAllowedForObject.Invoke(null, cachedObjectParameterArray);
#else
			return true;
#endif
		}

		static void RemoveDestroyedGizmoDrawers () {
			MarkerFilterDestroyedObjects.Begin();
			for (int i = 0; i < gizmoDrawers.Count; i++) {
				var group = gizmoDrawers[i];
				int j = 0;
				for (int k = 0; k < group.drawers.Count; k++) {
					var v = group.drawers[k];
					if (v as MonoBehaviour) {
						group.drawers[j] = v;
						j++;
					}
				}
				group.drawers.RemoveRange(j, group.drawers.Count - j);
			}
			MarkerFilterDestroyedObjects.End();
		}

#if UNITY_EDITOR
		void DrawGizmos (bool usingRenderPipeline) {
			GizmoContext.SetDirty();

			// Reduce overhead if there's nothing to render
			if (gizmoDrawers.Count == 0) return;

			MarkerGizmosAllowed.Begin();

			// When playing, reduce how often we check for updates to the gizmo info, to avoid allocating garbage unnecessarily.
			// It's not possible to check for gizmo info without generating garbage unfortunately.
			bool refreshGizmoInfo = !Application.isPlaying || Time.realtimeSinceStartup - lastGizmoInfoRefresh > 0.1f;
			if (refreshGizmoInfo) {
				lastGizmoInfoRefresh = Time.realtimeSinceStartup;

				// Figure out which component types should be rendered
				for (int i = 0; i < gizmoDrawers.Count; i++) {
					var group = gizmoDrawers[i];
#if UNITY_2022_1_OR_NEWER
					// In Unity 2022.1 we can use a new utility class which is more robust.
					if (GizmoUtility.TryGetGizmoInfo(group.type, out var gizmoInfo)) {
						group.enabled = gizmoInfo.gizmoEnabled;
					} else {
						group.enabled = true;
					}
#else
					// We take advantage of the fact that IsGizmosAllowedForObject only depends on the type of the object and if it is active and enabled
					// and not the specific object instance.
					// When using a render pipeline the ShouldDrawGizmos method cannot be used because it seems to occasionally crash Unity :(
					// So we need these two separate cases.
					if (!usingRenderPipeline) {
						group.enabled = false;
						for (int j = group.drawers.Count - 1; j >= 0; j--) {
							// Find the first active and enabled drawer
							if ((group.drawers[j] as MonoBehaviour).isActiveAndEnabled) {
								group.enabled = ShouldDrawGizmos((UnityEngine.Object)group.drawers[j]);
								break;
							}
						}
					} else {
						group.enabled = true;
					}
#endif
					gizmoDrawers[i] = group;
				}
			}

			MarkerGizmosAllowed.End();

			// Set the current frame's redraw scope to an empty scope.
			// This is because gizmos are rendered every frame anyway so we never want to redraw them.
			// The frame redraw scope is otherwise used when the game has been paused.
			var frameRedrawScope = gizmos.frameRedrawScope;
			gizmos.frameRedrawScope = default(RedrawScope);

#if UNITY_EDITOR && UNITY_2020_1_OR_NEWER
			var currentStage = StageUtility.GetCurrentStage();
			var isInNonMainStage = currentStage != StageUtility.GetMainStage();
			var currentStageHandle = currentStage.stageHandle;
#endif

			// This would look nicer as a 'using' block, but built-in command builders
			// cannot be disposed normally to prevent user error.
			// The try-finally is equivalent to a 'using' block.
			var gizmoBuilder = gizmos.GetBuiltInBuilder();
			// Replace Draw.builder with a custom one just for gizmos
			var debugBuilder = Draw.builder;
			MarkerDrawGizmos.Begin();
			GizmoContext.drawingGizmos = true;
			try {
				Draw.builder = gizmoBuilder;

				for (int i = gizmoDrawers.Count - 1; i >= 0; i--) {
					var group = gizmoDrawers[i];
					if (group.enabled && group.drawers.Count > 0) {
						group.profilerMarker.Begin();
						for (int j = group.drawers.Count - 1; j >= 0; j--) {
							var mono = group.drawers[j] as MonoBehaviour;
							if (!mono.isActiveAndEnabled || (mono.hideFlags & HideFlags.HideInHierarchy) != 0) continue;

#if UNITY_EDITOR && UNITY_2020_1_OR_NEWER
							// True if the scene is in isolation mode (e.g. focusing on a single prefab) and this object is not part of that sub-stage
							var disabledDueToIsolationMode = isInNonMainStage && !currentStageHandle.Contains(mono.gameObject);
							if (disabledDueToIsolationMode) continue;
#endif

							try {
								group.drawers[j].DrawGizmos();
							} catch (System.Exception e) {
								Debug.LogException(e, mono);
							}
						}
						group.profilerMarker.End();
					}
				}
			} finally {
				GizmoContext.drawingGizmos = false;
				MarkerDrawGizmos.End();
				// Revert to the original builder
				Draw.builder = debugBuilder;
				gizmoBuilder.DisposeInternal();
			}

			gizmos.frameRedrawScope = frameRedrawScope;

			// Schedule jobs that may have been scheduled while drawing gizmos
			JobHandle.ScheduleBatchedJobs();
		}
#endif

		/// <summary>Submit a camera for rendering.</summary>
		/// <param name="allowCameraDefault">Indicates if built-in command builders and custom ones without a custom CommandBuilder.cameraTargets should render to this camera.</param>
		void Submit (Camera camera, DrawingData.CommandBufferWrapper cmd, bool usingRenderPipeline, bool allowCameraDefault) {
#if UNITY_EDITOR
			bool drawGizmos = Handles.ShouldRenderGizmos() || drawToAllCameras;
			// Only build gizmos if a camera actually needs them.
			// This is only done for the first camera that needs them each frame.
			if (drawGizmos && !builtGizmos && allowCameraDefault) {
				RemoveDestroyedGizmoDrawers();
				lastFilterFrame = Time.frameCount;
				builtGizmos = true;
				DrawGizmos(usingRenderPipeline);
			}
#else
			bool drawGizmos = false;
#endif

			MarkerSubmitGizmos.Begin();
			Draw.builder.DisposeInternal();
			Draw.ingame_builder.DisposeInternal();
			gizmos.Render(camera, drawGizmos, cmd, allowCameraDefault);
			Draw.builder = gizmos.GetBuiltInBuilder(false);
			Draw.ingame_builder = gizmos.GetBuiltInBuilder(true);
			MarkerSubmitGizmos.End();
		}

		/// <summary>
		/// Registers an object for gizmo drawing.
		/// The DrawGizmos method on the object will be called every frame until it is destroyed (assuming there are cameras with gizmos enabled).
		/// </summary>
		public static void Register (IDrawGizmos item) {
			if (ignoreAllDrawing) return;

			var tp = item.GetType();

			int index;
			if (!gizmoDrawerIndices.TryGetValue(tp, out index)) {
				// Use reflection to figure out if the DrawGizmos method has not been overriden from the MonoBehaviourGizmos class.
				// If it hasn't, then we know that this type will never draw gizmos and we can skip it.
				// This improves performance by not having to keep track of objects and check if they are active and enabled every frame.

				var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
				// Check for a public method first, and then an explicit interface implementation.
				var m = tp.GetMethod("DrawGizmos", flags) ?? tp.GetMethod("Pathfinding.Drawing.IDrawGizmos.DrawGizmos", flags) ?? tp.GetMethod("Drawing.IDrawGizmos.DrawGizmos", flags);
				if (m == null) {
					throw new System.Exception("Could not find the DrawGizmos method in type " + tp.Name);
				}
				var mayDrawGizmos = m.DeclaringType != typeof(MonoBehaviourGizmos);
				if (mayDrawGizmos) {
					index = gizmoDrawerIndices[tp] = gizmoDrawers.Count;
					gizmoDrawers.Add(new GizmoDrawerGroup {
						type = tp,
						enabled = true,
						drawers = new List<IDrawGizmos>(),
						profilerMarker = new ProfilerMarker(ProfilerCategory.Render, "Gizmos for " + tp.Name),
					});
#if UNITY_EDITOR
					lastGizmoInfoRefresh = float.NegativeInfinity;
#endif
				} else {
					index = gizmoDrawerIndices[tp] = -1;
				}
			}
			if (index == -1) return;

			gizmoDrawers[index].drawers.Add(item);
		}

		/// <summary>
		/// Get an empty builder for queuing drawing commands.
		///
		/// <code>
		/// // Create a new CommandBuilder
		/// using (var draw = DrawingManager.GetBuilder()) {
		///     // Use the exact same API as the global Draw class
		///     draw.WireBox(Vector3.zero, Vector3.one);
		/// }
		/// </code>
		/// See: <see cref="Drawing.CommandBuilder"/>
		/// </summary>
		/// <param name="renderInGame">If true, this builder will be rendered in standalone games and in the editor even if gizmos are disabled.
		/// If false, it will only be rendered in the editor when gizmos are enabled.</param>
		public static CommandBuilder GetBuilder(bool renderInGame = false) => instance.gizmos.GetBuilder(renderInGame);

		/// <summary>
		/// Get an empty builder for queuing drawing commands.
		///
		/// See: <see cref="Drawing.CommandBuilder"/>
		/// </summary>
		/// <param name="redrawScope">Scope for this command builder. See #GetRedrawScope.</param>
		/// <param name="renderInGame">If true, this builder will be rendered in standalone games and in the editor even if gizmos are disabled.
		/// If false, it will only be rendered in the editor when gizmos are enabled.</param>
		public static CommandBuilder GetBuilder(RedrawScope redrawScope, bool renderInGame = false) => instance.gizmos.GetBuilder(redrawScope, renderInGame);

		/// <summary>
		/// Get an empty builder for queuing drawing commands.
		///
		/// <code>
		/// // Just a nice looking curve (which uses a lot of complex math)
		/// // See https://en.wikipedia.org/wiki/Butterfly_curve_(transcendental)
		/// static float2 ButterflyCurve (float t) {
		///     t *= 12 * math.PI;
		///     var k = math.exp(math.cos(t)) - 2*math.cos(4*t) - math.pow(math.sin(t/12f), 5);
		///     return new float2(k * math.sin(t), k * math.cos(t));
		/// }
		///
		/// // Make the butterfly "flap its wings" two times per second
		/// var scale = Time.time % 0.5f < 0.25f ? new float2(1, 1) : new float2(0.7f, 1);
		///
		/// // Hash all inputs that you use for drawing something complex
		/// var hasher = new DrawingData.Hasher();
		/// // The only thing making the drawing change, in this case, is the scale
		/// hasher.Add(scale);
		///
		/// // Try to draw a previously cached mesh with this hash
		/// if (!DrawingManager.TryDrawHasher(hasher)) {
		///     // If there's no cached mesh, then draw it from scratch
		///     using (var builder = DrawingManager.GetBuilder(hasher)) {
		///         // Draw a complex curve using 10000 lines
		///         var prev = ButterflyCurve(0);
		///         for (float t = 0; t < 1; t += 0.0001f) {
		///             var next = ButterflyCurve(t);
		///             builder.xy.Line(prev*scale, next*scale, Color.white);
		///             prev = next;
		///         }
		///     }
		/// }
		/// </code>
		///
		/// See: <see cref="Drawing.CommandBuilder"/>
		/// See: caching (view in online documentation for working links)
		/// </summary>
		/// <param name="hasher">Hash of whatever inputs you used to generate the drawing data.</param>
		/// <param name="redrawScope">Scope for this command builder. See #GetRedrawScope.</param>
		/// <param name="renderInGame">If true, this builder will be rendered in standalone games and in the editor even if gizmos are disabled.</param>
		public static CommandBuilder GetBuilder(DrawingData.Hasher hasher, RedrawScope redrawScope = default, bool renderInGame = false) => instance.gizmos.GetBuilder(hasher, redrawScope, renderInGame);

		/// <summary>
		/// Tries to draw a builder that was rendered during the last frame using the same hash.
		///
		/// Returns: True if the builder was found and scheduled for rendering. If false, you should draw everything again and submit a new command builder.
		///
		/// <code>
		/// // Just a nice looking curve (which uses a lot of complex math)
		/// // See https://en.wikipedia.org/wiki/Butterfly_curve_(transcendental)
		/// static float2 ButterflyCurve (float t) {
		///     t *= 12 * math.PI;
		///     var k = math.exp(math.cos(t)) - 2*math.cos(4*t) - math.pow(math.sin(t/12f), 5);
		///     return new float2(k * math.sin(t), k * math.cos(t));
		/// }
		///
		/// // Make the butterfly "flap its wings" two times per second
		/// var scale = Time.time % 0.5f < 0.25f ? new float2(1, 1) : new float2(0.7f, 1);
		///
		/// // Hash all inputs that you use for drawing something complex
		/// var hasher = new DrawingData.Hasher();
		/// // The only thing making the drawing change, in this case, is the scale
		/// hasher.Add(scale);
		///
		/// // Try to draw a previously cached mesh with this hash
		/// if (!DrawingManager.TryDrawHasher(hasher)) {
		///     // If there's no cached mesh, then draw it from scratch
		///     using (var builder = DrawingManager.GetBuilder(hasher)) {
		///         // Draw a complex curve using 10000 lines
		///         var prev = ButterflyCurve(0);
		///         for (float t = 0; t < 1; t += 0.0001f) {
		///             var next = ButterflyCurve(t);
		///             builder.xy.Line(prev*scale, next*scale, Color.white);
		///             prev = next;
		///         }
		///     }
		/// }
		/// </code>
		///
		/// See: caching (view in online documentation for working links)
		/// </summary>
		/// <param name="hasher">Hash of whatever inputs you used to generate the drawing data.</param>
		/// <param name="redrawScope">Optional redraw scope for this command builder. See #GetRedrawScope.</param>
		public static bool TryDrawHasher(DrawingData.Hasher hasher, RedrawScope redrawScope = default) => instance.gizmos.Draw(hasher, redrawScope);

		/// <summary>
		/// A scope which will persist rendered items over multiple frames until it is disposed.
		///
		/// You can use <see cref="GetBuilder(RedrawScope,bool)"/> to get a builder with a given redraw scope.
		/// Everything drawn using the redraw scope will be drawn every frame until the redraw scope is disposed.
		///
		/// <code>
		/// private RedrawScope redrawScope;
		///
		/// void Start () {
		///     redrawScope = DrawingManager.GetRedrawScope();
		///     using (var builder = DrawingManager.GetBuilder(redrawScope)) {
		///         builder.WireSphere(Vector3.zero, 1.0f, Color.red);
		///     }
		/// }
		///
		/// void OnDestroy () {
		///     redrawScope.Dispose();
		/// }
		/// </code>
		///
		/// See: caching (view in online documentation for working links)
		/// </summary>
		/// <param name="associatedGameObject">If not null, the scope will only be drawn if gizmos for the associated GameObject are drawn.
		/// 		This is useful in the unity editor when e.g. opening a prefab in isolation mode, to disable redraw scopes for objects outside the prefab. Has no effect in standalone builds.</param>
		public static RedrawScope GetRedrawScope (GameObject associatedGameObject = null) {
			var scope = new RedrawScope(instance.gizmos);
			scope.DrawUntilDispose(associatedGameObject);
			return scope;
		}
	}
}
