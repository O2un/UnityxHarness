#ifndef TOON_SHADOWCASTER_INCLUDED
#define TOON_SHADOWCASTER_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

float3 _LightDirection;
float3 _LightPosition;

struct ShadowAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS   : NORMAL;
};

struct ShadowVaryings
{
    float4 positionCS : SV_POSITION;
};

float4 GetShadowPositionHClip(ShadowAttributes input)
{
    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

#if _CASTING_PUNCTUAL_LIGHT_SHADOW
    float3 lightDirWS = normalize(_LightPosition - positionWS);
#else
    float3 lightDirWS = _LightDirection;
#endif

    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirWS));

#if UNITY_REVERSED_Z
    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
#else
    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
#endif

    return positionCS;
}

ShadowVaryings ShadowVert(ShadowAttributes input)
{
    ShadowVaryings output = (ShadowVaryings)0;
    output.positionCS = GetShadowPositionHClip(input);
    return output;
}

half4 ShadowFrag(ShadowVaryings input) : SV_Target
{
    return 0;
}

#endif
