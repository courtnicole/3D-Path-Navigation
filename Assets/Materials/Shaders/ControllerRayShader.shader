Shader "Interaction/ControllerRayShader"
{
    Properties
    {
        _Color0 ("Color0", Color) = (1,1,1,1)
        _Color1 ("Color1", Color) = (1,1,1,0)
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent+15" "IgnoreProjector"="True" "RenderType"="Transparent"
        }
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

                #if UNITY_VERSION >= 560
				UNITY_VERTEX_INPUT_INSTANCE_ID
                #endif
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;

                #if UNITY_VERSION >= 560
				UNITY_VERTEX_OUTPUT_STEREO
                #endif
            };

            v2f vert(appdata v) 
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.vertex.zz * 2.0f;
                return o;
            }

            uniform fixed4 _Color0;
            uniform fixed4 _Color1;

            fixed4 frag(v2f i) : SV_Target 
            {
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( i );

                fixed4 col = lerp(_Color0, _Color1, clamp(i.uv.x, 0.0f, 1.0f) * 1.5f);
                return col;
            }
            ENDCG
        }
    }
}
