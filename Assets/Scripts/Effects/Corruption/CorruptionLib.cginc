#ifndef CORRUPTION_LIB_INCLUDED
#define CORRUPTION_LIB_INCLUDED
		
float4 _CorruptionColor;
float _CorruptionCutoff;

float4 _TwinkleColor;
float _TwinkleFactor;
float _TwinkleMax;
float _TwinkleMin;
float _TwinkleSpeed;

float4 _StartPos;
float _CutoffRange;
float4 _OutlineColor;
float _OutlineRange;
float _OutlineFactor;
float _FalloffRange;

inline float4 CalcCorruption(float4 mainTex, float4 corruptionTex, float4 twinkleTex, float3 worldPosition)
{
	float4 twinkleColor = twinkleTex * _TwinkleColor * _TwinkleFactor * clamp((sin(_Time.y * _TwinkleSpeed) + 1), _TwinkleMin, _TwinkleMax);
	float grayScale = clamp(mainTex.a - _CorruptionCutoff, 0, mainTex.a);
	float4 corruptionColor = grayScale * corruptionTex * _CorruptionColor;
	corruptionColor.rgb += corruptionColor.rgb * twinkleColor.rgb * grayScale;
	corruptionColor.a = mainTex.a;

	//以目标点为圆心剔除
	float distance = length(worldPosition.xyz -  _StartPos.xyz);
	clip(distance - _CutoffRange);

	float outline = _CutoffRange + _OutlineRange;

	if(distance < outline)
	{
		//颜色随距离衰减
		float falloff = (outline - distance) / _OutlineRange;
		corruptionColor.rgb += _OutlineColor.rgb * _OutlineFactor * falloff;

		//根据纹理形状和衰减范围剔除
		clip(Luminance(corruptionTex.rgb) - falloff + _FalloffRange);
	}

	return corruptionColor;
}

#endif