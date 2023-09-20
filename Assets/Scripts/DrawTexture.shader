Shader "DrawTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    	_Paint("Paint Texture", 2D) = "white" {}
        [HideInInspector] _CurrentPosition("Current Position", Vector) = (0,0,0,0)
         [HideInInspector] _CurrentRotation("Current Rotation", Range(0, 360)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _Paint;
            float4 _Paint_ST;

            float4 _Paint_TexelSize;
			float4 _MainTex_TexelSize;

            float _CurrentRotation;
            float4 _CurrentPosition;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed2 posWithRotation(fixed2 pos) {
				fixed rad = radians(_CurrentRotation);
				fixed _s = sin(rad);
				fixed _c = cos(rad);
				float2x2 mat = float2x2(_c, _s, -_s, _c);
				pos -= 0.5;
				pos = mul(pos, mat);
				pos += 0.5;
				return pos;
			}
            
            fixed4 frag (v2f i) : SV_Target
            {
            	fixed4 col = tex2D(_MainTex, i.uv);
                fixed scale = (1.0 * _Paint_TexelSize.z * _MainTex_TexelSize.x);
				//fixed2 pos = clamp( i.uv - (_MaskPos.xy - 0.5 * scale ) , 0, 1);
				fixed2 pos = i.uv - (_CurrentPosition.xy - 0.5 * scale);
				pos = posWithRotation(pos / scale);

				fixed4 _maskCol = fixed4(1,1,1,0);
				if(pos.x > 0 && pos.x < 1 && pos.y > 0 && pos.y < 1)
					_maskCol = tex2D(_Paint, pos) * float4(1,0,1,1);

				return lerp(col, float4(_maskCol.rgb,1.0), _maskCol.a);
            }
            ENDCG
        }
    }
}
