#include "../noiseSimplex.cginc"

#ifndef GRASS_COMPUTE_FUNCTIONS_INCLUDED
#define GRASS_COMPUTE_FUNCTIONS_INCLUDED

void GetTriangleNormalAndTSToWSMatrix(float3 a, float3 b, float3 c, out float3 normalWS, out float3x3 tangentToWorld)
{
    // Calculate a basis for the tangent space
    // The tangent, or X direction, points from a to b
    float3 tangentWS = normalize(b - a);
    // The normal, or Z direction, is perpendicular to the lines formed by the triangle points
    normalWS = normalize(cross(tangentWS, c - a));
    // The bitangent, or Y direction, is perpendicular to the tangent and normal
    float3 bitangentWS = normalize(cross(tangentWS, normalWS));
    // Now we can construct a tangent -> world rotation matrix
    tangentToWorld = transpose(float3x3(tangentWS, bitangentWS, normalWS));
}

float3 GetTriangleCenter(float3 a, float3 b, float3 c)
{
    return (a + b + c) / 3.0;
}

float2 GetTriangleCenter(float2 a, float2 b, float2 c)
{
    return (a + b + c) / 3.0;
}

float rand(float4 value)
{
    float4 smallValue = sin(value);
    float random = dot(smallValue, float4(12.9898, 78.233, 37.719, 09.151));
    random = frac(sin(random) * 143758.5453);
    return random;
}

float rand(float3 pos, float offset)
{
    return rand(float4(pos, offset));
}

float randNegative1to1(float3 pos, float offset)
{
    return rand(pos, offset) * 2 - 1;
}

float3x3 AngleAxis3x3(float angle, float3 axis)
{
    float c, s;
    sincos(angle, s, c);

    float t = 1 - c;
    float x = axis.x;
    float y = axis.y;
    float z = axis.z;

    return float3x3
    (
        t * x * x + c, t * x * y - s * z, t * x * z + s * y,
        t * x * y + s * z, t * y * y + c, t * y * z - s * x,
        t * x * z - s * y, t * y * z + s * x, t * z * z + c
    );
}

float GrassNoise(float3 pos)
{
    return  ((snoise(normalize(pos) * 10.0) + 1.0) * 0.5);
}

void GrassNoise_float(float3 pos, out float output)
{
    output = GrassNoise(pos);
}

void CustomNoise(float3 position, float scale, out float output)
{
    output = snoise(position * scale);
}

void CustomNoise_float(float3 position, float scale, out float output)
{
    output = snoise(position * scale);
}

#endif
