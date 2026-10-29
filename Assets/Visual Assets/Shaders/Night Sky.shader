// Night skybox for the 3D night scene: a dark sky overhead, a bright glow band along the horizon and
// optional stars. Below the horizon it fades to Below The Horizon, which should match the scene's fog
// colour so the far edge of the ocean melts into the glow. No textures needed.
Shader "Lighthouse/Night Sky"
{
    Properties
    {
        _ZenithColor ("Sky Colour (overhead)", Color) = (0.006, 0.009, 0.022, 1)
        _HorizonColor ("Horizon Glow Colour", Color) = (0.72, 0.79, 0.87, 1)
        _GroundColor ("Below The Horizon (match the fog colour)", Color) = (0.72, 0.79, 0.87, 1)
        _GlowHeight ("Glow Height", Range(0.005, 1)) = 0.12
        _GlowFalloff ("Glow Fall-off", Float) = 2.5
        _StarDensity ("Stars (0 = none)", Range(0, 1)) = 0.35
        _StarBrightness ("Star Brightness", Float) = 0.9
        _StarScale ("Star Spacing", Float) = 220
    }

    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" "RenderPipeline" = "UniversalPipeline" }
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _ZenithColor;
                half4 _HorizonColor;
                half4 _GroundColor;
                float _GlowHeight;
                float _GlowFalloff;
                float _StarDensity;
                half _StarBrightness;
                float _StarScale;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 direction : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.direction = input.positionOS.xyz;
                return output;
            }

            float Hash(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1) * 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 direction = normalize(input.direction);
                float height = direction.y;

                // The glow is strongest right at the horizon and fades into the dark sky above.
                float glow = pow(saturate(1.0 - height / _GlowHeight), _GlowFalloff);
                half3 color = lerp(_ZenithColor.rgb, _HorizonColor.rgb, glow);
                if (height < 0.0) color = lerp(_HorizonColor.rgb, _GroundColor.rgb, saturate(-height * 8.0));

                // Stars: a few random cells of a grid on the sky, only where the glow has faded.
                float3 cell = floor(direction * _StarScale);
                float3 local = frac(direction * _StarScale) - 0.5;
                float star = step(1.0 - _StarDensity * 0.02, Hash(cell)) * saturate(1.0 - length(local) * 3.0);
                color += star * _StarBrightness * saturate(height * 4.0) * (1.0 - glow);

                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
