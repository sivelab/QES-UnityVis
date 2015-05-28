Shader "Custom/ForceDepthOutput" {
	Properties {
	}
	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="Geometry-100" }
		LOD 200
		
		Pass {
		
		CGPROGRAM
#include "UnityCG.cginc"
		#pragma vertex vert
		#pragma fragment frag

		struct v2f {
			float4 pos : SV_POSITION;
   		};

		v2f vert (appdata_full v) {
	        v2f o;
	        o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
	        return o;
	    }

	    half4 frag (v2f i) : COLOR
	    {
	        return half4(0.5, 0.0, 1.0, 0.0);
	        
	    }
		ENDCG
		ColorMask 0
		ZWrite On
		ZTest Always
		}
	}
}
