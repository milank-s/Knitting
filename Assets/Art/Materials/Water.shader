Shader "Unlit/Water"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_WaveFreq ("Wave Frequenct", Float) = 1
		_WaveAmp ("Wave Amplitude", Float) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100							//how detailed your program is

		Pass
		{
			CGPROGRAM 					//marks the start of CG/HLSL code
			#pragma vertex vert 		//pragma = a compiler directive, tells to compile it in a certain way
			#pragma fragment frag 		//tell Unity that the fragment program is called "frag"
			// make fog work
			#pragma multi_compile_fog 
			
			#include "UnityCG.cginc" 	//import more shader code that Unity wrote already

			struct appdata				// struct = a class that doesn't do much. a container e.g. Vector3, RaycastHit
										// "appdata" is data loaded from your model
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
										
			struct v2f					//vertex to fragment, passes data from vert shader to frag shader
			{
				float2  color : COLOR1;
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;			//declare a variable of type "sampler2D" (texture) called _"MainTex"
			float4 _MainTex_ST;			// _ST stands for scale/transform that adjust tiling and offset
			half _WaveFreq;				//half precision float, uses less memory
			fixed _WaveAmp;				//fixed precision float, uses a little bit less memory

			v2f vert (appdata v)		//vert shader. returns V2F struct. takes in appdata
			{
				v2f o;
				v.vertex += float4(
					0,
					sin(_Time.y * _WaveFreq + sin(v.vertex.x) + 2 * sin(_Time.w + 5) + 3 * 2 - sin(v.vertex.z) * 5) * _WaveAmp,
					0,
					0
				);

				o.color = float2(v.vertex.x, v.vertex.y); 
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target	//frag shader. returns fixed4 (color), takes in v2f struct
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv + float2( _Time.y, _Time.x)/10);
				col = i.color.g + 0.4;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG							//end of CG/HLSL
		}
	}
}
