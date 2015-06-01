// Force geometry to get written to the depth buffer, overwriting
// anything in there previously.  This is used to "clear" the depth
// buffer on DirectX platforms after JustWriteDepth is used to populate
// the depth buffer with the back-face data, as Unity doesn't allow
// us to insert a direct call to clear.

// On DX platforms, the values written into the depth buffer are not
// just used for populating a texture, but are also used when rendering
// the scene as a sort of early-z test.  The (partial) order of
// rendering passes is therefore:
//
// 0. Clear color and depth (and stencil)
// 1. Render opaque objects using a replacement shader to form a depth
//    texture for later
// 2. Clear the color buffer
// 3. Render opaque geometry, USING THE SAVED DEPTH BUFFER (and a
//    less-than-or-equal comparison)
//
// This shader has a queue of Geometry-100 so that it will render in the
// third phase (opaque geometry) before all other objects.  By putting
// this shader on a plane at 99% of the far plane distance, we force a
// value of 99% to be used to "clear" the depth buffer.

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
		
		// Use a colormask of zero to prevent any RGBA data from
		// this shader getting written into the color buffer.
		ColorMask 0
		ZWrite On
		ZTest Always
		}
	}
}
