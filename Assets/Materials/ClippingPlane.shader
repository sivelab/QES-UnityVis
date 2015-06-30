Shader "Custom/ClippingPlane" {
	Properties {

		_MainTex ("Albedo (RGB)", 3D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		Pass {
		CGPROGRAM
		#include "UnityCG.cginc"
		
		#pragma vertex vert
		#pragma fragment frag
		

// MainTex contains the 3D volume data.  Currently, R, G, and B
// contain a uint8-quality representation of the normalized volume
// value.
sampler3D _MainTex;

// "Transfer function" used to assign color to volume samples.
// Currently, this is a simple 1D transfer function along the X
// axis, stored in an unmultiplied format
sampler2D _RampTex;

// Matrix to convert from world coordinates into volume coordinates
float4x4 WorldToVolume;

float4 _RelativeBounds;

struct v2f {
	// 3D vertex position in camera space
	float4 pos : SV_POSITION;
	
	// 3D position in volume space
	float3 uvw : TEXCOORD0;
	
};

v2f vert (appdata_full v)
{
    v2f o;
    o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
    o.uvw = mul(WorldToVolume, mul(_Object2World, v.vertex)).xyz;
    return o;
}

half4 frag (v2f i) : COLOR
{
	float3 clmp = clamp(i.uvw, 0.0, 1.0);
	if ( clmp.x != i.uvw.x || clmp.y != i.uvw.y || clmp.z != i.uvw.z) {
		discard;
	}
	half3 tx = i.uvw * _RelativeBounds.xyz;
	tx = clamp(tx, 0.0, 1.0);
	half3 dtxdx = ddx(tx);
	half3 dtxdy = ddy(tx);
	float val = tex3D(_MainTex, tx).x;
	float valx = tex3D(_MainTex, clamp(tx + dtxdx, 0.0, 1.0)).x;
	float valy = tex3D(_MainTex, clamp(tx + dtxdy, 0.0, 1.0)).x;
	half4 ans = half4(val, val*2.0, val*4.0, 1.0);
	if (floor(val * 5.0) != floor(valx * 5.0) || floor(val * 5.0) != floor(valy * 5.0)) {
		ans = half4(0.0, 0.0, 0.0, 1.0);
	}
	return ans;
    
}
		
		
		ENDCG
		}
		}
	FallBack "Diffuse"
}
