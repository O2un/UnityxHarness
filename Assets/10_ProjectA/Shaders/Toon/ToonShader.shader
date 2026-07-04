Shader "ProjectA/ToonShader"
{
    Properties
    {
        [Header(Base)]
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)

        [Header(Cel Shading)]
        _1st_ShadeColor ("1st Shade Color", Color) = (0.7, 0.7, 0.8, 1)
        _2nd_ShadeColor ("2nd Shade Color", Color) = (0.5, 0.5, 0.6, 1)
        _BaseColor_Step ("Base Color Step", Range(0, 1)) = 0.5
        _BaseShade_Feather ("Base Shade Feather", Range(0, 0.5)) = 0.05
        _ShadeColor_Step ("Shade Color Step", Range(0, 1)) = 0.25
        _1st2nd_Shades_Feather ("1st 2nd Shades Feather", Range(0, 0.5)) = 0.05

        [Header(Rim Light)]
        [Toggle] _RimLight ("Rim Light", Float) = 0
        _RimLightColor ("Rim Light Color", Color) = (1, 1, 1, 1)
        _RimLight_Power ("Rim Light Power", Range(0, 1)) = 0.5
        _RimLight_InsideMask ("Rim Light Inside Mask", Range(0, 1)) = 0.5
        [Toggle] _RimLight_FeatherOff ("Rim Light Feather Off", Float) = 0
        [Toggle] _LightDirection_MaskOn ("Light Direction Mask On", Float) = 0

        [Header(Outline)]
        _Outline_Color ("Outline Color", Color) = (0, 0, 0, 1)
        _Outline_Width ("Outline Width", Float) = 1.0
        _Nearest_Distance ("Nearest Distance", Float) = 1.0
        _Farthest_Distance ("Farthest Distance", Float) = 20.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Front
            ZWrite On

            HLSLPROGRAM
            #pragma vertex OutlineVert
            #pragma fragment OutlineFrag
            #pragma target 3.0

            #include "ToonInput.hlsl"
            #include "Outline.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ToonForward"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma vertex ToonVert
            #pragma fragment ToonFrag
            #pragma target 3.0

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "ToonInput.hlsl"
            #include "ToonForward.hlsl"
            ENDHLSL
        }
    }
}
