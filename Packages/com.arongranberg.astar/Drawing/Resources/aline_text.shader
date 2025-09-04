Shader "Hidden/ALINE/Font" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,0.5)
		_FadeColor ("Fade Color", Color) = (1,1,1,0.3)
		_MainTex ("Texture", 2D) = "white" {}
		_FallbackTex ("Fallback Texture", 2D) = "white" {}
		_FallbackAmount ("Fallback Amount", Range(0,1)) = 1.0
		_TransitionPoint ("Transition Point", Range(0,5)) = 0.6
		_MipBias ("Mip Bias", Range(-2,0)) = -1
		_GammaCorrection ("Gamma Correction", Range(0,2)) = 1
	}

	// First subshader is for the HighDefinitionRenderPipeline.
	// The shader contents are identical except that it defines UNTIY_HDRP.
	SubShader {
		PackageRequirements {
            "com.unity.render-pipelines.high-definition": "0.1"
        }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Offset -3, -50
		Cull Off
		Tags { "IgnoreProjector"="True" "RenderType"="Overlay" "RenderPipeline"="HighDefinitionRenderPipeline"}

		Pass {
			ZTest Greater

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define UNITY_HDRP
			#include "aline_common_text.cginc"
			v2f vert (vertex v, out float4 outpos : SV_POSITION) {
				return vert_base(v, _Color * _FadeColor, outpos);
			}
			ENDHLSL
		}

		Pass {
			ZTest LEqual

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define UNITY_HDRP
			#include "aline_common_text.cginc"
			v2f vert (vertex v, out float4 outpos : SV_POSITION) {
				return vert_base(v, _Color, outpos);
			}
			ENDHLSL
		}
	}

	SubShader {
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Offset -3, -50
		Cull Off
		Tags { "IgnoreProjector"="True" "RenderType"="Overlay" }

		Pass {
			ZTest Greater

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "aline_common_text.cginc"
			v2f vert (vertex v, out float4 outpos : SV_POSITION) {
				return vert_base(v, _Color * _FadeColor, outpos);
			}
			ENDHLSL
		}

		Pass {
			ZTest LEqual

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "aline_common_text.cginc"
			v2f vert (vertex v, out float4 outpos : SV_POSITION) {
				return vert_base(v, _Color, outpos);
			}
			ENDHLSL
		}
	}
	Fallback Off
}
