Shader "Custom/CurvedWorld" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;

        // Global variables fed by C#
        uniform float _GlobalCurveStrength;
        uniform float _GlobalCurveOffset;

        struct Input {
            float2 uv_MainTex;
        };

        void vert (inout appdata_full v) {
            float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
            
            // Calculate distance and apply global curve
            float dist = max(0, worldPos.z - _WorldSpaceCameraPos.z - _GlobalCurveOffset);
            worldPos.y -= dist * dist * _GlobalCurveStrength;
            
            v.vertex = mul(unity_WorldToObject, worldPos);
        }

        void surf (Input IN, inout SurfaceOutputStandard o) {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}