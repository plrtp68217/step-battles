Shader "Custom/VertexColorURP_Wireframe"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _BaseMap ("Texture", 2D) = "white" {}

        _WireColor ("Wire Color", Color) = (0,0,0,1)
        _WireWidth ("Wire Width", Range(0.5, 3.0)) = 1.0
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
            #pragma geometry geom
            #pragma fragment frag
            #pragma require geometry

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // ===== ДАННЫЕ ИЗ МЕША =====
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            // ===== ДАННЫЕ МЕЖДУ СТАДИЯМИ =====
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR;
                float3 barycentric : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            float4 _BaseColor;
            float4 _WireColor;
            float  _WireWidth;

            // ===== VERTEX SHADER =====
            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color;
                OUT.barycentric = 0; // заполним в geometry
                return OUT;
            }

            // ===== GEOMETRY SHADER =====
            [maxvertexcount(3)]
            void geom(triangle Varyings IN[3], inout TriangleStream<Varyings> stream)
            {
                for (int i = 0; i < 3; i++)
                {
                    Varyings o = IN[i];

                    // Настоящие barycentric координаты
                    o.barycentric = float3(
                        i == 0 ? 1 : 0,
                        i == 1 ? 1 : 0,
                        i == 2 ? 1 : 0
                    );

                    stream.Append(o);
                }
            }

            // ===== FRAGMENT SHADER =====
            half4 frag (Varyings IN) : SV_Target
            {
                // Базовый цвет
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half4 col = tex * _BaseColor * IN.color;

                // Расстояние до рёбер (anti-aliasing)
                float3 d = fwidth(IN.barycentric);
                float3 a = smoothstep(0, d * _WireWidth, IN.barycentric);

                // Минимум = ближе всего к ребру
                float edge = min(min(a.x, a.y), a.z);

                // Смешиваем wireframe с цветом
                col.rgb = lerp(_WireColor.rgb, col.rgb, edge);

                return col;
            }
            ENDHLSL
        }
    }
}
