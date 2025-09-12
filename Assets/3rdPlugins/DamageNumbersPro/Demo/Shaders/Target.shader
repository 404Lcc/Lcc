// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Damage Numbers Pro/Target"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[ASEEnd]_Brightness("Brightness", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}
	
	SubShader
	{
		Tags { "RenderType"="Opaque" }
	LOD 100
		Cull Back

		
		Pass
		{
			CGPROGRAM
			
			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif

			#pragma target 3.0 
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			

			struct appdata
			{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				
			};

			uniform sampler2D _MainTex;
			uniform fixed4 _Color;
			uniform float4 _MainTex_ST;
			uniform float _Brightness;

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.texcoord.xy = v.texcoord.xy;
				o.texcoord.zw = v.texcoord1.xy;
				
				// ase common template code
				
				
				v.vertex.xyz +=  float3(0,0,0) ;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				fixed4 myColorVar;
				// ase common template code
				float2 uv_MainTex = i.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 tex2DNode6 = tex2D( _MainTex, uv_MainTex );
				float4 appendResult18 = (float4(( (tex2DNode6).rgb * _Brightness ) , tex2DNode6.a));
				
				
				myColorVar = appendResult18;
				return myColorVar;
			}
			ENDCG
		}
	}
	
	
	
}
/*ASEBEGIN
Version=18912
724;334;1374;768;1236.372;778.1483;1.434665;True;False
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;21;-1161.77,-334.8346;Inherit;False;0;0;_MainTex;Shader;False;0;5;SAMPLER2D;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;6;-940.8149,-323.5914;Inherit;True;Property;_Texture;Texture;1;0;Create;True;0;0;0;False;0;False;-1;None;ea01e3fafd48986428f8fac7ecfe9432;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;8;-545.1206,-325.8462;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;1;-484.9998,-196.1928;Inherit;False;Property;_Brightness;Brightness;0;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-252.6386,-272.7975;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;18;6.048921,-265.9704;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;20;283.3574,-306.2935;Float;False;True;-1;2;;100;6;Damage Numbers Pro/Target;6e114a916ca3e4b4bb51972669d463bf;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;RenderType=Opaque=RenderType;False;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;0;;0;0;Standard;0;0;1;True;False;;False;0
WireConnection;6;0;21;0
WireConnection;8;0;6;0
WireConnection;9;0;8;0
WireConnection;9;1;1;0
WireConnection;18;0;9;0
WireConnection;18;3;6;4
WireConnection;20;0;18;0
ASEEND*/
//CHKSM=57D84B8464CD9EE5814B931FE6297A17F5B99204