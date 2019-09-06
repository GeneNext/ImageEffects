Shader "Hidden/WeatherEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM

            #include "UnityCG.cginc"
			#include "UnityStandardUtils.cginc"

			uniform float _Rain;
			uniform float _Snow;

            sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _RainNormalTex1;
			float4 _RainNormalTex1_ST;

			sampler2D _RainNormalTex2;
			float4 _RainNormalTex2_ST;

			float _RainIntensity;

			float _RainSpeed1;
			float _RainSpeed2;

			sampler2D _SnowTex;
			float4 _SnowTex_ST;

			sampler2D _SnowNormalTex;
			float4 _SnowNormalTex_ST;

			float _SnowCutout;
			float _SnowIntensity;

			float _Exposure;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv_MainTex : TEXCOORD0;
				float2 uv_RainNormalTex1 : TEXCOORD1;
				float2 uv_RainNormalTex2 : TEXCOORD2;
				float2 uv_SnowTex : TEXCOORD3;
				float2 uv_SnowNormalTex : TEXCOORD4;
            };
			
            #pragma vertex vert
            #pragma fragment frag

			#pragma multi_compile WEATHER_SNOW_OFF WEATHER_SNOW_ON
			#pragma multi_compile WEATHER_RAIN_OFF WEATHER_RAIN_ON

            v2f vert (appdata v)
            {
                v2f o;
				
				UNITY_INITIALIZE_OUTPUT(v2f, o);

                o.vertex = UnityObjectToClipPos(v.vertex);

                o.uv_MainTex = TRANSFORM_TEX(v.uv, _MainTex);
				
				#ifdef WEATHER_RAIN_ON
				o.uv_RainNormalTex1 = TRANSFORM_TEX(v.uv, _RainNormalTex1);
				o.uv_RainNormalTex2 = TRANSFORM_TEX(v.uv, _RainNormalTex2);
				#endif

				#ifdef WEATHER_SNOW_ON
				o.uv_SnowTex = TRANSFORM_TEX(v.uv, _SnowTex);
				o.uv_SnowNormalTex = TRANSFORM_TEX(v.uv, _SnowNormalTex);
				#endif

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				fixed3 normal = 0;
				fixed2 normalOffset = 0;
				
				#ifdef WEATHER_RAIN_ON
				//不支持repeat，原因未知。。
				float2 uv1 = float2(i.uv_RainNormalTex1.x, i.uv_RainNormalTex1.y + _RainSpeed1 * _Time.x);
				uv1.y = uv1.y % 1;
				float2 uv2 = float2(i.uv_RainNormalTex2.x, i.uv_RainNormalTex1.y + _RainSpeed2 * _Time.x);
				uv2.y = uv2.y % 1;
				fixed3 normal1 = UnpackNormal(tex2D(_RainNormalTex1, uv1));
				fixed3 normal2 = UnpackNormal(tex2D(_RainNormalTex2, uv2));
				//normal = normal * 0.5;
				normal = lerp(normal1, normal2, 0.5);
				normalOffset = (normal.xy * normal.z) * _RainIntensity * _Rain;
				#endif

				#ifdef WEATHER_SNOW_ON
				fixed4 snow = saturate(saturate(tex2D(_SnowTex, i.uv_SnowTex) - _SnowCutout * (1 - _Snow)) * _SnowIntensity * _Snow);
				normal = UnpackNormal(tex2D(_SnowNormalTex, i.uv_SnowNormalTex));
				normalOffset += (normal.xy * normal.z) * _SnowIntensity * Luminance(snow.rgb);
				#endif

                fixed4 col = tex2D(_MainTex, i.uv_MainTex + normalOffset);

				#ifdef WEATHER_SNOW_ON
				col = lerp(col , snow, Luminance(snow.rgb));
				//col = col + snow;
				#endif

				col = col * _Exposure;

                return saturate(col);
            }
            ENDCG
        }
    }
}
