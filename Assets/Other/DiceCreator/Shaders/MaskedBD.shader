// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "IDS/MaskedBumpDiffuse" {
	Properties {
		[Header(Main mask)]
		[NoScaleOffset] _MaskTex ("Mask (RGB)", 2D) = "white" {}

		[Header(Black area settings)]
		_BlackColor ("Black area tint", Color) = (1,1,1,1)
		_BlackTex ("Black area fill (RGB)", 2D) = "white" {}
		_BlackGlossiness("Black area smoothness", Range(0,1)) = 0.5
		_BlackMetallic("Black area metallic", Range(0,1)) = 0.0

		[Header(White area settings)]
		_WhiteColor ("White area tint", Color) = (1,1,1,1)
		_WhiteTex("White area fill (RGB)", 2D) = "white" {}
		_WhiteGlossiness ("White area smoothness", Range(0,1)) = 0.5
		_WhiteMetallic ("White area metallic", Range(0,1)) = 0.0

		[Header(Shared settings)]
		[NoScaleOffset] _BumpMap("Bumpmap", 2D) = "bump" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MaskTex;
		
		fixed4 _BlackColor;
		sampler2D _BlackTex;
		half _BlackGlossiness;
		half _BlackMetallic;
		
		fixed4 _WhiteColor;
		sampler2D _WhiteTex;
		half _WhiteGlossiness;
		half _WhiteMetallic;

		sampler2D _BumpMap;

		struct Input {
			float2 uv_MaskTex;
			float2 uv_BlackTex;
			float2 uv_WhiteTex;
			float2 uv_BumpMap;
		};


		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from two tinted textures mixed by a mask
			fixed4 mask = tex2D(_MaskTex, IN.uv_MaskTex);
			fixed4 black = tex2D(_BlackTex, IN.uv_BlackTex) * _BlackColor;
			fixed4 white = tex2D(_WhiteTex, IN.uv_WhiteTex) * _WhiteColor;
			fixed4 result = lerp(black, white, mask.r);
			o.Albedo = result.rgb;
			
			// Metallic and smoothness come from slider variables
			o.Metallic = lerp (_BlackMetallic, _WhiteMetallic, mask.r);
			o.Smoothness = lerp(_BlackGlossiness, _WhiteGlossiness, mask.r);
			
			//normal comes from a texture
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
			
			o.Alpha = result.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
