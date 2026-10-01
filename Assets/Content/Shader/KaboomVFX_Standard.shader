Shader "Kaboom/VFX_Standard"
{
    Properties
    {
        // 主贴图属性
        [Header(Main Texture)]
        _MainTex ("Texture", 2D) = "white" {}
        [Toggle] _SingleChannel ("单通道开关", Float) = 1
        [Toggle] _AlphaChannel ("透明通道开关", Float) = 1
        [Toggle] _TexRotateEnable ("UV旋转开关", Float) = 0
        _TexRotation ("UV旋转偏移角度", Range(0, 360)) = 0
        _TexFlowSpeed ("UV流动方向速度", Vector) = (0,0,0,0)
        [HDR]_TexColor ("颜色", Color) = (1,1,1,1)
        _TexIntensity ("颜色强度", Range(0, 5)) = 1
        _TexContrast ("对比度", Range(0, 2)) = 1
        [Toggle]_UseCustomData ("启用自定义数据", Float) = 0
        
        [Header(Dissolve)]
        [Toggle] _DissolveEnable ("溶解开关", Float) = 0
        _DissolveTex ("溶解纹理", 2D) = "white" {}
        _DissolveThreshold ("溶解阈值", Range(0,1)) = 0.5
        _DissolveEdgeWidth ("溶解边缘宽度", Range(0,0.5)) = 0.05
        [HDR]_DissolveEdgeColor ("溶解边缘颜色", Color) = (0,0,0,1)
        _DissolveSpeed ("流动速度", Vector) = (0,0,0,0)
        
        // 遮罩贴图属性
        [Header(Mask Texture)]
        _MaskTex ("Mask Texture", 2D) = "white" {}
        [Toggle] _MaskSingleChannel ("单通道开关", Float) = 1
        [Toggle] _MaskAlphaChannel ("透明通道开关", Float) = 1
        [Toggle] _MaskRotateEnable ("UV旋转开关", Float) = 0
        _MaskRotation ("UV旋转偏移角度", Range(0, 360)) = 0
        _MaskFlowSpeed ("UV流动方向速度", Vector) = (0,0,0,0)
        [HDR]_MaskColor ("颜色", Color) = (1,1,1,1)
        _MaskIntensity ("颜色强度", Range(0, 2)) = 1
        _MaskContrast ("对比度", Range(0, 2)) = 1
        
        [Space]
        [Header(WPO)]
        _WPOMap ("WPO Map", 2D) = "white" {}
        _WPOOffset ("WPO Offset", Range(0, 1)) = 0
        _WPOSpeed ("流动方向速度", Vector) = (0,0,0,0)
        
        [Header(BlendMode)]
        _Blend("__blend", Float) = 0
        _SrcBlend("__src", Float) = 0
        _DstBlend("__dst", Float) = 0
        _Cull("__cull", Float) = 0
        [Toggle]_ZWrite("__zw", Float) = 0
    }

    SubShader
    {
        Tags 
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True"
            "ShaderModel"="2.0"
        }
        LOD 100
        //Blend SrcAlpha OneMinusSrcAlpha
        Blend[_SrcBlend][_DstBlend]
        ZWrite [_ZWrite]
        Cull [_Cull]

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma only_renderers gles gles3 glcore d3d11 metal
            #pragma target 2.0

            #pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON

            #pragma multi_compile_instancing
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ DEBUG_DISPLAY
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float3 normalOS         : NORMAL;
                float4 uvoffset : TEXCOORD1;
                float4 customdata2 : TEXCOORD2;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float4 uvoffset : TEXCOORD1;
                float4 customdata2 : TEXCOORD2;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
            // 主贴图属性
            sampler2D _MainTex;
            float4 _MainTex_ST;
            half _SingleChannel;
            half _AlphaChannel;
            half _TexRotateEnable;
            float _TexRotation;
            float4 _TexFlowSpeed;
            half4 _TexColor;
            half _TexIntensity;
            half _TexContrast;
            half _UseCustomData;
            
            // 遮罩贴图属性
            sampler2D _MaskTex;
            float4 _MaskTex_ST;
            half _MaskSingleChannel;
            half _MaskAlphaChannel;
            half _MaskRotateEnable;
            half _MaskRotation;
            float4 _MaskFlowSpeed;
            half4 _MaskColor;
            half _MaskIntensity;
            half _MaskContrast;

            // WPO贴图属性
            TEXTURE2D(_WPOMap);
            SAMPLER(sampler_WPOMap);
            float4 _WPOMap_ST;
            float _WPOOffset;
            float4 _WPOSpeed;
            
            CBUFFER_END

            // customdata控制UV偏移
            #ifdef UNITY_DOTS_INSTANCING_ENABLED
                UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
                    UNITY_DOTS_INSTANCED_PROP(float4, _TexOffset)
                UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
                #define _TexOffset  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4 , Metadata_TexOffset)
            #endif

            #include "KaboomDissolve.hlsl"
            
            // UV旋转函数
            float2 RotateUV(float2 uv, float rotation, float2 offset)
            {
                rotation = radians(rotation);
                float sinRot = sin(rotation);
                float cosRot = cos(rotation);
                float2 center = float2(0.5, 0.5) + offset;
                
                uv -= center;
                float2 rotatedUV;
                rotatedUV.x = uv.x * cosRot - uv.y * sinRot;
                rotatedUV.y = uv.x * sinRot + uv.y * cosRot;
                rotatedUV += center;
                
                return rotatedUV;
            }

            // 对比度调整
            half3 ApplyContrast(half3 color, half contrast)
            {
                return saturate(lerp(0.5, color, contrast));
            }

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v,o);

                float2 WPOUV = v.uv * _WPOMap_ST.xy + _WPOMap_ST.zw;
                WPOUV += _WPOSpeed.xy * _Time.y;
                half noise = SAMPLE_TEXTURE2D_LOD(_WPOMap, sampler_WPOMap, WPOUV, 0).r;
                half3 positionOS = v.positionOS.xyz + v.normalOS * (noise - 0.5) * _WPOOffset;
                half3 positionWS = TransformObjectToWorld(positionOS);
                
                o.positionCS = TransformWorldToHClip(positionWS);
                o.uv = v.uv;
                o.color = v.color;
                o.uvoffset = v.uvoffset;
                o.customdata2 = v.customdata2;
                
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                
                float time = _Time.y;
                
                // =================== 主贴图处理 ===================
                float2 texUV = i.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                
                // UV变换
                //texUV = texUV * _TexTiling.xy + _TexOffset.xy;

                if(_UseCustomData > 0.5)
                {
                    texUV *= i.uvoffset.zw;
                    texUV += i.uvoffset.xy;
                }
                
                // UV流动
                texUV += _TexFlowSpeed.xy * time;
                
                // UV旋转
                if (_TexRotateEnable > 0.5) 
                {
                    texUV = RotateUV(texUV, _TexRotation, _TexFlowSpeed.zw);
                }
                
                // 采样贴图
                half4 texSample = tex2D(_MainTex, texUV);

                if (_SingleChannel > 0.5)
                {
                    texSample = texSample.b;
                }
                
                // 颜色处理
                half3 texColor = texSample.rgb * _TexColor.rgb * _TexIntensity * i.color.rgb;
                texColor = ApplyContrast(texColor, _TexContrast);
                
                // 透明度处理
                half texAlpha = _AlphaChannel > 0.5 ? texSample.a : 1;
                texAlpha *= _TexColor.a;
                
                // =================== 遮罩贴图处理 ===================
                float2 maskUV = i.uv;
                
                // UV变换
                maskUV = maskUV * _MaskTex_ST.xy + _MaskTex_ST.zw;;
                
                // UV流动
                maskUV += _MaskFlowSpeed.xy * time;

                if(_UseCustomData > 0.5)
                {
                    maskUV += i.customdata2.yz;
                }
                
                // UV旋转
                if (_MaskRotateEnable > 0.5) 
                {
                    maskUV = RotateUV(maskUV, _MaskRotation, _MaskFlowSpeed.zw);
                }
                
                // 采样遮罩
                half4 maskSample = tex2D(_MaskTex, maskUV);

                if (_MaskSingleChannel > 0.5)
                {
                    maskSample = maskSample.b;
                }
                
                // 透明
                half maskAlpha = _MaskAlphaChannel > 0.5 ? maskSample.a : maskSample.r;
                maskAlpha = ApplyContrast(maskAlpha, _MaskContrast).r;
                maskAlpha *= _MaskColor.a * _MaskIntensity;
                
                half3 finalColor = texColor;
                half finalAlpha = texAlpha * maskAlpha * i.color.a;               
                
                // =================== 溶解处理 =========
                half customDissolveIntensity = i.customdata2.x;
                finalAlpha = ApplyDissolve(i.uv, finalAlpha, customDissolveIntensity);
                
                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
   
        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore d3d11 metal
            #pragma target 2.0

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
        
        Pass
        {
            Name "Meta"
            Tags{"LightMode" = "Meta"}

            Cull Off

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore d3d11 metal
            #pragma target 2.0

            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMetaUnlit
            #pragma shader_feature EDITOR_VISUALIZATION

            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitMetaPass.hlsl"

            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "Script.Editor.Engine.VFXShaderGUI"
}
