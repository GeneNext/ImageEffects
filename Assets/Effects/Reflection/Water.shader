Shader "CITT/Water" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_AddColor ("Add Color", Color) = (0,0,0,0)

		_MainBumpTex ("Main Bump Tex", 2D) = "bump" {}
		_MainSpeedX ("Main Speed X", Range(-3, 3)) = 0
		_MainSpeedY ("Main Speed Y", Range(-3, 3)) = 0

		_MixBumpTex ("Mix Bump Tex", 2D) = "bump" {}
		_MixSpeedX ("Mix Speed X", Range(-3, 3)) = 1
		_MixSpeedY ("Mix Speed Y", Range(-3, 3)) = 1

		_Gloss ("Gloss", Range(0, 3)) = 1
		_Shininess ("Shininess", Range(0.03, 1)) = 0.5

		_ReflectTex ("Reflect Tex", 2D) = "black" {}
		_ReflectFactor ("Reflect Factor", Range(0, 2)) = 1

		_DepthFactor ("Depth Factor", Range(0, 3)) = 0

		_RimFactor ("Rim Factor", Range(0, 1)) = 0.8
	}

	SubShader 
	{
		Tags 
		{
			"Queue"="Transparent"
			"RenderType"="Transparent" 
		}
		
		GrabPass { "_GrabTex" }

		Pass
		{
            Tags {"LightMode" = "ForwardBase"}

			Cull Back
			ZWrite On
			//Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            #pragma multi_compile_fwdbase
			#pragma multi_compile_fog
			#pragma target 3.0

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			sampler2D _GrabTex;
			float4 _GrabTex_ST;
			float4 _GrabTex_TexelSize;

			float4 _Color;
			float4 _AddColor;

			sampler2D _MainBumpTex;
			float4 _MainBumpTex_ST;
			float _MainSpeedX;
			float _MainSpeedY;
		
			sampler2D _MixBumpTex;
			float4 _MixBumpTex_ST;
			float _MixSpeedX;
			float _MixSpeedY;

			float _Gloss;
			float _Shininess;

			sampler2D _ReflectTex;
			float4 _ReflectTex_ST;
			float _ReflectFactor;

			sampler2D _CameraDepthTexture;

			float _DepthFactor;
			float _RimFactor;

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
				float2 uv_MainBumpTex:TEXCOORD0;
				float2 uv_MixBumpTex:TEXCOORD1;

				float4 worldPosition:TEXCOORD2;
				float3 worldNormal:TEXCOORD3;
				float3 worldTangent : TEXCOORD4;
				float3 worldBinormal : TEXCOORD5;

				float4 TBN0 : TEXCOORD6;
				float4 TBN1 : TEXCOORD7;
				float4 TBN2 : TEXCOORD8;

				float3 viewDir:TEXCOORD9;
				float3 lightDir:TEXCOORD10;

				float4 screenPosition:TEXCOORD11;
				float4 grabScreenPosition:TEXCOORD12;

				UNITY_FOG_COORDS(13)
				SHADOW_COORDS(14)
			};
		
			VertexOutput vert (VertexInput v)
			{
				VertexOutput output;

				UNITY_INITIALIZE_OUTPUT(VertexOutput, output);

				output.uv_MainBumpTex = TRANSFORM_TEX(v.texcoord, _MainBumpTex);
				output.uv_MixBumpTex = TRANSFORM_TEX(v.texcoord, _MixBumpTex);
			
				output.vertex = UnityObjectToClipPos(v.vertex);
				output.worldPosition = mul(unity_ObjectToWorld, v.vertex);
				output.worldNormal = UnityObjectToWorldNormal(v.normal);
				output.worldTangent = UnityObjectToWorldDir(v.tangent);
				float tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				output.worldBinormal = cross(output.worldNormal, output.worldTangent) * tangentSign;

				output.viewDir = UnityWorldSpaceViewDir(output.worldPosition);
				output.lightDir = UnityWorldSpaceLightDir(output.worldPosition);

				output.screenPosition = ComputeScreenPos(output.vertex);
				output.grabScreenPosition = ComputeGrabScreenPos(output.vertex);
				COMPUTE_EYEDEPTH(output.screenPosition.z);
		
				UNITY_TRANSFER_FOG(output, output.vertex)
                TRANSFER_SHADOW(output);

				return output;
			}
		
			float4 frag (VertexOutput input) : COLOR
			{
				//深度差
				float sceneDepth = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, input.screenPosition).r);
				float partDepth = input.screenPosition.z;
				float depthFade = pow(saturate(sceneDepth - partDepth), _DepthFactor);

				//标准化
				input.worldTangent = normalize(input.worldTangent);
				input.worldBinormal = normalize(input.worldBinormal);
				input.worldNormal = normalize(input.worldNormal);

				input.viewDir = normalize(input.viewDir);
				input.lightDir = normalize(input.lightDir);

				//混合
				float2 mainUV = float2(_MainSpeedX, _MainSpeedY);
				float4 mainBump = tex2D(_MainBumpTex, input.uv_MainBumpTex + mainUV * _Time.x);
				float4 mainBump2 = tex2D(_MainBumpTex, input.uv_MainBumpTex + mainUV * _Time.x * 2);
				mainBump = (mainBump + mainBump2) * 0.5;

				float2 mixUV = float2(_MixSpeedX, _MixSpeedY);
				float4 mixBump = tex2D(_MixBumpTex, input.uv_MixBumpTex + mixUV * _Time.x);
				float4 mixBump2 = tex2D(_MixBumpTex, input.uv_MixBumpTex + mixUV * _Time.x * 3);
				mixBump = (mixBump + mixBump2) * 0.5;

				float4 bump = (mainBump + mixBump) * 0.5;

				float3 normal = UnpackNormal(bump);
				float3 worldNormal;
				float4 TBN0 = float4(input.worldTangent.x, input.worldBinormal.x, input.worldNormal.x, input.worldPosition.x);
				float4 TBN1 = float4(input.worldTangent.y, input.worldBinormal.y, input.worldNormal.y, input.worldPosition.y);
				float4 TBN2 = float4(input.worldTangent.z, input.worldBinormal.z, input.worldNormal.z, input.worldPosition.z);
				//float3 TBN0 = fixed3(input.worldTangent.x, input.worldBinormal.x, input.worldNormal.x);
				//float3 TBN1 = fixed3(input.worldTangent.y, input.worldBinormal.y, input.worldNormal.y);
				//float3 TBN2 = fixed3(input.worldTangent.z, input.worldBinormal.z, input.worldNormal.z);
				worldNormal.x = dot(TBN0, normal);
				worldNormal.y = dot(TBN1, normal);
				worldNormal.z = dot(TBN2, normal);
				normal = normalize(worldNormal);

				//边缘
				float rim = saturate(1 - saturate(pow(max(0, dot(normal, input.viewDir)), _RimFactor)) + _AddColor.a); 

				//混合权重
				float blendWeight = saturate(depthFade * rim);

				//相机距离
				float cameraDistance = distance(_WorldSpaceCameraPos.xyz, input.vertex.xyz);
				float cameraFade = clamp(cameraDistance, 0.1, 1);

				//偏移
				float2 normalOffset = normal.xy * normal.z * blendWeight;
				normalOffset.y = max(0, normalOffset.y);

				//反射
				input.screenPosition.xy = (input.screenPosition.xy + normalOffset) / input.screenPosition.z;
				float4 reflectColor = tex2D(_ReflectTex, input.screenPosition.xy) * _ReflectFactor * blendWeight;

				//折射
				input.grabScreenPosition.xy = (input.grabScreenPosition.xy + normalOffset) / input.grabScreenPosition.w;
				float4 refractColor = tex2D(_GrabTex, input.grabScreenPosition.xy);

				//输出
				float4 outputColor;
				outputColor.rgb = refractColor.rgb + reflectColor.rgb;
				outputColor.rgb = lerp(outputColor.rgb, outputColor.rgb * _Color.rgb, blendWeight);
				outputColor.a = blendWeight * _Color.a;

				float shadowAttenuation = SHADOW_ATTENUATION(input);

				//漫反射
				float diff = max(0, dot(normal, input.lightDir));
				outputColor.rgb *= lerp(1, diff * _LightColor0.rgb + UNITY_LIGHTMODEL_AMBIENT.rgb, blendWeight) * shadowAttenuation;
				
				//高光
				half3 h = normalize(normalize(input.lightDir) + normalize(input.viewDir));
				float nh = max(0, dot(normal, h)); 
				float4 specColor = saturate(pow(nh, _Shininess * 128.0) * _Gloss) * outputColor * shadowAttenuation;
				outputColor.rgb += specColor.rgb;
				
				outputColor.rgb *= shadowAttenuation;

				UNITY_APPLY_FOG(input.fogCoord, outputColor);

				return saturate(outputColor);
			}
			ENDCG
		}
	}

	FallBack "CITT/BaseShadowCaster"
}
