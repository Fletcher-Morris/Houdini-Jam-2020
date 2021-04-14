Shader "Custom/GeoGrass" {
	Properties {
		_TipColor("Tip Colour", Color) = (1,1,1,1)
		_RootColor("Root Colour", Color) = (1,1,1,1)
		_Width("Width", Float) = 1
		_RandomWidth("Random Width", Float) = 1
		_Height("Height", Float) = 1
		_RandomHeight("Random Height", Float) = 1
		_BladeSegments("Blade Segments", Range(1,4)) = 2
		_WindStrength("Wind Strength", Float) = 0.1
		[Space]
		_TessellationUniform("Tessellation Uniform", Range(1, 16)) = 1
		[Space]
		_GrassBlades("Blades Per Vertex", Range(0,16)) = 4
		_MinimumGrassBlades("Minimum Blades Per Vertex", Range(0,8)) = 1
		_MinCameraDistance("Min camera distance", float) = 5
		_MaxCameraDistance("Max camera distance", float) = 50
		_TrueNormal("True Normal", Range(0,1)) = 0.5
	}

	SubShader {
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
		LOD 300

		Cull Off

		Pass {
			Name "Geometry Pass"
			Tags {"LightMode" = "UniversalForward"}

			ZWrite On

			HLSLPROGRAM
			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x gles
			#pragma target 4.5

			#pragma require geometry

			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			#pragma hull hull
			#pragma domain domain

			//	Recieve Main Light Shadows
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE

			//	Additional Lights & Shadows
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS

			// Soft Shadows
			#pragma multi_compile _ _SHADOWS_SOFT
			
			//	Other (Mixed lighting, baked lightmaps, fog)
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON


			// Includes

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			#include "grass_struct.hlsl"
			#include "CustomTessellation.hlsl"
			#include "grass.hlsl"

			// Fragment

			half4 frag(GeometryOutput input, bool vf : SV_ISFRONTFACE) : SV_Target {
				#if SHADOWS_SCREEN
					float4 clipPos = TransformWorldToHClip(input.positionWS);
					float4 shadowCoord = ComputeScreenPos(clipPos);
				#else
					float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
				#endif

				Light mainLight = GetMainLight(shadowCoord);
				half4 result = lerp(_RootColor, _TipColor, input.uv.y) * mainLight.shadowAttenuation;
				return result;
			}

			ENDHLSL
		}

		Pass {
			
			Name "Shadows Pass"
			Tags {"Lightmode" = "ShadowCaster"}

			ZWrite On
			ZTest LEqual

			HLSLPROGRAM

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x gles
			#pragma target 4.5

			#pragma shader_feature _ALPHATEST_ON
			#pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			#pragma require geometry

			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			#pragma hull hull
			#pragma domain domain
			#pragma multi_compile_shadowcaster

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl
			#include "grass_struct.hlsl"
			#include "CustomTessellation.hlsl"
			#include "grass.hlsl"

			float4 frag(GeometryOutput input) : SV_Target {
				UNITY_PASS_SHADOWCASTER(input);
			}

			ENDHLSL
		}
	}
}