// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "IDS/MaskedBumpDiffuseMetallicSmoothness" {
	Properties {
		[Header(Main mask)]
		[NoScaleOffset] _MaskTex ("Mask (RGB)", 2D) = "white" {}

		[Header(Black area settings)]
		_BlackAlbedoColor ("Albedo color", Color) = (1,1,1,1)
		_BlackAlbedoTexture ("Albedo texture", 2D) = "white" {}
		
		[Header(White area settings)]
		_WhiteAlbedoColor ("Albedo color", Color) = (1,1,1,1)
		_WhiteAlbedoTexture("Albedo texture", 2D) = "white" {}
		
		[Header(Shared settings)]
		[NoScaleOffset] _BumpMap("Bumpmap", 2D) = "bump" {}
		[NoScaleOffset] _MetallicTexture("Metallic", 2D) = "white" {}
		[NoScaleOffset] _SmoothnessTexture("Smoothness", 2D) = "white" {}
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
		
		fixed4 _BlackAlbedoColor;
		sampler2D _BlackAlbedoTexture;
		
		fixed4 _WhiteAlbedoColor;
		sampler2D _WhiteAlbedoTexture;

		sampler2D _MetallicTexture;
		sampler2D _SmoothnessTexture;
		sampler2D _BumpMap;

		struct Input {
			float2 uv_MaskTex;
			float2 uv_BlackAlbedoTexture;
			float2 uv_WhiteAlbedoTexture;
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

			o.Albedo = lerp(
				tex2D(_BlackAlbedoTexture, IN.uv_BlackAlbedoTexture) * _BlackAlbedoColor,
				tex2D(_WhiteAlbedoTexture, IN.uv_WhiteAlbedoTexture) * _WhiteAlbedoColor,
				mask.r
			);
			
			// Metallic and smoothness come from slider variables
			o.Metallic = tex2D(_MetallicTexture, IN.uv_MaskTex);
			o.Smoothness = tex2D(_SmoothnessTexture, IN.uv_MaskTex);

			//normal comes from a texture
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MaskTex));
			
			o.Alpha = 1;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
