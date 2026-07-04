#ifndef TOON_OUTLINE_INCLUDED
#define TOON_OUTLINE_INCLUDED

struct OutlineAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS   : NORMAL;
};

struct OutlineVaryings
{
    float4 positionCS : SV_POSITION;
};

OutlineVaryings OutlineVert(OutlineAttributes input)
{
    OutlineVaryings output = (OutlineVaryings)0;

    float3 pivotWS = TransformObjectToWorld(float3(0.0, 0.0, 0.0));
    float pivotDistance = distance(GetCameraPositionWS(), pivotWS);

    float range = max(_Farthest_Distance - _Nearest_Distance, 1e-4);
    float distanceScale = saturate((_Farthest_Distance - pivotDistance) / range);

    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

    float width = _Outline_Width * 0.01 * distanceScale;
    positionWS += normalWS * width;

    output.positionCS = TransformWorldToHClip(positionWS);
    return output;
}

half4 OutlineFrag(OutlineVaryings input) : SV_Target
{
    return half4(_Outline_Color.rgb, 1.0);
}

#endif
