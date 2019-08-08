Shader "CITT/Crystal_Sort" 
{
	Properties 
	{
		_Color ("Color", Color) = (0, 0, 0, 0)
		_MainTex ("Albedo (RGB)", 2D) = "black" {}
		
		_NormalTex ("Normal", 2D) = "bump" {}

		_Gloss ("Gloss", Range(0, 3)) = 1
		_Shininess ("Shininess", Range(0.03, 1)) = 0.5

		_AreaTex ("Area Tex", 2D) = "black" {}

		_ReflectTex ("Reflect", CUBE) = "black" {}
		_ReflectFactor ("Reflect Factor", Range(0, 10)) = 1

		_RimFactor ("Rim Factor", Range(0, 10)) = 0
		_RimIntensity ("Rim Intensity", Range(0, 5)) = 0
		_RimAdd ("Rim Add", Range(0, 1)) = 0
		
		_FlowTex ("Flow Tex", 2D) = "black" {}
		_FlowSpeedX ("Speed X", float) = 0
		_FlowSpeedY ("Speed Y", float) = 0
		_FlowColor ("Flow Color", Color) = (0, 0, 0, 0)
		_FlowFactor ("Flow Factor", Range(0, 20)) = 0

		_EmissionColor ("Emission Color", Color) = (0, 0, 0, 0)

		_NoiseTex("NoiseTex", 2D) = "white" {}
		[HDR]_EdgeColor("Edge Color", Color) = (1,1,1,1)
		_EdgeSize("EdgeSize", Range(0,1)) = 0.2
		_Cutoff("cutoff", Range(0,1)) = 0.0

		_OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
	}

	CGINCLUDE
		
	#pragma multi_compile_fog

	#include "UnityCG.cginc"
	#include "Lighting.cginc"
		
	float4 _Color;

	sampler2D _MainTex;
	float4 _MainTex_ST;

	sampler2D _NormalTex;
	float4 _NormalTex_ST;
	float4 _NormalTex_TexelSize;

	float _Gloss;
	float _Shininess;
	
	sampler2D _AreaTex;
	float4 _AreaTex_ST;

	samplerCUBE _ReflectTex;
	float _ReflectFactor;

	float _RimFactor;
	float _RimIntensity;
	float _RimAdd;

	sampler2D _FlowTex;
	float4 _FlowTex_ST;
	float _FlowSpeedX;
	float _FlowSpeedY;
	float4 _FlowColor;
	float _FlowFactor;

	float4  _EmissionColor;

	sampler2D _NoiseTex;
	float4 _NoiseTex_ST;
	half _EdgeSize;
	half4 _EdgeColor;
	half _Cutoff;

	struct VertexInput
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		float4 tangent : TANGENT;
		float4 texcoord : TEXCOORD0;
		float4 texcoord1 : TEXCOORD1;
	};

    struct VertexOutput 
	{
        float4 vertex : SV_POSITION;

        float2 uv_MainTex : TEXCOORD0;
		float2 uv_NormalTex : TEXCOORD1;
		float2 uv_AreaTex : TEXCOORD2;
		float2 uv2_FlowTex : TEXCOORD3;
		float2 uv_NoiseTex : TEXCOORD4;
		
		float3 viewDir:TEXCOORD5;
		float3 lightDir:TEXCOORD6;

        float3 worldNormal : TEXCOORD7;
		float3 tangent : TEXCOORD8;
		float3 bitangent : TEXCOORD9;

		UNITY_FOG_COORDS(10)
    };

	VertexOutput vert (VertexInput v)
	{
		VertexOutput output;

		UNITY_INITIALIZE_OUTPUT(VertexOutput, output);

		//uv
		output.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
		output.uv_NormalTex = TRANSFORM_TEX(v.texcoord, _NormalTex);
		output.uv_AreaTex = TRANSFORM_TEX(v.texcoord, _AreaTex);
		output.uv2_FlowTex = TRANSFORM_TEX(v.texcoord1, _FlowTex);
		output.uv_NoiseTex = TRANSFORM_TEX(v.texcoord, _NoiseTex);

		//
		output.vertex = UnityObjectToClipPos(v.vertex);
		float3 worldPosition = mul(unity_ObjectToWorld, v.vertex);
		output.viewDir = UnityWorldSpaceViewDir(worldPosition);
		output.lightDir = UnityWorldSpaceLightDir(worldPosition);
		output.worldNormal = UnityObjectToWorldNormal(v.normal);
        output.tangent = UnityObjectToWorldNormal(v.tangent);
        output.bitangent = cross(output.tangent, output.worldNormal);
		
		//Îí
		UNITY_TRANSFER_FOG(output, output.vertex);

		return output;
	}

	float4 frag (VertexOutput input) : COLOR
	{
		float4 outputColor;

		float4 mainTex = tex2D(_MainTex, input.uv_MainTex);
		outputColor.rgb = mainTex.rgb * _Color.rgb;
		outputColor.a = 0;

		input.tangent = normalize(input.tangent);
		input.bitangent = normalize(input.bitangent);
		input.worldNormal = normalize(input.worldNormal);

		float3 normal = UnpackNormal(tex2D(_NormalTex, input.uv_NormalTex));
		normal = float3(input.tangent * normal.r + input.bitangent * normal.g + input.worldNormal * normal.b);
		
		//area
		float4 areaTex = tex2D(_AreaTex, input.uv_AreaTex);
		outputColor.a += areaTex.r * 1.5;

		//reflect
		float3 reflectColor = texCUBE(_ReflectTex, reflect(-input.viewDir, normal)).rgb * _ReflectFactor;
		outputColor.rgb += reflectColor.rgb;

		//rim
		float rim = 1 - dot(normal, input.viewDir);
		rim = saturate(pow(rim * _RimIntensity, _RimFactor)) + _RimAdd;
		outputColor.a += rim;

		//flow
		float4 flowColor = tex2D(_FlowTex, input.uv2_FlowTex + float2(_FlowSpeedX, _FlowSpeedY) * _Time.x) * _FlowColor * _FlowFactor * areaTex.g;
		outputColor.rgb += flowColor.rgb;

		//Âþ·´Éä
		float diff = max(0, dot(normal, input.lightDir));
		outputColor.rgb *= saturate(diff * _LightColor0.rgb + UNITY_LIGHTMODEL_AMBIENT.rgb);
		
		//¸ß¹â
		half3 h = normalize(normalize(input.lightDir) + normalize(input.viewDir));
		float nh = max(0, dot(normal, h)); 
		float4 specColor = saturate(pow(nh, _Shininess * 128.0) * _Gloss) * outputColor;
		outputColor.rgb += specColor.rgb;

		//dissolve
		half3 noise = tex2D(_NoiseTex, input.uv_NoiseTex);
		float cutoff = lerp(0, _Cutoff + _EdgeSize, _Cutoff);
		half edge = smoothstep(cutoff + _EdgeSize, cutoff, clamp(noise.r, _EdgeSize, 1));
		outputColor.rgb += _EdgeColor.rgb * edge;

		clip(noise - cutoff);
		
		//alpha
		outputColor.a *= _Color.a;

		//add emission
		outputColor.rgb += _EmissionColor.rgb * (areaTex.r + areaTex.g + areaTex.b);
		
		UNITY_APPLY_FOG(input.fogCoord, outputColor);

		return saturate(outputColor);
	}

	ENDCG

    SubShader
	{
        Tags 
		{ 
			"Queue" = "AlphaTest" 
			"RenderType" = "OUTLINE" 
		}        
		
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

	Fallback "CITT/BaseShadowCaster"
}
