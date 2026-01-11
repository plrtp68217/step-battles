Shader "Custom/VertexColorURP_EdgeContrast"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _BaseMap ("Texture", 2D) = "white" {}

        _EdgeStrength ("Edge Strength", Range(0,2)) = 0.6
        _EdgePower ("Edge Sharpness", Range(1,8)) = 3
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float4 color : COLOR;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            float4 _BaseColor;
            float _EdgeStrength;
            float _EdgePower;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS = normalize(TransformObjectToWorldNormal(IN.normalOS));
                OUT.uv = IN.uv;
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half4 col = tex * _BaseColor * IN.color;

                // Edge factor based on normal direction
                float edge = 1.0 - abs(IN.normalWS.y);
                edge = pow(edge, _EdgePower);

                // Darken edges slightly
                col.rgb *= (1.0 - edge * _EdgeStrength);
                col.rgb = min(col.rgb, 1.0);

                return col;
            }
            ENDHLSL
        }
    }
}
