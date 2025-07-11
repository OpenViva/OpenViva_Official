//////////////////////////////////////////////////////
// Foliage Renderer
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

Texture2D _MainTex;
SamplerState sampler_MainTex;

Texture2D _CameraDepthTexture;
SamplerState sampler_CameraDepthTexture;

float4 _MainTex_TexelSize;

struct Input
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct V2P
{
    float4 vertex : SV_POSITION;
    float2 uv : TEXCOORD0;
};


V2P vert(in Input input)
{
    V2P output;

    output.vertex = UnityObjectToClipPos(input.vertex.xyz);
    output.uv = input.uv;

#if UNITY_UV_STARTS_AT_TOP
    if (_MainTex_TexelSize.y < 0)
        output.uv.y = 1. - input.uv.y;
#endif

    return output;
}

float4 Blit(in V2P input) : SV_Target
{
    return _CameraDepthTexture.Sample(sampler_CameraDepthTexture, input.uv).r * 2;
}

float4 Reduce(in V2P input) : SV_Target
{
    float4 r = _MainTex.GatherRed(sampler_MainTex, input.uv);
    float minimum = min(min(min(r.x, r.y), r.z), r.w);

    return float4(minimum, 1, 0, 0);
}

