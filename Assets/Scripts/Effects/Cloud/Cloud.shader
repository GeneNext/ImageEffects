Shader "TEST/Cloud" 
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

	SubShader 
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }

		Cull Off

		CGPROGRAM
		#pragma surface surf Lambert vertex:vert alpha nodirlightmap

		#include "UnityCG.cginc"

		float4 _Color;
		float4 _AddColor;
		sampler2D _MainTex;
		float _FlowSpeedX;
		float _FlowSpeedY;

		sampler2D _MixTex;
		float _MixSpeedX;
		float _MixSpeedY;

		sampler2D _CameraDepthTexture;

		float _DepthFactor;
		float _DepthRange;
		float _RimFactor;

		struct Input 
		{
			float2 uv_MainTex;
			float2 uv_MixTex;
			float3 viewDir;
			float3 normal;
			float4 proj;
		};

		void vert (inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			
			//float2 uv = TRANSFORM_TEX(v.texcoord, _AnimTex);
			//float animTex_x = (_Time.x * _AnimSpeed+ uv.x) ;
			//float animTex_y = (_Time.x * _AnimSpeed + uv.y);
			//float4 pos = tex2Dlod(_AnimTex, float4(animTex_x, animTex_y, 0, 0)) * _AnimIntensity;
			//v.vertex.xyz += pos.xyz * v.color;

			//float2 uv = TRANSFORM_TEX(v.texcoord, _AnimTex);
			//float animTex_x = (_Time.x * _AnimSpeed+ uv.x) ;
			//float animTex_y = (_Time.x * _AnimSpeed + uv.y);
			//float4 pos = tex2Dlod(_AnimTex, float4(animTex_x, animTex_y, 0, 0)) * _AnimIntensity;
			//v.vertex.y += sin(_Time.x * _AnimSpeed + pos.y);
			
			o.normal = UnityObjectToWorldNormal(v.normal);

			o.proj = ComputeScreenPos(UnityObjectToClipPos(v.vertex));
			COMPUTE_EYEDEPTH(o.proj.z);
		}

		void surf (Input IN, inout SurfaceOutput o) 
		{
			//计算深度差
			float sceneDepth = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, IN.proj).r);
			float partDepth = IN.proj.z;
			float depthFade = saturate((sceneDepth - partDepth) * _DepthFactor);
			depthFade = depthFade * _DepthRange;

			//计算边缘
			float rim = saturate(pow(max(0,dot(IN.normal,IN.viewDir)), _RimFactor));

			float alpha = depthFade * rim;

			float2 flowSpeed = float2(_FlowSpeedX, _FlowSpeedY) * _Time.x;
			float4 color1 = tex2D(_MainTex, IN.uv_MainTex + flowSpeed);

			float2 mixSpeed = float2(_MixSpeedX, _MixSpeedY);
			float4 color2 = tex2D(_MixTex, IN.uv_MixTex + flowSpeed * mixSpeed);

			float4 c = lerp(color1, color2, 0.5);
			alpha *= Luminance(c.rgb);

			c = c * _Color;

			o.Emission = c.rgb + _AddColor.rgb;
			o.Alpha = alpha * _Color.a;
			o.Alpha = clamp(o.Alpha, 0, 1);
		}
		ENDCG
	}
        
	FallBack "Diffuse"
}
