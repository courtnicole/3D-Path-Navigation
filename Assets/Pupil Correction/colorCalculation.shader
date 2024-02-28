Shader "CustomRenderTexture/colorCalculation"
{
    Properties
    {
        _MainTex("InputTex", 2D) = "white" {}
    }

    SubShader
    {
        Blend One Zero

        Pass
        {
            Name "colorCalculation"

            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0

            sampler2D _MainTex;

            float4 frag(v2f_customrendertexture IN) : COLOR
            {
                const float2 uv = IN.localTexcoord.xy;
                float4 color = tex2D(_MainTex, uv);

                half4 color2 = float4(0, 0, 0, 0);

                half4 color_IN_CIE_SPACE = tex2D(_MainTex, uv);

                // DO A BUNCH OF COOL CONVERSION STUFF HERE. CONVERT
                //    the CIE back into RGB for example ... SKIPPED...

                //color = float4(R, G, B, 1);

                return color;
            }
            ENDCG
        }
    }
}