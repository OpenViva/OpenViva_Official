Shader "Viva/Effects/PlayerHurtOverlay"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Overlay ("Overlay Texture", 2D) = "white" {}
        _Alpha ("Hurt Alpha", Range(0., 1.0)) = 1.0
        _CloudsRT ("Cloud Render Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            Name "ForwardBase"
            
            Tags { "LightMode"="UniversalForward" }
            
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
            TEXTURE2D(_Overlay);
            TEXTURE2D(_CloudsRT);
            SAMPLER(sampler_MainTex);
            SAMPLER(sampler_Overlay);
            SAMPLER(sampler_CloudsRT);
            float _Alpha;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                col.rgb += SAMPLE_TEXTURE2D(_CloudsRT, sampler_CloudsRT, IN.uv).rgb * saturate(1. - col.a);
                half overlay = SAMPLE_TEXTURE2D(_Overlay, sampler_Overlay, IN.uv).r * _Alpha;
                col.bg -= overlay;
                col.r *= 1. - overlay;
                return half4(col.rgb, 1.);
            }
            ENDHLSL
        }
    }
}
