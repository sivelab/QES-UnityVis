// This shader is used for the QESv3 volume rendering code.  This shader
// uses VolumeRender.cginc to provide the volume rendering on the clipped
// portion of the rendered volume.
//
// See the  full description in DrawQESVolume3.cs for how the pieces fit together.

Shader "Custom/boxClipPlane" {
	Properties {
	}
	SubShader {
		// Render in queue Transparent+1 so that transparent objects (such as 
		// the volume boundary volume rendering code) is rendered first
		Tags { "RenderType"="Transparent" "Queue" = "Transparent+1" }
		LOD 200
		
		// Only output fragments where the stencil value is 1, corresponding
		// to areas where the volume backfaces were drawn but the volume front
		// faces were not
		
		Pass {
		Stencil {
    		Ref 1
    		Comp Equal
    	}
		
		CGPROGRAM
		#include "UnityCG.cginc"
		#pragma vertex vert
		#pragma fragment frag

		#include "VolumeRender.cginc"
    	ENDCG
    	Blend SrcAlpha OneMinusSrcAlpha
    	ZWrite Off
    	// Set z-test to always pass since we will be drawing a plane that has
    	// a physical position further away than the near plane to avoid any
    	// potential inaccuracies if math causes a vertex to lie slightly
    	// behind the near plane.
    	ZTest Always
    	Cull Off
    	}
	} 
}