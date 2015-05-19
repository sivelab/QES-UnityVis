Shader "Custom/JustWriteDepth" {
	Properties {
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		Pass {
		Stencil {
    		Ref 1
    		Pass Replace
    		Fail Zero
    		ZFail Replace
    		Comp Always
    	}
		
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
	    	
	        return half4(1.0, 1.0, 0.0, 0.0);
	        
	    }
		ENDCG
		ColorMask 0
		}
	}
	FallBack "Diffuse"
}
