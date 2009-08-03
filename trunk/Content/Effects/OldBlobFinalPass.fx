
texture NormalBuffer;
texture ColorBuffer;
texture CubeTex;

sampler Normalsampler = 
sampler_state
{
    Texture = <NormalBuffer>;
    MINFILTER = POINT;
    MAGFILTER = POINT;
    MIPFILTER = POINT;
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
};

sampler ColorBufferSampler = 
sampler_state
{
    Texture = <ColorBuffer>;
    MINFILTER = POINT;
    MAGFILTER = POINT;
    MIPFILTER = POINT;
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
};

sampler CubeMapSampler =
sampler_state
{
    Texture = <CubeTex>;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = input.Position;
    output.TexCoord = input.TexCoord;

    return output;
}

float3 LightDir = {-10.0f, 5.0f, .25f};
static const float THRESHOLD = 10.f;

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    static const float aaval = THRESHOLD * .07f;

    float4 blobdata = tex2D( Normalsampler, input.TexCoord);
    float4 color = tex2D( ColorBufferSampler, input.TexCoord);
    
    color /= blobdata.w;
    
    float3 surfacept = float3(blobdata.x/blobdata.w,
                              blobdata.y/blobdata.w,
                              blobdata.w-THRESHOLD);
                              
    float3 thenorm = normalize(-surfacept);
    thenorm.z = -thenorm.z;
    
    float I = dot(LightDir, thenorm);

    float4 Output;
    Output.rgb = color.rgb * .5f + texCUBE(CubeMapSampler, thenorm);
    Output.rgb *= saturate ((blobdata.a - THRESHOLD)/aaval);
    //Output.rgb *= I;  // Blob lighting
    Output.a = 1; //saturate ((blobdata.a - THRESHOLD)/aaval);
    
    return Output;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
