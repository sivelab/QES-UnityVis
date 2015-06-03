// This include file renders the 3D volume.  This code is run
// on the front faces of the volume as well as the clipped plane
// of the volume.
//
// This code works by tracing a ray through the volume and accumulating
// color.  Each ray has a fixed number of samples, so the spacing
// along an individual ray is based upon the distance between
// the point where the ray enters the visible portion of the
// volume and the point where it exits.  See DrawQESVolume3.cs for more
// information on the technique used.

// Texture provided by Unity: Depth texture from camera's view.
// This is used to compute the world- (and then volume-)space
// coordinates of the exit point of the ray passing through a
// given fragment
sampler2D _CameraDepthTexture;

// MainTex contains the 3D volume data.  Currently, R, G, and B
// contain a uint8-quality representation of the normalized volume
// value.
sampler3D _MainTex;

// "Transfer function" used to assign color to volume samples.
// Currently, this is a simple 1D transfer function along the X
// axis, stored in an unmultiplied format
sampler2D _RampTex;
sampler2D _NoiseTex;

// _CameraToWorld is provided by Unity, but we need to add this
// symbol to have it populated.
float4x4 _CameraToWorld;

// WorldToLocal maps from world-space coordinates 
float4x4 WorldToLocal;

// For XYZ axes, a value ranging from 0.5 to 1 indicating what
// proportion of the texture range encodes actual data.  This
// is needed since Unity (at least 4.x) demanded that 3D texture
// be power-of-two.
float4 _RelativeBounds;

// Relative size of each axis of the volume.  The longest axis
// is given a value of '1', and the others proportionally less.
// This is used to convert between steps in volume space and
// distances in world space
float4 _RelativeSize;

// number of samples to take along each ray
float NumSteps;

struct v2f {
	// 3D vertex position in camera space
	float4 pos : SV_POSITION;
	
	// 3D position of entry point of ray in volume space
	float3 uvw : TEXCOORD0;
	// TODO: is this even used anymore?
	float4 screenPos : TEXCOORD1;
	// Ray used to look up values in depth texture to compute
	// exit point
	float3 ray: TEXCOORD2;
};

// Vertex shader to unpack entry point coordinates from the two
// texture coordinate variables passed in.  Unity only allows U and V
// to be specified for each texture coordinate, so encoding a 3D
// point requires swizzling like this.
//
// The code to output 'ray' and to use it in the fragment shader to
// look up the depth texture and compute the world-space position is
// adapted from the Unity built-in shaders.
v2f vert (appdata_full v)
{
    v2f o;
    o.uvw.xy = v.texcoord.xy;
    o.uvw.z = v.texcoord1.x; 
    o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
    o.screenPos = ComputeScreenPos(o.pos);
    float4 tmp = mul (UNITY_MATRIX_MV, v.vertex);
    o.ray = tmp.xyz/tmp.w * float3(-1,-1,1);
    return o;
}

half4 frag (v2f i) : COLOR
{
	half3 entryPoint = i.uvw;
	i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
    float2 uv = i.screenPos.xy / i.screenPos.w;
   
    float depth = UNITY_SAMPLE_DEPTH(tex2D (_CameraDepthTexture, uv));
    depth = Linear01Depth (depth);
    float4 vpos = float4(i.ray * depth,1);
    float4 wpos = mul (_CameraToWorld, vpos);
    float3 exitPoint = mul(WorldToLocal, float4(wpos.xyz/wpos.w, 1.0)).xyz;
    
    float4 ans = float4(0.0, 0.0, 0.0, 0.0);
    float weight = length(entryPoint * _RelativeSize.xyz - exitPoint * _RelativeSize.xyz);
	weight /= NumSteps;
	
	// Fixed factor to cause the volume to be sufficiently but not too opaque.
	// TODO: make this tuneable
	weight *= 1.0;
	
	// Step through the volume, accumulating color into the 'ans' variable.
	// Tracing is performed from exit point to entry point so that alpha equation
	// is simpler.  While running, 'ans' stores the color as premultiplied alpha
	// since it makes iterated blending far simpler.
	for (float interp=0.0; interp <= NumSteps; interp += 1.0) {
		float blendAmount = (interp/NumSteps);
		//float noiseVal = tex2D(_NoiseTex, i.screenPos * 10.0 + half2(10.6, 15.4) * i.screenPos.z).x - 0.5;
		//blendAmount += noiseVal * 2.0 / NumSteps;
		float3 pos = entryPoint * blendAmount + exitPoint * (1.0 - blendAmount);
		float val = tex3D(_MainTex, pos * _RelativeBounds.xyz);
		//float val = (pos.x + pos.y + pos.z)/3.0;
		float4 col = tex2D(_RampTex, half2( val, val));
		col.a *= weight;
		col.a = clamp(weight, 0.0, 1.0);
		col.rgb *= col.a;
		ans = ans * (1.0 - col.a) + col;
	}
	// Convert from premultiplied to unmultiplied alpha.
	ans.rgb /= ans.a;
	//ans.rgb = exitPoint;
	//ans.a = 1.0;
	return ans;
    
}