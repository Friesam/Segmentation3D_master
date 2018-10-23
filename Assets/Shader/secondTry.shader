// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Unlit/FadingEffect" {
	Properties{
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
		_MaxTransparencyLimit("MaxTransparencyLimit", float) = 0.18
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
	};

	sampler2D _MainTex;
	float4 _MainTex_ST;

	uniform float _tst;
	float _MaxTransparencyLimit;

	v2f vert(appdata_t v)
	{
		v2f o;
		o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
		o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
		return o;
	}

	fixed4 frag_red(v2f i) : SV_Target
	{
		fixed4 col = tex2D(_MainTex, i.texcoord);
	    col.a = 1.0;
		col.r = 1;
		col.g = _tst;
		col.b = _tst;
	return col;
	}

    fixed4 frag(v2f i) : SV_Target
	{
		fixed4 col = tex2D(_MainTex, i.texcoord);

		if(_tst < 0.2 && col.a <= 0.85)
		{
			col.a = (1 - ((_tst - 0.15) / 0.05));	//_MaxTransparencyLimit;
			if (col.a > 1)
			{
				col.a = 1;
			}
			if (col.a < 0 ) {
				col.a = 0;
				}
		}
		return col;
	}


		ENDCG
	}
	}

}
