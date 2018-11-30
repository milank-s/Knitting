Shader "Custom/SimpleAlpha" {
    Properties {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [PerRendererData] _Color ("Tint", Color) = (1,1,1,1)
    }
    SubShader {
        Tags { "RenderType"="Cutout" "Queue"="Geometry" }
        //Tags { "RenderType"="Opaque" }
        LOD 300

        ZWrite On

        CGPROGRAM
        #pragma surface surf Lambert alpha finalcolor:mycolor
        #pragma target 2.0
        #pragma multi_compile_instancing
        #pragma multi_compile _ PIXELSNAP_ON
        #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        fixed4 _Color;

        struct Input {
            float2 uv_MainTex;
            float4 color : COLOR;
        };

        void mycolor (Input IN, SurfaceOutput o, inout fixed4 color)
        {
         color = fixed4(IN.color.r, IN.color.g, IN.color.b, color.a);
         }

        void surf (Input IN, inout SurfaceOutput o) {
            half4 c = tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo = IN.color;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
