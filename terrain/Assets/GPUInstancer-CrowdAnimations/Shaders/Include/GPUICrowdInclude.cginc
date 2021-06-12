
#include "./../../../GPUInstancer/Shaders/Include/GPUInstancerInclude.cginc"

#ifndef GPUI_CROWD_ANIMATIONS_INCLUDED
#define GPUI_CROWD_ANIMATIONS_INCLUDED

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
uniform StructuredBuffer<float4x4> gpuiAnimationBuffer;
uniform float totalNumberOfBones;
uniform float4x4 bindPoseOffset;

#ifndef GPUI_CUSTOM_INPUT
#define GPUI_STRUCT_NAME appdata_full
#define GPUI_BONE_INDEX texcoord2
#define GPUI_BONE_WEIGHT texcoord3
#define GPUI_VERTEX vertex
#define GPUI_NORMAL normal
#define GPUI_TANGENT tangent
#endif


void gpuiAnimate(inout GPUI_STRUCT_NAME v)
{
    uint addition = gpuiTransformationMatrix[unity_InstanceID] * totalNumberOfBones;

    // required for some shader compilers like PSSL
    float4x4 weightX = float4x4(
            v.GPUI_BONE_WEIGHT.x, 0, 0, 0, 
            0, v.GPUI_BONE_WEIGHT.x, 0, 0, 
            0, 0, v.GPUI_BONE_WEIGHT.x, 0, 
            0, 0, 0, v.GPUI_BONE_WEIGHT.x
        );
    float4x4 weightY = float4x4(
            v.GPUI_BONE_WEIGHT.y, 0, 0, 0, 
            0, v.GPUI_BONE_WEIGHT.y, 0, 0, 
            0, 0, v.GPUI_BONE_WEIGHT.y, 0, 
            0, 0, 0, v.GPUI_BONE_WEIGHT.y
        );
    float4x4 weightZ = float4x4(
            v.GPUI_BONE_WEIGHT.z, 0, 0, 0, 
            0, v.GPUI_BONE_WEIGHT.z, 0, 0, 
            0, 0, v.GPUI_BONE_WEIGHT.z, 0, 
            0, 0, 0, v.GPUI_BONE_WEIGHT.z
        );
    float4x4 weightW = float4x4(
            v.GPUI_BONE_WEIGHT.w, 0, 0, 0, 
            0, v.GPUI_BONE_WEIGHT.w, 0, 0, 
            0, 0, v.GPUI_BONE_WEIGHT.w, 0, 
            0, 0, 0, v.GPUI_BONE_WEIGHT.w
        );

#ifdef GPUI_CA_BINDPOSEOFFSET
    float4x4 boneMatrix = mul(mul(gpuiAnimationBuffer[addition + v.GPUI_BONE_INDEX.x], bindPoseOffset), weightX);
    boneMatrix += mul(mul(gpuiAnimationBuffer[addition + v.GPUI_BONE_INDEX.y], bindPoseOffset), weightY);
    boneMatrix += mul(mul(gpuiAnimationBuffer[addition + v.GPUI_BONE_INDEX.z], bindPoseOffset), weightZ);
    boneMatrix += mul(mul(gpuiAnimationBuffer[addition + v.GPUI_BONE_INDEX.w], bindPoseOffset), weightW);
#else
    float4x4 boneMatrix = mul(gpuiAnimationBuffer[addition + v.GPUI_BONE_INDEX.x], weightX);
    boneMatrix += mul(gpuiAnimationBuffer[addition + v.GPUI_BONE_INDEX.y], weightY);
    boneMatrix += mul(gpuiAnimationBuffer[addition + v.GPUI_BONE_INDEX.z], weightZ);
    boneMatrix += mul(gpuiAnimationBuffer[addition + v.GPUI_BONE_INDEX.w], weightW);	
#endif

    float3x3 boneMat3 = (float3x3) boneMatrix;
    float3 posDiff = boneMatrix._14_24_34;
    v.GPUI_VERTEX.xyz = mul(boneMat3, v.GPUI_VERTEX.xyz);
    v.GPUI_VERTEX.xyz += posDiff;
    v.GPUI_NORMAL.xyz = normalize(mul(boneMat3, v.GPUI_NORMAL.xyz));
    v.GPUI_TANGENT.xyz = normalize(mul(boneMat3, v.GPUI_TANGENT.xyz));
}
#define GPUI_CROWD_VERTEX(v) gpuiAnimate(v)
#else
#define GPUI_CROWD_VERTEX(v)
#endif // UNITY_PROCEDURAL_INSTANCING_ENABLED
#endif // GPUI_CROWD_ANIMATIONS_INCLUDED