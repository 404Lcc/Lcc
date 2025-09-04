#if MODULE_RENDER_PIPELINES_UNIVERSAL
using UnityEngine;
using UnityEngine.Rendering;
#if MODULE_RENDER_PIPELINES_UNIVERSAL_17_0_0_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#endif
using UnityEngine.Rendering.Universal;

namespace Pathfinding.Drawing {
	/// <summary>Custom Universal Render Pipeline Render Pass for ALINE</summary>
	public class AlineURPRenderPassFeature : ScriptableRendererFeature {
		/// <summary>Custom Universal Render Pipeline Render Pass for ALINE</summary>
		public class AlineURPRenderPass : ScriptableRenderPass {
			/// <summary>This method is called before executing the render pass</summary>
#if MODULE_RENDER_PIPELINES_UNIVERSAL_17_0_0_OR_NEWER
			[System.Obsolete]
#endif
			public override void Configure (CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
			}

#if MODULE_RENDER_PIPELINES_UNIVERSAL_17_0_0_OR_NEWER
			[System.Obsolete]
#endif
			public override void Execute (ScriptableRenderContext context, ref RenderingData renderingData) {
				DrawingManager.instance.ExecuteCustomRenderPass(context, renderingData.cameraData.camera);
			}

			public AlineURPRenderPass() : base() {
				profilingSampler = new ProfilingSampler("ALINE");
			}

#if MODULE_RENDER_PIPELINES_UNIVERSAL_17_0_0_OR_NEWER
			private class PassData {
				public Camera camera;
				public bool allowDisablingWireframe;
			}

			public override void RecordRenderGraph (RenderGraph renderGraph, ContextContainer frameData) {
				var cameraData = frameData.Get<UniversalCameraData>();
				var resourceData = frameData.Get<UniversalResourceData>();

				// This could happen if the camera does not have a color target or depth target set.
				// In that case we are probably rendering some kind of special effect. Skip ALINE rendering in that case.
				if (!resourceData.activeColorTexture.IsValid() || !resourceData.activeDepthTexture.IsValid()) {
					return;
				}

				using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass<PassData>("ALINE", out PassData passData, profilingSampler)) {
					passData.allowDisablingWireframe = false;

					if (Application.isEditor && (cameraData.cameraType & (CameraType.SceneView | CameraType.Preview)) != 0) {
						// We need this to be able to disable wireframe rendering in the scene view
						builder.AllowGlobalStateModification(true);
						passData.allowDisablingWireframe = true;
					}

					builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
					builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture);
					passData.camera = cameraData.camera;

					builder.SetRenderFunc<PassData>(
						(PassData data, RasterGraphContext context) => {
						DrawingManager.instance.ExecuteCustomRenderGraphPass(new DrawingData.CommandBufferWrapper { cmd2 = context.cmd, allowDisablingWireframe = data.allowDisablingWireframe }, data.camera);
					}
						);
				}
			}
#endif

			public override void FrameCleanup (CommandBuffer cmd) {
			}
		}

		AlineURPRenderPass m_ScriptablePass;

		public override void Create () {
			m_ScriptablePass = new AlineURPRenderPass();

			// Configures where the render pass should be injected.
			// URP's post processing actually happens in BeforeRenderingPostProcessing, not after BeforeRenderingPostProcessing as one would expect.
			// Use BeforeRenderingPostProcessing-1 to ensure this pass gets executed before post processing effects.
			m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing-1;
		}

		/// <summary>This method is called when setting up the renderer once per-camera</summary>
		public override void AddRenderPasses (ScriptableRenderer renderer, ref RenderingData renderingData) {
			AddRenderPasses(renderer);
		}

		public void AddRenderPasses (ScriptableRenderer renderer) {
			renderer.EnqueuePass(m_ScriptablePass);
		}
	}
}
#endif
