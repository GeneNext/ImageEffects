Shader "TEST/FrostedGlass" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "black" {}
		_BlurRange ("BlurRange", Range(0, 0.01)) = 0.001
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
			CGPROGRAM

			#include "UnityCG.cginc"

			float4 _Color;

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;

			sampler2D _GrabTex;
			float4 _GrabTex_ST;
			float4 _GrabTex_TexelSize;

			float _BlurRange;

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
			};
			
			struct VertexOutput 
			{
			    float4 vertex : SV_POSITION;
				float4 screenPosition : TEXCOORD0;
				float2 uv_MainTex : TEXCOORD2;
			};

			#pragma vertex vert
			#pragma fragment frag

			VertexOutput vert (VertexInput v)
			{
				VertexOutput output;

				UNITY_INITIALIZE_OUTPUT(VertexOutput, output);

				output.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);

#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0)
			output.uv_MainTex.y = 1 - output.uv_MainTex.y;
#endif	
				
				output.vertex = UnityObjectToClipPos(v.vertex);
				output.screenPosition = ComputeGrabScreenPos(output.vertex);

				return output;
			}
				
			float4 frag (VertexOutput input) : COLOR
			{
				//main
				float4 mainTex = tex2D(_MainTex, input.uv_MainTex) * _Color;
				
				input.screenPosition.xy = input.screenPosition.xy / input.screenPosition.w;
				float4 grabTex1 = tex2D(_GrabTex, input.screenPosition.xy);
				float4 offset1 = float4(0, 1, 0, -1) * _BlurRange * Luminance(mainTex.rgb);
				grabTex1 += tex2D(_GrabTex, input.screenPosition.xy + offset1.xy);
				grabTex1 += tex2D(_GrabTex, input.screenPosition.xy + offset1.zw);
				grabTex1 += tex2D(_GrabTex, input.screenPosition.xy + offset1.xy * 2);
				grabTex1 += tex2D(_GrabTex, input.screenPosition.xy + offset1.zw * 2);
				grabTex1 += tex2D(_GrabTex, input.screenPosition.xy + offset1.xy * 3);
				grabTex1 += tex2D(_GrabTex, input.screenPosition.xy + offset1.zw * 3);
				grabTex1 = grabTex1 / 7;
				
				float4 grabTex2 = tex2D(_GrabTex, input.screenPosition.xy);
				float4 offset2 = float4(1, 0, -1, 0) * _BlurRange * Luminance(mainTex.rgb);
				grabTex2 += tex2D(_GrabTex, input.screenPosition.xy + offset2.xy);
				grabTex2 += tex2D(_GrabTex, input.screenPosition.xy + offset2.zw);
				grabTex2 += tex2D(_GrabTex, input.screenPosition.xy + offset2.xy * 2);
				grabTex2 += tex2D(_GrabTex, input.screenPosition.xy + offset2.zw * 2);
				grabTex2 += tex2D(_GrabTex, input.screenPosition.xy + offset2.xy * 3);
				grabTex2 += tex2D(_GrabTex, input.screenPosition.xy + offset2.zw * 3);
				grabTex2 = grabTex2 / 7;

				float4 grabTex = lerp(grabTex1, grabTex2, 0.5);
				float4 outputColor;
				outputColor.rgb = grabTex.rgb * _Color.rgb;
				outputColor.a = 1;

				return saturate(outputColor);
			}

			ENDCG
		}
	}

	FallBack "Diffuse"
}
