
Shader "GPUInstancer/CrowdAnimations/Crowd Animation Test" {

	Properties {

		_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
		_Color ("Main Color", Color) = (1,1,1,1)
		_BumpMap("BumpMap", 2D) = "white" {}

		_gpuiAnimationTexture ("Anim Tex", 2D) = "white" {}
		_animationTextureSizeX ("Tex Size X", Float) = 0
		_animationTextureSizeY ("Tex Size Y", Float) = 0
		_totalNumberOfBones ("Total Bones", Float) = 0
		_frameIndex ("Frame Index", Float) = 0
	}

	SubShader { 
		Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
		LOD 400
	
		CGPROGRAM
		#include "UnityCG.cginc"
		#pragma multi_compile_instancing
		#pragma surface surf Lambert addshadow fullforwardshadows vertex:vertCrowdAnimTestGPUI
		#pragma target 3.5

		#ifndef GPUI_CUSTOM_INPUT
		#define GPUI_STRUCT_NAME appdata_full
		#define GPUI_BONE_INDEX texcoord2
		#define GPUI_BONE_WEIGHT texcoord3
		#define GPUI_VERTEX vertex
		#define GPUI_NORMAL normal
		#define GPUI_TANGENT tangent
		#endif

		float4 _Color;
		sampler2D _MainTex;
		sampler2D _BumpMap;

		sampler2D _gpuiAnimationTexture;
		float _animationTextureSizeX;
		float _animationTextureSizeY;
		float _totalNumberOfBones;
		float _frameIndex;
		float4x4 _bindPoseOffset;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
		};

		float4x4 getBoneMatrixFromTexture(int frameIndex, float boneIndex)
		{
			uint textureIndex = frameIndex * _totalNumberOfBones + boneIndex;

			float4 texIndex;
			texIndex.x = ((textureIndex % _animationTextureSizeX) + 0.5) / _animationTextureSizeX;
			texIndex.y = ((floor(textureIndex / _animationTextureSizeX) * 4) + 0.5) / _animationTextureSizeY;
			texIndex.z = 0;
			texIndex.w = 0;
			float offset = 1.0f / _animationTextureSizeY;

			float4x4 boneMatrix;
			boneMatrix._11_21_31_41 = tex2Dlod(_gpuiAnimationTexture, texIndex);
			texIndex.y += offset;
			boneMatrix._12_22_32_42 = tex2Dlod(_gpuiAnimationTexture, texIndex);
			texIndex.y += offset;
			boneMatrix._13_23_33_43 = tex2Dlod(_gpuiAnimationTexture, texIndex);
			texIndex.y += offset;
			boneMatrix._14_24_34_44 = tex2Dlod(_gpuiAnimationTexture, texIndex);
			return mul(boneMatrix, _bindPoseOffset);
			//return float4x4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
		}

		void applyGPUISkinning(inout GPUI_STRUCT_NAME v)
		{
			float4 boneIndexes = float4(v.GPUI_BONE_INDEX.x, v.GPUI_BONE_INDEX.y, v.GPUI_BONE_INDEX.z, v.GPUI_BONE_INDEX.w);
			float4 boneWeights = float4(v.GPUI_BONE_WEIGHT.x, v.GPUI_BONE_WEIGHT.y, v.GPUI_BONE_WEIGHT.z, v.GPUI_BONE_WEIGHT.w);
    
			uint frameStart = floor(_frameIndex);
			uint frameEnd = ceil(_frameIndex);
			float progress = frac(_frameIndex);
		
			float4x4 boneMatrix = mul(getBoneMatrixFromTexture(frameStart, boneIndexes.x), boneWeights.x * (1.0 - progress));
			boneMatrix += mul(getBoneMatrixFromTexture(frameStart, boneIndexes.y), boneWeights.y * (1.0 - progress));
			boneMatrix += mul(getBoneMatrixFromTexture(frameStart, boneIndexes.z), boneWeights.z * (1.0 - progress));
			boneMatrix += mul(getBoneMatrixFromTexture(frameStart, boneIndexes.w), boneWeights.w * (1.0 - progress));

			boneMatrix += mul(getBoneMatrixFromTexture(frameEnd, boneIndexes.x), boneWeights.x * progress);
			boneMatrix += mul(getBoneMatrixFromTexture(frameEnd, boneIndexes.y), boneWeights.y * progress);
			boneMatrix += mul(getBoneMatrixFromTexture(frameEnd, boneIndexes.z), boneWeights.z * progress);
			boneMatrix += mul(getBoneMatrixFromTexture(frameEnd, boneIndexes.w), boneWeights.w * progress);

			float3x3 boneMat3 = (float3x3) boneMatrix;
			float3 posDiff = boneMatrix._14_24_34;
			v.GPUI_VERTEX.xyz = mul(boneMat3, v.GPUI_VERTEX.xyz);
			v.GPUI_VERTEX.xyz += posDiff;
			v.GPUI_NORMAL.xyz = normalize(mul(boneMat3, v.GPUI_NORMAL.xyz));
			v.GPUI_TANGENT.xyz = normalize(mul(boneMat3, v.GPUI_TANGENT.xyz));
		}

		void vertCrowdAnimTestGPUI(inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			applyGPUISkinning(v);
		}

		void surf (Input IN, inout SurfaceOutput o) 
		{
			float4 tex = tex2D( _MainTex, IN.uv_MainTex );
			o.Albedo =  tex.rgb * _Color.rgb;
			o.Normal = UnpackNormal( tex2D( _BumpMap, IN.uv_BumpMap ) );			
		}

		ENDCG
	}
}
