#if GPU_INSTANCER
using System.Collections.Generic;
using UnityEngine;

namespace GPUInstancer.CrowdAnimations
{
    public class GPUICrowdPrefabDebugger : MonoBehaviour
    {
        [HideInInspector]
        public GPUICrowdPrototype crowdPrototype;
        [HideInInspector]
        public List<Material> testMaterials;
        [HideInInspector]
        public float frameIndex;

        public void OnFrameIndexChanged()
        {
            if (frameIndex < 0 || frameIndex > crowdPrototype.animationData.totalFrameCount)
                frameIndex = 0;

            foreach (Material mat in testMaterials)
            {
                mat.SetTexture("_gpuiAnimationTexture", crowdPrototype.animationData.animationTexture);
                mat.SetFloat("_animationTextureSizeX", crowdPrototype.animationData.textureSizeX);
                mat.SetFloat("_animationTextureSizeY", crowdPrototype.animationData.textureSizeY);
                mat.SetFloat("_totalNumberOfBones", crowdPrototype.animationData.totalBoneCount);
                mat.SetFloat("_frameIndex", frameIndex);
            }
        }
    }
}
#endif //GPU_INSTANCER