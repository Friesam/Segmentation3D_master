Shader "Unlit/secondTry"
{
	Properties{
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
		_Test("_test", Float ) = test
	}

		SubShader{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 100

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fog

#include "UnityCG.cginc"

		struct appdata_t {
		float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
	};

	struct v2f {
		float4 vertex : SV_POSITION;
		half2 texcoord : TEXCOORD0;
		UNITY_FOG_COORDS(1)
	};

	sampler2D _MainTex;
	float4 _MainTex_ST;
	float _test;

	v2f vert(appdata_t v)
	{
		v2f o;
		o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
		o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
		UNITY_TRANSFER_FOG(o,o.vertex);
		return o;
	}

	/*fixed4 frag_Ghassem: SV_Target
	{
	fixed4 col = tex2D(_MainTex, i.texcoord);
	UNITY_APPLY_FOG(i.fogCoord, col);
	return col;
	}*/


	fixed4 frag(v2f i) : SV_Target
	{
		// sample the texture
		fixed4 col = tex2D(_MainTex, i.texcoord);
	// apply fog
	fixed4 output = col;
	//UNITY_APPLY_FOG(i.fogCoord, col);
	//return col;

	if (col.a == 0) //background
	{
		col.a = _test;
		UNITY_APPLY_FOG(i.fogCoord, col);
		//col.a = 1 - col.a;
		col.r = 1;
		col.g = 0;
		col.b = 0;
		/*	col.r = col.a;
		col.g = col.a;
		col.b = col.a;
		col.a = 1;
		*/	return col;
	}
	else if (col.a == 1) //foreground
	{
		UNITY_APPLY_FOG(i.fogCoord, col);
		col.r = col.a;
		col.g = col.a;
		col.b = col.a;
		col.a = 1;
		return col;
	}

	col.r = 0;
	col.g = 0;
	col.b = 1;
	return col;

	}
			ENDCG
		}
	}
}
