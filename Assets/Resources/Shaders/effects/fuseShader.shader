Shader "Viva/Effects/Fuse"
{
    Properties
    {
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _Fuse ("Fuse", Range(0,1)) = 1.0
        _FuseGlowLength ("Fuse Glow Length", Range(1,16)) = 1.0
        _FuseGlow ("Fuse Glow", Range(0,1)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        LOD 200

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

            sampler2D _MainTex;
            float _Fuse;
            float _FuseGlowLength;
            float _FuseGlow;

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

            float4 frag(Varyings IN) : SV_Target
            {
                float4 color = tex2D(_MainTex, IN.uv);
                float glow = saturate(1.0 - (_Fuse - IN.uv.x) * _FuseGlowLength) * 2.0 * _FuseGlow;
                float3 emissionColor = glow * float3(1.0, 0.6, 0.3);
                clip(_Fuse - IN.uv.x);
                
                return float4(color.rgb, color.a); // Alpha channel is used as is from texture
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Transparent"
}
