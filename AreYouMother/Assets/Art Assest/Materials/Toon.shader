Shader "Custom/Toon"
{
    Properties
    {
        _MainColor ("主颜色", Color) = (1, 1, 1, 1)
        _ShadowColor ("阴影颜色", Color) = (0, 0, 0, 0)
        _ShadowSharp ("阴影锐度", Range(0, 1)) = 5
        _ShadowOffset ("阴影偏移", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            float Sigmoid(float x, float k, float b)
            {
                return 1.0 / (1.0 + exp(-k * 250 * (x - b)));
            }
            
            CBUFFER_START(UnityPerMaterial)
                half4 _MainColor;
                half4 _ShadowColor;
                half _ShadowSharp;
                half _ShadowOffset;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 L = normalize(GetMainLight().direction);
                half3 N = normalize(IN.normalWS);
                float lambert = dot(L, N);
                float toon = Sigmoid(lambert, _ShadowSharp, _ShadowOffset);
                
                half3 finalColor = lerp(_ShadowColor.rgb * _MainColor.rgb, _MainColor.rgb, toon);
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
