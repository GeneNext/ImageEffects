Shader "CITT/BaseShadowCaster" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_AlphaTex ("Alpha", 2D) = "white" {}
		_Cutoff ("Cutoff", range(0, 1)) = 0.0001
	}

	SubShader 
	{
		Tags 
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
		
	    Pass
		{
	        Name "Caster"
	        Tags { "LightMode" = "ShadowCaster" }
	
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"
			
			struct v2f 
			{
			    V2F_SHADOW_CASTER;
				float2 uv_AlphaTex : TEXCOORD1;
			    UNITY_VERTEX_OUTPUT_STEREO
			};
			
			sampler2D _AlphaTex;
			float4 _AlphaTex_ST;
			float _Cutoff;
			float4 _Color;
			
			v2f vert(appdata_base v)
			{
			    v2f o;
			    UNITY_SETUP_INSTANCE_ID(v);
			    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			    TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				o.uv_AlphaTex = TRANSFORM_TEX(v.texcoord, _AlphaTex);
			    return o;
			}
			
			float4 frag(v2f i) : SV_Target
			{
			    float4 alphaTex = tex2D(_AlphaTex, i.uv_AlphaTex);
			    clip(alphaTex.r - _Cutoff);
			
			    SHADOW_CASTER_FRAGMENT(i)
			}

			ENDCG
	    }
	}
}
