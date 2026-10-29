// Moving ocean for URP (3D renderer). Four Gerstner waves move the surface; small ripples,
// a sky reflection, a sun highlight and crest foam are added per pixel. No textures needed.
// Waves fade out with distance from the camera so the far ocean stays calm and cheap.
// Spotlights and point lights (e.g. the lighthouse beam) light up the water they reach.
Shader "Lighthouse/Ocean"
{
    Properties
    {
        [Header(Colours)]
        _DeepColor ("Deep Colour", Color) = (0.02, 0.09, 0.16, 1)
        _ShallowColor ("Wave Crest Colour", Color) = (0.07, 0.27, 0.35, 1)
        _SkyColor ("Sky Reflection (overhead)", Color) = (0.42, 0.57, 0.76, 1)
        _HorizonColor ("Sky Reflection (horizon)", Color) = (0.72, 0.79, 0.87, 1)
        _FoamColor ("Foam Colour", Color) = (0.9, 0.95, 1, 1)
        _HorizonSharpness ("Horizon Reflection Sharpness", Float) = 2
        _LitWaterColor ("Water Colour In Spotlights", Color) = (0.18, 0.26, 0.28, 1)

        [Header(Waves . XZ direction then steepness then wavelength in metres)]
        _WaveSpeed ("Wave Speed", Float) = 0.8
        _WaveA ("Wave A", Vector) = (1, 0.3, 0.18, 60)
        _WaveB ("Wave B", Vector) = (0.6, 1, 0.15, 31)
        _WaveC ("Wave C", Vector) = (-0.4, 0.9, 0.12, 18)
        _WaveD ("Wave D", Vector) = (0.9, -0.6, 0.08, 11)
        _WaveFadeDistance ("Waves Fade Out By (m)", Float) = 700

        [Header(Surface)]
        _RippleStrength ("Ripple Strength", Range(0, 1)) = 0.4
        _RippleScale ("Ripple Size (m)", Float) = 3
        _Glossiness ("Sun Highlight Sharpness", Float) = 400
        _SpecularStrength ("Sun Highlight Strength", Float) = 1.5
        _FoamHeight ("Foam Starts At Height (m)", Float) = 2
        _FoamStrength ("Foam Strength", Range(0, 1)) = 0.45
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _DeepColor;
                half4 _ShallowColor;
                half4 _SkyColor;
                half4 _HorizonColor;
                half4 _FoamColor;
                half _HorizonSharpness;
                half4 _LitWaterColor;
                float _WaveSpeed;
                float4 _WaveA;
                float4 _WaveB;
                float4 _WaveC;
                float4 _WaveD;
                float _WaveFadeDistance;
                half _RippleStrength;
                float _RippleScale;
                half _Glossiness;
                half _SpecularStrength;
                float _FoamHeight;
                half _FoamStrength;
            CBUFFER_END

            static const float2 kRippleDirections[4] = { float2(0.8, 0.6), float2(-0.5, 0.86), float2(0.2, -0.98), float2(-0.92, -0.38) };
            static const float kRippleFrequencies[4] = { 1.0, 1.7, 2.3, 3.1 };

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float height : TEXCOORD2;
                float waveFade : TEXCOORD3;
                float fogFactor : TEXCOORD4;
            };

            // One Gerstner wave: returns the offset and adds its slope to the tangent/binormal.
            float3 GerstnerWave(float4 wave, float3 p, float time, float fade, inout float3 tangent, inout float3 binormal)
            {
                float steepness = wave.z * fade;
                float k = TWO_PI / max(wave.w, 0.01);
                float speed = sqrt(9.8 / k);
                float2 d = normalize(wave.xy);
                float f = k * (dot(d, p.xz) - speed * time);
                float amplitude = steepness / k;
                float s = sin(f);
                float c = cos(f);

                tangent += float3(-d.x * d.x * steepness * s, d.x * steepness * c, -d.x * d.y * steepness * s);
                binormal += float3(-d.x * d.y * steepness * s, d.y * steepness * c, -d.y * d.y * steepness * s);
                return float3(d.x * amplitude * c, amplitude * s, d.y * amplitude * c);
            }

            // Slope of a small sum-of-sines height field, used to wiggle the normal.
            float2 RippleSlope(float2 xz, float time)
            {
                float2 slope = 0;
                [unroll]
                for (int i = 0; i < 4; i++)
                {
                    float k = TWO_PI * kRippleFrequencies[i] / max(_RippleScale, 0.01);
                    float phase = k * dot(kRippleDirections[i], xz) + time * (1.5 + i * 0.7);
                    slope += kRippleDirections[i] * cos(phase) / kRippleFrequencies[i];
                }
                return slope * 0.15;
            }

            // Light from spotlights / point lights: a soft glow on the water plus glints on the waves.
            half3 AdditionalLights(float3 positionWS, float4 positionCS, float3 normal, float3 reflected)
            {
                half3 result = 0;
            #if defined(_ADDITIONAL_LIGHTS) || defined(_CLUSTER_LIGHT_LOOP)
                InputData inputData = (InputData)0;
                inputData.positionWS = positionWS;
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(positionCS);
                uint lightCount = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(lightCount)
                    Light light = GetAdditionalLight(lightIndex, positionWS);
                    half3 radiance = light.color * light.distanceAttenuation * light.shadowAttenuation;
                    half diffuse = saturate(dot(normal, light.direction));
                    half glint = pow(saturate(dot(reflected, light.direction)), _Glossiness * 0.25) * _SpecularStrength;
                    result += radiance * (_LitWaterColor.rgb * diffuse + glint);
                LIGHT_LOOP_END
            #endif
                return result;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 p = TransformObjectToWorld(input.positionOS.xyz);
                float fade = saturate(1.0 - distance(p.xz, _WorldSpaceCameraPos.xz) / max(_WaveFadeDistance, 1.0));
                float time = _Time.y * _WaveSpeed;

                float3 tangent = float3(1, 0, 0);
                float3 binormal = float3(0, 0, 1);
                float3 offset = GerstnerWave(_WaveA, p, time, fade, tangent, binormal);
                offset += GerstnerWave(_WaveB, p, time, fade, tangent, binormal);
                offset += GerstnerWave(_WaveC, p, time, fade, tangent, binormal);
                offset += GerstnerWave(_WaveD, p, time, fade, tangent, binormal);

                float3 displaced = p + offset;
                output.positionWS = displaced;
                output.normalWS = normalize(cross(binormal, tangent));
                output.height = offset.y;
                output.waveFade = fade;
                output.positionCS = TransformWorldToHClip(displaced);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float time = _Time.y * _WaveSpeed;
                float distanceToCamera = distance(input.positionWS, _WorldSpaceCameraPos);
                float rippleFade = saturate(1.0 - distanceToCamera / max(_WaveFadeDistance * 0.5, 1.0));
                float2 slope = RippleSlope(input.positionWS.xz, time) * _RippleStrength * rippleFade;
                float3 normal = normalize(input.normalWS + float3(-slope.x, 0, -slope.y));

                float3 viewDir = normalize(_WorldSpaceCameraPos - input.positionWS);
                Light mainLight = GetMainLight();
                float3 reflected = reflect(-viewDir, normal);

                half fresnel = 0.04 + 0.96 * pow(1.0 - saturate(dot(normal, viewDir)), 5.0);
                // Only reflections pointing close to the horizon pick up its glow.
                half horizonBand = saturate(1.0 - abs(reflected.y) * _HorizonSharpness);
                half3 sky = lerp(_SkyColor.rgb, _HorizonColor.rgb, horizonBand);

                half crest = saturate(input.height / max(_FoamHeight, 0.01) * 0.5 + 0.5);
                half3 water = lerp(_DeepColor.rgb, _ShallowColor.rgb, crest);
                half3 lit = water * (SampleSH(normal) + mainLight.color * saturate(dot(normal, mainLight.direction)) * 0.6);

                half sun = pow(saturate(dot(reflected, mainLight.direction)), _Glossiness) * _SpecularStrength;
                // Foam only on the tallest crests, broken up by the ripples so it isn't a flat blob.
                half foamNoise = saturate(0.5 + (slope.x + slope.y) * 3.0);
                half foam = smoothstep(_FoamHeight, _FoamHeight * 1.4, input.height) * input.waveFade * foamNoise;

                half3 color = lerp(lit, sky, fresnel) + mainLight.color * sun;
                color = lerp(color, _FoamColor.rgb, foam * _FoamStrength);
                color += AdditionalLights(input.positionWS, input.positionCS, normal, reflected);
                color = MixFog(color, input.fogFactor);
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
