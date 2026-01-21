Shader "Unlit/FillerShader"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        [PerRendererData] _Color("Tint", Color) = (1,1,1,1)

        _Fill("Fill", Range(0, 1)) = 1
        _FillUpperBound("Fill Upper Bound", Range(0, 1)) = 1
        _FillThreshold("Fill Threshold", Range(0, 1)) = 0
        
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0

        _SpriteData("X, Y, Width, Height", Vector) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        ZWrite Off
        Lighting Off

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON PIXELSNAP_ON
                #include "UnityCG.cginc"

                struct appdata {
                    float4 vert        : POSITION;
                    float4 color    : COLOR;
                    float2 uv0        : TEXCOORD0;

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f {
                    float4 vert        : SV_POSITION;
                    fixed4 color    : COLOR;
                    float2 uv0        : TEXCOORD0;

                    UNITY_VERTEX_OUTPUT_STEREO
                };

                fixed4 _Color;
                float _Fill;
                float _FillUpperBound;
                float _FillLowerBound;
                float _FillThreshold;
                half4 _SpriteData;

                sampler2D _MainTex;
                float4 _MainTex_ST;

                sampler2D _AlphaTex;  

                v2f vert(appdata IN) {
                    v2f OUT;

                    UNITY_SETUP_INSTANCE_ID(IN);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                  
                    OUT.color = IN.color * _Color;
                    OUT.vert = UnityObjectToClipPos(IN.vert);

                    //Transforms 2D UV by scale/bias property
                    OUT.uv0 = TRANSFORM_TEX(IN.uv0, _MainTex);

                    #ifdef PIXELSNAP_ON
                        OUT.vert = UnityPixelSnap(OUT.vert);
                    #endif

                    return OUT;
                }

                fixed4 SampleSpriteTexture(float2 uv) {
                    fixed4 color = tex2D(_MainTex, uv);

#if ETC1_EXTERNAL_ALPHA
                    //Get the color from an external texture (usecase: Alpha support for ETC1 on android)
                    fixed4 alpha = tex2D(_AlphaTex, uv);
                    color.a = lerp(color.a, alpha.r, _EnableExternalAlpha);
#endif

                    return color;
                }

                fixed4 frag(v2f IN) : SV_Target
                {
                    fixed4 c = SampleSpriteTexture(IN.uv0) * IN.color;
                    c.rgb *= c.a;

                    // Convert UV coordinate space to cartesian coordinate space (0;1) to (-1;1)
                    // While taking into account the sprite's position in the texture and centering is around the origin
                    float y = lerp(-1, 1, (IN.uv0.y - _SpriteData.y) / _SpriteData.w);

                    // Calculate the real fill amount based on the _FillUpperBound and _FillLowerBound
                    const float real_fill = lerp(0, _FillUpperBound, _Fill);
                    const float clipped_real_fill = lerp(0, _FillUpperBound, _FillThreshold);
                    
                    // Calculate vertical fill based on _ClipAngle (used as a percentage here)
                    const float fill_threshold = lerp(-1, 1, real_fill);
                    const float clipped_fill_threshold = lerp(-1, 1, clipped_real_fill);

                    if (y >= fill_threshold || y <= clipped_fill_threshold)
                    {
                        clip(c.a - 1.0);
                    }

                    return c;
                }
            ENDCG
        }
    }
}