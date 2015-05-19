Shader "Custom/boxClipPlane" {
	Properties {
		//_MainTex ("Volume (RGB)", 3D) = "white" {}
		//_RampTex ("Ramp Texture (RGBA)", 2D) = "white" {}
		//_NoiseTex ("Noise Texture (RGBA)", 2D) = "white" {}
		//_RelativeBounds ("Relative Bounds", Vector) = (0.5, 0.5, 0.5, 0.0)
		//_CameraTexPosition ("Camera Texture Coordinates", Vector) = (0,0,0,0)
		//_NumSlices ("Number of slices", Float) = 100
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue" = "Transparent+1" }
		LOD 200
		ZWrite Off
		Pass {
		Stencil {
    		Ref 1
    		Comp Equal
    	}
		
		CGPROGRAM
		#pragma exclude_renderers d3d11 xbox360
		#include "UnityCG.cginc"
		#pragma vertex vert
		#pragma fragment frag
		#pragma glsl

		#include "VolumeRender.cginc"
    	ENDCG
    	Blend SrcAlpha OneMinusSrcAlpha
    	ZWrite Off
    	ZTest Always
    	Cull Off
    	}
	} 
}