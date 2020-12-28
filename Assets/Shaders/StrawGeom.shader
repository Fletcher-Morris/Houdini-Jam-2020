Shader "Custom/Straw_Geometry_Shader"
{

	Properties
	{
		_GradientMap("Gradient map", 2D) = "white" {}
		_TipColorVariance("Tip Variance", Range(0.0,1.0)) = 0.0
		_TipColor("Tip Color", Color) = (0.2574063, 0.3773585, 0, 0)
		_RootColor("Root Color", Color) = (0.1116701, 0.245283, 0, 0)
		_PlacementTexture("Placement texture", 2D) = "white" {}
		_TerrainScale("Terrain scale", float) = 1
		_MinAltitude("Minimum Altitude", float) = 50
		_MaxAltitude("Maximum Altitude", float) = 200
		_AltitudeHeightFade("Altitude Height Fade", float) = 0.25
		_NoiseTexture("Noise texture", 2D) = "white" {}
		_WindTexture("Wind texture", 2D) = "white" {}
		_WindStrength("Wind strength", float) = 0
		_WindSpeed("Wind speed", float) = 0
		_GrassHeight("Grass height", float) = 0
		_GrassWidth("Grass width", Range(0.0, 1.0)) = 1.0
		_PositionRandomness("Position randomness", float) = 0
		_HeightRandomness("Height randomness", float) = 0
		_GrassBlades("Grass blades per triangle ", Range(0, 25)) = 25
		_MinimunGrassBlades("Minimum grass blades per triangle", Range(0, 25)) = 1
		_MidpointVertexLerp("Lerp position of middle vertex", Range(0, 1)) = 0.5
		_MinCameraDistance("Min camera distance", float) = 5
		_MaxCameraDistance("Max camera distance", float) = 50
		_MinNormal("Min Normal ", Range(0.0, 1.0)) = 0.5
		_TrueNormal("True Normal ", Range(0.0, 1.0)) = 0.5
		_TessellationUniform("Tessellation Uniform", Range(1, 4)) = 2
	}


		CGINCLUDE
		#include "CGIncludes/UnityCG.cginc"
		#include "CustomTessellation.cginc"
		#include "CGIncludes/AutoLight.cginc"
		#include "CGIncludes/UnityShadowLibrary.cginc"

		struct appdata
		{
			float4 vertex: POSITION;
		};

		struct v2g
		{
			float4 vertex: POSITION;
		};

		struct g2f
		{
			float2 uv: TEXCOORD0;
			float4 pos: SV_POSITION;
			float4 col: COLOR;
			unityShadowCoord4 _ShadowCoord : TEXCOORD1;
			float3 midpoint : POSITION1;
		};

		sampler2D _GradientMap;
		sampler2D _PlacementTexture;
		sampler2D _PlacementTexture_ST;
		float _TerrainScale;
		float _MinAltitude;
		float _MaxAltitude;
		float _AltitudeHeightFade;
		sampler2D _NoiseTexture;
		float4 _NoiseTexture_ST;
		sampler2D _WindTexture;
		float4 _WindTexture_ST;
		float _WindStrength;
		float _WindSpeed;
		float _GrassHeight;
		float _GrassWidth;
		float _PositionRandomness;
		float _HeightRandomness;
		float _GrassBlades;
		float _MinimunGrassBlades;
		float _MinCameraDistance;
		float _MaxCameraDistance;
		float _MinNormal;
		float _TrueNormal;
		float4 _RootColor;
		float4 _TipColor;
		float _TipColorVariance;
		float _MidpointVertexLerp;
		float4 DistortionbObjects[32];

		float random2(float2 st)
		{
			return frac(sin(dot(st.xy,float2(12.9898, 78.233))) * 43758.5453123);
		}
		float random3(float3 co)
		{
			return frac(sin( dot(co.xyz ,float3(12.9898,78.233,45.5432) )) * 43758.5453);
 		}


		g2f GetVertex(float4 pos, float2 uv, fixed4 col, float4 midpoint)
		{
			g2f o;
			o.pos = UnityObjectToClipPos(pos);
			o.midpoint = midpoint.xyz;
			o.col = col;
			o.uv = uv;
			o._ShadowCoord = ComputeScreenPos(pos);
			return o;
		}

		v2g vert(appdata v)
		{
			v2g o;
			o.vertex = v.vertex;
			return o;
		}

		float invLerp(float from, float to, float value)
		{
			return (value - from) / (to - from);
		}

		float ObjectDistortion(float3 pos)
		{
			float total = 0.1;
			for (int i = 0; i < 32; i++)
			{
				float d = distance(pos, DistortionbObjects[i].xyz);
				total += step(d,DistortionbObjects[i].w);
			}
			return clamp(1.0 - total,0.1,1.0);
		}

		[maxvertexcount(60)] void geom(triangle v2g input[3], inout TriangleStream < g2f > triStream)
		{
			float3 normal = normalize(cross(input[1].vertex - input[0].vertex, input[2].vertex - input[0].vertex));

			float realCamDist = distance(_WorldSpaceCameraPos,mul(unity_ObjectToWorld, input[0].vertex));
			float tooCloseCamDist = clamp(pow(realCamDist/_MinCameraDistance,6),0.1,1.0);
			float camDist = clamp(invLerp(_MinCameraDistance, _MaxCameraDistance, realCamDist), 0.0, 1.0);
			int grassBlades = ceil(lerp(_GrassBlades, _MinimunGrassBlades, camDist));

			for (uint i1 = 0; i1 < grassBlades; i1++)
			{
				float r1 = random2(mul(unity_ObjectToWorld, input[0].vertex).xy * (i1 + 1));
				float r2 = random2(mul(unity_ObjectToWorld, input[1].vertex).xy * (i1 + 1));
				float r3 = random2(mul(unity_ObjectToWorld, input[2].vertex).xy * (i1 + 1));
				float4 midpoint = (1 - sqrt(r1)) * input[0].vertex + (sqrt(r1) * (1 - r2)) * input[1].vertex + (sqrt(r1) * r2) * input[2].vertex;
				normal = normalize(lerp(normal, normalize(midpoint), _TrueNormal));
				r1 = r1 * 2.0 - 1.0;
				r2 = r2 * 2.0 - 1.0;
				r3 = r3 * 2.0 - 1.0;
				float4 worldPos = mul(unity_ObjectToWorld, midpoint);
				float2 windTex = tex2Dlod(_WindTexture, float4(worldPos.xz * _WindTexture_ST.xy + _Time.y * _WindSpeed, 0.0, 0.0)).xy;
				float2 wind = (windTex * 2.0 - 1.0) * _WindStrength;
				float useWidth = _GrassWidth / 1000;
				float noise = tex2Dlod(_NoiseTexture, float4(worldPos.xz * _NoiseTexture_ST.xy, 0.0, 0.0)).x;
				float d = 1.0 / _TerrainScale;
				float place = saturate(tex2Dlod(_PlacementTexture,float4(worldPos.x * d, worldPos.z * d, 0.0, 0.0)).r);
				float heightFactor = 1.0;
				heightFactor += _HeightRandomness*(r1+r2+r3)/3.0;
				heightFactor *= ObjectDistortion(worldPos.xyz);
				heightFactor *= place;
				heightFactor *= noise;
				heightFactor *= tooCloseCamDist;

				float worldHeight = length(worldPos);
				heightFactor *= step(_MinAltitude, worldHeight);
				heightFactor *= step(worldHeight, _MaxAltitude);
				float heightBlend = clamp(0,1,invLerp(_MinAltitude, _MinAltitude + abs(_AltitudeHeightFade), worldHeight));
				heightFactor *= heightBlend;


				heightFactor *= step(0.005, heightFactor);
				useWidth *= step(0.005, heightFactor);

				float4 pointA = midpoint + useWidth * normalize(input[i1 % 3].vertex - midpoint);
				float4 pointB = midpoint - useWidth * normalize(input[i1 % 3].vertex - midpoint);
				float4 pointC = midpoint + float4(normal, 0.0) * (heightFactor * _GrassHeight / 100) + float4(r1, r2, r3, 0.0) * _PositionRandomness + float4(wind.x, 0.0, wind.y, 0.0);
				float4 pointD = pointC + (pointB - pointA);
				triStream.Append(GetVertex(pointA, float2(0, 0), fixed4(0, 0, 0, 1),midpoint));
				triStream.Append(GetVertex(pointB, float2(1, 0), fixed4(0, 0, 0, 1),midpoint));
				triStream.Append(GetVertex(pointC, float2(0, 1), fixed4(1.0, length(windTex), 1.0, 1.0),midpoint));
				triStream.Append(GetVertex(pointD, float2(1, 1), fixed4(1.0, length(windTex), 1.0, 1.0),midpoint));

				triStream.RestartStrip();
			}

			triStream.RestartStrip();
		}

		

		ENDCG
		
	SubShader
	{
		Cull Off

		Pass
		{
			Tags
			{
				//"RenderType" = "Opaque"
				//"LightMode" = "ForwardBase"
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			#pragma hull hull
			#pragma domain domain
			#pragma target 4.6
			#pragma multi_compile_fwdbase

			fixed4 frag(g2f i): SV_Target
			{
				fixed4 gradientMapCol = tex2D(_GradientMap, float2(i.col.x, 0.0));
				fixed4 col = lerp(_RootColor, _TipColor, i.col.x);
				//col *= AmbientColor;
				//col = SHADOW_ATTENUATION(i);
				return col;
			}

			ENDCG
		}

		Pass
		{
			Tags
			{
				"LightMode" = "ShadowCaster"
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			#pragma hull hull
			#pragma domain domain
			#pragma target 4.6
			#pragma multi_compile_shadowcaster

			float4 frag(g2f i) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i)
			}

			ENDCG
		}
	}

	Fallback "Cuttout"
}