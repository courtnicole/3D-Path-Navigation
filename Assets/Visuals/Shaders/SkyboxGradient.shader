Shader "Custom/SkyboxGradient"
{
    Properties {
        _MainColor ("Main Color", Color) = (1,1,1,1)
        _SecondaryColor ("Secondary Color", Color) = (0,0,0,1)
        _Center ("Center", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Radius", Range(0, 1)) = 0.5
    }

    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Opaque"}
        LOD 100

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _MainColor;
            float4 _SecondaryColor;
            float4 _Center;
            float _Radius;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float4 gradient = _SecondaryColor;
                float2 uv = i.uv - _Center.xy;
                float distance = length(uv);
                if (distance < _Radius) {
                    gradient = lerp(_SecondaryColor, _MainColor, distance / _Radius);
                }
                return gradient;
            }
            ENDCG
        }
    }
}
