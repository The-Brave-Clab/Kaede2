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
            #include "Color.cginc"

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