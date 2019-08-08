Shader "CITT/Crystal" 
{
	Properties 
	{
		_MainTex ("Albedo (RGB)", 2D) = "black" {}
	}

    SubShader
	{
        Tags 
		{ 
			"Queue" = "AlphaTest" 
			"RenderType" = "OUTLINE" 
		}

       Pass
		{
           CGPROGRAM
		
           #pragma vertex vert
           #pragma fragment frag
		
		   #pragma multi_compile_fog
		   
		   #include "UnityCG.cginc"
		   	
		   sampler2D _MainTex;
		   float4 _MainTex_ST;
		   
		   struct VertexInput
		   {
		   	float4 vertex : POSITION;
		   	float4 texcoord : TEXCOORD0;
		   };
		   
		      struct VertexOutput 
		   {
		          float4 vertex : SV_POSITION;
		          float2 uv_MainTex : TEXCOORD0;
				  UNITY_FOG_COORDS(1)
		   };
		   
		   VertexOutput vert (VertexInput v)
		   {
				 VertexOutput output;
				 
				 UNITY_INITIALIZE_OUTPUT(VertexOutput, output);
				 
				 //uv
				 output.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
				 
				 //Он
				 UNITY_TRANSFER_FOG(output, output.vertex);
				 
				 return output;
		   }
		   
		   float4 frag (VertexOutput input) : COLOR
		   {
		   		float4 mainTex = tex2D(_MainTex, input.uv_MainTex);
		   		mainTex.a = Luminance(mainTex.rgb);
		   
		   		UNITY_APPLY_FOG(input.fogCoord, mainTex);
		   
		   		return saturate(mainTex);
		   }
		
           ENDCG
       }
    }

	Fallback "CITT/BaseShadowCaster"
}
