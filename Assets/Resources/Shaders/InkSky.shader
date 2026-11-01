// Liangshan 108 - procedural dusk skybox with ink-wash ridges.
Shader "Liangshan/InkSky"
{
    Properties
    {
        _SunDir ("Sun Direction", Vector) = (-0.42, 0.20, -0.88, 0)
        _SunColor ("Sun Color", Color) = (1.0, 0.72, 0.42, 1)
        _HorizonColor ("Horizon Color", Color) = (0.94, 0.66, 0.47, 1)
        _ZenithColor ("Zenith Color", Color) = (0.20, 0.27, 0.38, 1)
        _CloudColor ("Cloud Color", Color) = (0.80, 0.62, 0.55, 1)
        _MountainFar ("Far Mountain", Color) = (0.33, 0.39, 0.47, 1)
        _MountainNear ("Near Mountain", Color) = (0.16, 0.20, 0.25, 1)
        _HazeColor ("Haze Color", Color) = (0.78, 0.60, 0.50, 1)
        _CloudStrength ("Cloud Strength", Range(0, 1)) = 0.45
        _MountainStrength ("Mountain Strength", Range(0, 1)) = 1.0
        _Exposure ("Exposure", Range(0.2, 2)) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" "RenderPipeline"="UniversalPipeline" }
        Cull Off
        ZWrite Off
        ZTest LEqual

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _SunDir;
                half4 _SunColor;
                half4 _HorizonColor;
                half4 _ZenithColor;
                half4 _CloudColor;
                half4 _MountainFar;
                half4 _MountainNear;
                half4 _HazeColor;
                half _CloudStrength;
                half _MountainStrength;
                half _Exposure;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 dir        : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.dir = input.positionOS.xyz;
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

            float fbm(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float2x2 rot = float2x2(0.80, 0.60, -0.60, 0.80);
                for (int i = 0; i < 5; i++)
                {
                    value += amplitude * valueNoise(p);
                    p = mul(rot, p) * 2.03 + 13.1;
                    amplitude *= 0.5;
                }
                return value;
            }

            float ridge(float angle, float seed, float frequency)
            {
                float a = sin(angle * frequency + seed);
                float b = sin(angle * (frequency * 2.0 + 1.0) + seed * 1.7);
                float c = sin(angle * (frequency * 4.0 + 3.0) + seed * 2.3);
                return 0.55 * a + 0.30 * b + 0.15 * c;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 dir = normalize(input.dir);
                float3 sunDir = normalize(_SunDir.xyz);
                float up = saturate(dir.y);
                float horizon = pow(1.0 - up, 5.0);

                // Painted dusk gradient.
                float3 color = lerp(_HorizonColor.rgb, _ZenithColor.rgb, pow(up, 0.55));
                color += _HorizonColor.rgb * horizon * 0.18;

                // Warm bloom around the setting sun.
                float sunDot = saturate(dot(dir, sunDir));
                float sunGlow = pow(sunDot, 7.0) * 0.55 + pow(sunDot, 48.0) * 0.7;
                color += _SunColor.rgb * sunGlow;
                float sunDisc = smoothstep(0.9993, 0.99975, sunDot);
                color = lerp(color, _SunColor.rgb * 2.4, sunDisc);

                // Long ink-wash cloud bands.
                float2 cloudUV = dir.xz / max(dir.y + 0.28, 0.18);
                cloudUV = cloudUV * 0.52 + float2(_Time.y * 0.004, 0.0);
                float clouds = fbm(cloudUV + fbm(cloudUV * 0.7) * 0.7);
                float band = smoothstep(0.50, 0.72, clouds) * smoothstep(0.005, 0.14, dir.y);
                float streak = smoothstep(0.55, 0.9, fbm(cloudUV * float2(0.32, 1.8) + 3.7));
                band *= 0.7 + 0.3 * streak;
                float3 cloudColor = lerp(_CloudColor.rgb, _SunColor.rgb, sunGlow * 0.55);
                color = lerp(color, cloudColor, band * _CloudStrength);

                // Two layers of far mountains. Each layer uses a different ridge line.
                float angle = atan2(dir.z, dir.x);
                float farRidge = 0.075 + 0.030 * ridge(angle, 1.4, 3.0) + 0.020 * ridge(angle, 4.1, 8.0);
                float nearRidge = 0.040 + 0.022 * ridge(angle, 2.2, 4.0) + 0.015 * ridge(angle, 7.7, 11.0);

                float farMask = smoothstep(farRidge + 0.004, farRidge - 0.004, dir.y);
                float nearMask = smoothstep(nearRidge + 0.003, nearRidge - 0.003, dir.y);

                float haze = pow(1.0 - up, 3.0);
                float3 farColor = lerp(_MountainFar.rgb, _HazeColor.rgb, 0.35 + haze * 0.35);
                float3 nearColor = lerp(_MountainNear.rgb, _HazeColor.rgb, 0.12 + haze * 0.22);
                color = lerp(color, farColor, farMask * _MountainStrength);
                color = lerp(color, nearColor, nearMask * _MountainStrength * 0.95);

                // Below the horizon fades into the misty water tone.
                color = lerp(color, _HazeColor.rgb * 0.45, 1.0 - smoothstep(-0.10, 0.02, dir.y));
                color *= _Exposure;
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
