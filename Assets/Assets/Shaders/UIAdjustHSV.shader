Shader "UI/HSV Adjustable"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
//        _Color ("Tint", Color) = (1,1,1,1)
        _Hue ("Hue", Range(-0.5, 0.5)) = 0.0
        _Saturation ("Saturation", Range(-1, 1)) = 0.0
        _Value ("Value", Range(-1, 1)) = 0.0

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
            float _Hue;
            float _Saturation;
            float _Value;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            float3 RGBtoHSV(float3 rgb)
            {
                float R = rgb.x;
                float G = rgb.y;
                float B = rgb.z;
                float minRGB = min(min(R, G), B);
                float maxRGB = max(max(R, G), B);
                float deltaRGB = maxRGB - minRGB;

                float H = 0.0;
                float S = 0.0;
                float V = maxRGB;

                if (deltaRGB != 0) {
                    S = deltaRGB / maxRGB;
                    float deltaR = (((maxRGB - R) / 6) + (deltaRGB / 2)) / deltaRGB;
                    float deltaG = (((maxRGB - G) / 6) + (deltaRGB / 2)) / deltaRGB;
                    float deltaB = (((maxRGB - B) / 6) + (deltaRGB / 2)) / deltaRGB;

                    if (R == maxRGB) H = deltaB - deltaG;
                    else if (G == maxRGB) H = (1.0 / 3.0) + deltaR - deltaB;
                    else if (B == maxRGB) H = (2.0 / 3.0) + deltaG - deltaR;

                    if (H < 0) H += 1;
                    if (H > 1) H -= 1;
                }

                return float3(H, S, V);
            }

            float3 HSVtoRGB(float3 hsv)
            {
                float H = hsv.x;
                float S = hsv.y;
                float V = hsv.z;
                float R, G, B;

                if (S == 0) {
                    R = G = B = V;
                } else {
                    uint i = uint(H * 6);
                    float f = (H * 6) - i;
                    float p = V * (1 - S);
                    float q = V * (1 - f * S);
                    float t = V * (1 - (1 - f) * S);

                    switch (i % 6) {
                        case 0: R = V; G = t; B = p; break;
                        case 1: R = q; G = V; B = p; break;
                        case 2: R = p; G = V; B = t; break;
                        case 3: R = p; G = q; B = V; break;
                        case 4: R = t; G = p; B = V; break;
                        case 5: R = V; G = p; B = q; break;
                    }
                }

                return float3(R, G, B);
            }

            float Frac(float num)
            {
                return step(0.0, -num) + frac(num);
            }

            float4 AdjustHSV(float4 rgba, float3 hsvAdjustment)
            {
                float3 hsv = RGBtoHSV(rgba.rgb);
                hsv.x = Frac(hsv.x + hsvAdjustment.x);
                hsv.y = clamp(hsv.y + hsvAdjustment.y, 0, 1);
                hsv.z = clamp(hsv.z + hsvAdjustment.z, 0, 1);
                return float4(HSVtoRGB(hsv), rgba.a);
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
                float3 hsvAdjustment = float3(_Hue, _Saturation, _Value);
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