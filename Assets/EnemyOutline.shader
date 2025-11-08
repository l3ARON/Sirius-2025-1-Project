Shader "Custom/EnemyOutline"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _OutlineColor ("Outline Color", Color) = (1,0,0,1)
        _OutlineSize ("Outline Size", Float) = 0.0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _Color;
            float4 _OutlineColor;
            float _OutlineSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                float alpha = col.a;

                // 이미 스프라이트가 있으면 그냥 그리기
                if (alpha > 0.001)
                    return col;

                // 주변 픽셀들에서 알파 검사 (아웃라인 영역)
                float2 offset = _OutlineSize * _MainTex_TexelSize.xy;

                float a =
                    tex2D(_MainTex, i.uv + float2( offset.x,  0)).a +
                    tex2D(_MainTex, i.uv + float2(-offset.x,  0)).a +
                    tex2D(_MainTex, i.uv + float2( 0,  offset.y)).a +
                    tex2D(_MainTex, i.uv + float2( 0, -offset.y)).a;

                if (a > 0.001)
                {
                    fixed4 ocol = _OutlineColor;
                    ocol.a = saturate(a); // 알파 꽉 차게
                    return ocol;
                }

                // 아무것도 없으면 투명
                return fixed4(0,0,0,0);
            }
            ENDCG
        }
    }
}
