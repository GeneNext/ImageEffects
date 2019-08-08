#ifndef FUR_LIB_INCLUDED
#define FUR_LIB_INCLUDED

#include "UnityCG.cginc"
#include "Lighting.cginc"

struct VertexOutput 
{
	float4 pos : SV_POSITION;
	half2 uv_MainTex : TEXCOORD0;
	half2 uv_FurTex : TEXCOORD1;
	float3 worldNormal : TEXCOORD2;
	float3 viewDir : TEXCOORD3;
	float3 lightDir : TEXCOORD4;

	UNITY_FOG_COORDS(5)
};
 
sampler2D _MainTex;
float4 _MainTex_ST;

sampler2D _FurTex;
float4 _FurTex_ST;

float _FurLength;
 
float _Cutoff;
float _Thickness;
float _RimFactor;
 
VertexOutput vert(appdata_base input) 
{
	VertexOutput output;

	UNITY_INITIALIZE_OUTPUT(VertexOutput, output);

	input.vertex.xyz += input.normal * _FurLength * FURSTEP;
	output.pos = UnityObjectToClipPos(input.vertex);
	output.uv_MainTex = TRANSFORM_TEX(input.texcoord, _MainTex);
	output.uv_FurTex = TRANSFORM_TEX(input.texcoord, _FurTex);
	float3 worldPosition = mul(unity_ObjectToWorld, input.vertex);
	output.viewDir = UnityWorldSpaceViewDir(worldPosition);
	output.lightDir = UnityWorldSpaceLightDir(worldPosition);
	output.worldNormal = normalize(UnityObjectToWorldNormal(input.normal));
	float4 worldPos = mul(unity_ObjectToWorld ,input.vertex);
	
	UNITY_TRANSFER_FOG(output, output.vertex);

	return output;
}
 
fixed4 frag(VertexOutput input) : SV_Target 
{
	input.viewDir = normalize(input.viewDir);
	input.lightDir = normalize(input.lightDir);
	input.worldNormal = normalize(input.worldNormal);

	float rim = saturate(dot(input.worldNormal, normalize(input.viewDir)));
	rim = pow(rim, _RimFactor);

	fixed4 outputColor = tex2D(_MainTex, input.uv_MainTex);
	float diffuse = max(0, dot(input.worldNormal, input.lightDir));
	outputColor.rgb = outputColor.rgb * (diffuse * _LightColor0.rgb + UNITY_LIGHTMODEL_AMBIENT.rgb);

	fixed alpha = tex2D(_FurTex, input.uv_FurTex).r;
	outputColor.a = step(lerp(_Cutoff, _Thickness, FURSTEP), alpha);
	outputColor.a *= rim * (1 - FURSTEP);

	UNITY_APPLY_FOG(input.fogCoord, outputColor);

	return saturate(outputColor);
}

#endif