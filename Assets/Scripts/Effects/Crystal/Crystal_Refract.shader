Shader "CITT/Crystal_Refract" 
{
	Properties 
	{
		_RefractFactor ("Refract Factor", range(0, 0.5)) = 0.1
	}

	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		
		GrabPass { "_GrabTex" }
		
		pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _GrabTex;
			float4 _GrabTex_ST;
			float4 _GrabTex_TexelSize;

			float _RefractFactor;

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
				float3 objectNormal : TEXCOORD1;
			};

			VertexOutput vert (VertexInput v)
			{
				VertexOutput output;

				UNITY_INITIALIZE_OUTPUT(VertexOutput, output);
				
				output.vertex = UnityObjectToClipPos(v.vertex);
				output.screenPosition = ComputeGrabScreenPos(output.vertex);
				output.objectNormal = v.normal;

				return output;
			}
			
			float4 frag (VertexOutput input) : COLOR
			{
				float4 outputColor;
	
				//grab
				input.objectNormal = normalize(input.objectNormal);
				float2 refractOffset = input.objectNormal.xy * input.objectNormal.z * _RefractFactor;
				input.screenPosition.xy = (input.screenPosition.xy + refractOffset) / input.screenPosition.w;
				float4 refractColor = tex2D(_GrabTex, input.screenPosition.xy);
				outputColor.rgb = refractColor.rgb;
				outputColor.a = 1;

				return outputColor;
			}

			ENDCG
		}
	}
}
