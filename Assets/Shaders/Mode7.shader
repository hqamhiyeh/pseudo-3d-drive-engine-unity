Shader "Custom/Mode7"
{
    Properties
    {
        [Header(Texture Properties)][Space]
        _MainTex ("Texture", 2D) = "white" {}
        
        [Header(Translate 1 Properties)][Space]
        _Translate1X ("X", Range(-1.0,1.0)) = 0.0
        _Translate1Y ("Y", Range(-1.0,1.0)) = 0.0
        _Translate1Z ("Z", Range(-1.0,1.0)) = 1.0

        [Header(Yaw Properties)][Space]
        _YawAngle ("Angle", Range(-3.14, 3.14)) = 0.0

        [Header(Pitch Properties)][Space]
        _Pitch ("Pitch", Range(-10.0,10.0)) = 0.0

        [Header(Roll Properties)][Space]
        _Roll ("Roll", Range(-10.0,10.0)) = 0.0

        [Header(Translate 2 Properties)][Space]
        _Translate2X ("X", Range(-1.0,1.0)) = 0.0
        _Translate2Y ("Y", Range(-1.0,1.0)) = 0.0
        _Translate2Z ("Z", Range(-1.0,1.0)) = 1.0

        [Header(Scale Properties)][Space]
        _ScaleX ("X", Range(-1.0,1.0)) = 1.0
        _ScaleY ("Y", Range(-1.0,1.0)) = 1.0
        _ScaleZ ("Z", Range(-1.0,1.0)) = 1.0
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
       
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            sampler2D _MainTex;
            float _Translate1X;
            float _Translate1Y;
            float _Translate1Z;
            float _YawAngle;
            float _Pitch;
            float _Roll;
            float _Translate2X;
            float _Translate2Y;
            float _Translate2Z;
            float _ScaleX;
            float _ScaleY;
            float _ScaleZ;

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
 
            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord.xy;
                return o;
            }
 
            fixed4 frag(v2f i) : SV_Target
            {
                float3 _Translate1 = { _Translate1X, _Translate1Y, _Translate1Z };
                float3 _Translate2 = { _Translate2X, _Translate2Y, _Translate2Z };
                float3 _Scale = { _ScaleX, _ScaleY, _ScaleZ };
                
                float3x3 translate1 = {
                    1.0, 0.0, _Translate1.x,
                    0.0, 1.0, _Translate1.y,
                    0.0, 0.0, _Translate1.z
                };

                float3x3 yaw = {
                    cos(_YawAngle), sin(_YawAngle), 0.0,
                    -sin(_YawAngle), cos(_YawAngle), 0.0,
                    0.0, 0.0, 1.0
                };

                float3x3 pitch = {
                    1.0, 0.0, 0.0,
                    0.0, 1.0, 0.0,
                    _Roll, _Pitch, 1.0
                };

                float3x3 roll = {
                    1.0, 0.0, 0.0,
                    0.0, 1.0, 0.0,
                    0, 0.0, 1.0
                };

                float3x3 translate2 = {
                    1.0, 0.0, _Translate2.x,
                    0.0, 1.0, _Translate2.y,
                    0.0, 0.0, _Translate2.z
                };

                float3x3 scale = {
                    _Scale.x, 0.0, 0.0,
                    0.0, _Scale.y, 0.0,
                    0.0, 0.0, _Scale.z
                };

                float3x3 transform;
                transform = translate1;
                transform = mul(yaw, transform);
                transform = mul(pitch, transform);
                transform = mul(roll, transform);
                transform = mul(translate2, transform);
                transform = mul(scale, transform);

                float3 uv = { (i.uv.x - 0.5), (i.uv.y - 0.5), 1.0 };
                uv = mul(transform,uv);
                
                if(uv.z < 0.0) discard;

                // Texel sampling
                return tex2D(_MainTex, (uv.xy / uv.z) + 0.5);
            }
            ENDCG
        }
    }
}