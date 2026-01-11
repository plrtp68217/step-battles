Shader "Custom/VertexColorURP_Wireframe"
{
    Properties
    {
        _BaseColor ("Color", Color) = (1,1,1,1)
        _BaseMap ("Texture", 2D) = "white" {}
        [Toggle]_ShowWire ("Show Wireframe", Float) = 0
        _WireColor ("Wire Color", Color) = (0,0,0,1)
        _WireWidth ("Wire Width", Range(0, 0.05)) = 0.002
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
            #pragma require geometry

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float3 barycentric : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            float4 _BaseColor;
            float _ShowWire;
            float4 _WireColor;
            float _WireWidth;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color;
                
                // Barycentric coordinates for wireframe
                if (IN.vertexID % 3 == 0)
                    OUT.barycentric = float3(1, 0, 0);
                else if (IN.vertexID % 3 == 1)
                    OUT.barycentric = float3(0, 1, 0);
                else
                    OUT.barycentric = float3(0, 0, 1);
                    
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half4 finalColor = tex * _BaseColor * IN.color;
                
                // Draw wireframe on top
                if (_ShowWire > 0.5)
                {
                    // Calculate distance to edges
                    float3 d = fwidth(IN.barycentric);
                    float3 a3 = smoothstep(float3(0, 0, 0), d * _WireWidth * 100, IN.barycentric);
                    float edgeFactor = min(min(a3.x, a3.y), a3.z);
                    
                    if (edgeFactor < 0.9)
                    {
                        return lerp(_WireColor, finalColor, edgeFactor);
                    }
                }
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}