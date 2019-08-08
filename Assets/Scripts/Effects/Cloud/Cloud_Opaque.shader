Shader "TEST/Cloud_Opaque" 
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
	}

	CGINCLUDE

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
        float2 uv_MainTex : TEXCOORD3;
        float2 uv_MixTex : TEXCOORD4;
		UNITY_FOG_COORDS(5)
    };

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
		
		//雾
		UNITY_TRANSFER_FOG(output, output.vertex);

		return output;
	}

	float4 frag (VertexOutput input) : COLOR
	{
		float speed = Luminance(tex2D(_MainTex, input.uv_MainTex).rgb);

		//云层1
		float2 flowUV = float2(_FlowSpeedX, _FlowSpeedY) * (_Time.x + speed * 0.1);
		float4 flowColor = tex2D(_MainTex, input.uv_MainTex + flowUV);

		//云层2
		float2 mixUV = float2(_MixSpeedX, _MixSpeedY) * _Time.x;
		float4 mixColor = tex2D(_MixTex, input.uv_MixTex + mixUV);

		//混合
		float4 c;
		c.rgb = (flowColor.rgb + mixColor.rgb) * _Color.rgb;//TODO lighting
		c.rgb += _AddColor.rgb;
		c.a = 1;

		//输出
		c = saturate(c);
		
		UNITY_APPLY_FOG(input.fogCoord, c);

		return c;
	}

	ENDCG

    SubShader
	{
        Tags 
		{ 
			"Queue" = "Geometry" 
			"RenderType" = "Opaque" 
		}
		
        Pass
		{
			ZWrite On

            CGPROGRAM
		
            #pragma vertex vert
            #pragma fragment frag
		
			#pragma multi_compile_fog
		
            ENDCG
        }
    }
}
