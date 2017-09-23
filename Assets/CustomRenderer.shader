Shader "Custom/CustomRenderer"
{
	Properties
	{
		_Color ("Color", color) = (1,1,0,1)
	}
	SubShader
	{
		Pass
		{
			// Have to write to stencil buffer between [128,255] or we get artifacts
			Stencil{
				Ref 128
				Comp always
				Pass replace
			}
			
			Name "MAINPASS"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 vertex : SV_POSITION;
				half3 worldNormal : TEXCOORD1;
			};

			float4 _Color;
			
			v2f vert (appdata_base v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex); // Unity functions from UnityCG.cginc
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				return o;
			}
			
			void frag(
				v2f i,
				out half4 outDiffuse : COLOR0,			// RT0: diffuse color (rgb), --unused-- (a)
				out half4 outSpecRoughness : COLOR1,	// RT1: spec color (rgb), roughness (a)
				out half4 outNormal : COLOR2,			// RT2: normal (rgb), --unused-- (a)
				out half4 outEmission : COLOR3			// RT3: emission (rgb), --unused-- (a)
			) {
				outDiffuse = half4(_Color.xyz, 1);
				outSpecRoughness = half4(0.0, 0.0, 0.0, 0.0);
				outNormal = half4(i.worldNormal*0.5 + 0.5,1); // Have to get values into range [0,1] so need to do ((worldNormal * 0.5) + 0.5)
				outEmission = half4(0, 0, 0, 1);
			}
			ENDCG
		}
		// Shadow pass, pasted from here https://docs.unity3d.com/Manual/SL-VertexFragmentShaderExamples.html except I added a name: Name "SHADOWPASS"
		Pass
		{
			Name "SHADOWPASS"
			Tags{ "LightMode" = "ShadowCaster" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"

			struct v2f {
				V2F_SHADOW_CASTER;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
					return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	}
}
