
float4x4 WorldViewProjection;
float4x4 Projection;

static const float GAUSSIANTEXSIZE = 64.0f;

texture GaussBlob;

sampler Blobsampler = 
sampler_state
{
    Texture = <GaussBlob>;
    MINFILTER = POINT;
    MAGFILTER = POINT;
    MIPFILTER = POINT;
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
};

struct VertexShaderInput
{
    float4 Position	: POSITION0;
    float3 Color	: COLOR;
};

struct VertexShaderOutput
{
	float4 Position	: POSITION0;
	float1 Size 		: PSIZE0;
    float3 Color		: COLOR0;
};

struct PixelShaderInput
{
	#ifdef XBOX // Not used in the sample
        float4 texCoord : SPRITETEXCOORD;
    #else
        float2 texCoord : TEXCOORD0;
    #endif
    float1 Size 		: PSIZE0;
    float3 Color		: COLOR0;
};

int ParticleSize;
int ViewportHeight;
float Size = 1.0f; // "Correct" value shoud be around 32, but I like this better ^^

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = mul(input.Position, WorldViewProjection);
    output.Size = ParticleSize * Projection._m11 / output.Position.w * ViewportHeight / 2;
    output.Color = input.Color;
    
    return output;
}

struct PixelShaderOutput
{
	float4 Color1 : COLOR0;	// NORMAL
	float4 Color2 : COLOR1;	// COLOR
};


void Bilinear( in float2 texCoord, out float4 outval )
{
	float2 pixelpos = GAUSSIANTEXSIZE * texCoord;
	float2 lerps = frac( pixelpos );
    float3 lerppos = float3((pixelpos-(lerps/GAUSSIANTEXSIZE))/GAUSSIANTEXSIZE,0);

    float4 samples[4];
    samples[0] = tex2D( Blobsampler, lerppos );  
    samples[1] = tex2D( Blobsampler, lerppos + float3(1.0/GAUSSIANTEXSIZE, 0,0) );
    samples[2] = tex2D( Blobsampler, lerppos + float3(0, 1.0/GAUSSIANTEXSIZE,0) );
    samples[3] = tex2D( Blobsampler, lerppos + float3(1.0/GAUSSIANTEXSIZE, 1.0/GAUSSIANTEXSIZE,0) );

    outval = lerp( lerp( samples[0], samples[1], lerps.x ),
                   lerp( samples[2], samples[3], lerps.x ),
                   lerps.y );
}

PixelShaderOutput PixelShaderFunction(PixelShaderInput Input)
{
    PixelShaderOutput output;
    
    float4 weight;
    
    #ifdef XBOX  // Not used in the sample
		float2 TexCoords = abs(Input.texCoord.zw);
	#else
		float2 TexCoords = Input.texCoord.xy;
	#endif
   
    Bilinear( TexCoords, weight );
    
    float4 NormalData = float4((TexCoords.x-0.5) * Size, 
                                (TexCoords.y-0.5) * Size,
                                0,
                                1);
    NormalData *= weight.r;
    
    float4 ColorData = float4(Input.Color.r,
                               Input.Color.g,
                               Input.Color.b,
                               0);
    ColorData *= weight.r;
    
    output.Color1 = NormalData;
    output.Color2 = ColorData;
    
    return output;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
