Shader "Kaboom/VFX_SkillRange"
{
    Properties
    {
        [PerRendererData] _MainTex ("贴图", 2D) = "white" {}
        [HDR][MainColor] _Color ("颜色", Color) = (2, 2, 2, 1)

        [Header(Sector Clip)]
        [Toggle] _SectorClipEnable ("扇形剔除开关", Float) = 0
        _SectorAngle ("扇形圆心角", Range(0, 360)) = 360
        _SectorAngleOffset ("扇形角度偏移", Range(-180, 180)) = 0
        _SectorFeather ("扇形边缘羽化", Range(0, 20)) = 1

        [Header(Sector Edge)]
        [HDR] _SectorEdgeColor ("扇形描边颜色", Color) = (2, 0, 0, 1)
        _SectorEdgeWidth ("扇形描边宽度(UV)", Range(0.001, 0.05)) = 0.01
        _SectorRadius ("扇形外沿半径(UV)", Range(0.3, 0.5)) = 0.5
        _SectorEdgeIntensity ("扇形描边强度", Range(0, 5)) = 2

        [Header(Blend)]
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 1
        [Enum(Off, 0, Front, 1, Back, 2)] _Cull ("Cull", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "True"
            "PreviewType" = "Plane"
        }

        Blend [_SrcBlend] [_DstBlend]
        ZWrite Off
        Cull [_Cull]

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma only_renderers gles gles3 glcore d3d11 metal
            #pragma target 2.0
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            half4 _Color;
            float _SectorClipEnable;
            float _SectorAngle;
            float _SectorAngleOffset;
            float _SectorFeather;
            half4 _SectorEdgeColor;
            float _SectorEdgeWidth;
            float _SectorRadius;
            float _SectorEdgeIntensity;
            CBUFFER_END

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half3 finalColor = tex.rgb * _Color.rgb * i.color.rgb;
                half finalAlpha = tex.a * _Color.a * i.color.a;

                if (_SectorClipEnable > 0.5 && _SectorAngle < 359.9)
                {
                    float2 d = i.uv - 0.5;
                    float r = length(d);
                    float ang = degrees(atan2(d.x, d.y)) + _SectorAngleOffset;
                    ang = ang - 360.0 * floor((ang + 180.0) / 360.0);
                    float halfAngle = max(0.0, _SectorAngle * 0.5);
                    float feather = max(0.001, _SectorFeather);
                    float angAbs = abs(ang);
                    float sectorMask = 1.0 - smoothstep(halfAngle - feather, halfAngle + feather, angAbs);
                    finalAlpha *= sectorMask;

                    float edgeWidth = max(0.0001, _SectorEdgeWidth);
                    float distToSide = r * abs(sin(radians(angAbs - halfAngle)));
                    float edge = 1.0 - smoothstep(0.0, edgeWidth, distToSide);

                    float rOuter = max(0.001, _SectorRadius);
                    float outerMask = 1.0 - smoothstep(rOuter - edgeWidth, rOuter, r);
                    float texMask = max(tex.a, max(tex.r, max(tex.g, tex.b)));
                    edge *= sectorMask * outerMask * saturate(texMask * 2.0);

                    finalColor += _SectorEdgeColor.rgb * (edge * _SectorEdgeIntensity);
                    finalAlpha = max(finalAlpha, edge * _SectorEdgeColor.a);
                }

                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
