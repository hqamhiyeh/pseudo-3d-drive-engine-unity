Shader "Custom/Pseudo3D"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        [Header(Camera Position Properties)][Space]
        _CameraX ( "X", Float ) = 0.0
        _CameraY ( "Y", Float ) = 10.0
        _CameraZ ( "Z", Float ) = 0.0

        [Header(Camera Rotation Properties)][Space]
        _Pan  ( "Pan" , Range(-360.0, 360.0) ) = 0.0
        _Tilt ( "Tilt", Range(-360.0, 360.0) ) = 0.0
        _Roll ( "Roll", Range(-360.0, 360.0) ) = 0.0

        [Header(Projection Properties)][Space]
        _FovY    ( "FovY"   , Range( 1.0,  120.0) ) = 60.0
        _Aspect  ( "Aspect" , Range( 1.0,    2.0) ) = 1.0
        _Near    ( "Near"   , Range( 0.1,  100.0) ) = 0.3
        _Far     ( "Far"    , Range( 0.1, 1000.0) ) = 1000.0

        [Header(Viewport Properties)][Space]
        _Width  ( "Width" , Integer ) = 1920
        _Height ( "Height", Integer ) = 1080
        _PPU    ( "PPU"   , Integer ) = 100

        [Header(Texture Scale Properties)][Space]
        _TexScaleX ( "X", Range(-1.0,2.0) ) = 1.0
        _TexScaleY ( "Y", Range(-1.0,2.0) ) = 1.0
        _TexScaleZ ( "Z", Range(-1.0,2.0) ) = 1.0
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            // transformation matrices set via script
            float4x4 _View;
            float4x4 _Perspective;
            float4x4 _Viewport;

            v2f vert (appdata v)
            {
                float4x4 transform =
                    {
                        1.0, 0.0, 0.0, 0.0,
                        0.0, 1.0, 0.0, 0.0,
                        0.0, 0.0, 1.0, 0.0,
                        0.0, 0.0, 0.0, 1.0
                    };
                
                transform = mul(_View       , transform);
                transform = mul(_Perspective, transform);
                transform = mul(_Viewport   , transform);
                
                v.vertex = mul(transform,v.vertex);
                v.vertex = mul(UNITY_MATRIX_M,v.vertex);
                v.vertex = mul(UNITY_MATRIX_V,v.vertex);
                v.vertex = mul(UNITY_MATRIX_P,v.vertex);
                
                v2f o;
                o.vertex = v.vertex;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            // texture scale matrix values
            float _TexScaleX;
            float _TexScaleY;
            float _TexScaleZ;

            fixed4 frag (v2f i) : SV_Target
            {
                float3x3 texScale =
                    {
                        _TexScaleX, 0.0, 0.0,
                        0.0, _TexScaleY, 0.0,
                        0.0, 0.0, _TexScaleZ
                    };

                float3 uv = { i.uv.x - 0.5, i.uv.y - 0.5, 1.0 };
                uv = mul(texScale, uv);
                i.uv = (uv.xy / uv.z) + 0.5;

                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
