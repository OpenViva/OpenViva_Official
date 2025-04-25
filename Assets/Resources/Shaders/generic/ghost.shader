Shader "Custom/URPGhost"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Strength ("Strength", Range(0,0.2)) = 0.1
        _Spread ("Spread", Range(0,4)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float _Strength;
            float _Spread;


            float2 ghostUV(float2 uv, float offset, float scale)
            {
                float s1 = sin((uv.x + uv.y) * scale + offset);
                float s2 = sin(s1 + (uv.x - uv.y) * scale * 1.1);
                float s3 = sin(s2 + (uv.x + uv.y) * scale * 1.2);
                float c1 = sin(s3 + (uv.x - uv.y) * scale * 1.3);
                return uv + float2(s3, c1) * _Strength;
            }

            float4 ObjectToClipPos (float3 pos)
            {
                return mul (UNITY_MATRIX_VP, mul (UNITY_MATRIX_M, float4 (pos,1)));
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = ObjectToClipPos(IN.positionOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = 0;
                float time = _Time.y;

                color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, ghostUV(IN.uv, time, 4.0));
                color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, ghostUV(IN.uv, time + _Spread, 4.0)) * 0.2;
                color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, ghostUV(IN.uv, time + _Spread * 2.0, 4.0)) * 0.2;

                return half4(color.rgb, color.a);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}
