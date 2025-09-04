#include "aline_common.cginc"

struct v2f {
    float4  pos : SV_POSITION;
    float4 col : COLOR;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_OUTPUT_STEREO
};

v2f vert_base (appdata_color v, float4 tint, float scale) {
    UNITY_SETUP_INSTANCE_ID(v);
    v2f o;
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    o.pos = TransformObjectToHClip(v.vertex.xyz);

    float4 worldSpace = mul(UNITY_MATRIX_M, v.vertex);
    o.uv = float2 (worldSpace.x*scale,worldSpace.z*scale);
    o.col = v.color * tint;
    o.col.rgb = ConvertSRGBToDestinationColorSpace(o.col.rgb);
    return o;
}
