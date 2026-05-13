Shader "RPG Creation Kit/Skybox Blended Cycle"
{
    Properties
    {
        [Header(Night Settings)]
        _TintNight ("Night Tint", Color) = (1,1,1,1)
        [NoScaleOffset] _SkyboxNight ("Night Skybox (Cube)", Cube) = "grey" {}

        [Header(Dusk Settings)]
        _TintDusk ("Dusk Tint", Color) = (1,1,1,1)
        [NoScaleOffset] _SkyboxDusk ("Dusk Skybox (Cube)", Cube) = "grey" {}

        [Header(Day Settings)]
        _TintDay ("Day Tint", Color) = (1,1,1,1)
        [NoScaleOffset] _SkyboxDay ("Day Skybox (Cube)", Cube) = "grey" {}

        [Header(Global Controls)]
        _Blend ("Time Cycle (0.3-0.7=Day Hold)", Range(0,1)) = 0.0
        _Exposure ("Exposure", Range(0.0, 8.0)) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            samplerCUBE _SkyboxNight, _SkyboxDusk, _SkyboxDay;
            half4 _SkyboxNight_HDR, _SkyboxDusk_HDR, _SkyboxDay_HDR;
            fixed4 _TintNight, _TintDusk, _TintDay;
            float _Blend, _Exposure;

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float3 dir : TEXCOORD0; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.dir = v.vertex.xyz;
                return o;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                float3 dir = normalize(IN.dir);
                float t = saturate(_Blend);
                half3 finalCol;

                half3 cNight = DecodeHDR(texCUBE(_SkyboxNight, dir), _SkyboxNight_HDR) * _TintNight.rgb;
                half3 cDusk  = DecodeHDR(texCUBE(_SkyboxDusk,  dir), _SkyboxDusk_HDR)  * _TintDusk.rgb;
                half3 cDay   = DecodeHDR(texCUBE(_SkyboxDay,   dir), _SkyboxDay_HDR)   * _TintDay.rgb;

                // 7-Stage Logic with Extended Day
                if (t <= 0.1) 
                {
                    finalCol = cNight;
                }
                else if (t <= 0.2) 
                {
                    // Night to Dusk (10% width)
                    float f = (t - 0.1) / 0.1;
                    finalCol = lerp(cNight, cDusk, f);
                }
                else if (t <= 0.3) 
                {
                    // Dusk to Day (10% width)
                    float f = (t - 0.2) / 0.1;
                    finalCol = lerp(cDusk, cDay, f);
                }
                else if (t <= 0.7) 
                {
                    // Extend day
                    finalCol = cDay;
                }
                else if (t <= 0.8) 
                {
                    // Day to Dusk (10% width)
                    float f = (t - 0.7) / 0.1;
                    finalCol = lerp(cDay, cDusk, f);
                }
                else if (t <= 0.9) 
                {
                    // Dusk to Night (10% width)
                    float f = (t - 0.8) / 0.1;
                    finalCol = lerp(cDusk, cNight, f);
                }
                else 
                {
                    finalCol = cNight;
                }

                return half4(finalCol * _Exposure, 1.0);
            }
            ENDCG
        }
    }
    Fallback "Skybox/Cubemap"
}