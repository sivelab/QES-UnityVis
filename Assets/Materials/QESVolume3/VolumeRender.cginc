// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members screenPos)
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members ray)
#pragma exclude_renderers d3d11 xbox360

sampler2D _CameraDepthTexture;

sampler3D _MainTex;
sampler2D _RampTex;
sampler2D _NoiseTex;
//float4 _RelativeBounds;
//float4 _CameraTexPosition;
//float _NumSlices;

float4x4 _CameraToWorld;
float4x4 WorldToLocal;
float4 _RelativeBounds;
float4 _RelativeSize;
float NumSteps;

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
    
    float4 ans = float4(0.0, 0.0, 0.0, 0.0);
    float weight = length(entryPoint * _RelativeSize.xyz - exitPoint * _RelativeSize.xyz);
	weight /= NumSteps;
	weight *= 90.0;
	for (float interp=0.0; interp <= NumSteps; interp += 1.0) {
		float blendAmount = (interp/NumSteps);
		//float noiseVal = tex2D(_NoiseTex, i.screenPos * 10.0 + half2(10.6, 15.4) * i.screenPos.z).x - 0.5;
		//blendAmount += noiseVal * 2.0 / NumSteps;
		half3 pos = entryPoint * blendAmount + exitPoint * (1.0 - blendAmount);
		float val = tex3D(_MainTex, pos * _RelativeBounds.xyz);
		half4 col = tex2D(_RampTex, half2( val, val));
		col.a *= weight;
		col.rgb *= col.a;
		ans = ans * (1.0 - col.a) + col;
	}
	ans.rgb /= ans.a;
    return ans;
    
}