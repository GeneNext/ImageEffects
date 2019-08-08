Shader "CITT/Creep_Surface" 
{
	Properties 
	{
		[HDR]_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}

		_NormalTex ("Normal Tex", 2D) = "bump" {}

		[HDR]_EmissionColor ("Emission Color", Color) = (1,1,1,1)
		_EmissionTex ("Emission Tex", 2D) = "white" {}
		_EmissionFactor ("Emission Factor", Range(0, 2)) = 0
		
		_Gloss ("Gloss", Range(0, 3)) = 1
		_Shininess ("Shininess", Range(0.03, 1)) = 0.5

		_ReflectTex ("Reflect", CUBE) = "black" {}
		_ReflectFactor ("Reflect Factor", Range(0, 10)) = 1

		_FlowSpeedX ("Flow Speed X", float) = 0
		_FlowSpeedY ("Flow Speed Y", float) = 0

		_DepthFactor ("Depth Factor", Range(0, 3)) = 0.05
	}

	SubShader 
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }

		Cull Back
		ZWrite On
		
		CGPROGRAM

		float4 _Color;
		
		sampler2D _MainTex;

		sampler2D _NormalTex;

		float4 _EmissionColor;
		sampler2D _EmissionTex;
		float _EmissionFactor;
	
		samplerCUBE _ReflectTex;
		float _ReflectFactor;

		float _Gloss;
		float _Shininess;
	
		float _FlowSpeedX;
		float _FlowSpeedY;
		
		sampler2D _CameraDepthTexture;
		float _DepthFactor;
	
		struct Input 
		{
			float2 uv_MainTex;
			float2 uv_NormalTex;
			float2 uv_EmissionTex;
			float3 viewDir;
			float4 screenPosition;
		};
		
		#pragma surface surf Specular vertex:vert alpha:blend nodirlightmap
		#pragma multi_compile_fog

		#include "UnityCG.cginc"
		#include "SpecularLighting.cginc"

		void vert (inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.screenPosition = ComputeScreenPos(UnityObjectToClipPos(v.vertex));
			COMPUTE_EYEDEPTH(o.screenPosition.z);
		}
	
		void surf (Input input, inout SpecularOutput o) 
		{
			//计算深度差
			float sceneDepth = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, input.screenPosition).r);
			float partDepth = input.screenPosition.z;
			float depthFade = pow(saturate(sceneDepth - partDepth), _DepthFactor);

			float2 flowUV = float2(_FlowSpeedX, _FlowSpeedY) * _Time.x;
		
			float4 flowColor = tex2D (_MainTex, input.uv_MainTex + flowUV);

			float4 outputColor = flowColor * _Color;
		
			float3 normal = UnpackNormal(tex2D(_NormalTex, input.uv_NormalTex + flowUV));

			float3 reflectColor = texCUBE(_ReflectTex, reflect(-input.viewDir, normal)).rgb * _ReflectFactor;
			outputColor.rgb += flowColor * reflectColor.rgb;

			o.Albedo = outputColor.rgb;
			o.Gloss = flowColor.a * _Gloss;
			o.Specular = _Shininess;
			o.Emission = tex2D (_EmissionTex, input.uv_EmissionTex + flowUV) * _EmissionColor * _EmissionFactor;
			o.Alpha = depthFade;
		}
		
		ENDCG
	}

	FallBack "CITT/BaseShadowCaster"
}
