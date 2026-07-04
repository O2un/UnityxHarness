#ifndef TOON_FORWARD_INCLUDED
#define TOON_FORWARD_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "RimLight.hlsl"

struct ToonAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS   : NORMAL;
    float2 uv         : TEXCOORD0;
};

struct ToonVaryings
{
    float4 positionCS : SV_POSITION;
    float3 positionWS : TEXCOORD0;
    float3 normalWS   : TEXCOORD1;
    float2 uv         : TEXCOORD2;
};

ToonVaryings ToonVert(ToonAttributes input)
{
    ToonVaryings output = (ToonVaryings)0;

    VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
    output.positionCS = positionInputs.positionCS;
    output.positionWS = positionInputs.positionWS;
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
    return output;
}

half4 ToonFrag(ToonVaryings input) : SV_Target
{
    float3 normalWS = normalize(input.normalWS);
    float3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);

    Light mainLight = GetMainLight();
    float3 lightDirWS = normalize(mainLight.direction);

    float halfLambert = dot(normalWS, lightDirWS) * 0.5 + 0.5;

    float baseStep = smoothstep(_BaseColor_Step - _BaseShade_Feather,
                                _BaseColor_Step + _BaseShade_Feather, halfLambert);
    float shadeStep = smoothstep(_ShadeColor_Step - _1st2nd_Shades_Feather,
                                 _ShadeColor_Step + _1st2nd_Shades_Feather, halfLambert);

    float4 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
    float3 baseColor = baseTex.rgb * _BaseColor.rgb;

    float3 firstShade = baseColor * _1st_ShadeColor.rgb;
    float3 secondShade = baseColor * _2nd_ShadeColor.rgb;

    float3 shadeColor = lerp(secondShade, firstShade, shadeStep);
    float3 celColor = lerp(shadeColor, baseColor, baseStep);

    celColor *= mainLight.color;

    float3 rimColor = CalcRimLight(normalWS, viewDirWS, lightDirWS);

    return half4(celColor + rimColor, 1.0);
}

#endif
