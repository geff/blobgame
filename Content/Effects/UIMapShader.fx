
float4x4 WorldViewProjection;
float4x4 Projection;

float2 Position;

texture Tex;

sampler TexSampler = 
sampler_state
{
    Texture = <Tex>;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
    
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
    float2 TexCoord		: TEXCOORD1;
    float3 PlayerColor	: COLOR1;
};

float ViewportHeight;
float Zoom;

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = mul(input.Position* float4(1,-1,1,1), WorldViewProjection);
    
    output.Size = input.Size;
    
    output.Color = input.Color;
    output.Polar = input.Polar;
    
    output.Selected = input.Selected;
    output.PlayerColor = input.PlayerColor;
    
    return output;
}

struct PixelShaderOutput
{
	float4 Color1 : COLOR0;
};

PixelShaderOutput PixelShaderFunction(VertexShaderOutput Input)
{
    PixelShaderOutput output = (PixelShaderOutput)0;
    
    float4 texMapColor = tex2D(TexSampler, Input.TexCoord);
    
	//--- InfoData
	float4 ColorData = float4(Input.Color.rgb,1);
	//float4 ColorData = texMapColor;
	
	/*
	if(Input.Selected>0)
	{
		InfoData.r = 1;
	}

	if(Input.Selected>0)
	{
		InfoData.g = 1;
	}
	*/
	
	//---
    
    output.Color1 = ColorData;
    
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
