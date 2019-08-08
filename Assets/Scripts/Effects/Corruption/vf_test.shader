Shader "TEST/vs_test"
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Main Tex", 2D) = "white" {}

		_CorruptionColor ("Corruption Color", Color) = (1,1,1,1)
		_CorruptionTex ("Corruption Tex", 2D) = "black" {}
		_CorruptionCutoff ("Color Cutoff", float) = 0.2

		_TwinkleColor ("Twinkle Color", Color) = (1,1,1,1)
		_TwinkleFactor ("Twinkle Factor", float) = 1
		_TwinkleMax ("Twinkle Max", float) = 1
		_TwinkleMin ("Twinkle Min", float) = 0.2
		_TwinkleSpeed ("Twinkle Speed", float) = 0.5

		_StartPos ("StartPosition", Vector) = (0,0,0,0)
		_CutoffRange ("Cutoff Range", float) = 1
		_OutlineColor ("Outline Color", Color) = (1,1,1,1)
		_OutlineRange ("Outline Range", float) = 0
		_OutlineFactor ("Outline Factor", float) = 1
		_FalloffRange ("Falloff Range", float) = 1
	}
 
	SubShader
	{
		Pass
		{
			CGPROGRAM

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			
			fixed4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			float4 _CorruptionColor;
			float4 _StartPos;
			float _CutoffRange;
			float _OutlineRange;
			
			struct vertex_output
			{
				float4 position : SV_POSITION;
			    float3 worldNormal : TEXCOORD0;
				float3 viewDir : TEXCOORD1;
			    float4 proj : TEXCOORD2;
			    float2 uv_MainTex : TEXCOORD3;
				UNITY_FOG_COORDS(4)

				#ifndef LIGHTMAP_OFF
				float2 uv_Lightmap : TEXCOORD5;
				#endif
			};
			
			vertex_output vert(appdata_full v)
			{
				vertex_output output;

				UNITY_INITIALIZE_OUTPUT(vertex_output, output);

				//uv
				output.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);

				#ifndef LIGHTMAP_OFF
				output.uv_Lightmap = TRANSFORM_TEX(v.texcoord1, unity_Lightmap);
				#endif

				//
				output.position = UnityObjectToClipPos(v.vertex);
				output.worldNormal = UnityObjectToWorldNormal(v.normal);
				output.viewDir = normalize(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, v.vertex).xyz);
				output.proj = ComputeScreenPos(UnityObjectToClipPos(v.vertex));
				COMPUTE_EYEDEPTH(output.proj.z);
				
				//雾
				UNITY_TRANSFER_FOG(output, output.position);

				return output;
			}
			
			fixed4 frag(vertex_output input) : SV_Target
			{
				float4 mainColor = tex2D(_MainTex, input.uv_MainTex);
				//以目标点为圆心剔除
				float distance = length(input.position.xyz -  _StartPos.xyz);

				float outline = _CutoffRange + _OutlineRange;
				float falloff = _CutoffRange - distance;
				if(distance < outline)
				{
					//颜色随距离衰减
					falloff = (outline - distance) / _OutlineRange;
					falloff = 1 - clamp(falloff, 0, 1);
				}
				else
				{
					falloff = 1;
				}

				float4 c;
				c.rgb = mainColor.rgb * _Color.rgb;// - falloff * (1 - lerp(1, _CorruptionColor, mainColor.a));
				c.a = mainColor.a;
				c.r = clamp(c.r, 0, 1);
				c.g = clamp(c.g, 0, 1);
				c.b = clamp(c.b, 0, 1);
				c.a = clamp(c.a, 0, 1);

				UNITY_APPLY_FOG(input.fogCoord, c);

				return c;
			}
			
			//使用vert函数和frag函数
			#pragma vertex vert
			#pragma fragment frag

			ENDCG
		}
		
		//正常着色的Pass
		Pass
		{
			CGPROGRAM	
			
			//引入头文件
			#include "Lighting.cginc"
			//定义Properties中的变量
			fixed4 _Diffuse;
			sampler2D _MainTex;
			//使用了TRANSFROM_TEX宏就需要定义XXX_ST
			float4 _MainTex_ST;
 
			//定义结构体：vertex shader阶段输出的内容
			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 worldNormal : TEXCOORD0;
				float2 uv : TEXCOORD1;
			};
 
			//定义顶点shader,参数直接使用appdata_base（包含position, noramal, texcoord）
			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				//通过TRANSFORM_TEX宏转化纹理坐标，主要处理了Offset和Tiling的改变,默认时等同于o.uv = v.texcoord.xy;
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.worldNormal = mul(v.normal, (float3x3)unity_WorldToObject);
				return o;
			}
 
			//定义片元shader
			fixed4 frag(v2f i) : SV_Target
			{
				//unity自身的diffuse也是带了环境光，这里我们也增加一下环境光
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * _Diffuse.xyz;
				//归一化法线，即使在vert归一化也不行，从vert到frag阶段有差值处理，传入的法线方向并不是vertex shader直接传出的
				fixed3 worldNormal = normalize(i.worldNormal);
				//把光照方向归一化
				fixed3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);
				//根据半兰伯特模型计算像素的光照信息
				fixed3 lambert = 0.5 * dot(worldNormal, worldLightDir) + 0.5;
				//最终输出颜色为lambert光强*材质diffuse颜色*光颜色
				fixed3 diffuse = lambert * _Diffuse.xyz * _LightColor0.xyz + ambient;
				//进行纹理采样
				fixed4 color = tex2D(_MainTex, i.uv);
				color.rgb = color.rgb* diffuse;
				return fixed4(color);
			}
 
			//使用vert函数和frag函数
			#pragma vertex vert
			#pragma fragment frag	
 
			ENDCG
		}
	}
	//前面的Shader失效的话，使用默认的Diffuse
	FallBack "Diffuse"
}