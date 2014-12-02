Shader "Custom/borde" {
	Properties{
		_Color("Color Principal",Color)=(1.0,1.0,1.,1.0)
		_ColorBorder("Color Borde",Color)=(0.0,0.0,0.0,0.0)
		_tamBorde ("tamaño del borde", Range (.002, 0.05)) = .005
		
		_SpecColor("Color especular",Color)=(1.0,1.0,1.,1.0)
		_Shininess("brillo",float)=10
		_RimColor("Rim Color",Color)=(1.0,1.0,1.,1.0)
		_RimPower("Rim Power",Range(1.0,10.0))=3.0
		_MainTex("Difusse texture",2D)="white"{}
	}	
	SubShader{
		Pass{
			Tags{ "LightMode"="ForwardBase"}
			Cull Front
			CGPROGRAM
			//pragmas
			#pragma vertex vert
			#pragma fragment frag	
			#include "UnityCG.cginc"
			
			//user defined variables
			uniform float4 _Color;
			uniform float4 _ColorBorder;
			uniform float _tamBorde;

			//unity defined variables
			uniform float4 _LightColor0;

			//base input struts
			struct vertexInput{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};
			struct vertexOutput{
				float4 pos: SV_POSITION;
			};		

			//vertex function
			vertexOutput vert(vertexInput v)
			{
				vertexOutput o;
				o.pos=mul(UNITY_MATRIX_MVP,v.vertex);
				
				float3 norm   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
				float2 offset = TransformViewToProjection(norm.xy);

				o.pos.xy += offset * _tamBorde;
				
				return o;
			}

			//fragment function
			float4 frag(vertexOutput i): COLOR
			{
				return _ColorBorder;
			}

			ENDCG
		}

		Pass
		{
			Tags{ "LightMode"="ForwardBase"}
			CGPROGRAM
			//pragmas
			#pragma vertex vert
			#pragma fragment frag

			//user defined variables
			uniform float4 _Color;
			uniform float4 _SpecColor;
			uniform float4 _RimColor;
			uniform float _Shininess;
			uniform float _RimPower;
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
				//vectors
				float3 normalDirection= i.normalDir;
				float3 viewDirection=normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
				float3 lightDirection;
				float attent;
				
				if(_WorldSpaceLightPos0.w==0.0)
				{
					attent=1.0;
					lightDirection=normalize(_WorldSpaceLightPos0.xyz);
				}
				else
				{
					float3 fragmentToLight=_WorldSpaceLightPos0.xyz - i.posWorld.xyz;
					float distance= length(fragmentToLight);
					attent=1.0/distance;
					lightDirection=normalize(fragmentToLight);
				}
				float3 difusseReflection=attent * _LightColor0.rgb * max(0.0,dot(normalDirection,lightDirection));
				float3 specularReflection= attent * _SpecColor.rgb * _LightColor0.rgb * max(0.0,dot(normalDirection,lightDirection))
				 *pow(max(0.0,dot(reflect(-lightDirection,normalDirection),viewDirection)),_Shininess);
				 
				float rim= 1- saturate(dot(normalize(viewDirection), normalDirection));
				float3 rimLighting=attent * _RimColor.rgb * _LightColor0.rgb*saturate(dot(normalDirection,lightDirection))* pow(rim,_RimPower);


				float3 lightFinal= rimLighting + difusseReflection + specularReflection + UNITY_LIGHTMODEL_AMBIENT.xyz;
				
				//textures
				float4 tex=tex2D(_MainTex,i.tex.xy * _MainTex_ST.xy + _MainTex_ST.zw);

				return  float4(tex.xyz * lightFinal * _Color.rgb, 1.0);

			}

			ENDCG
		}
		
		Pass
		{
			Tags{ "LightMode"="ForwardAdd"}
			Blend one one
			CGPROGRAM
			//pragmas
			#pragma vertex vert
			#pragma fragment frag

			//user defined variables
			uniform float4 _Color;
			uniform float4 _SpecColor;
			uniform float4 _RimColor;
			uniform float _Shininess;
			uniform float _RimPower;
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
				//vectors
				float3 normalDirection= i.normalDir;
				float3 viewDirection=normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
				float3 lightDirection;
				float attent;
				
				if(_WorldSpaceLightPos0.w==0.0)
				{
					attent=1.0;
					lightDirection=normalize(_WorldSpaceLightPos0.xyz);
				}
				else
				{
					float3 fragmentToLight=_WorldSpaceLightPos0.xyz - i.posWorld.xyz;
					float distance= length(fragmentToLight);
					attent=1.0/distance;
					lightDirection=normalize(fragmentToLight);
				}
				float3 difusseReflection=attent * _LightColor0.rgb * max(0.0,dot(normalDirection,lightDirection));
				float3 specularReflection= attent * _SpecColor.rgb * _LightColor0.rgb * max(0.0,dot(normalDirection,lightDirection))
				 *pow(max(0.0,dot(reflect(-lightDirection,normalDirection),viewDirection)),_Shininess);
				 
				float rim= 1- saturate(dot(normalize(viewDirection), normalDirection));
				float3 rimLighting=attent * _RimColor.rgb * _LightColor0.rgb*saturate(dot(normalDirection,lightDirection))* pow(rim,_RimPower);


				float3 lightFinal= rimLighting + difusseReflection + specularReflection + UNITY_LIGHTMODEL_AMBIENT.xyz;
				
				//textures
				float4 tex=tex2D(_MainTex,i.tex.xy * _MainTex_ST.xy + _MainTex_ST.zw);

				return  float4(tex.xyz * lightFinal * _Color.rgb, 1.0);

			}

			ENDCG
		}
		
		
	}
	
}
