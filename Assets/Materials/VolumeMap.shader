Shader "Custom/VolumeMap" {
	Properties {
		_MainTex ("Volume (RGB)", 3D) = "white" {}
		_RampTex ("Ramp Texture (RGBA)", 2D) = "white" {}
		_NoiseTex ("Noise Texture (RGBA)", 2D) = "white" {}
		_RelativeBounds ("Relative Bounds", Vector) = (0.5, 0.5, 0.5, 0.0)
		_CameraTexPosition ("Camera Texture Coordinates", Vector) = (0,0,0,0)
		_NumSlices ("Number of slices", Float) = 100
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
		LOD 200
		ZWrite Off
		Pass {
		
		CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members screenPos)
#pragma exclude_renderers d3d11 xbox360
		#include "UnityCG.cginc"
		#pragma vertex vert
		#pragma fragment frag
		#pragma glsl

		sampler3D _MainTex;
		sampler2D _RampTex;
		sampler2D _NoiseTex;
		float4 _RelativeBounds;
		float4 _CameraTexPosition;
		float _NumSlices;

		struct v2f {
        	float4 pos : SV_POSITION;
  	    	float3 uvw : TEXCOORD0;
  	    	float4 screenPos : TEXCOORD1;
   		};

	    v2f vert (appdata_full v)
	    {
	        v2f o;
	        o.uvw.xy = v.texcoord.xy;
	        o.uvw.z = v.texcoord1.x;
	        o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
	        o.screenPos = ComputeScreenPos(o.pos);
	        o.screenPos /= o.screenPos.w;
	        o.screenPos.xy += half2(1.6, 1.4) * o.screenPos.z;
	        return o;
	    }

	    half4 frag (v2f i) : COLOR
	    {
	    	float noiseVal = tex2D(_NoiseTex, i.screenPos * 10.0).x;
	    	half4 noiseVector = half4(0.0, 0.0, 0.0, 0.0);
	    	noiseVector = normalize(half4(i.uvw,1.0) - _CameraTexPosition) / _NumSlices;
	    	half3 newvw = i.uvw + (noiseVal - 0.5) * 150 * noiseVector.xyz;
	    	if (newvw.x < 0.0 ||
	    		newvw.x > 1.0 ||
	    		newvw.y < 0.0 ||
	    		newvw.y > 1.0 ||
	    		newvw.z < 0.0 ||
	    		newvw.z > 1.0) {
	    		discard;
	    	}
	    	
	        float val = tex3D(_MainTex, newvw * _RelativeBounds.xyz);
	        
	        half4 ans = tex2D(_RampTex, half2( val, val));
	        
	        ans.a = pow (ans.a, sqrt(_NumSlices / 200.0));
	        
	        return ans;
	        
	    }
    	ENDCG
    	Blend SrcAlpha OneMinusSrcAlpha 
    	}
	} 
	FallBack "Diffuse"
}
