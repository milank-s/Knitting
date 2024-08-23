Shader "Custom/NoAlpha" {
    Properties {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        
        _Color ("Color", Color) = (0,0,0,1)
        _Cutoff("Alpha cutoff", Range(0,1)) = 0.1
    }

    SubShader {
        Tags { "RenderType"="Opaque" "IgnoreProjector"="True" "Queue"="AlphaTest" }
        LOD 300

        ZWrite On
        ZTest Always
        Cull Off

        CGPROGRAM
        #pragma surface surf NoLighting alphatest:_Cutoff
        #pragma target 2.0
        #pragma multi_compile_instancing
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        float4 _Color;

        struct Input {
            float2 uv_MainTex;
            float4 color : COLOR;
        };

        void surf (Input IN, inout SurfaceOutput o) {
            half4 c = tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo = IN.color * _Color;
            o.Alpha = c.a * _Color.a;
        }
        
        fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
        {
            fixed4 c;
            c.rgb = s.Albedo;
            c.a = s.Alpha;
            return c;
        }
    
        ENDCG
    }

    // SubShader {
    //     Tags { "RenderType"="Transparent" "IgnoreProjector"="True" "Queue"="Transparent" }
    //     LOD 300

    //     CGPROGRAM
    //     #pragma surface surf NoLighting alpha
    //     #pragma target 2.0
    //     #pragma multi_compile_instancing
    //     #include "UnityCG.cginc"

    //     sampler2D _MainTex;

    //     struct Input {
    //         float2 uv_MainTex;
    //         float4 color : COLOR;
    //     };

    
    //     void surf (Input IN, inout SurfaceOutput o) {
    //         half4 c = tex2D (_MainTex, IN.uv_MainTex);
    //         o.Albedo = IN.color;
    //         o.Alpha = IN.color.a;
    //     }
        
    //     fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
    //     {
    //         fixed4 c;
    //         c.rgb = s.Albedo;
    //         c.a = s.Alpha;
    //         return c;
    //     }
    
    //     ENDCG
    // }

    
    FallBack "Particles/Standard Surface"
}
