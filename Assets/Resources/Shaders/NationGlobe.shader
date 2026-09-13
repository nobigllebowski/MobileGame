Shader "Nation/Globe"
{
    Properties
    {
        _MainTex ("Earth Mask (R land, G coast, B lights)", 2D) = "black" {}
        _OceanColor ("Ocean", Color) = (0.04, 0.08, 0.19, 1)
        _LandColor ("Land", Color) = (0.23, 0.31, 0.46, 1)
        _NightColor ("Night", Color) = (0.01, 0.02, 0.06, 1)
        _LightsColor ("City Lights", Color) = (0.95, 0.76, 0.48, 1)
        _AtmosphereColor ("Atmosphere", Color) = (0.23, 0.51, 0.96, 1)
        _LightDir ("Light Direction", Vector) = (-0.55, 0.35, -0.75, 0)
        _Tier ("Quality Tier", Float) = 2
    }
    SubShader
    {
        Tags { "Queue" = "Geometry" "RenderType" = "Opaque" }
        Cull Back
        ZWrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _OceanColor;
            fixed4 _LandColor;
            fixed4 _NightColor;
            fixed4 _LightsColor;
            fixed4 _AtmosphereColor;
            float4 _LightDir;
            float _Tier;

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
                float3 viewDir : TEXCOORD2;
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
                o.uv = v.uv;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 mask = tex2D(_MainTex, i.uv);
                float land = mask.r;
                float coast = mask.g;
                float lights = mask.b;

                float3 base = lerp(_OceanColor.rgb, _LandColor.rgb, land);
                if (_Tier >= 1.0)
                {
                    float n = valueNoise(i.uv * float2(180.0, 90.0)) * 0.6 + valueNoise(i.uv * float2(520.0, 260.0)) * 0.4;
                    base *= 1.0 + (n - 0.5) * 0.18 * land;
                    base += coast * float3(0.05, 0.09, 0.16);
                }

                float3 lightDir = normalize(_LightDir.xyz);
                float ndl = dot(normalize(i.worldNormal), lightDir);
                float lit = saturate(ndl * 0.85 + 0.15);
                float3 day = base * (0.35 + 0.75 * lit);
                float3 night = lerp(_NightColor.rgb, base * 0.18, land);
                float dayWeight = smoothstep(-0.15, 0.25, ndl);
                float3 color = lerp(night, day, dayWeight);

                if (_Tier >= 2.0)
                {
                    float sparkle = step(0.93, valueNoise(i.uv * float2(2400.0, 1200.0))) * land;
                    float glow = saturate(lights * 1.4 + sparkle * 0.35);
                    color += _LightsColor.rgb * glow * (1.0 - dayWeight) * 1.1;
                }

                if (_Tier >= 1.0)
                {
                    float fresnel = pow(1.0 - saturate(dot(normalize(i.worldNormal), normalize(i.viewDir))), 3.0);
                    color += _AtmosphereColor.rgb * fresnel * (0.35 + 0.35 * dayWeight);
                }

                return fixed4(color, 1.0);
            }
            ENDCG
        }
    }
    Fallback "Unlit/Texture"
}
