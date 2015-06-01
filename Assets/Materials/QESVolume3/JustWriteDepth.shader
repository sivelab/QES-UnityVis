// Shader used to store depth information for the backfaces of
// the volume.  This also sets the stencil buffer to 1 at any
// place where fragments are generated.

Shader "Custom/JustWriteDepth" {
	Properties {
	}
	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="Geometry+10" }
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
		ZWrite Off
		}
	}
	FallBack "Diffuse"
}
