//-----------------------------------------------------------------------------
// SpriteEffect.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#include "Macros.fxh"


DECLARE_TEXTURE(Texture, 0);


BEGIN_CONSTANTS
MATRIX_CONSTANTS

    float4x4 MatrixTransform    _vs(c0) _cb(c0);

END_CONSTANTS


struct VSOutput
{
	float4 position		: SV_Position;
	float4 color		: COLOR0;
	float4 color1		: COLOR1;
    float2 texCoord		: TEXCOORD0;
	float2 texCoord1		: TEXCOORD1;
	float2 texCoord2		: TEXCOORD2;
	float2 texCoord3		: TEXCOORD3;
};

VSOutput SpriteVertexShader(	float4 position	: POSITION0,
								float4 color	: COLOR0,
								float4 color1		: COLOR1,
								float2 texCoord	: TEXCOORD0,
								float2 texCoord1	: TEXCOORD1,
								float2 texCoord2	: TEXCOORD2,
								float2 texCoord3	: TEXCOORD3)
{
	VSOutput output;
    output.position = mul(position, MatrixTransform);
	output.color = color;
	output.color1 = color1;
	output.texCoord = texCoord;
	output.texCoord1 = texCoord1;
	output.texCoord2 = texCoord2;
	output.texCoord3 = texCoord3;
	return output;
}


float4 SpritePixelShader(VSOutput input) : SV_Target0
{
    return SAMPLE_TEXTURE(Texture, input.texCoord) * input.color;
}

TECHNIQUE( SpriteBatch, SpriteVertexShader, SpritePixelShader );