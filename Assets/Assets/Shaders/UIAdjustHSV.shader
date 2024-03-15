Shader "UI/HSV Adjustable"
{
    Properties
    {
        [HideInInspector] [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
//        _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] _ReferenceColor ("Reference Color", Color) = (1,0,0,1)
        [HideInInspector] _TargetColor ("Target Color", Color) = (1,0,0,1)

        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255

        [HideInInspector] _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            // fixed4 _Color;
            fixed4 _ReferenceColor;
            fixed4 _TargetColor;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            // -------------------------------------------
            // from com.unity.render-pipeline.core

            // Hue, Saturation, Value
            // Ranges:
            //  Hue [0.0, 1.0]
            //  Sat [0.0, 1.0]
            //  Lum [0.0, HALF_MAX]
            fixed3 RgbToHsv(fixed3 c)
            {
                const fixed4 K = fixed4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                fixed4 p = lerp(fixed4(c.bg, K.wz), fixed4(c.gb, K.xy), step(c.b, c.g));
                fixed4 q = lerp(fixed4(p.xyw, c.r), fixed4(c.r, p.yzx), step(p.x, c.r));
                float d = q.x - min(q.w, q.y);
                const float e = 1.0e-4;
                return fixed3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }

            fixed3 HsvToRgb(fixed3 c)
            {
                const fixed4 K = fixed4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                fixed3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }

            float RotateHue(float value, float low, float hi)
            {
                return (value < low)
                        ? value + hi
                        : (value > hi)
                            ? value - hi
                            : value;
            }
            // -------------------------------------------

            fixed3 CalculateHSVAdjustment(fixed3 referenceColor, fixed3 targetColor)
            {
                const fixed3 hsv1 = RgbToHsv(referenceColor);
                const fixed3 hsv2 = RgbToHsv(targetColor);

                fixed3 hsv = hsv2 - hsv1;

                return hsv;
            }

            fixed4 AdjustHSV(fixed4 rgba, fixed3 hsvAdjustment)
            {
                fixed3 hsv = RgbToHsv(rgba.rgb);
                hsv.x = RotateHue(hsv.x + hsvAdjustment.x, 0, 1);
                hsv.y = clamp(hsv.y + hsvAdjustment.y, 0, 1);
                hsv.z = clamp(hsv.z + hsvAdjustment.z, 0, 1);
                return fixed4(HsvToRgb(hsv), rgba.a);
            }

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color.a/* * _Color*/;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                const fixed3 hsvAdjustment = CalculateHSVAdjustment(_ReferenceColor, _TargetColor);
                color = AdjustHSV(color, hsvAdjustment);

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
        ENDCG
        }
    }
}