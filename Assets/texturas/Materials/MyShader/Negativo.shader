Shader "Custom/Negativo" {
	Properties
	{
		_Color("Color",Color)=(1.0,1.0,1.,1.0)
		_MainTex("Difusse texture",2D)="white"{}
	}
	SubShader
	{		
		Pass
		{
			Tags{ "LightMode"="ForwardBase"}
			CGPROGRAM
			//pragmas
			#pragma vertex vert
			#pragma fragment frag

			//user defined variables
			uniform float4 _Color;
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;

			//unity defined variables
			uniform float4 _LightColor0;
			
			//unity 3.5 definition
			//float4x4 _Object2World;
			//float4x4 _World2Object;
			//float4 _WorldSpaceLightPos0;

			//base input struts
			struct vertexInput{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD0;
			};
			struct vertexOutput{
				float4 pos: SV_POSITION;
				float4 tex : TEXCOORD0;
				float4 posWorld: TEXCOORD1;
				float3 normalDir: TEXCOORD2;
				
			};

			//vertex function
			vertexOutput vert(vertexInput v)
			{
				vertexOutput o;				
				
				o.posWorld= mul(_Object2World,v.vertex);
				o.normalDir=normalize(mul(float4(v.normal,0.0),_World2Object).xyz);	
				o.tex=v.texcoord;
				o.pos=mul(UNITY_MATRIX_MVP,v.vertex);
				return o;
			}

			//fragment function
			float4 frag(vertexOutput i): COLOR
			{
				float3 lightFinal= UNITY_LIGHTMODEL_AMBIENT.xyz;
				
				//textures
				float4 tex=tex2D(_MainTex,i.tex.xy * _MainTex_ST.xy + _MainTex_ST.zw);
				
				float3 colorFinal=_Color.rgb;
				
				float3 textureFinal= float3(1-tex.x,1-tex.y,1-tex.z);
				

				return  float4(textureFinal* colorFinal, 1.0);

			}

			ENDCG
		}	
				
	}
}
