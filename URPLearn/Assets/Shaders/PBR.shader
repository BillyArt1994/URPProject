Shader "URP/PBR/Base"
{
    Properties
    {
        _Color ("Color", Color)=(1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _Gloss ("Gloss",Range(8,512)) = 64
        _Value ("Value",Range(0,1)) = 0
        [MaterialToggle(_Test)] _Test("Test", Float) = 0 
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }

        //Lighting render Pass
        Pass
        {
            Tags{"LightMode"="UniversalForward"}
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma shader_feature _ _Test

       //     #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct a2v
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                half3 worldViewDir : TEXCOORD2;
                half3 worldNormalDir : TEXCOORD3;
                half3 worldLightDir : TEXCOORD4;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _Color;
            half _Value;
            half _Gloss;
            CBUFFER_END

            v2f vert (a2v v)
            {
                v2f o;
                o.vertex  = TransformObjectToHClip(v.vertex);
                o.worldPos =  TransformObjectToWorld(v.vertex);
                o.worldViewDir =  _WorldSpaceCameraPos.xyz -  o.worldPos.xyz;
                o.worldNormalDir = TransformObjectToWorldNormal(v.normal);
                o.worldLightDir = _MainLightPosition.xyz;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half3 worldNormalDir = normalize(i.worldNormalDir);
                half3 worldLightDir = normalize(i.worldLightDir);
                half3 worldViewDir = normalize(i.worldViewDir);
                half3 halfwayDir = normalize(worldViewDir+worldLightDir) ;
                half NdotL=max(dot(worldNormalDir,worldLightDir),0.0001) ;
                half NdotH = max(dot(halfwayDir,worldNormalDir),0.0001) ;
                half specular = pow(NdotH,_Gloss);

                half4 col = NdotL*_Color+specular;
                #if _Test
                col.rgb = half4(1.0,_Value,0.0,0.0);
                #endif
                return col;
            }
            ENDHLSL
        }

        //Shadow Pass
        pass{
            Tags{"LightMode" = "ShadowCaster"}
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
    CustomEditor "PBRShaderGUIBase"
}