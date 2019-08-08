#ifndef TERRAIN_LIB_CGINC
#define TERRAIN_LIB_CGINC

half4 _Color;

half _uvScaleX;
half _uvScaleY;

half _BumpedPower;

sampler2D _Control;
sampler2D _Control2;

sampler2D _Splat0; 
sampler2D _Splat0_bumpMap;
half _Splat0_uvScale;

sampler2D _Splat1; 
sampler2D _Splat1_bumpMap;
half _Splat1_uvScale;

sampler2D _Splat2; 
sampler2D _Splat2_bumpMap;
half _Splat2_uvScale;

sampler2D _Splat3; 
sampler2D _Splat3_bumpMap;
half _Splat3_uvScale;

sampler2D _Splat4; 
sampler2D _Splat4_bumpMap;
half _Splat4_uvScale;

sampler2D _Splat5; 
sampler2D _Splat5_bumpMap;
half _Splat5_uvScale;

sampler2D _Splat6; 
sampler2D _Splat6_bumpMap;
half _Splat6_uvScale;

sampler2D _Splat7; 
sampler2D _Splat7_bumpMap;
half _Splat7_uvScale;

half _Shininess;
half _Gloss;

struct Input 
{
	float2 uv_Control;
	float2 uv_Control2;
};

void surf(Input IN, inout SurfaceOutput o)
{
	half2 uvScale = half2(_uvScaleX, _uvScaleY);

	half4 splat_control = tex2D (_Control, IN.uv_Control);
	half2 uv_splat_control = IN.uv_Control * uvScale;
	half weight = dot(splat_control, half4(1,1,1,1));
    splat_control = splat_control / weight;
	splat_control = half4(max(splat_control.r, 0), max(splat_control.g, 0), max(splat_control.b, 0), max(splat_control.a, 0));
	
	half4 splat_control2 = tex2D (_Control2, IN.uv_Control2);
	half2 uv_splat_control2 = IN.uv_Control2 * uvScale;
	half weight2 = dot(splat_control2, half4(1,1,1,1));
    splat_control2 = splat_control2 / weight2;
	splat_control2 = half4(max(splat_control2.r, 0), max(splat_control2.g, 0), max(splat_control2.b, 0), max(splat_control2.a, 0));

	o.Alpha = weight;
	
	half4 albedo = 0;
	half4 diffuse  = 0.0f;

	diffuse += splat_control.r * tex2D (_Splat0, _Splat0_uvScale * uv_splat_control);

	diffuse += splat_control.g * tex2D (_Splat1, _Splat1_uvScale * uv_splat_control);
	
	diffuse += splat_control.b * tex2D (_Splat2, _Splat2_uvScale * uv_splat_control);

	diffuse += splat_control.a * tex2D (_Splat3, _Splat3_uvScale * uv_splat_control);

	diffuse += splat_control2.r * tex2D (_Splat4, _Splat4_uvScale * uv_splat_control2);

	diffuse += splat_control2.g * tex2D (_Splat5, _Splat5_uvScale * uv_splat_control2);

	diffuse += splat_control2.b * tex2D (_Splat6, _Splat6_uvScale * uv_splat_control2);

	diffuse += splat_control2.a * tex2D (_Splat7, _Splat7_uvScale * uv_splat_control2);

	albedo = diffuse * weight;
	o.Albedo = albedo.rgb * _Color.rgb;

	half4 normal = 0.0f;
		
	normal += splat_control.r * tex2D(_Splat0_bumpMap, _Splat0_uvScale * uv_splat_control);

	normal += splat_control.g * tex2D(_Splat1_bumpMap, _Splat1_uvScale * uv_splat_control);

	normal += splat_control.b * tex2D (_Splat2_bumpMap, _Splat2_uvScale * uv_splat_control);

	normal += splat_control.a * tex2D (_Splat3_bumpMap, _Splat3_uvScale * uv_splat_control);

	normal += splat_control2.r * tex2D (_Splat4_bumpMap, _Splat4_uvScale * uv_splat_control2);

	normal += splat_control2.g * tex2D (_Splat5_bumpMap, _Splat5_uvScale * uv_splat_control2);

	normal += splat_control2.b * tex2D (_Splat6_bumpMap, _Splat6_uvScale * uv_splat_control2);

	normal += splat_control2.a * tex2D (_Splat7_bumpMap, _Splat7_uvScale * uv_splat_control2);

	o.Normal = UnpackNormal(normal * _BumpedPower);
	o.Normal = o.Normal;

	o.Specular = _Shininess * albedo.a;
	o.Gloss = _Gloss * albedo.a;
}
#endif