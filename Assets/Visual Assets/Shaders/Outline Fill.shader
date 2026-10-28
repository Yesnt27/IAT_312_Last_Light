// The outline itself: the model pushed out along its normals by Width pixels and drawn in one colour,
// only where Outline Mask didn't mark the model, so just a rim around its edge shows.
// ModelOutline sets the colour and width per object.
Shader "Lighthouse/Outline Fill"
{
    Properties
    {
        _OutlineColor ("Colour", Color) = (0.25, 1, 0.35, 1)
        _OutlineWidth ("Width (pixels)", Float) = 3
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+110" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "OutlineFill"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Off
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            Stencil
            {
                Ref 1
                Comp NotEqual
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                float _OutlineWidth;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            float4 vert(Attributes input) : SV_POSITION
            {
                float4 positionCS = TransformObjectToHClip(input.positionOS.xyz);
                float3 normalCS = TransformWorldToHClipDir(TransformObjectToWorldNormal(input.normalOS));

                // Push out in screen space, so the width is the same number of pixels at any distance.
                float2 offset = normalize(normalCS.xy + 1e-6) * _OutlineWidth * 2.0 / _ScreenParams.xy;
                positionCS.xy += offset * positionCS.w;
                return positionCS;
            }

            half4 frag() : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
}
