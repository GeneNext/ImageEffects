// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/RadialBlurEffect"
{
	Properties
	{
		_MainTex ("MainTex", 2D) = "white" {}        
		_BlurTex("BlurTex", 2D) = "white"{}  
	}

	CGINCLUDE
			
	#include "UnityCG.cginc"
	
	uniform sampler2D _MainTex;  
    uniform sampler2D _BlurTex; 
				
	uniform half4 _MainTex_TexelSize;
	half4 _MainTex_ST;

	uniform float _SampleDistance;  
    uniform float _SampleStrength;  
	
	struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};
	
	struct v2f
	{
		float4 vertex : SV_POSITION;
		float2 uv : TEXCOORD0;

        #if UNITY_UV_STARTS_AT_TOP
				half2 uv2 : TEXCOORD1;
		#endif
	};
	
	v2f vert (appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = UnityStereoScreenSpaceUVAdjust(v.uv, _MainTex_ST);
        	
        #if UNITY_UV_STARTS_AT_TOP
        	o.uv2 = o.uv;				
        	if (_MainTex_TexelSize.y < 0.0)
        		o.uv.y = 1.0 - o.uv.y;
        #endif
		return o;
	}
	
	fixed4 frag_blur (v2f i) : COLOR
	{
		fixed2 dir = 0.5 - i.uv;
		fixed distance = length(dir);
		dir /= distance;
		dir *= _SampleDistance;

		fixed4 sum = 0;
		sum += tex2D(_MainTex, i.uv - dir * 0.02);
		sum += tex2D(_MainTex, i.uv - dir * 0.05);
		sum += tex2D(_MainTex, i.uv + dir * 0.02);
		sum += tex2D(_MainTex, i.uv + dir * 0.05);
		sum *= 0.25;
		
		return sum;
	}

	fixed4 frag_combine(v2f i) : COLOR
	{
		// fixed2 dir = 0.5 - i.uv;
		fixed distance = length(0.5 - i.uv);
		#if UNITY_UV_STARTS_AT_TOP
		fixed4 col = tex2D(_MainTex, i.uv2);
		#else
		fixed4 col = tex2D(_MainTex, i.uv);
		#endif
		fixed4 blur = tex2D(_BlurTex, i.uv);
		col = lerp(col, blur, saturate(_SampleStrength * distance));
		return col;
	}

	ENDCG

	SubShader
	{
		// No culling or depth
		//Cull Off ZWrite Off ZTest Always

		ZTest Always  
		ZWrite Off 
		Cull Off 
		Blend Off

		Fog { Mode off } 

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_blur
			//#pragma fragmentoption ARB_precision_hint_fastest 
			ENDCG
		}
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_combine
			//#pragma fragmentoption ARB_precision_hint_fastest 
			ENDCG
		}
	}
}
