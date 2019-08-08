// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'
// Upgrade NOTE: replaced '_ProjectorClip' with 'unity_ProjectorClip'

Shader "ProjectiveTexturing" 
{
	  Properties
     {
        _ShadowTex ("Cookie", 2D) = "gray" {}
        _FalloffTex ("FallOff", 2D) = "white" {}
     }
      
     Subshader
     {
         Tags { "RenderType"="Transparent-1" }
         Pass
         {
			ZWrite Off
			Offset -1, -1
			AlphaTest Greater 0
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
                          
             CGPROGRAM
             #pragma vertex vert
             #pragma fragment frag
             #pragma fragmentoption ARB_fog_exp2
             #pragma fragmentoption ARB_precision_hint_fastest
             #include "UnityCG.cginc"
              
             struct v2f
             {
                 float4 pos : SV_POSITION;
                 float4 uv_Main : TEXCOORD0;
                 float4 uv_Clip : TEXCOORD1;
             };
             
              
             sampler2D _ShadowTex;
             sampler2D _FalloffTex;
             float4x4 unity_Projector;
             float4x4 unity_ProjectorClip;
              
             v2f vert(appdata_tan v)
             {
                 v2f o;
                 o.pos = UnityObjectToClipPos (v.vertex);
                 o.uv_Main = mul (unity_Projector, v.vertex);
                 o.uv_Clip = mul (unity_ProjectorClip, v.vertex);
                 return o;
             }
              
             half4 frag (v2f i) : COLOR
             {
                 half4 tex = tex2Dproj(_ShadowTex, i.uv_Main);
                 //half4 falloff = tex2D(_FalloffTex, i.uv_Clip.xy);
                 //tex = lerp(float4(1,1,1,1),tex,falloff.a);
                 return tex;
             }
             ENDCG
      
         }
     }
}
