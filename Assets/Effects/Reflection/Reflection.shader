Shader "CITT/Reflection" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Main Tex", 2D) = "white" {}

		_NormalTex ("Normal Tex", 2D) = "bump" {}

		_Gloss ("Gloss", Range(0, 3)) = 1
		_Shininess ("Shininess", Range(0.03, 1)) = 0.5

		_ReflectTex ("Reflect Tex", 2D) = "black" {}
		_ReflectFactor ("Reflect Factor", Range(0, 2)) = 1
	}

	SubShader 
	{
		Tags 
		{
			"Queue"="Geometry"
			"RenderType"="Opaque" 
		}
		
		Pass
		{
            Tags {"LightMode" = "ForwardBase"}

			Cull Back
			ZWrite On

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            #pragma multi_compile_fwdbase
			#pragma multi_compile_fog
			#pragma target 3.0

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			float4 _Color;

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _NormalTex;
			float4 _NormalTex_ST;

			float _Gloss;
			float _Shininess;

			sampler2D _ReflectTex;
			float4 _ReflectTex_ST;
			float _ReflectFactor;

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
				float2 uv_MainTex:TEXCOORD0;
				float2 uv_NormalTex:TEXCOORD1;
				float2 uv_ReflectTex:TEXCOORD2;
				float4 worldPosition:TEXCOORD3;
				float3 worldNormal:TEXCOORD4;
				float3 tangent : TEXCOORD5;
				float3 bitangent : TEXCOORD6;
				float3 viewDir:TEXCOORD7;
				float3 lightDir:TEXCOORD8;
				float4 screenPosition:TEXCOORD9;
				float4 grabScreenPosition:TEXCOORD10;
				UNITY_FOG_COORDS(11)
				SHADOW_COORDS(12)
			};
		
			VertexOutput vert (VertexInput v)
			{
				VertexOutput output;

				UNITY_INITIALIZE_OUTPUT(VertexOutput, output);

				output.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
				output.uv_NormalTex = TRANSFORM_TEX(v.texcoord, _NormalTex);
				output.uv_ReflectTex = TRANSFORM_TEX(v.texcoord, _ReflectTex);
			
				output.vertex = UnityObjectToClipPos(v.vertex);
				output.worldPosition = mul(unity_ObjectToWorld, v.vertex);
				output.worldNormal = UnityObjectToWorldNormal(v.normal);
				output.tangent = UnityObjectToWorldNormal(v.tangent);
				output.bitangent = cross(output.tangent, output.worldNormal);
				output.viewDir = UnityWorldSpaceViewDir(output.worldPosition);
				output.lightDir = UnityWorldSpaceLightDir(output.worldPosition);
				output.screenPosition = ComputeScreenPos(output.vertex);
				output.grabScreenPosition = ComputeGrabScreenPos(output.vertex);
				COMPUTE_EYEDEPTH(output.screenPosition.z);
		
				UNITY_TRANSFER_FOG(output, output.vertex)
                TRANSFER_SHADOW(output)

				return output;
			}
		
			float4 frag (VertexOutput input) : COLOR
			{
				//标准化
				input.tangent = normalize(input.tangent);
				input.bitangent = normalize(input.bitangent);
				input.worldNormal = normalize(input.worldNormal);

				input.viewDir = normalize(input.viewDir);
				input.lightDir = normalize(input.lightDir);

				//float4 main = tex2D(_MainTex, input.uv_MainTex) * _Color;

				//法线
				float3 normal = UnpackNormal(tex2D(_NormalTex, input.uv_NormalTex));
				normal = float3(input.tangent * normal.r + input.bitangent * normal.g + input.worldNormal * normal.b);
				
				//输出
				float4 outputColor;
				outputColor.rgb = (tex2D(_MainTex, input.uv_MainTex) * _Color).rgb;

				//反射
				float2 normalOffset = normal.xy * normal.z;
				input.screenPosition.xy = (input.screenPosition.xy) / input.screenPosition.z;
				float4 reflectArea = tex2D(_ReflectTex, input.screenPosition.xy + normalOffset);
				float4 reflectColor = tex2D(_ReflectTex, input.screenPosition.xy + normalOffset * Luminance(reflectArea.rgb)) * _ReflectFactor;
				outputColor.rgb += reflectColor.rgb;

				//漫反射
				float diff = max(0, dot(normal, input.lightDir));
				diff = diff * 0.5 + 0.5;
				outputColor.rgb *= diff;

				//高光
				half3 h = normalize(normalize(input.lightDir) + normalize(input.viewDir));
				float nh = max(0, dot(normal, h)); 
				float4 specColor = saturate(pow(nh, _Shininess * 128.0) * _Gloss) * outputColor;
				outputColor.rgb += specColor.rgb;


                float shadowAttenuation = SHADOW_ATTENUATION(input);
				outputColor *= (1 + shadowAttenuation);

				outputColor.rgb = outputColor.rgb * (_LightColor0.rgb + UNITY_LIGHTMODEL_AMBIENT.rgb);
				outputColor.a = 1;

				UNITY_APPLY_FOG(input.fogCoord, outputColor);

				return saturate(outputColor);
			}
			ENDCG
		}
	}

	FallBack "CITT/BaseShadowCaster"
}
