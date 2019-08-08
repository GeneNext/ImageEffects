Shader "TEST/Cloud_Sort" 
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_AddColor ("Add Color", Color) = (0,0,0,0)
		_MainTex ("Main Tex", 2D) = "white"
		_FlowSpeedX ("Speed X", float) = 0
		_FlowSpeedY ("Speed Y", float) = 0

		_MixTex ("Mix Tex", 2D) = "white"
		_MixSpeedX ("Mix Speed X", float) = 1
		_MixSpeedY ("Mix Speed Y", float) = 1

		_DepthFactor ("Depth Factor", float) = 0.05
		_DepthRange ("Depth Range", float) = 10

		_RimFactor ("Rim Factor", float) = 0.8
	}

	CGINCLUDE
		
	#pragma multi_compile_fog

	#include "UnityCG.cginc"

	float4 _Color;
	float4 _AddColor;
	sampler2D _MainTex;
	float4 _MainTex_ST;
	float _FlowSpeedX;
	float _FlowSpeedY;

	sampler2D _MixTex;
	float4 _MixTex_ST;
	float _MixSpeedX;
	float _MixSpeedY;

	sampler2D _CameraDepthTexture;

	float _DepthFactor;
	float _DepthRange;
	float _RimFactor;

	struct VertexInput
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		float4 texcoord : TEXCOORD0;
		float4 texcoord1 : TEXCOORD1;
		float4 color : COLOR;
	};

    struct VertexOutput 
	{
        float4 vertex : SV_POSITION;
        float4 worldPosition : TEXCOORD0;
        float3 worldNormal : TEXCOORD1;
		float3 viewDir : TEXCOORD2;
        float4 proj : TEXCOORD3;
        float2 uv_MainTex : TEXCOORD4;
        float2 uv_MixTex : TEXCOORD5;
		UNITY_FOG_COORDS(6)
    };

	inline float4 ClampColor(float4 color, float min, float max)
	{
		color.r = clamp(color.r, min, max);
		color.g = clamp(color.g, min, max);
		color.b = clamp(color.b, min, max);
		color.a = clamp(color.a, min, max);
		return color;
	}

	VertexOutput vert (VertexInput v)
	{
		VertexOutput output;

		UNITY_INITIALIZE_OUTPUT(VertexOutput, output);

		//uv
		output.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
		output.uv_MixTex = TRANSFORM_TEX(v.texcoord, _MixTex);

		//
		output.vertex = UnityObjectToClipPos(v.vertex);
		output.worldPosition = mul(unity_ObjectToWorld, v.vertex);
		output.worldNormal = UnityObjectToWorldNormal(v.normal);
		output.viewDir = normalize(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, v.vertex).xyz);
		output.proj = ComputeScreenPos(UnityObjectToClipPos(v.vertex));
		COMPUTE_EYEDEPTH(output.proj.z);
		
		//ŒÌ
		UNITY_TRANSFER_FOG(output, output.vertex);

		return output;
	}

	float4 frag (VertexOutput input) : COLOR
	{
		//º∆À„…Ó∂»≤Ó
		float sceneDepth = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, input.proj).r);
		float partDepth = input.proj.z;
		float depthFade = (sceneDepth - partDepth) * _DepthFactor;
		depthFade = saturate(depthFade) * _DepthRange;

		//º∆À„±ﬂ‘µ
		//float rim = saturate(dot(normalize(input.viewDir), input.worldNormal)) - _RimFactor;
		float rim = pow(max(0, dot(input.worldNormal, input.viewDir)), _RimFactor);
		rim = saturate(rim);

		float alpha = depthFade * rim;

		float noiseSpeed = tex2D(_MainTex, input.uv_MainTex + _Time.x * 0.1).r * 0.1;

		//‘∆≤„1
		float2 flowUV = float2(_FlowSpeedX, _FlowSpeedY);
		float4 flowColor = tex2D(_MainTex, input.uv_MainTex + flowUV * _Time.x + flowUV * noiseSpeed);

		//‘∆≤„2
		float2 mixUV = float2(_MixSpeedX, _MixSpeedY);
		float4 mixColor = tex2D(_MixTex, input.uv_MixTex + mixUV * _Time.x + mixUV * noiseSpeed);

		//ªÏ∫œ
		float4 c;
		c.rgb = (flowColor.rgb + mixColor.rgb) * _Color.rgb;//TODO lighting
		c.rgb += _AddColor.rgb;
		c.a = alpha * Luminance(c.rgb) * _Color.a;
		
		UNITY_APPLY_FOG(input.fogCoord, c);

		// ‰≥ˆ
		c = saturate(c);

		return c;
	}

	ENDCG

    SubShader
	{
        Tags 
		{ 
			"Queue" = "Transparent" 
			"RenderType" = "Transparent" 
		}
		
        Pass
		{
            Cull Front
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
		
            CGPROGRAM
		
            #pragma vertex vert
            #pragma fragment frag
		
            ENDCG
        }

        Pass
		{
            Cull Back
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            ENDCG
        }
    }
}
