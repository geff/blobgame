
float4x4 WorldViewProjection;
float4x4 Projection;

float2 Position;

texture MapPic;
texture MapPoint;

sampler MapPicSampler = 
sampler_state
{
    Texture = <MapPic>;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

sampler MapPointSampler = 
sampler_state
{
    Texture = <MapPoint>;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
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


PixelShaderOutput PixelShaderFunction()//VertexShaderOutput Input)
{
    PixelShaderOutput output = (PixelShaderOutput)0;
    
    float4 texMapPicColor = 0;//tex2D(MapPicSampler, Input.TexCoord);
    float4 texMapPointColor = 0;//tex2D(MapPointSampler, Input.TexCoord);
    
	float4 ColorData = float4(0,0,0, 1);
	
	if(texMapPicColor.w > 0)
	{
		if(texMapPointColor.w > 0)
		{
			ColorData = 0.5f * texMapPicColor + 0.5f * texMapPointColor;
		}
		else
		{
			ColorData = texMapPicColor;
		}
	}
    
    output.Color1 = ColorData;
    
    return output;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = null;//compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
