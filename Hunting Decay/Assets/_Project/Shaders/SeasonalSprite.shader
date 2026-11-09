// URP's Sprite-Lit-Default with one addition: the photo's foliage changes color with the season.
// Green hues are pulled toward a target hue (orange in autumn) and paled toward white in winter. Atmosphere sets the
// global _Season* values from the day. With _SeasonStrength at 0 it renders exactly like Sprite-Lit-Default.
Shader "Hunting Decay/Seasonal Sprite"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {}
        _MaskTex("Mask", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        [MaterialToggle] _ZWrite("ZWrite", Float) = 0

        [HideInInspector] _Color("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _AlphaTex("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite [_ZWrite]

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            #pragma vertex LitVertex
            #pragma fragment SeasonalFragment

            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/ShapeLightShared.hlsl"

            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY
            #pragma multi_compile _ SKINNED_SPRITE

            struct Attributes
            {
                COMMON_2D_INPUTS
                half4 color        : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_LIT_OUTPUTS
                half4 color        : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Lit2DCommon.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
            CBUFFER_END

            // Globals, set by Atmosphere. Hues are in turns, so 1 is a full circle.
            float _SeasonStrength;
            // x: target hue, y: how far foliage moves toward it (0 to 1), z: how much of its own hue variation it keeps.
            float4 _SeasonHue;
            float _SeasonSaturation;
            float _SeasonWhiten;
            // x: center of the foliage hues, y: half width of the band, z: soft edge on each side.
            float4 _SeasonFoliageBand;

            half3 SeasonRemap(half3 color)
            {
                if (_SeasonStrength <= 0.0)
                    return color;

                // Hue math in sRGB, so the band and shift match what a color picker shows.
                float3 srgb = LinearToSRGB(color);
                float3 hsv = RgbToHsv(srgb);
                float distance = abs(frac(hsv.x - _SeasonFoliageBand.x + 0.5) - 0.5);
                float inBand = 1.0 - smoothstep(_SeasonFoliageBand.y, _SeasonFoliageBand.y + _SeasonFoliageBand.z, distance);
                // Grays, browns and near-black pixels have no reliable hue, so they stay as they are.
                float mask = inBand * smoothstep(0.08, 0.25, hsv.y) * smoothstep(0.0, 0.15, hsv.z) * _SeasonStrength;

                // Every photo's greens land near the same target, whatever shade of green they started as.
                float offset = frac(hsv.x - _SeasonFoliageBand.x + 0.5) - 0.5;
                float goal = _SeasonHue.x + offset * _SeasonHue.z;
                float hue = frac(hsv.x + (frac(goal - hsv.x + 0.5) - 0.5) * _SeasonHue.y);
                float3 shifted = HsvToRgb(float3(hue, saturate(hsv.y * _SeasonSaturation), hsv.z));
                float luma = dot(shifted, float3(0.2126, 0.7152, 0.0722));
                // Frost lifts the bright leaves more than the shaded ones, so the shading underneath survives.
                float3 snow = luma + (1.0 - luma) * 0.55 * smoothstep(0.0, 0.5, luma);
                shifted = lerp(shifted, snow, _SeasonWhiten);

                return SRGBToLinear(lerp(srgb, shifted, mask));
            }

            Varyings LitVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

                Varyings o = CommonLitVertex(input);
                o.color = input.color * _Color * unity_SpriteColor;
                return o;
            }

            // CommonLitFragment from Lit2DCommon.hlsl, with the season applied to the sampled color.
            half4 SeasonalFragment(Varyings input) : SV_Target
            {
                half4 main = input.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                main.rgb = SeasonRemap(main.rgb);
                const half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, input.uv);
                const half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv));

                SurfaceData2D surfaceData;
                InputData2D inputData;
                InitializeSurfaceData(main.rgb, main.a, mask, normalTS, surfaceData);
                InitializeInputData(input.uv, input.lightingUV, inputData);

            #if defined(DEBUG_DISPLAY)
                SETUP_DEBUG_TEXTURE_DATA_2D_NO_TS(inputData, input.positionWS, input.positionCS, _MainTex);
                surfaceData.normalWS = input.normalWS;
            #endif

                return CombinedShapeLightShared(surfaceData, inputData);
            }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "NormalsRendering"}

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex NormalsRenderingVertex
            #pragma fragment NormalsRenderingFragment

            #pragma multi_compile_instancing
            #pragma multi_compile _ SKINNED_SPRITE

            struct Attributes
            {
                COMMON_2D_NORMALS_INPUTS
                float4 color        : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_NORMALS_OUTPUTS
                half4   color           : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Normals2DCommon.hlsl"

            CBUFFER_START( UnityPerMaterial )
                half4 _Color;
            CBUFFER_END

            Varyings NormalsRenderingVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

                Varyings o = CommonNormalsVertex(input);
                o.color = input.color * _Color * unity_SpriteColor;
                return o;
            }

            half4 NormalsRenderingFragment(Varyings input) : SV_Target
            {
                return CommonNormalsFragment(input, input.color);
            }
            ENDHLSL
        }
    }
}
