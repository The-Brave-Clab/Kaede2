Shader "UI/UI Color Adjustment"
{

    Properties
    {
        _HueAdjustment("Hue", Range(-1, 1)) = 0
        _SaturationAdjustment("Saturation", Range(-1, 1)) = 0
        _ValueAdjustment("Value", Range(-1, 1)) = 0

        // see Stencil in UI/Default
        [HideInInspector][PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [HideInInspector]_StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector]_Stencil ("Stencil ID", Float) = 0
        [HideInInspector]_StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector]_StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector]_StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector]_ColorMask ("Color Mask", Float) = 15
        [HideInInspector]_UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    Category
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
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

        SubShader
        {

            GrabPass
            {
                Tags
                { 
                    "LightMode" = "Always"
                    "Queue" = "Background"  
                }
            }
            Pass
            {
                Name "Adjust Color"
                Tags{ "LightMode" = "Always" }


            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma fragmentoption ARB_precision_hint_fastest
                #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
                #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

                #include "Color.cginc"

                struct appdata_t
                {
                    float4 vertex : POSITION;
                    float2 texcoord: TEXCOORD0;
                    float4 color : COLOR;
                };

                struct v2f
                {
                    float4 vertex : POSITION;
                    float4 uvgrab : TEXCOORD0;
                    float4 worldpos : TEXCOORD1;
                    float2 uvmain : TEXCOORD2;
                    float4 color : COLOR;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;

                v2f vert(appdata_t v)
                {
                    v2f OUT;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                    OUT.worldpos = v.vertex;
                    OUT.vertex = UnityObjectToClipPos(v.vertex);
                #if UNITY_UV_STARTS_AT_TOP
                    float scale = -1.0;
                #else
                    float scale = 1.0;
                #endif
                    OUT.uvgrab.xy = (float2(OUT.vertex.x, OUT.vertex.y*scale) + OUT.vertex.w) * 0.5;
                    OUT.uvgrab.zw = OUT.vertex.zw;
                    OUT.uvmain = TRANSFORM_TEX(v.texcoord, _MainTex);
                    OUT.color = v.color;

                    return OUT;
                }

                sampler2D _GrabTexture;
                float4 _GrabTexture_TexelSize;

                fixed _HueAdjustment;
                fixed _SaturationAdjustment;
                fixed _ValueAdjustment;

                fixed4 frag(v2f IN) : SV_Target
                {
                    half4 pixel = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(IN.uvgrab));
                    half3 hsv = RgbToHsv(pixel.rgb);
                    hsv.x += _HueAdjustment;
                    hsv.y += _SaturationAdjustment;
                    hsv.z += _ValueAdjustment;

                    hsv.yz = saturate(hsv.yz);

                    half3 rgb = HsvToRgb(hsv);
                    return half4(rgb, pixel.a);
                }
            ENDCG
            }

        }
    }
    Fallback "UI/Default"
}