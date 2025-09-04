Shader "Hidden/ALINE/Surface" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,0.5)
		_MainTex ("Texture", 2D) = "white" { }
		_Scale ("Scale", float) = 1
		_FadeColor ("Fade Color", Color) = (1,1,1,0.3)
	}

	HLSLINCLUDE
	float4 _MainTex_ST;
	float4 _Color;
	float4 _FadeColor;
	float _Scale;

	#pragma vertex vert
	#pragma fragment frag
	ENDHLSL

	// First subshader is for the HighDefinitionRenderPipeline.
	// The shader contents are identical except that it defines UNTIY_HDRP.
	SubShader {
		PackageRequirements {
            "com.unity.render-pipelines.high-definition": "0.1"
        }
		Tags {"Queue"="Transparent+1" "IgnoreProjector"="True" "RenderType"="Transparent" "RenderPipeline"="HighDefinitionRenderPipeline"}
		LOD 200
		Offset -2, -20
		Cull Off

		Pass {
			// Z-write further back to avoid lines drawn at the same z-depth to partially clip the surface
			Offset 0, 0
			ZWrite On
			ColorMask 0

			HLSLPROGRAM
			#define UNITY_HDRP
			#include "aline_common_surface.cginc"

			UNITY_DECLARE_TEX2D(_MainTex);

			v2f vert (appdata_color v) {
				return vert_base(v, _Color, _Scale);
			}

			float4 frag (v2f i) : SV_Target {
				if (i.col.a < 0.3) discard;
				return float4(1,1,1,1);
			}
			ENDHLSL
		}


		// Render behind
		Pass {
			ZWrite Off
			ZTest Greater
			Blend SrcAlpha OneMinusSrcAlpha

			HLSLPROGRAM
			#define UNITY_HDRP
			#include "aline_common_surface.cginc"

			UNITY_DECLARE_TEX2D(_MainTex);

			v2f vert (appdata_color v) {
				return vert_base(v, _Color * _FadeColor, _Scale);
			}

			float4 frag (v2f i) : SV_Target {
				return UNITY_SAMPLE_TEX2D(_MainTex, i.uv) * i.col;
			}
			ENDHLSL

		}

		// Render in front
		Pass {
			ZWrite Off
			ZTest LEqual
			Blend SrcAlpha OneMinusSrcAlpha

			HLSLPROGRAM
			#define UNITY_HDRP
			#include "aline_common_surface.cginc"

			UNITY_DECLARE_TEX2D(_MainTex);

			v2f vert (appdata_color v) {
				return vert_base(v, _Color, _Scale);
			}

			float4 frag (v2f i) : SV_Target {
				return UNITY_SAMPLE_TEX2D(_MainTex, i.uv) * i.col;
			}
			ENDHLSL
		}
	}

	SubShader {
		Tags {"Queue"="Transparent+1" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 200
		Offset -2, -20
		Cull Off

		Pass {
			// Z-write further back to avoid lines drawn at the same z-depth to partially clip the surface
			Offset 0, 0
			ZWrite On
			ColorMask 0

			HLSLPROGRAM
			#include "aline_common_surface.cginc"

			UNITY_DECLARE_TEX2D(_MainTex);

			v2f vert (appdata_color v) {
				return vert_base(v, _Color, _Scale);
			}

			float4 frag (v2f i) : SV_Target {
				if (i.col.a < 0.3) discard;
				return float4(1,1,1,1);
			}
			ENDHLSL
		}


		// Render behind
		Pass {
			ZWrite Off
			ZTest Greater
			Blend SrcAlpha OneMinusSrcAlpha

			HLSLPROGRAM
			#include "aline_common_surface.cginc"

			UNITY_DECLARE_TEX2D(_MainTex);

			v2f vert (appdata_color v) {
				return vert_base(v, _Color * _FadeColor, _Scale);
			}

			float4 frag (v2f i) : SV_Target {
				return UNITY_SAMPLE_TEX2D(_MainTex, i.uv) * i.col;
			}
			ENDHLSL

		}

		// Render in front
		Pass {
			ZWrite Off
			ZTest LEqual
			Blend SrcAlpha OneMinusSrcAlpha

			HLSLPROGRAM
			#include "aline_common_surface.cginc"

			UNITY_DECLARE_TEX2D(_MainTex);

			v2f vert (appdata_color v) {
				return vert_base(v, _Color, _Scale);
			}

			float4 frag (v2f i) : SV_Target {
				return UNITY_SAMPLE_TEX2D(_MainTex, i.uv) * i.col;
			}
			ENDHLSL
		}
	}

	Fallback Off
}
