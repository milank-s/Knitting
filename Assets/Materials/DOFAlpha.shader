Shader "Custom/SimpleAlpha" {
    Properties {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
    }
    SubShader {
        Tags { "RenderType"="Cutout" "Queue"="Geometry" }
        LOD 300

        ZWrite On

        CGPROGRAM
        #pragma surface surf NoLighting alpha 
        #pragma target 2.0
        #pragma multi_compile_instancing
        #pragma multi_compile _ PIXELSNAP_ON
        #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
        #include "UnityCG.cginc"

        sampler2D _MainTex;

        struct Input {
            float2 uv_MainTex;
            float4 color : COLOR;
        };

    
        void surf (Input IN, inout SurfaceOutput o) {
            half4 c = tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo = IN.color;
            o.Alpha = clamp(c.a - (1 - IN.color.a), 0, 1);
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
    FallBack "Diffuse"
}
