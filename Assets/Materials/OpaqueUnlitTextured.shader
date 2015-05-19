﻿Shader "Custom/OpaqueUnlitTextured" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		Pass {
		
		CGPROGRAM
#include "UnityCG.cginc"
#pragma exclude_renderers d3d11 xbox360
		#pragma vertex vert
		#pragma fragment frag

		sampler2D _MainTex;

		struct v2f {
			float4 pos : SV_POSITION;
  	    	float2 uv : TEXCOORD0;
   		};

		v2f vert (appdata_full v) {
	        v2f o;
	        o.uv = v.texcoord.xy;
	        o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
	        return o;
	    }

	    half4 frag (v2f i) : COLOR
	    {
	    	half4 ans = tex2D(_MainTex, i.uv);
	        return ans;
	        
	    }
		ENDCG
		}
	} 
	FallBack "Diffuse"
}
