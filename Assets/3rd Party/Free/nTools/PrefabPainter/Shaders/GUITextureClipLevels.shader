
Shader "Hidden/nTools/PrefabPainter/GUITextureClipLevels"
{
    Properties
    {
        _MainTex ("Texture", Any) = "white" {}
        _InvGamma ("InvGamma", Color) = (1, 1, 1, 1)
        _InBlack ("InBlack", Color) = (0, 0, 0, 0)
        _InWhite ("InWhite", Color) = (1, 1, 1, 1)
        _OutBlack ("InBlack", Color) = (0, 0, 0, 0)
        _OutWhite ("InWhite", Color) = (1, 1, 1, 1)
    }

    CGINCLUDE
    #pragma vertex vert
    #pragma fragment frag
    #pragma target 2.0

    #include "UnityCG.cginc"
    
    struct appdata_t {
        float4 vertex : POSITION;
        fixed4 color : COLOR;
        float2 texcoord : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct v2f {
        float4 vertex : SV_POSITION;
        fixed4 color : COLOR;
        float2 texcoord : TEXCOORD0;
        float2 clipUV : TEXCOORD1;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    sampler2D _MainTex;
    sampler2D _GUIClipTexture;

    uniform float4 _MainTex_ST;
    uniform fixed4 _Color;
    uniform float4x4 unity_GUIClipTextureMatrix;

    float4 _InvGamma;
    float4 _InBlack;
    float4 _InWhite;
    float4 _OutBlack;
    float4 _OutWhite;

    v2f vert (appdata_t v)
    {
        v2f o;
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        o.vertex = UnityObjectToClipPos(v.vertex);
        float3 eyePos = UnityObjectToViewPos(v.vertex);
        o.clipUV = mul(unity_GUIClipTextureMatrix, float4(eyePos.xy, 0, 1.0));
        o.color = v.color;
        o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
        return o;
    }

    fixed4 frag (v2f i) : SV_Target
    {
        fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;

        #if UNITY_COLORSPACE_GAMMA
        #else
        col.rgb = LinearToGammaSpace(col.rgb);
        #endif
        
        col = (col - _InBlack) / (_InWhite - _InBlack);
        col = pow(col, _InvGamma);
        col = col * (_OutWhite - _OutBlack) + _OutBlack;

        col.a *= tex2D(_GUIClipTexture, i.clipUV).a;
        return col;
    }
    ENDCG

    SubShader {

        Tags { "ForceSupported" = "True" }

        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha, One One
        Cull Off
        ZWrite Off
        ZTest Always

        Pass {
            CGPROGRAM
            ENDCG
        }
    }

    SubShader {

        Tags { "ForceSupported" = "True" }

        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest Always

        Pass {
            CGPROGRAM
            ENDCG
        }
    }
}
