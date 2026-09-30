Shader "Unlit/Protal Roll"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Speed ("Rotate Speed", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Speed;

            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord.xy;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv - 0.5;           // 以中心為軸
                float a = _Time.y * _Speed;
                float s = sin(a), c = cos(a);
                uv = float2(uv.x * c - uv.y * s, uv.x * s + uv.y * c);
                uv += 0.5;
                return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }
}