Shader "Pupil/LuminosityRT"
{
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100
        Cull Off
        ZWrite Off
        //ZTest NotEqual 
        ZTest Always
        //Blend One Zero

        Pass
        {
            Name "LuminosityCalculation"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = float4(input.positionOS.xyz, 1.0);
                #if UNITY_UV_STARTS_AT_TOP
                output.positionCS.y *= -1;
                #endif

                output.uv = input.uv;
                return output;
            }

            TEXTURE2D_X(_LuminosityTex);
            SAMPLER(sampler_LuminosityTex);

            float3 positive_pow(const float3 base, const float3 power)
            {
                return pow(abs(base), power);
            }

            float3 get_srgb_to_linear(const float3 c)
            {
                const float3 linear_rgb_lo = c / 12.92;
                const float3 linear_rgb_hi = positive_pow((c + 0.055) / 1.055, float3(2.4, 2.4, 2.4));
                return (c <= 0.04045) ? linear_rgb_lo : linear_rgb_hi;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                half4 sampled_color = SAMPLE_TEXTURE2D_X(_LuminosityTex, sampler_LuminosityTex, input.uv);
                return sampled_color;
            }
            ENDHLSL
        }
    }
}