Shader "Hidden/Glitch"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
	    _Intensity ("Glitch Intensity", Range(0.1, 1.0)) = 1
        _Noise ("Glitch Intensity", Range(0.1, 1.0)) = 1
    }
    SubShader
    {
        // No culling or depth
        ZTest Always Cull Off ZWrite Off
		Fog { Mode off }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            
		    uniform sampler2D _MainTex;
		    float _Intensity;
             float _Noise;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : POSITION;
			float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }


            fixed4 frag (v2f i) : SV_Target
            {
               //i.uv.y = (1 - (i.uv.y)) * step(i.uv.y, flip_up) + (1 - (i.uv.y)) * step(flip_down, i.uv.y);

                float rounding = pow(sin(i.uv.y * 3.14),2);
			    i.uv.x += _Intensity * sin((i.uv.y+ _Noise)*100) * rounding;
                
			
			    half4 color = tex2D(_MainTex,  i.uv.xy);
                return color;
            }
            ENDCG
        }
    }
}
