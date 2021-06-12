using UnityEngine;

namespace GPUInstancer.CrowdAnimations
{
    public static class GPUICrowdConstants
    {
#if GPU_INSTANCER
        public static GPUICrowdSettings gpuiCrowdSettings;
        public static readonly string GPUI_EXTENSION_CODE = "CROWD_ANIM";

        #region CS Skinned Mesh
        public static readonly string COMPUTE_SKINNED_MESH_BAKE_PATH = "Compute/CSSkinnedMeshBake";
        public static readonly string COMPUTE_ANIMATION_TO_TEXTURE_KERNEL = "CSAnimationToTexture";
        public static readonly string COMPUTE_ANIMATION_TEXTURE_TO_BUFFER_KERNEL = "CSAnimationTextureToBuffer";
        
        public static readonly string COMPUTE_SKINNED_MESH_ANIMATE_PATH = "Compute/CSSkinnedMeshAnimate";
        public static readonly string COMPUTE_ANIMATE_BONES_KERNEL = "CSAnimateBones";
        public static readonly string COMPUTE_ANIMATE_BONES_LERPED_KERNEL = "CSAnimateBonesLerped";
        public static readonly string COMPUTE_FIX_WEIGHTS_KERNEL = "CSBonesFixWeights";

        public static readonly string COMPUTE_CROWD_ANIMATOR_PATH = "Compute/CSCrowdAnimatorController";
        public static readonly string COMPUTE_CROWD_ANIMATOR_KERNEL = "CSCrowdAnimatorController";

        public static class CrowdKernelPoperties
        {
            public static readonly int ANIMATION_DATA = Shader.PropertyToID("gpuiAnimationData");
            public static readonly int ANIMATION_BUFFER = Shader.PropertyToID("gpuiAnimationBuffer");
            public static readonly int ANIMATION_TEXTURE = Shader.PropertyToID("gpuiAnimationTexture");
            public static readonly int ANIMATION_TEXTURE_SIZE_X = Shader.PropertyToID("animationTextureSizeX");
            public static readonly int TOTAL_NUMBER_OF_FRAMES = Shader.PropertyToID("totalNumberOfFrames");
            public static readonly int TOTAL_NUMBER_OF_BONES = Shader.PropertyToID("totalNumberOfBones");
            public static readonly int INSTANCE_COUNT = Shader.PropertyToID("instanceCount");
            public static readonly int DELTA_TIME = Shader.PropertyToID("deltaTime");
            public static readonly int CURRENT_TIME = Shader.PropertyToID("currentTime");
            public static readonly int FRAME_RATE = Shader.PropertyToID("frameRate");
            public static readonly int CROWD_ANIMATOR_CONTROLLER = Shader.PropertyToID("gpuiCrowdAnimatorController");
            public static readonly int BINDPOSE_OFFSET = Shader.PropertyToID("bindPoseOffset");
        }

        public static readonly string KEYWORD_GPUI_CA_BINDPOSEOFFSET = "GPUI_CA_BINDPOSEOFFSET";

        #endregion CS Skinned Mesh

        #region Shaders
        public static readonly string SHADER_UNITY_AUTODESK_INTERACTIVE = "Autodesk Interactive";
        public static readonly string SHADER_UNITY_HDRP_LIT = "HDRP/Lit";
        public static readonly string SHADER_UNITY_LWRP_LIT = "Lightweight Render Pipeline/Lit";
        public static readonly string SHADER_UNITY_URP_LIT = "Universal Render Pipeline/Lit";

        public static readonly string SHADER_GPUI_CROWD_STANDARD = "GPUInstancer/CrowdAnimations/Standard";
        public static readonly string SHADER_GPUI_CROWD_STANDARD_SPECULAR = "GPUInstancer/CrowdAnimations/Standard (Specular setup)";
        public static readonly string SHADER_GPUI_CROWD_AUTODESK_INTERACTIVE = "GPUInstancer/CrowdAnimations/Autodesk Interactive";
        public static readonly string SHADER_GPUI_CROWD_HDRP_LIT = "GPUInstancer/CrowdAnimations/HDRP/Lit";
        public static readonly string SHADER_GPUI_CROWD_LWRP_LIT = "GPUInstancer/CrowdAnimations/Lightweight Render Pipeline/Lit";
        public static readonly string SHADER_GPUI_CROWD_URP_LIT = "GPUInstancer/CrowdAnimations/Universal Render Pipeline/Lit";
        public static readonly string SHADER_GPUI_CROWD_TEST = "GPUInstancer/CrowdAnimations/Crowd Animation Test";
        public static readonly string SHADER_GPUI_CROWD_TEST_HDRP = "GPUInstancer/CrowdAnimations/HDRP/Crowd Animation Test";
        public static readonly string SHADER_GPUI_CROWD_TEST_LWRP = "GPUInstancer/CrowdAnimations/Lightweight Render Pipeline/Crowd Animation Test";
        public static readonly string SHADER_GPUI_CROWD_TEST_URP = "GPUInstancer/CrowdAnimations/Universal Render Pipeline/Crowd Animation Test";

        #endregion Shaders

        #region Paths
        // GPUInstancer Default Paths
        public static readonly string DEFAULT_PATH_GUID = "3ac41bd0ad94c784e83f5e717440e9ed";
        public static readonly string RESOURCES_PATH = "Resources/";
        public static readonly string SHADERS_PATH = "Shaders/";

        public static readonly string GPUI_SETTINGS_DEFAULT_NAME = "GPUICrowdSettings";
        public static readonly string PROTOTYPES_ANIMATION_TEXTURES_PATH = "PrototypeData/Crowd/Textures/";
        public static readonly string PROTOTYPES_CROWD_PATH = "PrototypeData/Crowd/Prefab/";
        public static readonly string PROTOTYPES_CROWD__ANIMATION_DATA_PATH = "PrototypeData/Crowd/AnimationData/";

        private static string _defaultPath;
        public static string GetDefaultPath()
        {
            if (string.IsNullOrEmpty(_defaultPath))
            {
#if UNITY_EDITOR
                _defaultPath = UnityEditor.AssetDatabase.GUIDToAssetPath(DEFAULT_PATH_GUID);
                if (!string.IsNullOrEmpty(_defaultPath) && !_defaultPath.EndsWith("/"))
                    _defaultPath = _defaultPath + "/";
#endif
                if (string.IsNullOrEmpty(_defaultPath))
                    _defaultPath = "Assets/GPUInstancer/Extensions/CrowdAnimations/";
            }
            return _defaultPath;
        }
        #endregion Paths

        #region Texts
#if UNITY_EDITOR
        // Editor Texts
        public static readonly string TEXT_PREFAB_NO_SKINNEDMESH = "GPUI Crowd Manager only accepts prefabs with a Skinned Mesh Renderer component.";
        public static readonly string TEXT_PREFAB_NO_ANIMATOR = "GPUI Crowd Manager only accepts prefabs with an Animator component.";
        public static readonly string TEXT_PREFAB_WITH_CHILD_ANIMATOR = "GPUI Crowd Manager only accepts prefabs with an Animator component on the parent GameObject of the prefab.";
#endif
        #endregion Texts
#endif //GPU_INSTANCER

        public static readonly string ERROR_GPUI_Dependency = "GPU Instancer asset is required for Crowd Animations. Please download the latest version of the GPU Instancer from the Asset Store Window and reimport Crowd Animations.";
    }
}