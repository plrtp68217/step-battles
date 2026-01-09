Shader "Custom/VertexColorURP"
{
    /*
        _BaseColor — цвет материала
        _BaseMap — текстура
        Значения редактируются в иснпекторе
    */
    Properties
    {
        _BaseColor ("Color", Color) = (1,1,1,1)
        _BaseMap ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        //этот шейдер ТОЛЬКО для URP
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
        }
        //Pass — один проход рендеринга
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            //ДАННЫЕ ВЕРШИН (Mesh -> GPU)
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            //ДАННЫЕ МЕЖДУ ШЕЙДЕРАМИ
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            float4 _BaseColor;

            //ВЕРШИННЫЙ ШЕЙДЕР
            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color;
                return OUT;
            }

            //ФРАГМЕНТНЫЙ ШЕЙДЕР
            half4 frag (Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                return tex * _BaseColor * IN.color;
            }
            ENDHLSL
        }
    }
}
