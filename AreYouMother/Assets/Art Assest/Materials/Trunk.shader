Shader "Custom/URP/TrunkToonShadow"
{
    Properties
    {
        _BaseMap("Bark Texture", 2D) = "white" {}

        _BaseColor("Light Color", Color) = (0.45, 0.28, 0.12, 1)
        _ShadowColor("Shadow Color", Color) = (0.18, 0.09, 0.035, 1)

        _ShadowThreshold("Toon Shadow Threshold", Range(0, 1)) = 0.45
        _ShadowSoftness("Toon Shadow Softness", Range(0, 0.2)) = 0.02

        _ReceiveShadowThreshold("Receive Shadow Threshold", Range(0, 1)) = 0.5
        _ReceiveShadowSoftness("Receive Shadow Softness", Range(0, 0.2)) = 0.02
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Cull Back
        ZWrite On

        HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;

            float4 _BaseColor;
            float4 _ShadowColor;

            float _ShadowThreshold;
            float _ShadowSoftness;

            float _ReceiveShadowThreshold;
            float _ReceiveShadowSoftness;
        CBUFFER_END

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS   : NORMAL;
            float2 uv         : TEXCOORD0;
        };

        struct Varyings
        {
            float4 positionHCS : SV_POSITION;
            float2 uv          : TEXCOORD0;
            float3 normalWS    : TEXCOORD1;
            float4 shadowCoord : TEXCOORD2;
        };

        Varyings Vert(Attributes input)
        {
            Varyings output;

            float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);

            output.positionHCS = TransformWorldToHClip(positionWS);
            output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
            output.normalWS = TransformObjectToWorldNormal(input.normalOS);
            output.shadowCoord = TransformWorldToShadowCoord(positionWS);

            return output;
        }

        half4 Frag(Varyings input) : SV_Target
        {
            half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);

            half3 normalWS = normalize(input.normalWS);

            Light mainLight = GetMainLight(input.shadowCoord);

            // 树干是实体，不用 abs
            // 背光面应该进入暗部
            half NdotL = saturate(dot(normalWS, mainLight.direction));

            // 卡通明暗分界
            half lightArea = smoothstep(
                _ShadowThreshold - _ShadowSoftness,
                _ShadowThreshold + _ShadowSoftness,
                NdotL
            );

            // 接收阴影也做成卡通硬边
            half receiveShadow = smoothstep(
                _ReceiveShadowThreshold - _ReceiveShadowSoftness,
                _ReceiveShadowThreshold + _ReceiveShadowSoftness,
                mainLight.shadowAttenuation
            );

            half toonArea = lightArea * receiveShadow;

            half3 lightColor = tex.rgb * _BaseColor.rgb;
            half3 shadowColor = tex.rgb * _ShadowColor.rgb;

            half3 finalColor = lerp(
                shadowColor,
                lightColor,
                toonArea
            );

            return half4(finalColor, 1);
        }

        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM

            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;

            struct ShadowVaryings
            {
                float4 positionHCS : SV_POSITION;
            };

            ShadowVaryings ShadowVert(Attributes input)
            {
                ShadowVaryings output;

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                float3 biasedPositionWS = ApplyShadowBias(
                    positionWS,
                    normalWS,
                    _LightDirection
                );

                output.positionHCS = TransformWorldToHClip(biasedPositionWS);

                return output;
            }

            half4 ShadowFrag(ShadowVaryings input) : SV_Target
            {
                return 0;
            }

            ENDHLSL
        }
    }
}