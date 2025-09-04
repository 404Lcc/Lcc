
#ifdef UNITY_HDRP
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

// Unity does not define the UNITY_DECLARE_TEX2D macro when using HDRP, at least at the time of writing this.
// But luckily HDRP is only supported on platforms where separate sampler states are supported, so we can define it like this.
#if !defined(UNITY_DECLARE_TEX2D)
// This is copied from com.unity.shadergraph@14.0.6/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Shim/HLSLSupportShim.hlsl
#define UNITY_DECLARE_TEX2D(tex) TEXTURE2D(tex); SAMPLER(sampler##tex)
#endif

#if !defined(UNITY_SAMPLE_TEX2D)
#define UNITY_SAMPLE_TEX2D(tex,coord) SAMPLE_TEXTURE2D(tex, sampler##tex, coord)
#endif

// This is not defined in HDRP either, but we do know that HDRP only supports these platforms (at least I think so...)
#define UNITY_SEPARATE_TEXTURE_SAMPLER

#else
#include "UnityCG.cginc"

// These exist in the render pipelines, but not in UnityCG
float4 TransformObjectToHClip(float3 x) {
	return UnityObjectToClipPos(float4(x, 1.0));
}

half3 FastSRGBToLinear(half3 sRGB) {
	return GammaToLinearSpace(sRGB);
}
#endif

// Tranforms a direction from object to homogenous space
inline float4 UnityObjectToClipDirection(in float3 pos) {
	// More efficient than computing M*VP matrix product
	return mul(UNITY_MATRIX_VP, mul(UNITY_MATRIX_M, float4(pos, 0)));
}

float lengthsq(float3 v) {
	return dot(v,v);
}

float4 ComputeScreenPos (float4 pos, float projectionSign)
{
  float4 o = pos * 0.5f;
  o.xy = float2(o.x, o.y * projectionSign) + o.w;
  o.zw = pos.zw;
  return o;
}


// Converts to linear space from sRGB if linear is the current color space
inline float3 ConvertSRGBToDestinationColorSpace(float3 sRGB) {
#ifdef UNITY_COLORSPACE_GAMMA
	return sRGB;
#else
	return FastSRGBToLinear(sRGB);
#endif
}

struct appdata_color {
	float4 vertex : POSITION;
	half4 color : COLOR;
	float3 normal : NORMAL;
	float2 uv : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

// Unity sadly does not include a bias macro for texture sampling.
#if defined(UNITY_SEPARATE_TEXTURE_SAMPLER)
#define UNITY_SAMPLE_TEX2D_BIAS(tex, uv, bias) tex.SampleBias(sampler##tex, uv, bias)
#else
#define UNITY_SAMPLE_TEX2D_BIAS(tex, uv, bias) tex2Dbias(float4(uv, 0, bias))
#endif
