// Liangshan 108 - hand-painted toon shader (URP, Forward)
// A quiet three-band ramp, warm rim light, and inverted-hull ink outline.
Shader "Liangshan/InkToon"
{
    Properties
    {
        [MainColor] _BaseColor ("Base Color", Color) = (0.78, 0.76, 0.70, 1)
        _ShadowColor ("Shadow Tint", Color) = (0.20, 0.24, 0.32, 1)
        _RimColor ("Rim Color", Color) = (1.0, 0.86, 0.62, 1)
        _RimPower ("Rim Power", Range(0.5, 10)) = 3.2
        _RimStrength ("Rim Strength", Range(0, 2)) = 0.42
        _LightStrength ("Light Strength", Range(0, 3)) = 1.0
        _AmbientStrength ("Ambient Strength", Range(0, 3)) = 0.8
        _RampSoft ("Ramp Softness", Range(0.001, 0.6)) = 0.12
        _VertexColor ("Use Vertex Color", Range(0, 1)) = 0
        _Emission ("Emission", Color) = (0, 0, 0, 0)
        _OutlineColor ("Outline Color", Color) = (0.045, 0.05, 0.055, 1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            Cull [_Cull]
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _ShadowColor;
                half4 _RimColor;
                half4 _Emission;
                half4 _OutlineColor;
                half _OutlineWidth;
                half _RimPower;
                half _RimStrength;
                half _LightStrength;
                half _AmbientStrength;
                half _RampSoft;
                half _VertexColor;
                half _Cull;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 color      : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float3 normalWS    : TEXCOORD0;
                float3 positionWS  : TEXCOORD1;
                float4 color       : COLOR;
                float4 shadowCoord : TEXCOORD2;
                float  fogFactor   : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                output.color = input.color;
                output.shadowCoord = GetShadowCoord(positionInputs);
                output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(GetCameraPositionWS() - input.positionWS);

                Light mainLight = GetMainLight(input.shadowCoord);
                float ndotl = dot(normalWS, mainLight.direction);
                float wrapped = saturate(ndotl * 0.5 + 0.5);
                float ramp = smoothstep(0.5 - _RampSoft, 0.5 + _RampSoft, wrapped);
                float shadow = lerp(1.0, mainLight.shadowAttenuation, 0.72);

                half3 baseColor = _BaseColor.rgb;
                baseColor = lerp(baseColor, baseColor * input.color.rgb, _VertexColor);

                half3 shadowTone = baseColor * _ShadowColor.rgb;
                half3 litTone = baseColor;
                half3 albedo = lerp(shadowTone, litTone, ramp);

                half3 ambient = SampleSH(normalWS) * _AmbientStrength;
                half3 direct = mainLight.color * (_LightStrength * shadow * saturate(ndotl * 0.82 + 0.18));
                half3 color = albedo * (ambient + direct);

                #if defined(_ADDITIONAL_LIGHTS)
                    uint lightCount = GetAdditionalLightsCount();
                    for (uint i = 0; i < lightCount; ++i)
                    {
                        Light addLight = GetAdditionalLight(i, input.positionWS);
                        float addNdotL = saturate(dot(normalWS, addLight.direction));
                        color += albedo * addLight.color * (addNdotL * addLight.distanceAttenuation * addLight.shadowAttenuation * 0.55);
                    }
                #endif

                half fresnel = pow(saturate(1.0 - dot(normalWS, viewDirWS)), _RimPower);
                half rim = fresnel * _RimStrength * (0.35 + 0.65 * ramp);
                color += _RimColor.rgb * rim;
                color += _Emission.rgb;

                color = MixFog(color, input.fogFactor);
                return half4(color, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Front
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _ShadowColor;
                half4 _RimColor;
                half4 _Emission;
                half4 _OutlineColor;
                half _OutlineWidth;
                half _RimPower;
                half _RimStrength;
                half _LightStrength;
                half _AmbientStrength;
                half _RampSoft;
                half _VertexColor;
                half _Cull;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float3 cameraWS = GetCameraPositionWS();
                float distanceToCamera = length(cameraWS - positionWS);
                float width = _OutlineWidth * saturate(distanceToCamera * 0.16);
                positionWS += normalWS * width;

                output.positionCS = TransformWorldToHClip(positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
                positionCS = ApplyShadowClamping(positionCS);
                output.positionCS = positionCS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ZWrite On
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
