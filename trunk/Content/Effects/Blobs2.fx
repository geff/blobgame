
float4x4 WorldViewProjection;
float4x4 Projection;

static const float GAUSSIANTEXSIZE = 256.0f;

texture GaussBlob;

sampler Blobsampler = 
sampler_state
{
    Texture = <GaussBlob>;
    MINFILTER = 0.0000001;
    MAGFILTER = 0.0000001;
    MIPFILTER = 0.0000001;
    
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
};

struct VertexShaderInput
{
    float4 Position			: POSITION0;
    float3 Color			: COLOR0;
    float1 Size				: PSIZE0;
    float1 Polar			: PSIZE1;
    float1 Selected			: PSIZE2;
    float3 PlayerColor		: COLOR1;
};

struct VertexShaderOutput
{
	float4 Position		: POSITION0;
	float1 Size 		: PSIZE0;
    float3 Color		: COLOR0;
    float1 Polar		: PSIZE1;
    float1 Selected		: PSIZE2;
    float2 texCoord		: TEXCOORD1;
    float3 PlayerColor	: COLOR1;
};

float ParticleSize;
float ViewportHeight;
float Zoom;

float Size = 30.0f; // "Correct" value shoud be around 32, but I like this better ^^
float3 invPosition = float3(1,-1,1);

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = mul(input.Position* float4(1,-1,1,1), WorldViewProjection);
    
    output.Size = (input.Size * ParticleSize * Projection._m11 / output.Position.w * ViewportHeight/2.0f)*0.6;
    
    output.Color = input.Color;
    output.Polar = input.Polar;
    
    //output.texCoord = input.Position.xy;
    output.Selected = input.Selected;
    output.PlayerColor = input.PlayerColor;
    
    return output;
}

struct PixelShaderOutput
{
	float4 Color1 : COLOR0;	// INFO
};

void Bilinear( in float2 texCoord, out float4 outval, in float1 polar )
{
	float2 pixelpos = GAUSSIANTEXSIZE * texCoord;
	float2 lerps = frac( pixelpos );
    float3 lerppos = float3((pixelpos-(lerps/GAUSSIANTEXSIZE))/GAUSSIANTEXSIZE,0);

    float4 samples[4];
    samples[0] = tex2D( Blobsampler, lerppos );  
    samples[1] = tex2D( Blobsampler, lerppos + float3(1.0f/GAUSSIANTEXSIZE, 0,0) );
    samples[2] = tex2D( Blobsampler, lerppos + float3(0, 1.0f/GAUSSIANTEXSIZE,0) );
    samples[3] = tex2D( Blobsampler, lerppos + float3(1.0f/GAUSSIANTEXSIZE, 1.0f/GAUSSIANTEXSIZE,0) );

    outval = lerp( lerp( samples[0], samples[1], lerps.x) * polar,
                   lerp( samples[2], samples[3], lerps.x) * polar,
                   lerps.y);
}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput Input)
{
    PixelShaderOutput output = (PixelShaderOutput)0;
    
    float4 weight;
	
	float2 TexCoords = Input.texCoord.xy;
   
    Bilinear( TexCoords, weight, Input.Polar);

	//--- InfoData
	float4 InfoData = float4(0,0,0,1);
	
	if(Input.Selected>0)
	{
		InfoData.r = 1;
	}

	if(Input.Selected>0)
	{
		InfoData.g = 1;
	}
	
	InfoData.r *= weight;
	InfoData.g *= weight*0.01;
	//---
    
    output.Color1 = InfoData;
    
    return output;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
