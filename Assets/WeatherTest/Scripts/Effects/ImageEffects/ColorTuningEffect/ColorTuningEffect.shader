// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/ColorTuningEffect"
{
	Properties
	{
		_MainTex ("MainTex", 2D) = "white" {}
	}

	CGINCLUDE
			
	#include "UnityCG.cginc"

	#define EPSILON 1e-10
	
	sampler2D _MainTex;
	float4 _MainTex_TexelSize;
	float4 _MainTex_ST;
	
	struct appdata
	{
		half4 vertex : POSITION;
		half2 uv : TEXCOORD0;
	};
	
	struct v2f
	{
		half2 uv : TEXCOORD0;
		half4 vertex : SV_POSITION;
	};
	
	v2f vert (appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv.xy = v.uv.xy;
#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0)
			o.uv.y = 1 - o.uv.y;
#endif	
		return o;
	}

	float4 _TintColor;
	float4 _AddColor;
	float _Brightness;
	float _Contrast;
	float _Saturation;

	float3 TuneImage(float3 color, float brightness, float contrast, float saturation)
	{
		float3 avgLuminance = float3(0.5, 0.5, 0.5);
		float3 luminanceCoeff = float3(0.2125, 0.7154, 0.0721);

		float3 brtColor = color * brightness;
		float intensityF = dot(brtColor, luminanceCoeff);
		float3 intensity = float3(intensityF, intensityF, intensityF);

		float3 satColor = lerp(intensity, brtColor, saturation);

		float3 conColor = lerp(avgLuminance, satColor, contrast);

		return conColor;
	}
	
	float4 frag_BaseColor (v2f i) : COLOR
	{
		float4 color = tex2D(_MainTex, i.uv) * _TintColor + _AddColor;
		color.rgb = TuneImage(color.rgb, _Brightness, _Contrast, _Saturation);
		return color;
	}

	uniform float _HueOffset;
	uniform float _SaturationOffset;
	uniform float _ValueOffset;

	inline float3 RGBtoHSV(float3 rgb)
	{
		float4 k = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
        float4 p = lerp(float4(rgb.bg, k.wz), float4(rgb.gb, k.xy), step(rgb.b, rgb.g));
		float4 q = lerp(float4(p.xyw, rgb.r), float4(rgb.r, p.yzx), step(p.x, rgb.r));
        float d = q.x - min(q.w, q.y);

        return float3(abs(q.z + (q.w - q.y) / (6.0 * d + EPSILON)), d / (q.x + EPSILON), q.x);
	}

	inline float3 HSVtoRGB(float3 hsv)
	{	
		float3 rgb = saturate(3.0 * abs(1.0 - 2.0 * frac(hsv.x + float3(0.0, -1.0 / 3.0, 1.0 / 3.0))) - 1);

        return lerp(float3(1, 1, 1), rgb, hsv.y) * hsv.z;
	}

	inline float3 UpdateHSV(float3 hsv)
	{
		hsv.r += _HueOffset;
		hsv.g += _SaturationOffset;
		hsv.b += _ValueOffset;

		return hsv;
	}

	float4 frag_HSV(v2f i) : COLOR
	{
		float4 color = tex2D(_MainTex, i.uv);
		color.rgb = RGBtoHSV(color.rgb);
		color.rgb = UpdateHSV(color.rgb);
		color.rgb = HSVtoRGB(color.rgb);

		return color;
	}

	float _Kelvins;

	inline float3 KelvinsToRGB(float kelvins)
	{
		float3 color = float3(1, 1, 1);
	
		kelvins = clamp(kelvins, 1000.0, 40000.0) / 100.0;
		
		if (kelvins <= 66.0)
		{
		    color.r = 1.0;
		    color.g = saturate(0.39008157876901960784 * log(kelvins) - 0.63184144378862745098);
		}
		else
		{
			float t = kelvins - 60.0;
		    color.r = saturate(1.29293618606274509804 * pow(t, -0.1332047592));
		    color.g = saturate(1.12989086089529411765 * pow(t, -0.0755148492));
		}
		
		if (kelvins >= 66.0)
		{
		    color.b = 1.0;
		}
		else if(kelvins <= 19.0)
		{
		    color.b = 0.0;
		}
		else
		{
		    color.b = saturate(0.54320678911019607843 * log(kelvins - 10.0) - 1.19625408914);
		}
		
		return color;
	}

	float4 frag_ColorTemperature(v2f i) : COLOR
	{
		float4 color = tex2D(_MainTex, i.uv);
		float3 colorTemperature = KelvinsToRGB(_Kelvins);
		color.rgb = color.rgb * colorTemperature.rgb;

		return color;
	}

	float3 HUEtoRGB(float hue)
	{
		float3 rgb = 0;    
		rgb.r = abs(hue * 6.0 - 3.0) - 1.0;
		rgb.g = 2.0 - abs(hue * 6.0 - 2.0);
		rgb.b = 2.0 - abs(hue * 6.0 - 4.0);

		return rgb;
	}

	float3 HSLtoRGB(float3 hsl)
	{
	    float3 rgb = HUEtoRGB(hsl.x);
	    float c = (1.0 - abs(2.0 * hsl.z - 1.0)) * hsl.y;
	    return (rgb - 0.5) * c + hsl.z;
	}

	float3 RGBtoHCV(float3 rgb)
	{
		float4 p = (rgb.g < rgb.b) ? float4(rgb.bg, -1.0, 2.0/3.0) : float4(rgb.gb, 0.0, -1.0/3.0);
		float4 q = (rgb.r < p.x) ? float4(p.xyw, rgb.r) : float4(rgb.r, p.yzx);
		float c = q.x - min(q.w, q.y);
		float h = abs((q.w - q.y) / (6.0 * c + EPSILON) + q.z);

		return float3(h, c, q.x);
	}

	float3 RGBtoHSL(float3 RGB)
	{
	    float3 HCV = RGBtoHCV(RGB);
	    float L = HCV.z - HCV.y * 0.5;
	    float S = HCV.y / (1.0 - abs(L * 2.0 - 1.0) + EPSILON);

	    return float3(HCV.x, S, L);
	}

	float4 frag_HSL(v2f i) : COLOR
	{
		float4 color = tex2D(_MainTex, i.uv);
		float luminance = Luminance(color.rgb);
		float3 hsl = RGBtoHSL(color.rgb);
		color.rgb = HSLtoRGB(float3(hsl.x, hsl.y, luminance));
		
		return color;
	}

	ENDCG

	SubShader
	{
		ZTest Always  
		ZWrite Off 
		Cull Off 
		Blend Off

		Fog { Mode off } 

		//pass 0 : base
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_BaseColor
			ENDCG
		}

		//pass 1 : hsv
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_HSV
			ENDCG
		}

		//pass 2 : color temperature
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_ColorTemperature
			ENDCG
		}

		//pass 3 : hsl
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_HSL
			ENDCG
		}
	}
}
