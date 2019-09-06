Shader "Hidden/OutlineEffectReplace"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}

	SubShader
	{
		Tags 
		{
			"Queue"="Geometry"
			"RenderType"="OUTLINE" 
		}

		CGPROGRAM

		#pragma surface surf Lambert nofog
		
		float4 _OutlineColor;

		sampler2D _MainTex;

		struct Input 
		{
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) 
		{
			o.Emission = _OutlineColor.rgb * _OutlineColor.a;
		}

		ENDCG
	}
}
