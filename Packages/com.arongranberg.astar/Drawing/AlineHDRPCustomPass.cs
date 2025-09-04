#if MODULE_RENDER_PIPELINES_HIGH_DEFINITION
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace Pathfinding.Drawing {
	/// <summary>Custom High Definition Render Pipeline Render Pass for ALINE</summary>
	class AlineHDRPCustomPass : CustomPass {
#if MODULE_RENDER_PIPELINES_HIGH_DEFINITION_9_0_0_OR_NEWER
		bool disabledDepth = false;

		protected override void Setup (ScriptableRenderContext renderContext, CommandBuffer cmd) {
			this.targetColorBuffer = TargetBuffer.Camera;
			this.targetDepthBuffer = TargetBuffer.Camera;
			disabledDepth = false;
		}

		protected override void Execute (CustomPassContext context) {
			UnityEngine.Profiling.Profiler.BeginSample("ALINE");
			if (!disabledDepth && context.cameraColorBuffer.isMSAAEnabled != context.cameraDepthBuffer.isMSAAEnabled) {
				Debug.LogWarning("A*: Cannot draw depth-tested gizmos due to limitations in Unity's high-definition render pipeline combined with MSAA. Typically this is caused by enabling Camera -> Frame Setting Overrides -> MSAA Within Forward.\n\nDepth-testing for gizmos will stay disabled until you disable this type of MSAA and recompile scripts.");
				// At this point, we only get access to the MSAA depth buffer, not the resolved non-MSAA depth buffer.
				// If we try to use the depth buffer, we will get an error message from Unity:
				// "Color and Depth buffer MSAA flags doesn't match, no rendering will occur."
				// Rendering seems to somewhat work even though that error is logged, but there are a lot of rendering artifacts.
				// So we will just disable depth testing.
				//
				// In the HDRenderPipeline.RenderGraph.cs script, the resolved non-msaa depth buffer is accessible, and this is the one
				// that Unity's own gizmos rendering code uses. However, Unity does not expose this buffer to custom render passes.
				disabledDepth = true;
				this.targetDepthBuffer = TargetBuffer.None;
			}
			DrawingManager.instance.SubmitFrame(context.hdCamera.camera, new DrawingData.CommandBufferWrapper { cmd = context.cmd }, true);
			UnityEngine.Profiling.Profiler.EndSample();
		}
#else
		protected override void Execute (ScriptableRenderContext context, CommandBuffer cmd, HDCamera camera, CullingResults cullingResult) {
			UnityEngine.Profiling.Profiler.BeginSample("ALINE");
			DrawingManager.instance.SubmitFrame(camera.camera, new DrawingData.CommandBufferWrapper { cmd = cmd }, true);
			UnityEngine.Profiling.Profiler.EndSample();
		}
#endif

		protected override void Cleanup () {
		}
	}
}
#endif
