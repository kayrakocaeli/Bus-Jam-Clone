Shader "Custom/GameBorderedBackground"
{
    Properties
    {
        _MainTex ("Floor Texture", 2D) = "white" {}
        _TileScale ("Tile Scale", Float) = 1.0
        _Color ("Floor Texture Tint", Color) = (1,1,1,1)
        _BackgroundColor ("Floor Background Color", Color) = (0.2, 0.2, 0.3, 1) 
        
        _HoleColor ("Hole Color", Color) = (0,0,0,0) 
        _BorderColor ("Border Color", Color) = (1,1,1,1) 
        _BorderThickness ("Border Thickness", Range(0.01, 0.5)) = 0.1
        _BorderOffset ("Border Offset", Range(0.0, 0.5)) = 0.05

        _CornerRadius ("Outer Corner Radius", Range(0.0, 0.5)) = 0.2
        
        _TileSize ("Tile Size", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 vertex : SV_POSITION; float3 worldPos : TEXCOORD0; };

            sampler2D _MainTex;
            float _TileScale;
            float4 _Color;
            float4 _BackgroundColor;
            
            float4 _HoleColor;
            float4 _BorderColor;
            float _BorderThickness;
            float _BorderOffset;

            float _CornerRadius;
            float _TileSize;

            float _OpenLimitZ;
            float _OpenSign;

            float4 _ActiveTiles[255];
            int _ActiveTileCount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            float sdRoundedBox(float2 p, float2 halfSize, float r)
            {
                float2 d = abs(p) - (halfSize - r);
                return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0) - r;
            }

            fixed4 frag (v2f i) : SV_TARGET
            {
                float2 wPos = i.worldPos.xz;
                float2 halfSize = float2(_TileSize * 0.5, _TileSize * 0.5);

                float minDist = 9999.0;
                for (int j = 0; j < _ActiveTileCount; j++)
                {
                    float d = sdRoundedBox(wPos - _ActiveTiles[j].xy, halfSize, _CornerRadius);
                    minDist = min(minDist, d);
                }

                float planeSDF = (wPos.y - _OpenLimitZ) * -_OpenSign;
                float dist = min(minDist, planeSDF);

                if (dist <= 0.0) return _HoleColor;

                if (dist <= _BorderOffset) return _HoleColor;

                if (dist <= _BorderOffset + _BorderThickness) return _BorderColor;
                
                float2 uv = wPos * _TileScale;
                fixed4 texCol = tex2D(_MainTex, uv) * _Color;
                fixed3 finalRGB = (texCol.rgb * texCol.a) + (_BackgroundColor.rgb * (1.0 - texCol.a));
                
                return fixed4(finalRGB, 1.0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
