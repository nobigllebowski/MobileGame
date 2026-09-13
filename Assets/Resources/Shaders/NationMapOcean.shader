Shader "Nation/MapOcean"
{
    Properties
    {
        _Color ("Ocean", Color) = (0.03, 0.05, 0.10, 1)
        _DeepColor ("Deep", Color) = (0.015, 0.027, 0.06, 1)
        _Extent ("Extent (w, h, cx, cy)", Vector) = (54, 26, 0, 0)
        _NoiseScale ("Noise Scale", Float) = 0.12
    }
    SubShader
    {
        Tags { "Queue" = "Geometry-10" "RenderType" = "Opaque" }
        Cull Off
        ZWrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            fixed4 _DeepColor;
            float4 _Extent;
            float _NoiseScale;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 world : TEXCOORD0;
            };

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float valueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(hash(i), hash(i + float2(1, 0)), f.x), lerp(hash(i + float2(0, 1)), hash(i + float2(1, 1)), f.x), f.y);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.world = mul(unity_ObjectToWorld, v.vertex).xy;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 rel = (i.world - _Extent.zw) / max(_Extent.xy * 0.5, 0.001);
                float radial = saturate(length(rel * float2(1.0, 1.15)));
                float n = valueNoise(i.world * _NoiseScale) * 0.6 + valueNoise(i.world * _NoiseScale * 2.7) * 0.4;
                float depth = saturate(radial * 0.75 + (n - 0.5) * 0.35);
                fixed3 c = lerp(_Color.rgb, _DeepColor.rgb, depth);
                c += (n - 0.5) * 0.012;
                return fixed4(c, 1.0);
            }
            ENDCG
        }
    }
    Fallback "Sprites/Default"
}
