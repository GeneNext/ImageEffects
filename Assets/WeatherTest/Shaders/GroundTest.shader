Shader "WeatherTest/Ground" 
{
	Properties
	{
		_MainTex("Main Tex", 2D) = "white" {}
		_NormalTex("Normal Tex", 2D) = "bump" {}

		_MaskTex("Mask Tex", 2D) = "black"{}
		_MaskScale("Mask Scale", Range(0, 5)) = 1
		_Metallic("Metallic", Range(0, 1)) = 1
		_Smoothness("Smoothness", Range(0, 1)) = 1

		_ReflectTex ("Reflect Tex", 2D) = "black" {}
		_ReflectFactor ("Reflect Factor", Range(0, 2)) = 1

		//_Wet("Wet", Range(0, 1)) = 0

		_SnowTex ("Snow Tex", 2D) = "white" {}
		_SnowLevel ("Snow Level", Range(0, 1)) = 0
        _SnowColor ("Snow Color", Color) = (1.0,1.0,1.0,1.0)
        _SnowDirection ("Snow Direction", Vector) = (0,1,0)
        _SnowDepth ("Snow Depth", Range(0, 10)) = 1    
		_SnowWetness ("Snow Wetness", Range(0, 0.5)) = 0.3
	}

	CGINCLUDE

	#include "UnityCG.cginc"

	struct Input 
	{
		float2 uv_MainTex;
		float2 uv_NormalTex;
		float2 uv_MaskTex;
		float2 uv_SnowTex;
		float4 screenPosition;
		float3 worldRefl;
		float3 worldNormal;
		INTERNAL_DATA
	};

	uniform float _Rain;
	uniform float _Snow;

	sampler2D _MainTex;
	sampler2D _NormalTex;

	sampler2D _MaskTex;
	fixed _MaskScale;
	fixed _Metallic;
	fixed _Smoothness;

	sampler2D _ReflectTex;
	fixed _ReflectFactor;

	sampler2D _SnowTex;
	float _SnowLevel;
    float4 _SnowColor;
    float4 _SnowDirection;
    float _SnowDepth;
	float _SnowWetness;

	void vert(inout appdata_full v, out Input o)
    {
        UNITY_INITIALIZE_OUTPUT(Input, o);

		////将_SnowDirection转化到模型局部坐标系下
		//float4 snow = mul(UNITY_MATRIX_IT_MV, _SnowDirection);
		//
		//fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
		//if(dot(worldNormal, snow.xyz) >= lerp(1,-1, (_SnowLevel * 2) / 3))
		//{
		//    v.vertex.xyz += worldNormal * _SnowDepth * _SnowLevel * _Snow;// + v.normal;
		//}

		o.screenPosition = ComputeScreenPos(UnityObjectToClipPos(v.vertex));
		COMPUTE_EYEDEPTH(o.screenPosition.z);
    }

	ENDCG
		
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
	
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows nodynlightmap 
		#pragma target 3.0

		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			float4 mask = tex2D(_MaskTex, IN.uv_MaskTex);
			mask = 1- mask;
			mask = lerp(mask, smoothstep(mask, 0, 1 - _Rain), _Rain) * _MaskScale;

			o.Normal = UnpackScaleNormal(tex2D (_NormalTex, IN.uv_NormalTex), mask);
			
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			float difference = dot(WorldNormalVector(IN, o.Normal), _SnowDirection.xyz) - lerp(1, -1, _SnowLevel);
            difference = saturate(difference / _SnowWetness);
			float4 snow = tex2D (_SnowTex, IN.uv_SnowTex) * _SnowColor * _SnowDepth;
			c = lerp(c, snow, difference * _Snow);

			o.Albedo = c.rgb;
			o.Metallic = _Metallic * _Rain;
			o.Smoothness = _Smoothness * _Rain;
			o.Alpha = c.a;
		}
		ENDCG

		CGPROGRAM
		#pragma surface surf BlinnPhong vertex:vert alpha:fade fullforwardshadows
		
		void surf(Input IN, inout SurfaceOutput o) 
		{
			float4 mask = tex2D(_MaskTex, IN.uv_MaskTex);
			mask = 1- mask;
			mask = smoothstep(mask, 0, 1 - _Rain) * _MaskScale;
		
			o.Specular = _Metallic * mask;
			o.Gloss = _Smoothness * mask;
		
			//反射
			float2 normalOffset = o.Normal.xy * o.Normal.z;
			IN.screenPosition.xy = IN.screenPosition.xy / IN.screenPosition.z;
			o.Albedo = tex2D(_ReflectTex, IN.screenPosition.xy + normalOffset) * _ReflectFactor;
			
			o.Alpha = mask;
		}
		ENDCG
	}

	Fallback "Diffuse"
}