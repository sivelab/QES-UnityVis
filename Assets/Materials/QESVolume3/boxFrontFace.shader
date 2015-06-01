// This shader is used for the QESv3 volume rendering code.  This shader
// uses VolumeRender.cginc to provide the volume rendering on the front
// surface of the rendered volume.
//
// See the full description in DrawQESVolume3.cs for how the pieces fit together.


Shader "Custom/boxFrontFace" {
	Properties {
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
		LOD 200
		
		// Zero the values in the stencil buffer wherever this polygon
		// is rasterized.  This will be used to add the clip plane using
		// boxClipPlane.shader
		Pass {
		Stencil {
    		Ref 1
    		Pass Zero
    		Fail Zero
    		ZFail Zero
    		
    	}
		
		CGPROGRAM
		#include "UnityCG.cginc"
		#pragma vertex vert
		#pragma fragment frag
		#pragma glsl

		#include "VolumeRender.cginc"
    	ENDCG
    	Blend SrcAlpha OneMinusSrcAlpha
    	ZWrite Off
    	
    	}
	} 
}