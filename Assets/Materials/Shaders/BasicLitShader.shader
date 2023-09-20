Shader "BasicLitShader"
{
    Properties
    {
        [NormalTexture] _NormalMap("Normal Map", 2D) = "white" {}
        [MainColor] _ColorTint ("Tint", Color) = (1, 1, 1, 1) 
        _DisplacementStrength("Displacement Strength", Float) = 1
        [HideInInspector] _SourceBlend("Source blend", Float) = 0
        [HideInInspector] _DestBlend("Destination blend", Float) = 0
        [HideInInspector] _ZWrite("ZWrite", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalRenderPipeline"
            "UniversalMaterialType" = "Lit"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "ForwardLit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
            };

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            CBUFFER_START(UnityPerMaterial)
            float4 _NormalMap_ST;
            float4 _ColorTint;
            float _DisplacementStrength;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                output.uv = TRANSFORM_TEX(input.uv, _NormalMap);
                float displacement = tex2Dlod(sampler_NormalMap, float4(output.uv.xy, 0, 0)).g;
                displacement = (displacement - 0.5) * _DisplacementStrength;
                input.positionOS.y += normalize(input.normalOS) * displacement;

                const VertexPositionInputs position_inputs = GetVertexPositionInputs(input.positionOS);
                const VertexNormalInputs normal_inputs = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = position_inputs.positionCS;
                output.normalWS = normal_inputs.normalWS;
                output.positionWS = position_inputs.positionWS;
                
                return output;
            }


            half4 frag(Varyings input) : SV_Target
            {
                float4 color_sample = _ColorTint; //SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                InputData lighting_input = (InputData)0;
                lighting_input.vertexLighting = VertexLighting(input.positionWS, input.normalWS);
                lighting_input.fogCoord = ComputeFogFactor(input.positionCS.z);
                lighting_input.positionCS = input.positionCS;
                lighting_input.positionWS = input.positionWS.xyz;
                lighting_input.normalWS = normalize(input.normalWS);
                lighting_input.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS.xyz);
                
                SurfaceData surface_input = (SurfaceData)0;
                surface_input.albedo = color_sample.rgb;
                surface_input.specular = 1;
                surface_input.smoothness = 1;
                surface_input.alpha = color_sample.a;
                
                return UniversalFragmentBlinnPhong(lighting_input, surface_input);
                
            }
            ENDHLSL
        }
    }
}