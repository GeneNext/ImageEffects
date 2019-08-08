Shader "CITT/Creep_Sort" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpTex ("Bump", 2D) = "bump" {}

		_DepthFactor ("Depth Factor", Range(0, 3)) = 0.05

		_NoiseSpeedX ("Noise Speed X", float) = 0
		_NoiseSpeedY ("Noise Speed Y", float) = 0

		_Snow ("Snow Level", Range(0,1)) = 0
        _SnowColor ("Snow Color", Color) = (1.0,1.0,1.0,1.0)
        _SnowDirection ("Snow Direction", Vector) = (0,1,0)
        _SnowDepth ("Snow Depth", Range(0,0.3)) = 0.1    
		_Wetness ("Wetness", Range(0, 0.5)) = 0.3
	}

	CGINCLUDE

	#pragma multi_compile_fog

	#include "UnityCG.cginc"
	#include "AutoLight.cginc"
		
	sampler2D _CameraDepthTexture;
	
	sampler2D _MainTex;
	float4 _MainTex_ST;
	
	sampler2D _NormalTex;
	float4 _NormalTex_ST;

	float4 _Color;
	
	float _DepthFactor;
	
	float _NoiseSpeedX;
	float _NoiseSpeedY;

		float _Snow;
        float4 _SnowColor;
        float4 _SnowDirection;
        float _SnowDepth;
		float _Wetness;

	struct VertexInput
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		float4 tangent : TANGENT;
		float4 texcoord : TEXCOORD0;
	};

	struct VertexOutput 
	{
        float4 vertex : SV_POSITION;
        float2 uv_MainTex : TEXCOORD0;
		float2 uv_NormalTex : TEXCOORD1;
		float3 viewDir : TEXCOORD2;
		float3 lightDir:TEXCOORD3;
		float3 worldNormal : TEXCOORD4;
		float3 tangent : TEXCOORD5;
		float3 bitangent : TEXCOORD6;
		float4 screenPosition : TEXCOORD7;
	};
	
	VertexOutput vert (VertexInput v)
	{
		VertexOutput output;

		UNITY_INITIALIZE_OUTPUT(VertexOutput, output);

		output.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
		output.uv_NormalTex = TRANSFORM_TEX(v.texcoord, _NormalTex);
		
		output.vertex = UnityObjectToClipPos(v.vertex);
		output.worldNormal = UnityObjectToWorldNormal(v.normal);
        output.tangent = UnityObjectToWorldNormal(v.tangent);
        output.bitangent = cross(output.tangent, output.worldNormal);
		float3 worldPosition = mul(unity_ObjectToWorld, v.vertex);
		output.viewDir = UnityWorldSpaceViewDir(worldPosition);
		output.lightDir = UnityWorldSpaceLightDir(worldPosition);
		output.screenPosition = ComputeScreenPos(output.vertex);
		COMPUTE_EYEDEPTH(output.screenPosition.z);

		   //将_SnowDirection转化到模型局部坐标系下
		   float4 sn = mul(UNITY_MATRIX_IT_MV, _SnowDirection);
		
		  if(dot(v.normal, sn.xyz) >= lerp(1,-1, (_Snow*2)/3))
		  {
		      output.vertex.xyz += (sn.xyz + v.normal) * _SnowDepth * _Snow;
		  }

		return output;
	}

	float4 frag (VertexOutput input) : COLOR
	{
		//计算深度差
		float sceneDepth = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, input.screenPosition).r);
		float partDepth = input.screenPosition.z;
		float depthFade = pow(saturate(sceneDepth - partDepth), _DepthFactor);
		
		float2 noiseUV = float2(_NoiseSpeedX, _NoiseSpeedY) * _Time.x;
		
		float4 flowColor = tex2D (_MainTex, input.uv_MainTex + noiseUV);
		
		input.tangent = normalize(input.tangent);
		input.bitangent = normalize(input.bitangent);
		input.worldNormal = normalize(input.worldNormal);

		float3 normal = UnpackNormal(tex2D(_NormalTex, input.uv_NormalTex + noiseUV));
		normal = float3(input.tangent * normal.r + input.bitangent * normal.g + input.worldNormal * normal.b);

		float4 c;
		c.rgb = flowColor.rgb * _Color.rgb;
		c.a = depthFade * _Color.a;
		
		//half lambert
		float diff = max(0, dot(normal, input.lightDir));
		diff = diff * 0.5 + 0.5;
		
		float difference = dot(normal, _SnowDirection.xyz) - lerp(1,-1,_Snow);;
        difference = saturate(difference / _Wetness);
        c.rgb = lerp(c.rgb, diff * _SnowColor.rgb, difference);

		return saturate(c);
	}

	ENDCG

	SubShader 
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }

		Pass
		{
			Cull Front
			ZWrite On
			Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			ENDCG
		}

		Pass
		{
		    Cull Back
		    ZWrite On
			Blend srcalpha oneminussrcalpha
		
		    CGPROGRAM
		
		    #pragma vertex vert
		    #pragma fragment frag
		
		    ENDCG
		}
	}

	FallBack "CITT/BaseShadowCaster"
}
