Shader "Custom/VolumeMap" {
	Properties {
		_MainTex ("Volume (RGB)", 3D) = "white" {}
		_RelativeBounds ("Relative Bounds", Vector) = (0.5, 0.5, 0.5, 0.0)
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
		LOD 200
		Pass {
		
		CGPROGRAM
		#include "UnityCG.cginc"
		#pragma vertex vert
		#pragma fragment frag
		#pragma glsl

		sampler3D _MainTex;
		float4 _RelativeBounds;

		struct v2f {
        	float4 pos : SV_POSITION;
  	    	float3 uvw : TEXCOORD0;
   		};

	    v2f vert (appdata_full v)
	    {
	        v2f o;
	        o.uvw.xy = v.texcoord.xy;
	        o.uvw.z = v.texcoord1.x;
	        o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
	        return o;
	    }

	    half4 frag (v2f i) : COLOR
	    {
	    	if (i.uvw.x < 0.0 ||
	    		i.uvw.x > 1.0 ||
	    		i.uvw.y < 0.0 ||
	    		i.uvw.y > 1.0 ||
	    		i.uvw.z < 0.0 ||
	    		i.uvw.z > 1.0) {
	    		discard;
	    	}
	        float val = tex3D(_MainTex, i.uvw * _RelativeBounds.xyz);
	        return half4(val * val, val, sqrt(val), pow(val, 3.0));
	    }
    	ENDCG
    	Blend SrcAlpha OneMinusSrcAlpha 
    	}
	} 
	FallBack "Diffuse"
}
