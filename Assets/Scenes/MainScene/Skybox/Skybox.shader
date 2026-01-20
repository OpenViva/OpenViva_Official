// Credit: Ddemon26 on GitHub

Shader "Custom/Skybox"
{
    Properties
    {
        _Tex1("Cubemap 1", Cube) = "white" {}
        _Tex2("Cubemap 2", Cube) = "white" {}
        _Blend("Blend", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float3 texcoord : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            samplerCUBE _Tex1;
            samplerCUBE _Tex2;
            float       _Blend;

            v2f vert(appdata v)
            {
                v2f o;
                o.texcoord = v.vertex.xyz;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Sample both cubemaps directly using the vertex direction
                fixed4 colorTex1 = texCUBE(_Tex1, i.texcoord);
                fixed4 colorTex2 = texCUBE(_Tex2, i.texcoord);

                // Blend the two colors
                return lerp(colorTex1, colorTex2, _Blend);
            }
            ENDCG
        }
    }
}