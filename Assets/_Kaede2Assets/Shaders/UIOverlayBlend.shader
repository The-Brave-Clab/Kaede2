Shader "UI/UI Overlay Blend"
{
    Properties
    {
        [HideInInspector] _ReferenceColor ("Reference Color", Color) = (1,0,0,1)
        [HideInInspector] _TargetColor ("Target Color", Color) = (1,0,0,1)

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

                fixed4 _ReferenceColor;
                fixed4 _TargetColor;

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

                fixed4 frag(v2f IN) : SV_Target
                {
                    fixed4 grabColor = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(IN.uvgrab));
                    fixed4 mainColor = tex2D(_MainTex, IN.uvmain);

                    const fixed3 hsvAdjustment = CalculateHSVAdjustment(_ReferenceColor, _TargetColor);
                    mainColor = AdjustHSV(mainColor, hsvAdjustment);

                    fixed3 adjustedAlpha = pow(saturate(mainColor.a * 2 - 1), 2);

                    fixed3 finalColor = lerp(grabColor.rgb + grabColor.rgb * mainColor.rgb * adjustedAlpha, mainColor.rgb, adjustedAlpha);
                    return saturate(fixed4(finalColor, 1));
                }
            ENDCG
            }

        }
    }
    Fallback "UI/Default"
}