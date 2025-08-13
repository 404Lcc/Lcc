Shader "Actor/RGBA_Flow"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _Cutoff("Alpha cut", Range(0,1)) = 0
        _TintColor("Tint Color", Color) = (0,0,0,0)
        _MultiColor("Multi Color", Color) = (1,1,1,1)
        _IsGray("Is Gray", Range(0,1)) = 0
        _IsFlow("Is Flow", Range(0,1)) = 0

        _FlowTex("流光贴图", 2D) = "gray" {}
        _MaskTex("流光遮罩", 2D) = "white" {}

        _FlowDirectionX("流光方向X", Range(-1, 1)) = -1
        _FlowDirectionY("流光方向Y", Range(-1, 1)) = -1
        _FlowSpace("流光间隔",Range(0.1, 10)) = 1
        _FlowSpeed("流光速度",Range(0, 10)) = 1
        _FlowColor("流光颜色", Color) = (1,1,1,1)
        _StrengthColor("每个通道的流光强度", Color) = (0.299,0.5871,0.114,1)
        _Strength("流光强度",Range(0, 10)) = 1

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP


            sampler2D _MainTex;
            sampler2D _FlowTex;
            sampler2D _MaskTex;

            fixed _FlowDirectionX;
            fixed _FlowDirectionY;
            fixed _FlowSpace;
            fixed _FlowSpeed;
            fixed4 _TintColor;
            fixed4 _MultiColor;
            float _IsGray;
            float _IsFlow;


            fixed4 _FlowColor;
            float _Strength;
            fixed4 _StrengthColor;
            fixed _Cutoff;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float4 mask : TEXCOORD2;
            };

            float4 _MainTex_ST;
            float4 _FlowTex_ST;

            float4 _ClipRect;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                //_FlowTex_ST.zw += fixed2(_FlowDirectionX, _FlowDirectionY) * _FlowTex_ST.xy * fmod(_Time.y * _FlowSpeed, _FlowSpace);

                o.texcoord1 = TRANSFORM_TEX(v.texcoord1, _FlowTex);

                float2 pixelSize = o.vertex.w;
                pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

                float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
                float2 maskUV = (v.vertex.xy - clampedRect.xy) / (clampedRect.zw - clampedRect.xy);
                o.mask = float4(v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));

                return o;
            }

            fixed4 frag(v2f i) : COLOR
            {
                fixed4 col = tex2D(_MainTex, i.texcoord);
                col.rgb = (col.rgb + _TintColor.rgb) * _MultiColor.rgb;
                col.a -= _Cutoff;

                //fixed2 uv = fixed2(fmod(.x, _FlowSpace), fmod(i.texcoord1.y, _FlowSpace));
                fixed2 flowUV = i.texcoord.xy;
                fixed currentTimePassed = fmod(_Time.y * _FlowSpeed, _FlowSpace);
                fixed offsetX = currentTimePassed;
                fixed2 offset = fixed2(offsetX - 0.5, 0);
                flowUV = flowUV - offset;
                fixed4 flowCol = tex2D(_FlowTex, flowUV);
                fixed4 maskCol = tex2D(_MaskTex, i.texcoord);


                _FlowColor *= flowCol;
                float strength = col.r * _StrengthColor.r + col.g * _StrengthColor.g + col.b * _StrengthColor.b;
                strength *= _FlowColor.a * _Strength * maskCol.a * maskCol.r;
                col.rgb += (_FlowColor.rgb * strength) * _IsFlow;

                half gray = col.r * 0.299 + col.g * 0.587 + col.b * 0.114;
                half3 grayCol = half3(gray, gray, gray);
                grayCol *= _IsGray;
                col.rgb *= (1 - _IsGray);
                half4 final = fixed4(grayCol + col.rgb, col.a);

                #ifdef UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(i.mask.xy)) * i.mask.zw);
                final.a *= m.x * m.y;
                #endif

                return final;
            }
            ENDCG
        }
    }
}