Shader "PostProcess/AtomOutlineShader"
{
    Properties
    {
        _ViewFrame ("Texture", 2D) = "white" {}
        _Scale ("Scale", Integer) = 1
        _Color ("Color", Color) = (0,0,0,1)
        _DepthThreshold ("Depth Threshold", Float) = 0.1
        _DepthNormalThreshold ("Depth Normal Threshold", Float) = 0.1
        _DepthNormalThresholdScale ("Depth Normal Threshold Scale", Float) = 1
        _NormalThreshold ("Normal Threshold", Float) = 0.1
    }
    SubShader
    {
        // No culling or depth
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "Outline"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Library/PackageCache/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                float3 viewSpaceDir : TEXCOORD2;
            };

            float4 _Color;
            float _Scale;
            TEXTURE2D_SAMPLER2D(_ViewFrame, sampler_ViewFrame);
            TEXTURE2D_SAMPLER2D(_CustomDepthTexture, sampler_CustomDepthTexture);
            TEXTURE2D_SAMPLER2D(_CustomNormalTexture, sampler_CustomNormalTexture);
            float4 _ViewFrame_TexelSize;

            float _DepthThreshold;
            float _DepthNormalThreshold;
            float _DepthNormalThresholdScale;

            float _NormalThreshold;

            float4x4 _ClipToView;

            float4 alphaBlend(float4 top, float4 bottom)
			{
				float3 color = (top.rgb * top.a) + (bottom.rgb * (1 - top.a));
				float alpha = top.a + bottom.a * (1 - top.a);

				return float4(color, alpha);
			}

            v2f vert(const appdata v)
            {
                v2f o;
                o.vertex = float4(v.vertex.xy, 0.0, 1.0);
                o.texcoord = TransformTriangleVertexToUV(v.vertex.xy);

                o.viewSpaceDir = mul(_ClipToView, o.vertex).xyz;
                return o;
            }

            float4 frag(const v2f i) : SV_Target
            {
                const float halfScaleFloor = floor(_Scale * 0.5);
                const float halfScaleCeil = ceil(_Scale * 0.5);

                const float2 bottomLeftUV =
                    i.texcoord - float2(_ViewFrame_TexelSize.x, _ViewFrame_TexelSize.y) * halfScaleFloor;
                const float2 topRightUV =
                    i.texcoord + float2(_ViewFrame_TexelSize.x, _ViewFrame_TexelSize.y) * halfScaleCeil;
                const float2 bottomRightUV =
                    i.texcoord + float2(_ViewFrame_TexelSize.x * halfScaleCeil, -_ViewFrame_TexelSize.y * halfScaleFloor);
                const float2 topLeftUV =
                    i.texcoord + float2(-_ViewFrame_TexelSize.x * halfScaleFloor, _ViewFrame_TexelSize.y * halfScaleCeil);

                float3 normal0 = SAMPLE_TEXTURE2D(_CustomNormalTexture, sampler_CustomNormalTexture, bottomLeftUV).rgb;
				float3 normal1 = SAMPLE_TEXTURE2D(_CustomNormalTexture, sampler_CustomNormalTexture, topRightUV).rgb;
				float3 normal2 = SAMPLE_TEXTURE2D(_CustomNormalTexture, sampler_CustomNormalTexture, bottomRightUV).rgb;
				float3 normal3 = SAMPLE_TEXTURE2D(_CustomNormalTexture, sampler_CustomNormalTexture, topLeftUV).rgb;
                
                const float depth0 = SAMPLE_TEXTURE2D(_CustomDepthTexture, sampler_CustomDepthTexture, bottomLeftUV).r;
                const float depth1 = SAMPLE_TEXTURE2D(_CustomDepthTexture, sampler_CustomDepthTexture, topRightUV).r;
                const float depth2 = SAMPLE_TEXTURE2D(_CustomDepthTexture, sampler_CustomDepthTexture, bottomRightUV).r;
                const float depth3 = SAMPLE_TEXTURE2D(_CustomDepthTexture, sampler_CustomDepthTexture, topLeftUV).r;
                
                const float3 viewNormal = normal0 * 2 - 1;
                const float ndot_v = 1 - dot(viewNormal, -i.viewSpaceDir);
                
                const float normalThreshold01 = saturate((ndot_v - _DepthNormalThreshold) / (1 - _DepthNormalThreshold));
				 
                const float normalThreshold = normalThreshold01 * _DepthNormalThresholdScale + 1;

				 
                const float depthThreshold = _DepthThreshold * depth0 * normalThreshold;

                const float depthFiniteDifference0 = depth1 - depth0;
                const float depthFiniteDifference1 = depth3 - depth2;
				 
				float edgeDepth = sqrt(pow(depthFiniteDifference0, 2) + pow(depthFiniteDifference1, 2)) * 100;
				edgeDepth = edgeDepth > depthThreshold ? 1 : 0;

                const float3 normalFiniteDifference0 = normal1 - normal0;
                const float3 normalFiniteDifference1 = normal3 - normal2;
				 
				float edgeNormal = sqrt(dot(normalFiniteDifference0, normalFiniteDifference0) + dot(normalFiniteDifference1, normalFiniteDifference1));
				edgeNormal = edgeNormal > _NormalThreshold ? 1 : 0;

                const float edge = max(edgeDepth, edgeNormal);

                const float4 edgeColor = float4(_Color.rgb, _Color.a * edge);

                const float4 color = SAMPLE_TEXTURE2D(_ViewFrame, sampler_ViewFrame, i.texcoord);

				return alphaBlend(edgeColor, color);
            }
            ENDHLSL
        }
    }
}