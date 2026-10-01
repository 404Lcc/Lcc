#ifndef KABOOM_DISSOLVE_INCLUDED
#define KABOOM_DISSOLVE_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

// ================= 溶解属性定义 =================
CBUFFER_START(DissolveProperties)
    float _DissolveEnable;
    sampler2D _DissolveTex;
    float4 _DissolveSpeed;
    float4 _DissolveTex_ST;
    float _DissolveThreshold;
    float _DissolveEdgeWidth;
    float4 _DissolveEdgeColor;
CBUFFER_END

// ================= 溶解函数 =================
/// <summary>
/// 应用溶解效果
/// </summary>
/// <param name="uv">基础 UV</param>
/// <param name="baseColor">输入颜色</param>
/// <param name="baseAlpha">输入透明度</param>
/// <returns>溶解后的 half4 (RGB+Alpha)</returns>
inline half ApplyDissolve(float2 uv, half baseAlpha,half customDissolveIntensity)
{
    _DissolveThreshold = customDissolveIntensity * (1 - _DissolveThreshold);
    
    // 采样噪声图
    float2 dissolveUV = uv * _DissolveTex_ST.xy + _DissolveTex_ST.zw;
    dissolveUV += _DissolveSpeed.xy * _Time.y;
    float dissolveNoise = tex2D(_DissolveTex, dissolveUV).r;

    // 溶解遮罩 (0~1)
    _DissolveThreshold -= 0.1;
    _DissolveThreshold *= 1.4;
    
    float dissolveMask = smoothstep(
        _DissolveThreshold - _DissolveEdgeWidth,
        _DissolveThreshold + _DissolveEdgeWidth,
        dissolveNoise
    );

    // 溶解边缘
    //float edgeMask = saturate((dissolveNoise - _DissolveThreshold) / _DissolveEdgeWidth);
    //half3 dissolveColor = lerp(_DissolveEdgeColor.rgb, baseColor, edgeMask);

    // 输出
    half finalAlpha = baseAlpha;
    if(_DissolveEnable > 0.5)
        finalAlpha = baseAlpha * dissolveMask;
    
    return finalAlpha;
}

#endif // KABOOM_DISSOLVE_INCLUDED
