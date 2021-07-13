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
float4 _BaseColor;
float4 _TipColor;
half _ColorRdm;
sampler2D _AlphaTex;

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

    float fres = Fresnel(lightingInput.normalWS, lightingInput.viewDirectionWS, 1.0);
    half3 fresCol = col;
    fresCol.r = fres;
    fresCol.g = fres;
    fresCol.b = fres;

	//col -= colRdm;

    return half4(UniversalFragmentBlinnPhong(lightingInput, col, 0.5, 0.25, 0, 1));
}

#endif
