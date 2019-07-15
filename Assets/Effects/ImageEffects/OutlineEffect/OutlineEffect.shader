Shader "Hidden/OutlineEffect"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_OutlineTex("Outline", 2D) = "white" {}
		_BlurTex("Blur", 2D) = "white" {}
		_NoiseTex("Noise", 2D) = "white" {}
	}
 
	CGINCLUDE

	#include "UnityCG.cginc"
	
	struct v2f
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};
 
	sampler2D _MainTex;
	float4 _MainTex_ST;
	float4 _MainTex_TexelSize;

	sampler2D _OutlineTex;
	float4 _OutlineTex_ST;
	float4 _OutlineTex_TexelSize;

	sampler2D _BlurTex;
	float4 _BlurTex_ST;
	float4 _BlurTex_TexelSize;

	sampler2D _NoiseTex;
	float4 _NoiseTex_ST;
	float4 _NoiseTex_TexelSize;

	float4 _OutlineColor;
	float _OutlineFactor;

	float _OutlineScale;

	float4 _NoiseSpeedFactors;
 
	v2f vert(appdata_img v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv.xy = v.texcoord.xy;

#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0)
			o.uv.y = 1 - o.uv.y;
#endif	

		return o;
	}
 
	ENDCG
 
	SubShader
	{
		//放大
		Pass
		{
			ZTest Off
			Cull Off
			ZWrite Off

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag_scale
 
			float4 frag_scale(v2f i) : SV_Target
			{
				float2 offset = _MainTex_TexelSize.xy * _OutlineScale;
				
				float4 main = 0;
				main += tex2D(_MainTex, i.uv + offset * float2(0, 0));
				main += tex2D(_MainTex, i.uv + offset * float2(0, 1));
				main += tex2D(_MainTex, i.uv + offset * float2(0, -1));
				main += tex2D(_MainTex, i.uv + offset * float2(1, 0));
				main += tex2D(_MainTex, i.uv + offset * float2(1, 1));
				main += tex2D(_MainTex, i.uv + offset * float2(1, -1));
				main += tex2D(_MainTex, i.uv + offset * float2(-1, 0));
				main += tex2D(_MainTex, i.uv + offset * float2(-1, 1));
				main += tex2D(_MainTex, i.uv + offset * float2(-1, -1));

				return saturate(main);
			}

			ENDCG
		}

		//扭动
		Pass
		{
			ZTest Off
			Cull Off
			ZWrite Off

			CGPROGRAM
	
			struct NoiseVertexOutput
			{
				float4 pos : SV_POSITION;
				float2 uv_MainTex : TEXCOORD0;
				float2 uv_NoiseTex : TEXCOORD1;
			};

			#pragma vertex vert_noise
			#pragma fragment frag_noise
 
			NoiseVertexOutput vert_noise(appdata_img v)
			{
				NoiseVertexOutput o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uv_NoiseTex = TRANSFORM_TEX(v.texcoord, _NoiseTex);

		#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					o.uv_MainTex.y = 1 - o.uv_MainTex.y;
				if (_NoiseTex_TexelSize.y < 0)
					o.uv_NoiseTex.y = 1 - o.uv_NoiseTex.y;
		#endif	

				return o;
			}
 
			float4 frag_noise(NoiseVertexOutput i) : SV_Target
			{
				float4 noise1 = tex2D(_NoiseTex, (i.uv_NoiseTex + _NoiseSpeedFactors.xy * _Time.x));
				float4 noise2 = tex2D(_NoiseTex, (i.uv_NoiseTex + noise1.xy * _Time.x));
				float4 noise = (noise1 + noise2) * 0.75;
				
				float4 main = tex2D(_MainTex, i.uv_MainTex);

				return saturate(main - noise);
			}

			ENDCG
		}

		//pass 1: 剔除
		Pass
		{
			ZTest Off
			Cull Off
			ZWrite Off
 
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag_clip
 
			float4 frag_clip(v2f i) : SV_Target
			{
				float4 main = tex2D(_MainTex, i.uv);

				float4 blur = tex2D(_BlurTex, i.uv);

				return saturate(blur - saturate(normalize(main.r + main.g + main.b)));
			}

			ENDCG
		}
 
		//pass 2: 合并
		Pass
		{
			ZTest Off
			Cull Off
			ZWrite Off
 
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag_combine
 
			float4 frag_combine(v2f i) : SV_Target
			{
				float4 main = tex2D(_MainTex, i.uv);

				float4 outline = tex2D(_OutlineTex, i.uv);

				return saturate(main + outline * _OutlineFactor);
			}

			ENDCG
		}
	}
}
