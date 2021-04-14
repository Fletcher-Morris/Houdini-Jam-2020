Shader "Custom/GeoGrass" {
		Properties {
			_Color("Colour", Color) = (1,1,1,1)
			_Color2("Colour2", Color) = (1,1,1,1)
			_Width("Width", Float) = 1
			_RandomWidth("Random Width", Float) = 1
			_Height("Height", Float) = 1
			_RandomHeight("Random Height", Float) = 1
			_WindStrength("Wind Strength", Float) = 0.1
			[Space]
			_TessellationUniform("Tessellation Uniform", Range(1, 64)) = 1
		}

		SubShader {
			Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
			LOD 300

			Cull Off

			Pass {
				Name "ForwardLit"
				Tags {"LightMode" = "UniversalForward"}

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

				float4 frag(GeometryOutput input) : SV_Target {
					#if SHADOWS_SCREEN
						float4 clipPos = TransformWorldToHClip(input.positionWS);
						float4 shadowCoord = ComputeScreenPos(clipPos);
					#else
						float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
					#endif

					Light mainLight = GetMainLight(shadowCoord);

					return lerp(_Color, _Color2, input.uv.y) * mainLight.shadowAttenuation;
				}

				ENDHLSL
			}

			Pass {
				
				Name "ShadowCaster"
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