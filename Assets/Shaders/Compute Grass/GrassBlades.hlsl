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
    half height; // The height of this vertex on the grass blade
    half width;
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
    half2 uv : TEXCOORD0; // The height of this vertex on the grass blade
    float3 positionWS : TEXCOORD1; // Position in world space
    float3 normalWS : TEXCOORD2; // Normal vector in world space

    float4 positionCS : SV_POSITION; // Position in clip space
};

// Properties
half4 _BaseColor;
half4 _TipColor;
half _ColorRdm;
sampler2D _AlphaTex;

// Globals
half4 LIGHT_COLOR;
half4 SHADOW_COLOR;
half4 SKY_COLOR;
half4 HORIZON_COLOR;

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

half4 Fragment(VertexOutput input) : SV_Target
{
    InputData lightingInput = (InputData)0;
    lightingInput.positionWS = input.positionWS;
    lightingInput.normalWS = input.normalWS;
    lightingInput.viewDirectionWS = GetViewDirectionFromPosition(input.positionWS);
    lightingInput.shadowCoord = CalculateShadowCoord(input.positionWS, input.positionCS);

    half clipVal = tex2D(_AlphaTex, input.uv).r;
    clip(clipVal - 0.1);

	half colRdm = rand(input.normalWS, 0) * _ColorRdm;

    half3 col = lerp(_BaseColor.rgb, _TipColor.rgb, input.uv.y);
    col.r -= colRdm;
    col.g -= colRdm;
    col.b -= colRdm;

    float fres = Fresnel(lightingInput.normalWS, lightingInput.viewDirectionWS, 3);
    fres = clamp(0.5, 1, fres);
    half3 fresCol = lerp(col, col * fres, 0.1);


    half3 lightBlend = lerp(fresCol, fresCol * LIGHT_COLOR, 0.9);

    return half4(lightBlend, 1);
}

#endif
