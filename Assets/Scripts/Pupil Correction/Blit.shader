Shader "Unlit/Blit"
{
    Properties
    {
        _MainTex ("Blit_Texture", any) = "" {}
    }
    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
            uniform float4 _MainTex_ST;
            uniform float4 _Color;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                return o;
            }

            float3 get_linear_to_xyz(const float3 color)
            {
                return float3(
                    dot(color, float3(0.4124564, 0.3575761, 0.1804375)),
                    dot(color, float3(0.2126729, 0.7151522, 0.0721750)),
                    dot(color, float3(0.0193339, 0.1191920, 0.9503041))
                );
            }


            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float4 color = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.texcoord);
                float3 xyz = get_linear_to_xyz(color.rgb);
                return float4(xyz, color.a);
            }
            ENDCG

        }
    }
    Fallback Off
}