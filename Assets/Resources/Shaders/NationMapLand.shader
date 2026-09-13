Shader "Nation/MapLand"
{
    Properties
    {
        _Dim ("Dim", Range(0, 1)) = 1
        _NoiseScale ("Noise Scale", Float) = 0.35
        _NoiseStrength ("Noise Strength", Range(0, 0.3)) = 0.07
        _EdgeLight ("Edge Light", Range(0, 0.5)) = 0.12
    }
    SubShader
    {
        Tags { "Queue" = "Geometry" "RenderType" = "Opaque" }
        Cull Off
        ZWrite On
        ZTest LEqual

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _Dim;
            float _NoiseScale;
            float _NoiseStrength;
            float _EdgeLight;

            struct appdata
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                fixed4 color : COLOR;
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
                float a = hash(i);
                float b = hash(i + float2(1, 0));
                float c = hash(i + float2(0, 1));
                float d = hash(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.world = mul(unity_ObjectToWorld, v.vertex).xy;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float n = valueNoise(i.world * _NoiseScale) * 0.65 + valueNoise(i.world * _NoiseScale * 3.1) * 0.35;
                float shade = 1.0 + (n - 0.5) * 2.0 * _NoiseStrength;
                float3 c = i.color.rgb * shade;
                c += _EdgeLight * 0.25 * saturate(i.world.y * 0.03 + 0.5) * i.color.rgb;
                c *= _Dim;
                return fixed4(c, 1.0);
            }
            ENDCG
        }
    }
    Fallback "Sprites/Default"
}
