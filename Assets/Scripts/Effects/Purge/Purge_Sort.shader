Shader "TEST/Purge_Sort" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_ColorFactor ("Color Factor", float) = 1

		_DepthFactor ("Depth Factor", Range(0, 3)) = 0.05

		_Alpha ("Alpha", float) = 1

		_NoiseSpeedX ("Noise Speed X", float) = 0.6
		_NoiseSpeedY ("Noise Speed Y", float) = 0.6
	}

	CGINCLUDE

	#pragma multi_compile_fog

	#include "UnityCG.cginc"
		
	sampler2D _CameraDepthTexture;
	
	sampler2D _MainTex;
	float4 _MainTex_ST;

	float4 _Color;
	float _ColorFactor;
	
	float _DepthFactor;
	float _DepthRange;
	
	float _Alpha;
	
	float _NoiseSpeedX;
	float _NoiseSpeedY;

	struct VertexInput
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		float4 texcoord : TEXCOORD0;
		float4 texcoord1 : TEXCOORD1;
		float4 tangent : TANGENT;
	};

	struct VertexOutput 
	{
        float4 vertex : SV_POSITION;
        float2 uv_MainTex : TEXCOORD0;
		float3 viewDir : TEXCOORD1;
		float3 worldNormal : TEXCOORD2;
		float4 screenPosition : TEXCOORD3;
	};
	
	VertexOutput vert (VertexInput v)
	{
		VertexOutput output;

		UNITY_INITIALIZE_OUTPUT(VertexOutput, output);

		output.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
		
		output.vertex = UnityObjectToClipPos(v.vertex);
		output.worldNormal = UnityObjectToWorldNormal(v.normal);
		output.viewDir = UnityWorldSpaceViewDir(mul(unity_ObjectToWorld, v.vertex));
		output.screenPosition = ComputeScreenPos(output.vertex);
		COMPUTE_EYEDEPTH(output.screenPosition.z);

		return output;
	}

	float4 frag (VertexOutput input) : COLOR
	{
		//计算深度差
		float sceneDepth = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, input.screenPosition).r);
		float partDepth = input.screenPosition.z;
		float depthFade = pow(saturate(sceneDepth - partDepth), _DepthFactor);
		
		float2 noiseUV = float2(_NoiseSpeedX, _NoiseSpeedY) * _Time.x;
		
		float4 flowColor1 = tex2D (_MainTex, input.uv_MainTex + noiseUV);
		float4 flowColor2 = tex2D (_MainTex, input.uv_MainTex + noiseUV);
		
		float4 c;
		c.rgb = lerp(flowColor1, flowColor2, 0.5).rgb * _Color.rgb * _ColorFactor;
		c.a = (Luminance(c.rgb) * depthFade) * _Alpha;
		c.a = clamp(c.a, 0, 1);

		return c;
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
			Blend SrcAlpha OneMinusSrcAlpha
		
		    CGPROGRAM
		
		    #pragma vertex vert
		    #pragma fragment frag
		
		    ENDCG
		}
	}

	FallBack "CITT/BaseShadowCaster"
}
