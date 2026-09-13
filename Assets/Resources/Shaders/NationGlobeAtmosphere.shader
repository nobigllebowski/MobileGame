Shader "Nation/GlobeAtmosphere"
{
    Properties
    {
        _Color ("Glow", Color) = (0.3, 0.55, 1.0, 1)
        _Power ("Power", Range(1, 8)) = 3.5
        _Strength ("Strength", Range(0, 2)) = 0.9
    }
    SubShader
    {
        Tags { "Queue" = "Transparent+10" "RenderType" = "Transparent" }
        Cull Front
        ZWrite Off
        Blend One One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _Power;
            float _Strength;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float rim = saturate(dot(normalize(-i.worldNormal), normalize(i.viewDir)));
                float glow = pow(rim, _Power) * _Strength;
                return fixed4(_Color.rgb * glow, 1.0);
            }
            ENDCG
        }
    }
    Fallback "Sprites/Default"
}
