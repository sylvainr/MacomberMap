struct VS_IN
{
	float4 pos : POSITION;
	float4 col : COLOR;
	float3 norm : NORMAL;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 col : COLOR;
};

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;

	output.pos = input.pos;
	//output.col = input.col;
	output.col.x = output.pos.z;
	output.col.y = output.pos.z;
	output.col.z = output.pos.z;
	output.col.w = 1.0;

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	return input.col;
}

technique10 Render
{
	pass P0
	{
		AlphaBlendEnable = TRUE;
		DestBlend = INVSRCALPHA;
		SrcBlend = SRCALPHA;
		SetGeometryShader(0);
		SetVertexShader(CompileShader(vs_4_0, VS()));
		SetPixelShader(CompileShader(ps_4_0, PS()));
	}
}