Shader "Nation/GlobeClouds"
{
    Properties
    {
        _CloudColor ("Cloud", Color) = (0.85, 0.9, 1.0, 1)
        _Coverage ("Coverage", Range(0, 1)) = 0.52
        _LightDir ("Light Direction", Vector) = (-0.55, 0.35, -0.75, 0)
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Cull Back
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _CloudColor;
            float _Coverage;
            float4 _LightDir;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
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

            float fbm(float2 p)
            {
                float v = 0.0;
                float a = 0.5;
                for (int k = 0; k < 4; k++)
                {
                    v += a * valueNoise(p);
                    p = p * 2.03 + float2(17.3, 9.1);
                    a *= 0.5;
                }
                return v;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 p = i.uv * float2(9.0, 4.5) + float2(_Time.y * 0.008, 0.0);
                float density = fbm(p);
                float alpha = smoothstep(_Coverage, _Coverage + 0.28, density);
                float ndl = dot(normalize(i.worldNormal), normalize(_LightDir.xyz));
                float lit = saturate(ndl * 0.8 + 0.2);
                float dayWeight = smoothstep(-0.2, 0.3, ndl);
                float3 color = _CloudColor.rgb * (0.25 + 0.75 * lit);
                return fixed4(color, alpha * (0.15 + 0.6 * dayWeight));
            }
            ENDCG
        }
    }
    Fallback "Sprites/Default"
}
