Shader "UI/CartoonIrisWipe"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Overlay Color", Color) = (0, 0, 0, 1)
        _Progress ("Wipe Progress", Range(0, 1)) = 0
        _Center ("Center (UV)", Vector) = (0.5, 0.46, 0, 0)
        _Feather ("Edge Feather", Range(0.001, 0.2)) = 0.025
        _AspectRatio ("Screen Aspect Ratio (0 for Auto)", Float) = 0
        _RimColor ("Rim Color", Color) = (0.95, 0.78, 0.25, 0.8)
        _RimWidth ("Rim Width", Range(0, 0.1)) = 0.015
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent+500"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            fixed4 _Color;
            fixed4 _RimColor;
            float _Progress;
            float4 _Center;
            float _Feather;
            float _AspectRatio;
            float _RimWidth;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Compute true viewport aspect ratio so the iris is always a geometrically perfect circle
                float aspect = (_ScreenParams.y > 0.0) ? (_ScreenParams.x / _ScreenParams.y) : 1.0;
                if (_AspectRatio > 0.05)
                {
                    aspect = _AspectRatio;
                }

                float2 uv = IN.texcoord - _Center.xy;
                uv.x *= aspect;
                float dist = length(uv);

                // Maximum radius in height-normalized units to fully cover any screen corners
                float maxRadius = 1.35;
                float currentRadius = (1.0 - _Progress) * maxRadius;

                // Outside currentRadius is black (alpha=1), inside is transparent (alpha=0)
                float alpha = smoothstep(currentRadius - _Feather, currentRadius + _Feather, dist);

                // Golden cartoon rim highlight at the iris boundary
                float innerRim = currentRadius - _Feather - _RimWidth;
                float outerRim = currentRadius + _Feather;
                float rimAlpha = smoothstep(innerRim, currentRadius - _Feather, dist)
                               * (1.0 - smoothstep(currentRadius, outerRim, dist))
                               * _RimColor.a;

                // Brilliant cartoon center spark at the focal point when iris is near or fully closed
                float sparkRadius = max(0.002, _Feather * 1.5);
                float spark = (1.0 - smoothstep(0.0, sparkRadius, dist)) * smoothstep(0.65, 1.0, _Progress);
                fixed3 sparkColor = fixed3(1.0, 0.98, 0.86); // brilliant glowing warm white light

                fixed4 finalColor = _Color;
                finalColor.rgb = lerp(finalColor.rgb, _RimColor.rgb, rimAlpha);
                finalColor.rgb = lerp(finalColor.rgb, sparkColor, spark);
                finalColor.a = saturate(alpha + rimAlpha + spark);

                return finalColor;
            }
            ENDCG
        }
    }
}
