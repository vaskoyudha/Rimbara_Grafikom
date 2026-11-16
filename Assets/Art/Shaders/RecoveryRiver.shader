Shader "Rimbara/Recovery River"
{
    Properties
    {
        _BaseColor ("Water tint", Color) = (0.22, 0.25, 0.18, 1)
        _RecoveredTint ("Recovered tint", Color) = (0.16, 0.40, 0.42, 1)
        _Recovery ("Cleanup progress", Range(0,1)) = 0
        _FlowSpeed ("Flow speed", Float) = 0.32
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "River"
            Tags { "LightMode"="UniversalForward" }
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _RecoveredTint;
                float _Recovery;
                float _FlowSpeed;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float fog : TEXCOORD1; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.fog = ComputeFogFactor(output.positionCS.z);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float along = input.uv.y + _Time.y * _FlowSpeed;
                float cross = input.uv.x;
                float wave = sin(along * 9.0 + sin(cross * 14.0 + along * 2.0) * 0.65);
                float ripple = smoothstep(0.89, 0.99, wave) * (0.35 + 0.65 * sin(cross * 10.0 + along) * sin(cross * 10.0 + along));
                float edge = smoothstep(0.35, 0.5, abs(cross - 0.5));
                half3 tint = lerp(_BaseColor.rgb, _RecoveredTint.rgb, _Recovery * 0.65);
                half3 color = tint * (0.92 + sin(along * 2.0 + cross * 6.0) * 0.07);
                color += ripple * lerp(half3(0.10,0.10,0.075), half3(0.23,0.29,0.27), _Recovery);
                color = lerp(color, tint * 0.73, edge * 0.5);
                return half4(MixFog(color, input.fog), 1);
            }
            ENDHLSL
        }
    }
}
