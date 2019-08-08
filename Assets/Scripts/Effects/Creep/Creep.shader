Shader "CITT/Creep" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}

		_NormalTex ("Normal Tex", 2D) = "bump" {}

		_ReflectTex ("Reflect", CUBE) = "black" {}
		_ReflectFactor ("Reflect Factor", Range(0, 10)) = 1

		_FlowSpeedX ("Flow Speed X", float) = 0
		_FlowSpeedY ("Flow Speed Y", float) = 0
	}

	CGINCLUDE

	#pragma multi_compile_fog

	#include "UnityCG.cginc"
	#include "Lighting.cginc"
	#include "AutoLight.cginc"

	float4 _Color;
		
	sampler2D _MainTex;
	float4 _MainTex_ST;

	sampler2D _NormalTex;
	float4 _NormalTex_ST;
	
	samplerCUBE _ReflectTex;
	float _ReflectFactor;
	
	float _FlowSpeedX;
	float _FlowSpeedY;
	

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
		float2 uv_NormalTex : TEXCOORD1;
		float3 viewDir : TEXCOORD2;
		float3 worldNormal : TEXCOORD3;
		float3 tangent : TEXCOORD4;
		float3 bitangent : TEXCOORD5;

		UNITY_FOG_COORDS(6)
		SHADOW_COORDS(7)
	};
	
	VertexOutput vert (VertexInput v)
	{
		VertexOutput output;

		UNITY_INITIALIZE_OUTPUT(VertexOutput, output);

		output.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
		output.uv_NormalTex = TRANSFORM_TEX(v.texcoord, _NormalTex);
		
		output.vertex = UnityObjectToClipPos(v.vertex);
		output.viewDir = UnityWorldSpaceViewDir(mul(unity_ObjectToWorld, v.vertex));
		output.worldNormal = UnityObjectToWorldNormal(v.normal);
        output.tangent = UnityObjectToWorldNormal(v.tangent);
        output.bitangent = cross(output.tangent, output.worldNormal);
		
		UNITY_TRANSFER_FOG(output, output.vertex);
		TRANSFER_SHADOW(output)

		return output;
	}

	float4 frag (VertexOutput input) : COLOR
	{
		input.tangent = normalize(input.tangent);
		input.bitangent = normalize(input.bitangent);
		input.worldNormal = normalize(input.worldNormal);

		float2 flowUV = float2(_FlowSpeedX, _FlowSpeedY) * _Time.x;
		
		float4 flowColor = tex2D (_MainTex, input.uv_MainTex + flowUV);

		float4 outputColor = flowColor * _Color;
		
		float3 normal = UnpackNormal(tex2D(_NormalTex, input.uv_NormalTex + flowUV));
		normal = float3(input.tangent * normal.r + input.bitangent * normal.g + input.worldNormal * normal.b);

		float3 reflectColor = texCUBE(_ReflectTex, reflect(-input.viewDir, normal)).rgb * _ReflectFactor;
		outputColor.rgb += flowColor * reflectColor.rgb;

        float shadowAttenuation = SHADOW_ATTENUATION(input);
		outputColor.rgb = outputColor.rgb * shadowAttenuation * (_LightColor0.rgb + UNITY_LIGHTMODEL_AMBIENT.rgb);

		UNITY_APPLY_FOG(input.fogCoord, outputColor);
		
		return saturate(outputColor);
	}

	ENDCG

	SubShader 
	{
		Tags { "Queue"="Geometry" "RenderType"="Opaque" }

		Pass
		{
			Cull Back
			ZWrite On
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			ENDCG
		}
	}

	FallBack "CITT/BaseShadowCaster"
}
