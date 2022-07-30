#ifndef GRASSBLADES_INCLUDED
#define GRASSBLADES_INCLUDED

// Include some helper functions
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "GrassGraphicFunctions.hlsl"
#include "GrassComputeFunctions.hlsl"

// This describes a vertex on the generated mesh
struct DrawVertex
{
    float3 positionWS; // The position in world space
    float height; // The height of this vertex on the grass blade
    float width;
};

// A triangle on the generated mesh
struct DrawTriangle
{
    float3 lightingNormalWS; // A normal, in world space, to use in the lighting algorithm
    DrawVertex vertices[3]; // The three points on the triangle
};

// A buffer containing the generated mesh
StructuredBuffer<DrawTriangle> _DrawTriangles;

struct VertexOutput
{
    float2 uv : TEXCOORD0; // The height of this vertex on the grass blade
    float3 positionWS : TEXCOORD1; // Position in world space
    float3 normalWS : TEXCOORD2; // Normal vector in world space
    float4 positionCS : SV_POSITION; // Position in clip space
};

// Properties
float4 _BaseColor;
float4 _TipColor;
float _ColorRdm;
sampler2D _AlphaTex;
float _DebugColors;

// Globals
float4 LIGHT_COLOR;
float4 SHADOW_COLOR;
float4 SKY_COLOR;
float4 HORIZON_COLOR;

// Vertex functions

DrawTriangle GetDrawTriangle(uint _vertexId)
{
    return _DrawTriangles[_vertexId / 3];
}
DrawVertex GetDrawVertex(DrawTriangle _tri, uint _vertexId)
{
    return _tri.vertices[_vertexId % 3];
}

VertexOutput Vertex(uint vertexID: SV_VertexID)
{
    VertexOutput output = (VertexOutput)0;
    DrawTriangle tri = GetDrawTriangle(vertexID);
    DrawVertex input = GetDrawVertex(tri, vertexID);

    output.positionWS = input.positionWS;
    output.normalWS = tri.lightingNormalWS;
    output.uv.x = input.width;
    output.uv.y = input.height;
    output.positionCS = TransformWorldToHClip(input.positionWS);

    return output;
}

// Fragment functions

float HardLightFloat( float s, float d )
{
	return (s < 0.5) ? 2.0 * s * d : 1.0 - 2.0 * (1.0 - s) * (1.0 - d);
}

float3 HardLight( float3 s, float3 d )
{
	float3 c;
	c.r = HardLightFloat(s.r,d.r);
	c.g = HardLightFloat(s.g,d.g);
	c.b = HardLightFloat(s.b,d.b);
	return c;
}

float4 Fragment(VertexOutput input) : SV_Target
{
    InputData lightingInput = (InputData)0;
    lightingInput.positionWS = input.positionWS;
    lightingInput.normalWS = input.normalWS;
    lightingInput.viewDirectionWS = GetViewDirectionFromPosition(input.positionWS);
    lightingInput.shadowCoord = CalculateShadowCoord(input.positionWS, input.positionCS);

    float clipVal = tex2D(_AlphaTex, input.uv).r;
    clip(clipVal - 0.1);

	float colRdm = rand(input.normalWS, 0) * _ColorRdm;

    float uvY = input.uv.y;
    float uvYClamped = clamp(0.0, 1.0, uvY);

    if (_DebugColors > 0)
    {
        return lerp(float4(0,0,0,0), float4(1,1,1,1), uvY);
    }

    float3 col = lerp(_BaseColor.rgb, _TipColor.rgb, uvY);
    col.r -= colRdm;
    col.g += colRdm;
    col.b -= colRdm;

    float3 lightBlend = HardLight(col, float3(LIGHT_COLOR.rgb));

    return float4(lightBlend, 1);
}

#endif
