float rand(float3 seed) {
	return frac(sin(dot(seed.xyz, float3(12.9898, 78.233, 53.539))) * 43758.5453);
}

// https://gist.github.com/keijiro/ee439d5e7388f3aafc5296005c8c3f33
float3x3 AngleAxis3x3(float angle, float3 axis) {
	float c, s;
	sincos(angle, s, c);

	float t = 1 - c;
	float x = axis.x;
	float y = axis.y;
	float z = axis.z;

	return float3x3(
	t * x * x + c, t * x * y - s * z, t * x * z + s * y,
	t * x * y + s * z, t * y * y + c, t * y * z - s * x,
	t * x * z - s * y, t * y * z + s * x, t * z * z + c
	);
}

float3 _LightDirection;

float4 GetShadowPositionHClip(float3 positionWS, float3 normalWS) {
	float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

	#if UNITY_REVERSED_Z
		positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
	#else
		positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
	#endif

	return positionCS;
}

float4 WorldToHClip(float3 positionWS, float3 normalWS) {
	#ifdef SHADOW
		return GetShadowPositionHClip(positionWS, normalWS);
	#else
		return TransformWorldToHClip(positionWS);
	#endif
}

// Variables
CBUFFER_START(UnityPerMaterial) // Required to be compatible with SRP Batcher
float4 _TipColor;
float4 _RootColor;
float _Width;
float _RandomWidth;
float _WindStrength;
float _Height;
float _RandomHeight;
uint _BladeSegments;
uint _GrassBlades = 4;
uint _MinimumGrassBlades = 1;
half _MaxCameraDistance = 50;
half _MinCameraDistance = 3;
half _TrueNormal = 0.5;
float4 DistortionbObjects[32];
CBUFFER_END

// Vertex, Geometry & Fragment Shaders

Varyings vert(Attributes input) {
	Varyings output = (Varyings)0;

	VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
	// Seems like GetVertexPositionInputs doesn't work with SRP Batcher inside geom function?
	// Had to move it here, in order to obtain positionWS and pass it through the Varyings output.

	output.positionOS = input.positionOS; //vertexInput.positionCS; //
	output.positionWS = vertexInput.positionWS;
	output.normal = input.normal;
	output.tangent = input.tangent;
	return output;
}

#define BLADE_SEGMENTS 2

GeometryOutput GenerateGrassVertex(float4 pos, float3 positionWS, float3 ws, float3 normalWS, float2 uv)
{
	GeometryOutput result;
	result.positionCS = WorldToHClip(positionWS, normalWS);

	return result;
}

half ObjectDistortion(float3 pos)
{
	half total = 0.1;
	for (int i = 0; i < 32; i++)
	{
		half d = distance(pos, DistortionbObjects[i].xyz);
		total += step(d,DistortionbObjects[i].w);
	}
	return clamp(1.0 - total,0.1,1.0);
}

float invLerp(float from, float to, float value)
{
	return (value - from) / (to - from);
}

[maxvertexcount(1024 / 9)]
void geom(uint primitiveID : SV_PrimitiveID, triangle Varyings input[3], inout TriangleStream<GeometryOutput> triStream) {
	GeometryOutput output = (GeometryOutput)0;

	float realCamDist = distance(_WorldSpaceCameraPos,mul(unity_ObjectToWorld, input[0].positionWS));
	float tooCloseCamDist = clamp(pow(realCamDist/_MinCameraDistance,6),0.1,1.0);
	float camDist = clamp(invLerp(_MinCameraDistance, _MaxCameraDistance, realCamDist), 0.0, 1.0);
	int grassBlades = ceil(lerp(_GrassBlades, _MinimumGrassBlades, camDist));

	for(uint blade = 0; blade < grassBlades; blade++)
	{
		// Construct World -> Tangent Matrix (for aligning grass with mesh normals)
		float3 normal = input[0].normal;
		float4 tangent = input[0].tangent;
		float3 binormal = cross(normal, tangent) * tangent.w;

		float3x3 tangentToLocal = float3x3
		(
		tangent.x, binormal.x, normal.x,
		tangent.y, binormal.y, normal.y,
		tangent.z, binormal.z, normal.z
		);

		float r1 = rand(mul(unity_ObjectToWorld, input[0].positionWS).xyz * (blade + 1));
		float r2 = rand(mul(unity_ObjectToWorld, input[1].positionWS).xyz * (blade + 1));
		float r3 = rand(mul(unity_ObjectToWorld, input[2].positionWS).xyz * (blade + 1));
		float3 positionWS = (1 - sqrt(r1)) * input[0].positionWS + (sqrt(r1) * (1 - r2)) * input[1].positionWS + (sqrt(r1) * r2) * input[2].positionWS;
		normal = normalize(lerp(normal, normalize(positionWS), _TrueNormal));
		r1 = r1 * 2.0 - 1.0;
		r2 = r2 * 2.0 - 1.0;
		r3 = r3 * 2.0 - 1.0;

		float r = rand(positionWS.xyz * (blade + 1));
		float3x3 randRotation = AngleAxis3x3(r * TWO_PI, float3(0, 0, 1));

		half heightFactor = _Height;
		heightFactor *- _RandomHeight * (rand(positionWS.yxz) - 0.5);
		heightFactor *= ObjectDistortion(positionWS);
		heightFactor *= tooCloseCamDist;

		// Wind (based on sin / cos, aka a circular motion, but strength of 0.1 * sine)
		float2 wind = float2(sin(_Time.y + positionWS.x * 0.5), cos(_Time.y + positionWS.z * 0.5)) * _WindStrength * sin(_Time.y + r);
		float3x3 windMatrix = AngleAxis3x3((wind * PI).y, normalize(float3(wind.x, wind.y, 0)));

		float3x3 transformMatrix = mul(tangentToLocal, randRotation);
		float3x3 transformMatrixWithWind = mul(mul(tangentToLocal, windMatrix), randRotation);

		float bend = rand(positionWS.xyz) - 0.5;
		float width = _Width + _RandomWidth * (rand(positionWS.zyx) - 0.5);

		float3 normalWS = mul(transformMatrix, float3(0, 1, 0)); //?

		if(heightFactor <= 0.005) continue;
		//	If grass is too short, skip to next blade.

		for(uint seg = 0; seg < _BladeSegments; seg++)
		{
			half lerpVal = seg / (half)_BladeSegments;
			half lerpedWidth = width * (1 - lerpVal);
			half lerpedHeight = heightFactor * lerpVal;
			half lerpedBend = bend * lerpVal;
			lerpedBend *= lerpedBend;

			float3 segPos = positionWS + (normal * lerpedHeight);

			output.positionWS = segPos + mul(transformMatrixWithWind, float3(lerpedWidth, bend, lerpedHeight));
			output.positionCS = WorldToHClip(output.positionWS, normalWS);
			output.uv = float2(0, lerpVal);
			triStream.Append(output);

			output.positionWS = segPos + mul(transformMatrixWithWind, float3(-lerpedWidth, bend, lerpedHeight));
			output.positionCS = WorldToHClip(output.positionWS, normalWS);
			output.uv = float2(0, lerpVal);
			triStream.Append(output);
		}
		
		// Final vertex at top of blade
		output.positionWS = positionWS + mul(transformMatrixWithWind, float3(0, bend, heightFactor));
		output.positionCS = WorldToHClip(output.positionWS, normalWS);
		output.uv = float2(0, 1);
		triStream.Append(output);

		triStream.RestartStrip();
	}
}