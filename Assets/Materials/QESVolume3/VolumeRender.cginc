// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members screenPos)
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members ray)
#pragma exclude_renderers d3d11 xbox360

sampler2D _CameraDepthTexture;

//sampler3D _MainTex;
//sampler2D _RampTex;
//sampler2D _NoiseTex;
//float4 _RelativeBounds;
//float4 _CameraTexPosition;
//float _NumSlices;

float4x4 _CameraToWorld;
float4x4 WorldToLocal;

struct v2f {
	float4 pos : SV_POSITION;
	float3 uvw : TEXCOORD0;
	float4 screenPos : TEXCOORD1;
	float3 ray;
};

v2f vert (appdata_full v)
{
    v2f o;
    o.uvw.xy = v.texcoord.xy;
    o.uvw.z = v.texcoord1.x; 
    o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
    o.screenPos = ComputeScreenPos(o.pos);
    o.ray = mul (UNITY_MATRIX_MV, v.vertex).xyz * float3(-1,-1,1);
    return o;
}

half4 frag (v2f i) : COLOR
{
	//half4 ans = half4(i.uvw, 1.0);
	//half4 ans = tex2D(_DepthTexture, i.screenPos.xy/i.screenPos.w);

	half3 entryPoint = i.uvw;
	i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
    float2 uv = i.screenPos.xy / i.screenPos.w;
   
    float depth = UNITY_SAMPLE_DEPTH(tex2D (_CameraDepthTexture, uv));
    depth = Linear01Depth (depth);
    float4 vpos = float4(i.ray * depth,1);
    float3 wpos = mul (_CameraToWorld, vpos).xyz;
    float3 exitPoint = mul(WorldToLocal, float4(wpos, 1.0)).xyz;
	float dist = length(entryPoint-exitPoint);
	float4 ans = float4(dist, dist, dist, 1.0);
    return ans;
    
}