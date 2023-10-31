Shader "Unlit/HighlightOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Threshold("Threshold", float) = 0.01
        _EdgeColor("Edge color", Color) = (0,0,0,1)
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            TEXTURE2D(_CameraNormalsTexture);
            SAMPLER(sampler_CameraNormalsTexture);

            TEXTURE2D(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

            float4 _MainTex_TexelSize;
            float _Scale;
            float4 _Color;
            float _DepthThreshold;
            float _DepthNormalThreshold;
            float _DepthNormalThresholdScale;
            float _NormalThreshold;
            float4x4 _ClipToView;

            CBUFFER_START(UnityPerMaterial)
                
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 vertexColor : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                float2 texcoordStereo : TEXCOORD1;
                float3 viewSpaceDir : TEXCOORD2;
                #if STEREO_INSTANCING_ENABLED
				uint stereoTargetEyeIndex : SV_RenderTargetArrayIndex;
                #endif
            };


            float4 alphaBlend(float4 top, float4 bottom)
            {
                float3 color = (top.rgb * top.a) + (bottom.rgb * (1 - top.a));
                float alpha = top.a + bottom.a * (1 - top.a);

                return float4(color, alpha);
            }

            float2 TransformStereoScreenSpaceTex(float2 uv, float w)
            {
                float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
                return uv.xy * scaleOffset.xy + scaleOffset.zw * w;
            }

            float2 TransformTriangleVertexToUV(float2 vertex)
            {
                float2 uv = (vertex + 1.0) * 0.5;
                return uv;
            }

            Varyings Vert(Attributes v)
            {
                Varyings o;
                o.vertex = float4(v.positionOS.xy, 0.0, 1.0);
                o.texcoord = TransformTriangleVertexToUV(v.positionOS.xy);
                o.viewSpaceDir = mul(_ClipToView, o.vertex).xyz;

                #if UNITY_UV_STARTS_AT_TOP
                o.texcoord = o.texcoord * float2(1.0, -1.0) + float2(0.0, 1.0);
                #endif

                o.texcoordStereo = TransformStereoScreenSpaceTex(o.texcoord, 1.0);

                return o;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                const float halfScaleFloor = floor(_Scale * 0.5);
                const float halfScaleCeil = ceil(_Scale * 0.5);

                const float2 bottomLeftUV = input.texcoord - float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y) * halfScaleFloor;
                const float2 topRightUV = input.texcoord + float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y) * halfScaleCeil;
                const float2 bottomRightUV = input.texcoord + float2(_MainTex_TexelSize.x * halfScaleCeil,-_MainTex_TexelSize.y * halfScaleFloor);
                const float2 topLeftUV = input.texcoord + float2(-_MainTex_TexelSize.x * halfScaleFloor, _MainTex_TexelSize.y * halfScaleCeil);

                const float3 normal0 = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, bottomLeftUV).rgb;
                const float3 normal1 = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, topRightUV).rgb;
                const float3 normal2 = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, bottomRightUV).rgb;
                const float3 normal3 = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, topLeftUV).rgb;

                const float depth0 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, bottomLeftUV).r;
                const float depth1 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, topRightUV).r;
                const float depth2 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, bottomRightUV).r;
                const float depth3 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, topLeftUV).r;
                
                const float3 viewNormal = normal0 * 2 - 1;
                const float NdotV = 1 - dot(viewNormal, -input.viewSpaceDir);

                const float normalThreshold01 = saturate((NdotV - _DepthNormalThreshold) / (1 - _DepthNormalThreshold));
                const float normalThreshold = normalThreshold01 * _DepthNormalThresholdScale + 1;
                
                const float depthThreshold = _DepthThreshold * depth0 * normalThreshold;

                const float depthFiniteDifference0 = depth1 - depth0;
                const float depthFiniteDifference1 = depth3 - depth2;
                
                // edgeDepth is calculated using the Roberts cross operator.
                // The same operation is applied to the normal below.
                // https://en.wikipedia.org/wiki/Roberts_cross
                float edgeDepth = sqrt(pow(depthFiniteDifference0, 2) + pow(depthFiniteDifference1, 2)) * 100;
                edgeDepth = edgeDepth > depthThreshold ? 1 : 0;

                const float3 normalFiniteDifference0 = normal1 - normal0;
                const float3 normalFiniteDifference1 = normal3 - normal2;
                
                float edgeNormal = sqrt(dot(normalFiniteDifference0, normalFiniteDifference0) + dot(normalFiniteDifference1, normalFiniteDifference1));
                edgeNormal = edgeNormal > _NormalThreshold ? 1 : 0;

                const float edge = max(edgeDepth, edgeNormal);

                const float4 edgeColor = float4(_Color.rgb, _Color.a * edge);

                const float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texcoord);

                return alphaBlend(edgeColor, color);
            }
            ENDHLSL
        }
    }
}