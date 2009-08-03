
texture NormalBuffer;
texture ColorBuffer;
texture PlayerBuffer;
texture UnitBuffer;
texture InfoBuffer;

texture CubeTex;


sampler InfoSampler = 
sampler_state
{
    Texture = <InfoBuffer>;
    MINFILTER = 0.0000001;
    MAGFILTER = 0.0000001;
    MIPFILTER = 0.0000001;
};

sampler UnitSampler = 
sampler_state
{
    Texture = <UnitBuffer>;
    MINFILTER = 0.0000001;
    MAGFILTER = 0.0000001;
    MIPFILTER = 0.0000001;
};

sampler PlayerSampler = 
sampler_state
{
    Texture = <PlayerBuffer>;
    MINFILTER = 0.0000001;
    MAGFILTER = 0.0000001;
    MIPFILTER = 0.0000001;
};

sampler Normalsampler = 
sampler_state
{
    Texture = <NormalBuffer>;
    /*
    MINFILTER = 0.0000001;
    MAGFILTER = 0.0000001;
    MIPFILTER = 0.0000001;
    //ADDRESSU = CLAMP;
    //ADDRESSV = CLAMP;
    */

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

float Zoom;

struct VertexShaderInput
{
    float4 Position			: POSITION0;
    float2 TexCoord         : TEXCOORD0;
    float3 Color			: COLOR0;
    float1 Size				: PSIZE0;
    float1 Polar			: PSIZE1;
    float1 Selected			: PSIZE2;
    float3 PlayerColor		: COLOR1;
};

struct VertexShaderOutput
{
    float4 Position		: POSITION0;
    float2 TexCoord		: TEXCOORD0;
    float3 PlayerColor	: COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = input.Position;
    output.TexCoord = input.TexCoord;
	output.PlayerColor = input.PlayerColor;
	
    return output;
}

//float3 LightDir = {-10.0f, 5.0f, .25f};
float3 LightDir = {0.0f, -2.0f, .25f};
static const float THRESHOLD = 150.f;

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    static const float aaval = THRESHOLD * .07f;

    float4 blobdata = tex2D(Normalsampler, input.TexCoord);
    float4 color = tex2D(ColorBufferSampler, input.TexCoord);
    float4 playerColor = tex2D(PlayerSampler, input.TexCoord);
    float4 unitColor = tex2D(UnitSampler, input.TexCoord);
    float4 infoColor = tex2D(InfoSampler, input.TexCoord);
    
    color /= blobdata.w;
    
    float3 surfacept = float3(blobdata.x/blobdata.w,
                              blobdata.y/blobdata.w,
                              blobdata.w-THRESHOLD);
                              
    float3 thenorm = normalize(-surfacept);
    thenorm.z = -thenorm.z;
    
    //------------
    float3 surfacept2 = float3(unitColor.x/unitColor.w,
                              unitColor.y/unitColor.w,
                              unitColor.w-THRESHOLD);
                              
    float3 thenorm2 = normalize(-surfacept2);
    thenorm2.z = -thenorm2.z/5;
    
    //thenorm2.x *= 5;
    //------------
    
    //float I = dot(LightDir, thenorm);
 
    float4 Output;
    
    float dotProduct = dot(float3(0,0,1), thenorm);
    
    float thresholdColorPlayer = max(90-Zoom, 5);
    float thresholdSelection = max(20-Zoom*2, 4);


	if (blobdata.w > thresholdColorPlayer && blobdata.w < 110 && dotProduct < 0.97f)
	{
		Output.rgb = float4(playerColor.r/3,playerColor.g/3,playerColor.b/3,1);
	}
	else if(blobdata.w > thresholdSelection && blobdata.w < 155 && dotProduct<0.97f && infoColor.r > 0.5)
	{
		float colorSelection = blobdata.w / 50.0f;
		
		Output.rgb = float4((float3)colorSelection,0);// + texCUBE(CubeMapSampler, thenorm);
	}
	else if(infoColor.g > 0.8f)
	{
		Output.rgb = color.rgb*0.5f + texCUBE(CubeMapSampler, thenorm*0.9f+thenorm2*0.1f);
		Output.rgb *= saturate ((blobdata.a - THRESHOLD)/aaval);
	}
	else
	{
		Output.rgb = color.rgb*0.5f + texCUBE(CubeMapSampler, thenorm);
		Output.rgb *= saturate ((blobdata.a - THRESHOLD)/aaval);
	}
	
	//if (playerColor.w > 250)
	if(dotProduct > 0.7f && dotProduct < 0.9999f)
	//if(playerColor.w > 50 && playerColor.w < 150)
	//if(playerColor.w == 1)
	{
		//Output.rgb = float4(color.r*2, color.g*2, color.b*2,1)* .5f + texCUBE(CubeMapSampler, thenorm);
		//Output.rgb *= saturate ((blobdata.a - THRESHOLD)/aaval);
		
		//Output.rgb = float4(1,1,0,1);
	}


	//Output.rgb *= I;  // Blob lighting
	Output.a = 1;
    //Output.a = 1*blobdata.w; //saturate ((blobdata.a - THRESHOLD)/aaval);
	
    return Output;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
