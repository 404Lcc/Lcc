Shader "Hidden/ALINE/Outline" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,0.5)
		_FadeColor ("Fade Color", Color) = (1,1,1,0.3)
		_PixelWidth ("Width (px)", Float) = 4
		_LengthPadding ("Length Padding (px)", Float) = 0
	}

	HLSLINCLUDE
	float4 _Color;
	float4 _FadeColor;
	float _PixelWidth;
	float _LengthPadding;

	// Number of screen pixels that the _Falloff texture corresponds to
	static const float FalloffTextureScreenPixels = 2;

	#pragma vertex vert
	#pragma fragment frag
	ENDHLSL

	// First subshader is for the HighDefinitionRenderPipeline.
	// The shader contents are identical except that it defines UNTIY_HDRP.
	SubShader {
		PackageRequirements {
            "com.unity.render-pipelines.high-definition": "0.1"
        }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Offset -3, -50
		Tags { "IgnoreProjector"="True" "RenderType"="Overlay" "RenderPipeline"="HighDefinitionRenderPipeline" }
		// With line joins some triangles can actually end up backwards, so disable culling
		Cull Off

		// Render behind objects
		Pass {
			ZTest Greater

			HLSLPROGRAM
			#define UNITY_HDRP
			#include "aline_common_line.cginc"

			line_v2f vert (appdata_color v, out float4 outpos : SV_POSITION) {
				return line_vert_raw(v, _Color * _FadeColor, _PixelWidth, _LengthPadding, outpos);
			}

			half4 frag (line_v2f i, float4 screenPos : VPOS) : COLOR {
				return i.col * float4(1,1,1, calculateLineAlpha(i, i.lineWidth, FalloffTextureScreenPixels));
			}
			ENDHLSL
		}

		// First pass writes to the Z buffer where the lines have a pretty high opacity
		Pass {
			ZTest LEqual
			ZWrite On
			ColorMask 0

			HLSLPROGRAM
			#define UNITY_HDRP
			#include "aline_common_line.cginc"

			line_v2f vert (appdata_color v, out float4 outpos : SV_POSITION) {
				line_v2f o = line_vert_raw(v, float4(1,1,1,1), _PixelWidth, _LengthPadding, outpos);
				o.col = float4(1,1,1,1);
				return o;
			}

			half4 frag (line_v2f i, float4 screenPos : VPOS) : SV_Target {
				float a = calculateLineAlpha(i, i.lineWidth, FalloffTextureScreenPixels);
				if (a < 0.7) discard;
				return float4(1,1,1,a);
			}
			ENDHLSL
		}

		// Render in front of objects
		Pass {
			ZTest LEqual

			HLSLPROGRAM
			#define UNITY_HDRP
			#include "aline_common_line.cginc"

			line_v2f vert (appdata_color v, out float4 outpos : SV_POSITION) {
				return line_vert_raw(v, _Color, _PixelWidth, _LengthPadding, outpos);
			}

			half4 frag (line_v2f i, float4 screenPos : VPOS) : SV_Target {
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				return i.col * float4(1,1,1, calculateLineAlpha(i, i.lineWidth, FalloffTextureScreenPixels));
			}
			ENDHLSL
		}
	}


	SubShader {
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Offset -3, -50
		Tags { "IgnoreProjector"="True" "RenderType"="Overlay" }
		// With line joins some triangles can actually end up backwards, so disable culling
		Cull Off

		// Render behind objects
		Pass {
			ZTest Greater

			HLSLPROGRAM
			#include "aline_common_line.cginc"

			line_v2f vert (appdata_color v, out float4 outpos : SV_POSITION) {
				return line_vert_raw(v, _Color * _FadeColor, _PixelWidth, _LengthPadding, outpos);
			}

			half4 frag (line_v2f i, float4 screenPos : VPOS) : SV_Target {
				return i.col * float4(1,1,1, calculateLineAlpha(i, i.lineWidth, FalloffTextureScreenPixels));
			}
			ENDHLSL
		}

		// First pass writes to the Z buffer where the lines have a pretty high opacity
		Pass {
			ZTest LEqual
			ZWrite On
			ColorMask 0

			HLSLPROGRAM
			#include "aline_common_line.cginc"

			line_v2f vert (appdata_color v, out float4 outpos : SV_POSITION) {
				line_v2f o = line_vert_raw(v, float4(1,1,1,1), _PixelWidth, _LengthPadding, outpos);
				o.col = float4(1,1,1,1);
				return o;
			}

			half4 frag (line_v2f i, float4 screenPos : VPOS) : SV_Target {
				float a = calculateLineAlpha(i, i.lineWidth, FalloffTextureScreenPixels);
				if (a < 0.7) discard;
				return float4(1,1,1,a);
			}
			ENDHLSL
		}

		// Render in front of objects
		Pass {
			ZTest LEqual

			HLSLPROGRAM
			#include "aline_common_line.cginc"

			line_v2f vert (appdata_color v, out float4 outpos : SV_POSITION) {
				return line_vert_raw(v, _Color, _PixelWidth, _LengthPadding, outpos);
			}

			half4 frag (line_v2f i, float4 screenPos : VPOS) : SV_Target {
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				return i.col * float4(1,1,1, calculateLineAlpha(i, i.lineWidth, FalloffTextureScreenPixels));
			}
			ENDHLSL
		}
	}
	Fallback Off
}
