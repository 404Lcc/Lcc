#include "aline_common.cginc"

float4 _Color;
float4 _FadeColor;
UNITY_DECLARE_TEX2D(_MainTex);
UNITY_DECLARE_TEX2D(_FallbackTex);
float _FallbackAmount;
float _TransitionPoint;
float _MipBias;
float _GammaCorrection;

struct vertex {
    float4 pos : POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f {
    float4 col : COLOR;
    float2 uv: TEXCOORD0;
    UNITY_VERTEX_OUTPUT_STEREO
};

v2f vert_base (vertex v, float4 tint, out float4 outpos : SV_POSITION) {
    UNITY_SETUP_INSTANCE_ID(v);
    v2f o;
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    o.uv = v.uv;
    o.col = v.color * tint;
    o.col.rgb = ConvertSRGBToDestinationColorSpace(o.col.rgb);
    outpos = TransformObjectToHClip(v.pos.xyz);
    return o;
}

// float getAlpha2(float2 uv) {
// 	const float textureWidth = 1024;
// 	float pixelSize = 0.5 * length(float2(ddx(uv.x), ddy(uv.x))) * textureWidth;

// 	// Depends on texture generation settings
// 	const float falloffPixels = 5;

// 	float sample = UNITY_SAMPLE_TEX2D(_MainTex, uv).a;
// 	// float scale = 1.0 / fwidth(sample);
// 	float signedDistance1 = (0.5 - sample) * falloffPixels;
// 	float signedDistance2 = signedDistance1 / pixelSize;

// 	return lineAA(signedDistance2 + 0.5);
// 	// float signedDistance = (sample - 0.5) * scale;
// 	// return fwidth(sample) * 10;
// 	// Use two different distance thresholds to get dynamically stroked text
// 	// float color = clamp(signedDistance + 0.5, 0.0, 1.0);
// 	// return color;
// }

float getAlpha(float2 uv) {
    float rawSignedDistance = UNITY_SAMPLE_TEX2D(_MainTex, uv).a;
    float scale = 1.0 / fwidth(rawSignedDistance);
    float thresholdedDistance = (rawSignedDistance - 0.5) * scale;
    float color = clamp(thresholdedDistance + 0.5, 0.0, 1.0);
    return color;
}

// Shader modified from https://evanw.github.io/font-texture-generator/example-webgl/
float4 frag (v2f i, float4 screenPos : VPOS) : SV_Target {
    // float halfpixelSize = 0.5 * 0.5 * length(float2(ddx(i.uv.x), ddy(i.uv.x)));
    // float fcolor0 = UNITY_SAMPLE_TEX2D(_FallbackTex, i.uv).a;
    // float fcolor1 = UNITY_SAMPLE_TEX2D(_FallbackTex, i.uv + float2(halfpixelSize * 0.6, halfpixelSize * 0.3)).a;
    // float fcolor2 = UNITY_SAMPLE_TEX2D(_FallbackTex, i.uv + float2(-halfpixelSize * 0.3, halfpixelSize * 0.6)).a;
    // float fcolor3 = UNITY_SAMPLE_TEX2D(_FallbackTex, i.uv + float2(-halfpixelSize * 0.6, -halfpixelSize * 0.3)).a;
    // float fcolor4 = UNITY_SAMPLE_TEX2D(_FallbackTex, i.uv + float2(halfpixelSize * 0.3, -halfpixelSize * 0.6)).a;
    // float fallbackAlpha = (fcolor0 + fcolor1 + fcolor2 + fcolor3 + fcolor4) * 0.2;
    // Bias the texture sampling to use a lower mipmap level. This makes the text much sharper and clearer.
    float fallbackAlpha = UNITY_SAMPLE_TEX2D_BIAS(_FallbackTex, i.uv, _MipBias).a;

    // The fallback is used for small font sizes.
    // Boost the alpha to make it more legible
    fallbackAlpha *= 1.2;

    // Approximate size of one screen pixel in UV-space
    float pixelSize = length(float2(ddx(i.uv.x), ddy(i.uv.x)));
    // float pixelSize2 = length(float2(ddx(i.uv.y), ddy(i.uv.y)));

    // float color0 = getAlpha(i.uv);
    // float color1 = getAlpha(i.uv + float2(halfpixelSize * 0.6, halfpixelSize * 0.3));
    // float color2 = getAlpha(i.uv + float2(-halfpixelSize * 0.3, halfpixelSize * 0.6));
    // float color3 = getAlpha(i.uv + float2(-halfpixelSize * 0.6, -halfpixelSize * 0.3));
    // float color4 = getAlpha(i.uv + float2(halfpixelSize * 0.3, -halfpixelSize * 0.6));
    // float color = (color0 + color1 + color2 + color3 + color4) * 0.2;

    float sdfAlpha = getAlpha(i.uv);

    // Transition from the SDF font to the fallback when the font's size on the screen
    // starts getting smaller than the size in the texture.
    float sdfTextureWidth = 1024;
    // How sharp the transition from sdf to fallback is.
    // A smaller value will make the transition cover a larger range of font sizes
    float transitionSharpness = 10;
    float blend = clamp(transitionSharpness*(_TransitionPoint*pixelSize*sdfTextureWidth - 1.0), 0, 1);

    float alpha = lerp(sdfAlpha, fallbackAlpha, blend * _FallbackAmount);

    float4 blendcolor = float4(1,1,1,1);
    // blendcolor = lerp(float4(0, 1, 0, 1), float4(1, 0, 0, 1), blend);

    return blendcolor * i.col * float4(1, 1, 1, alpha);
}