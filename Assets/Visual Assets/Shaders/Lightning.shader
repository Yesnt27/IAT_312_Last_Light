// A lightning bolt, drawn on LightningStrike's zigzag ribbon. This is the erode (dissolve) shader from Gabriel Aguiar's
// "Lightning Strike" tutorial, hand-written for URP: the bolt texture times a bright colour, added onto the picture,
// with Clip eating it away (0 = the whole bolt, 1 = gone; the soft edges go first). The texture's Offset Y slides it
// along the bolt. LightningStrike animates Colour, Clip and the offset during a strike.
Shader "Lighthouse/Lightning"
{
    Properties
    {
        _MainTex ("Bolt Texture (alpha = brightness)", 2D) = "white" {}
        [HDR] _Color ("Colour", Color) = (0.5, 0.75, 1, 1)
        _Clip ("Clip (erode)", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "Queue" = "Transparent+60" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass
        {
            Name "Lightning"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Blend One One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half _Clip;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half fade : TEXCOORD1;
                half fogFactor : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.fade = input.color.a;
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 bolt = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half strength = bolt.a * input.fade;
                clip(strength - _Clip - 0.002);

                // Additive, so fog can only dim it.
                half3 color = bolt.rgb * _Color.rgb * (strength * _Color.a) * ComputeFogIntensity(input.fogFactor);
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
