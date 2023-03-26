Shader "Grass/GrassBlades" {
    Properties {
        _BaseColor("Base color", Color) = (0, 0.5, 0, 1)
        _TipColorMin("Tip color min", Color) = (0, 1, 0, 1)
        _TipColorMax("Tip color max", Color) = (0, 1, 0, 1)
		_ColorRdmInfluence("Color rdm influence",  Range(0.0, 1.0)) = 0.5
        _AlphaTex("Alpha Texture", 2D) = "white" {}
        [MaterialToggle] _DebugColors("Debug Colors", Float) = 0
    }

    SubShader {
        // UniversalPipeline needed to have this render in URP
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}

        // Forward Lit Pass
        Pass {

            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}
            ZWrite True
            Cull Off

            HLSLPROGRAM
            // Signal this shader requires a compute buffer
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 4.5
            #pragma multi_compile_instancing

            // Lighting and shadow keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT

            // Register our functions
            #pragma vertex Vertex
            #pragma fragment Fragment

            // Incude our logic file
            #include "GrassBlades.hlsl"

            ENDHLSL
        }
    }
}