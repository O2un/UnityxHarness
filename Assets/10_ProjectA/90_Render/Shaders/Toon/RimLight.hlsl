#ifndef TOON_RIMLIGHT_INCLUDED
#define TOON_RIMLIGHT_INCLUDED

float3 CalcRimLight(float3 normalWS, float3 viewDirWS, float3 lightDirWS)
{
    float3 N = normalize(normalWS);
    float3 V = normalize(viewDirWS);
    float3 L = normalize(lightDirWS);

    float rim = 1.0 - saturate(dot(N, V));

    float sharpness = exp2(_RimLight_Power * 3.0);
    rim = pow(rim, sharpness);

    float threshold = _RimLight_InsideMask;
    float soft = smoothstep(threshold, 1.0, rim);
    float hard = step(threshold, rim);
    float mask = lerp(soft, hard, _RimLight_FeatherOff);

    float lightMask = 1.0 - saturate(dot(N, L));
    float dirMask = lerp(1.0, lightMask, _LightDirection_MaskOn);

    float intensity = mask * dirMask * _RimLight;

    return _RimLightColor.rgb * intensity;
}

#endif
