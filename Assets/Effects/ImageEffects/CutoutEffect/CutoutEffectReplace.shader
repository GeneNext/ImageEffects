Shader "Hidden/CutoutEffectReplace"
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
		
		sampler2D _MainTex;

		struct Input 
		{
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) 
		{
			o.Emission = tex2D(_MainTex, IN.uv_MainTex);
			o.Alpha = 1;
		}

		ENDCG
	}
}
