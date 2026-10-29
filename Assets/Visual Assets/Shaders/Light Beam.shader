// The visible part of a spotlight beam, used by LightBeam on a cone mesh. Additive glow that is
// brightest along the middle of the cone, soft at its edges, and fades out towards the far end.
Shader "Lighthouse/Light Beam"
{
    Properties
    {
        _Color ("Colour", Color) = (1, 0.95, 0.6, 1)
        _Strength ("Strength", Float) = 0.35
        _EdgeSoftness ("Edge Softness", Float) = 2
        _FadePower ("Fade Along The Beam", Float) = 1.5
    }

    SubShader
    {
        Tags { "Queue" = "Transparent+50" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass
        {
            Name "LightBeam"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Blend One One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Strength;
                half _EdgeSoftness;
                half _FadePower;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float along : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.along = input.uv.y;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 viewDir = normalize(_WorldSpaceCameraPos - input.positionWS);
                // Facing the camera = the middle of the beam; side-on = its edges, which fade away.
                half facing = abs(dot(normalize(input.normalWS), viewDir));
                half edge = pow(facing, _EdgeSoftness);
                half fade = saturate(input.along * 40.0) * pow(saturate(1.0 - input.along), _FadePower);
                return half4(_Color.rgb * _Strength * edge * fade, 1);
            }
            ENDHLSL
        }
    }
}
