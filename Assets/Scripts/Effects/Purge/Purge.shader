Shader "TEST/Purge" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_ColorFactor ("Color Factor", float) = 1

		_DepthFactor ("Depth Factor", float) = 0.05
		_DepthRange ("Depth Range", float) = 10

		_Alpha ("Alpha", float) = 1

		_NoiseFactor ("Noise Factor", float) = 0.3
		_NoiseSpeed ("Noise Speed", float) = 0.6
	}

	SubShader 
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		Cull Off

		CGPROGRAM
		#pragma surface surf Lambert vertex:vert alpha:blend nolightmap
        #pragma target 3.0
		
		sampler2D _CameraDepthTexture;

		sampler2D _MainTex;
		float4 _Color;
		float _ColorFactor;

		float _DepthFactor;
		float _DepthRange;

		float _Alpha;

		float _NoiseFactor;
		float _NoiseSpeed;

		struct Input 
		{
			float2 uv_MainTex;
			float3 viewDir;
			float3 normal;
			float4 proj;
		};

		void vert (inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			
			o.normal = UnityObjectToWorldNormal(v.normal);

			o.proj = ComputeScreenPos(UnityObjectToClipPos(v.vertex));
			COMPUTE_EYEDEPTH(o.proj.z);
		}

		void surf (Input IN, inout SurfaceOutput o) 
		{
			//计算深度差
			float sceneDepth = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, IN.proj).r);
			float partDepth = IN.proj.z;
			float depthFade = (sceneDepth - partDepth) * _DepthFactor;
			depthFade = 1 - clamp(depthFade, 0, 1) * _DepthRange;
			
			float speed = Luminance(tex2D(_MainTex, IN.uv_MainTex + _Time.x * 0.1).rgb) * _NoiseFactor + _Time.x * _NoiseSpeed;
			//float noiseX = tex2D(_MainTex, IN.uv_MainTex + float2(1, 0) * _Time.x * 0.5);
			//float noiseY = tex2D(_MainTex, IN.uv_MainTex + float2(0, 1) * _Time.x * 0.5);
			//float2 noiseUV = float2(noiseX, noiseY);
			float2 noiseUV = tex2D(_MainTex, IN.uv_MainTex + float2(speed, speed));

			float4 flowColor1 = tex2D (_MainTex, IN.uv_MainTex + noiseUV);
			float4 flowColor2 = tex2D (_MainTex, IN.uv_MainTex + noiseUV);

			float4 c = lerp(flowColor1, flowColor2, 0.5) * _Color * _ColorFactor;
			o.Emission = c.rgb;
			o.Alpha = Luminance(c.rgb) * depthFade * _Alpha;
			o.Alpha = 1 - clamp(o.Alpha, 0, 1);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
