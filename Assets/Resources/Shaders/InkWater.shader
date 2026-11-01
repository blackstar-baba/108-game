// Liangshan 108 - ink-wash lake surface with shore foam from scene depth.
Shader "Liangshan/InkWater"
{
    Properties
    {
        _DeepColor ("Deep Color", Color) = (0.075, 0.13, 0.15, 1)
        _ShallowColor ("Shallow Color", Color) = (0.20, 0.31, 0.32, 1)
        _SkyTint ("Sky Tint", Color) = (0.68, 0.58, 0.50, 1)
        _FoamColor ("Foam Color", Color) = (0.90, 0.84, 0.70, 1)
        _SunColor ("Sun Color", Color) = (1.0, 0.74, 0.45, 1)
        _WaveScale ("Wave Scale", Range(0.1, 4)) = 1.0
        _WaveStrength ("Wave Strength", Range(0, 0.4)) = 0.065
        _Opacity ("Opacity", Range(0, 1)) = 0.93
        _FoamWidth ("Foam Width", Range(0.1, 3)) = 0.80
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _DeepColor;
                half4 _ShallowColor;
                half4 _SkyTint;
                half4 _FoamColor;
                half4 _SunColor;
                half _WaveScale;
                half _WaveStrength;
                half _Opacity;
                half _FoamWidth;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 screenPos  : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float  fogFactor  : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.screenPos = ComputeScreenPos(positionInputs.positionCS);
                output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
                return output;
            }

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float valueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 viewDir = normalize(GetCameraPositionWS() - input.positionWS);
                float2 p = input.positionWS.xz;
                float t = _Time.y;

                float2 wave = float2(
                    sin(p.x * 0.52 * _WaveScale + t * 0.86) * 0.55 + sin((p.x + p.y) * 0.21 + t * 0.44) * 0.45,
                    sin(p.y * 0.43 * _WaveScale - t * 0.72) * 0.55 + sin((p.y - p.x) * 0.19 + t * 0.38) * 0.45
                ) * _WaveStrength;

                float3 normal = normalize(float3(wave.x, 1.0, wave.y));
                float fresnel = pow(saturate(1.0 - dot(normal, viewDir)), 3.0);

                float2 screenUV = input.screenPos.xy / max(input.screenPos.w, 0.0001);
                float rawDepth = SampleSceneDepth(screenUV);
                float sceneEye = LinearEyeDepth(rawDepth, _ZBufferParams);
                float waterEye = -TransformWorldToView(input.positionWS).z;
                float depthDiff = max(waterEye - sceneEye, 0.0);

                float movingInk = valueNoise(p * 0.16 + float2(t * 0.016, -t * 0.010));
                movingInk = smoothstep(0.42, 0.78, movingInk);

                half3 waterColor = lerp(_DeepColor.rgb, _ShallowColor.rgb, saturate(depthDiff * 0.07 + fresnel * 0.65));
                waterColor = lerp(waterColor, _SkyTint.rgb, fresnel * 0.55);
                waterColor += (movingInk - 0.5) * 0.035;

                Light mainLight = GetMainLight();
                float3 halfVector = normalize(mainLight.direction + viewDir);
                float specular = pow(saturate(dot(normal, halfVector)), 110.0);
                waterColor += _SunColor.rgb * specular * 1.3;

                float foamMask = 1.0 - smoothstep(0.0, _FoamWidth, depthDiff);
                float foamNoise = valueNoise(p * 1.3 + float2(t * 0.12, 0.0)) * 0.6
                                + valueNoise(p * 3.1 - float2(0.0, t * 0.17)) * 0.4;
                float foam = foamMask * smoothstep(0.38, 0.72, foamNoise + foamMask * 0.45);
                waterColor = lerp(waterColor, _FoamColor.rgb, foam * 0.82);

                float alpha = saturate(_Opacity + foam * 0.08);
                waterColor = MixFog(waterColor, input.fogFactor);
                return half4(waterColor, alpha);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
