Shader "CITT/Fur"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_FurTex("Fur pattern", 2D) = "white" {}
		_FurLength("Fur length", Range(0.0, 1)) = 0.5
		_Cutoff("Alpha cutoff", Range(0, 1)) = 0.5
		_Thickness("Thickness", Range(0, 1)) = 0
		_RimFactor("RimFactor", Range(0, 10)) = 0
	}
 
	SubShader
	{
		Tags
		{ 
			"Queue" = "Transparent"  
			"LightMode" = "ForwardBase" 
			"RenderType" = "Transparent"   
		}
 
		ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define FURSTEP 0.05
			#include "FurLib.cginc"
			ENDCG
		}
 
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define FURSTEP 0.10
			#include "FurLib.cginc"
			ENDCG
		}
 
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define FURSTEP 0.15
			#include "FurLib.cginc"
			ENDCG
		}
 
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define FURSTEP 0.20
			#include "FurLib.cginc"
			ENDCG
		}
 
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define FURSTEP 0.25
			#include "FurLib.cginc"
			ENDCG
		}
 
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define FURSTEP 0.30
			#include "FurLib.cginc"
			ENDCG
		}
 
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define FURSTEP 0.35
			#include "FurLib.cginc"
			ENDCG
		}
 
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define FURSTEP 0.40
			#include "FurLib.cginc"
			ENDCG
		}
 
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define FURSTEP 0.45
			#include "FurLib.cginc"
			ENDCG
		}
 
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define FURSTEP 0.50
			#include "FurLib.cginc"
			ENDCG
		}
 
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define FURSTEP 0.55
			#include "FurLib.cginc"
			ENDCG
		}
 
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define FURSTEP 0.60
			#include "FurLib.cginc"
			ENDCG
		}
 
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define FURSTEP 0.65
			#include "FurLib.cginc"
			ENDCG
		}
 
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define FURSTEP 0.70
			#include "FurLib.cginc"
			ENDCG
		}
 
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define FURSTEP 0.75
			#include "FurLib.cginc"
			ENDCG
		}
 
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define FURSTEP 0.80
			#include "FurLib.cginc"
			ENDCG
		}
 
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define FURSTEP 0.85
			#include "FurLib.cginc"
			ENDCG
		}
 
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define FURSTEP 0.90
			#include "FurLib.cginc"
			ENDCG
		}
 
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define FURSTEP 0.95
			#include "FurLib.cginc"
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define FURSTEP 1
			#include "FurLib.cginc"
			ENDCG
		}
	}
}