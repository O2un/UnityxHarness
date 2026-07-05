#ifndef TOON_DEPTHONLY_INCLUDED
#define TOON_DEPTHONLY_INCLUDED

struct DepthAttributes
{
    float4 positionOS : POSITION;
};

struct DepthVaryings
{
    float4 positionCS : SV_POSITION;
};

DepthVaryings DepthVert(DepthAttributes input)
{
    DepthVaryings output = (DepthVaryings)0;
    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
    return output;
}

half4 DepthFrag(DepthVaryings input) : SV_Target
{
    return 0;
}

#endif
