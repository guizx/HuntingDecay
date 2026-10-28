// Full-screen ordered dither into a fixed palette. Used by a Full Screen Pass Renderer Feature.
// The pattern is in screen space, so the dot size stays constant when the scope zooms in.
Shader "Hunting Decay/Palette Dither"
{
    Properties
    {
        [NoScaleOffset] _PaletteTex ("Palette (1 pixel per color, sRGB off)", 2D) = "black" {}
        [IntRange] _CellSize ("Dither Cell Size (screen px)", Range(1, 8)) = 1
        _DitherStrength ("Dither Strength", Range(0, 0.5)) = 0.12
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off ZTest Always Blend Off Cull Off

        Pass
        {
            Name "PaletteDither"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #define MAX_PALETTE_SIZE 64

            TEXTURE2D(_PaletteTex);
            float4 _PaletteTex_TexelSize;
            float _CellSize;
            float _DitherStrength;

            // Global, set by PaletteDither.SetEnabled. Not in Properties, so the material can't override it.
            float _PaletteDitherOff;

            static const float kBayer8[64] =
            {
                 0, 32,  8, 40,  2, 34, 10, 42,
                48, 16, 56, 24, 50, 18, 58, 26,
                12, 44,  4, 36, 14, 46,  6, 38,
                60, 28, 52, 20, 62, 30, 54, 22,
                 3, 35, 11, 43,  1, 33,  9, 41,
                51, 19, 59, 27, 49, 17, 57, 25,
                15, 47,  7, 39, 13, 45,  5, 37,
                63, 31, 55, 23, 61, 29, 53, 21
            };

            half4 Frag(Varyings input) : SV_Target
            {
                float4 source = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, input.texcoord);
                if (_PaletteDitherOff > 0.5)
                    return source;

                // Match in sRGB, the space the artist picked the palette in.
                float3 color = saturate(source.rgb);
                #if !defined(UNITY_COLORSPACE_GAMMA)
                color = LinearToSRGB(color);
                #endif

                uint2 cell = (uint2)(input.positionCS.xy / _CellSize) & 7;
                float threshold = (kBayer8[cell.y * 8 + cell.x] + 0.5) / 64.0 - 0.5;
                color += threshold * _DitherStrength;

                int paletteSize = (int)_PaletteTex_TexelSize.z;
                float3 nearest = 0;
                float nearestDistance = 1e5;
                for (int i = 0; i < MAX_PALETTE_SIZE; i++)
                {
                    if (i >= paletteSize)
                        break;
                    float3 entry = LOAD_TEXTURE2D(_PaletteTex, int2(i, 0)).rgb;
                    float3 delta = color - entry;
                    float distance = dot(delta, delta);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearest = entry;
                    }
                }

                #if !defined(UNITY_COLORSPACE_GAMMA)
                nearest = SRGBToLinear(nearest);
                #endif
                return half4(nearest, source.a);
            }
            ENDHLSL
        }
    }
}
