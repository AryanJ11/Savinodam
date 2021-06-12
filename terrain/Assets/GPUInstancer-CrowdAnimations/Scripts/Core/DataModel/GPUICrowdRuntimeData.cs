#if GPU_INSTANCER
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace GPUInstancer.CrowdAnimations
{
    public class GPUICrowdRuntimeData : GPUInstancerRuntimeData
    {
        public ComputeBuffer animationDataBuffer;
        public ComputeBuffer animationBakeBuffer;
        public Vector4[] animationData;
        public Dictionary<int, GPUIAnimationClipData> animationClipDataDict;
        public Dictionary<int, GPUIAnimatorState> animatorStateDict;

        public Vector4[] crowdAnimatorControllerData; // 0 to 4: x ->  minFrame, y -> maxFrame, z -> speed, w -> unused (cycleOffset?)
        public ComputeBuffer crowdAnimatorControllerBuffer;

        public bool disableFrameLerp;
        public List<GPUICrowdAnimator> transitioningAnimators;

        public bool isUVsSet;

        public bool hasEvents;
        public Dictionary<GPUIAnimationClipData, List<GPUIAnimationEvent>> eventDict;

        public GPUICrowdRuntimeData(GPUInstancerPrototype prototype) : base(prototype)
        {
            disableFrameLerp = true;
            transitioningAnimators = new List<GPUICrowdAnimator>();
            isUVsSet = false;
        }

        public override void InitializeData()
        {
            base.InitializeData();
            GPUICrowdUtility.SetAnimationData(this);
            GPUICrowdUtility.SetAppendBuffers(this);
            GPUICrowdUtility.SetMeshUVs(this);
        }

        public override void ReleaseBuffers()
        {
            base.ReleaseBuffers();
            if (animationDataBuffer != null)
                animationDataBuffer.Release();
            animationDataBuffer = null;
            if (animationBakeBuffer != null)
                animationBakeBuffer.Release();
            animationBakeBuffer = null;
            if (crowdAnimatorControllerBuffer != null)
                crowdAnimatorControllerBuffer.Release();
            crowdAnimatorControllerBuffer = null;
        }

        public override bool GenerateLODsFromLODGroup(GPUInstancerPrototype prototype)
        {
            LODGroup lodGroup = prototype.prefabObject.GetComponent<LODGroup>();

            if (instanceLODs == null)
                instanceLODs = new List<GPUInstancerPrototypeLOD>();
            else
                instanceLODs.Clear();

            for (int lod = 0; lod < lodGroup.GetLODs().Length; lod++)
            {
                List<SkinnedMeshRenderer> lodRenderers = new List<SkinnedMeshRenderer>();
                if (lodGroup.GetLODs()[lod].renderers != null)
                {
                    foreach (Renderer renderer in lodGroup.GetLODs()[lod].renderers)
                    {
                        if (renderer != null && renderer is SkinnedMeshRenderer)
                        {
                            lodRenderers.Add((SkinnedMeshRenderer)renderer);
                        }
                    }
                }

                if (lodRenderers.Count == 0)
                {
                    Debug.LogWarning("LODGroup has no mesh renderers. Prefab: " + lodGroup.gameObject.name + " LODIndex: " + lod);
                    continue;
                }

                AddLod(lodGroup.GetLODs()[lod].screenRelativeTransitionHeight);

                for (int r = 0; r < lodRenderers.Count; r++)
                {
                    List<Material> instanceMaterials = new List<Material>();
                    for (int m = 0; m < lodRenderers[r].sharedMaterials.Length; m++)
                    {
                        instanceMaterials.Add(GPUInstancerConstants.gpuiSettings.shaderBindings.GetInstancedMaterial(lodRenderers[r].sharedMaterials[m], GPUICrowdConstants.GPUI_EXTENSION_CODE));
                        if (prototype.isLODCrossFade)
                            instanceMaterials[m].EnableKeyword("LOD_FADE_CROSSFADE");
                    }

                    Matrix4x4 transformOffset = Matrix4x4.identity;
                    //Transform currentTransform = lodRenderers[r].gameObject.transform;
                    //while (currentTransform != lodGroup.gameObject.transform)
                    //{
                    //    transformOffset = Matrix4x4.TRS(currentTransform.localPosition, currentTransform.localRotation, currentTransform.localScale) * transformOffset;
                    //    currentTransform = currentTransform.parent;
                    //}

                    MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                    lodRenderers[r].GetPropertyBlock(mpb);
                    MaterialPropertyBlock shadowMPB = null;
                    if (prototype.isShadowCasting)
                    {
                        shadowMPB = new MaterialPropertyBlock();
                        lodRenderers[r].GetPropertyBlock(shadowMPB);
                    }
                    AddRenderer(lod, lodRenderers[r].sharedMesh, instanceMaterials, transformOffset, mpb, 
                        lodRenderers[r].shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off, lodRenderers[r].gameObject.layer, shadowMPB, lodRenderers[r]);
                }
            }
            return true;
        }

        public override bool CreateRenderersFromMeshRenderers(int lod, GPUInstancerPrototype prototype)
        {
            GPUICrowdPrototype crowdPrototype = (GPUICrowdPrototype)prototype;

            if (instanceLODs == null || instanceLODs.Count <= lod || instanceLODs[lod] == null)
            {
                Debug.LogError("Can't create renderer(s): Invalid LOD");
                return false;
            }

            if (!prototype.prefabObject)
            {
                Debug.LogError("Can't create renderer(s): reference GameObject is null");
                return false;
            }

            if (crowdPrototype.animationData == null || crowdPrototype.animationData.animationTexture == null)
            {
                Debug.LogError(prototype.prefabObject.name + " requires to Bake Animations.");
                return false;
            }

            List<SkinnedMeshRenderer> skinnedMeshRenderers = new List<SkinnedMeshRenderer>();
            GetSkinnedMeshRenderersOfTransform(prototype.prefabObject.transform, skinnedMeshRenderers);

            if (skinnedMeshRenderers == null || skinnedMeshRenderers.Count == 0)
            {
                Debug.LogError("Can't create renderer(s): no SkinnedMeshRenderers found in the reference GameObject <" + prototype.prefabObject.name +
                        "> or any of its children");
                return false;
            }

            foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
            {
                if (skinnedMeshRenderer.sharedMesh == null)
                {
                    Debug.LogWarning("SkinnedMeshRenderer with no Mesh found on GameObject <" + prototype.prefabObject.name +
                        "> (Child: <" + skinnedMeshRenderer.gameObject + ">). Are you missing a mesh reference?");
                    continue;
                }
                if(instanceLODs[lod].renderers == null)
                {
                    instanceLODs[lod].renderers = new List<GPUInstancerRenderer>();
                }
                if (crowdPrototype.hasOptionalRenderers && crowdPrototype.animationData.GetSkinnedMeshDataByName(skinnedMeshRenderer.gameObject.name).isOptional)
                {
                    continue;
                }

                List<Material> instanceMaterials = new List<Material>();

                for (int m = 0; m < skinnedMeshRenderer.sharedMaterials.Length; m++)
                {
                    instanceMaterials.Add(GPUInstancerConstants.gpuiSettings.shaderBindings.GetInstancedMaterial(skinnedMeshRenderer.sharedMaterials[m], GPUICrowdConstants.GPUI_EXTENSION_CODE));
                }

                Matrix4x4 transformOffset = Matrix4x4.identity;
                //Transform currentTransform = skinnedMeshRenderer.gameObject.transform;
                //while (currentTransform != prototype.prefabObject.transform)
                //{
                //    transformOffset = Matrix4x4.TRS(currentTransform.localPosition, currentTransform.localRotation, currentTransform.localScale) * transformOffset;
                //    currentTransform = currentTransform.parent;
                //}

                MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                skinnedMeshRenderer.GetPropertyBlock(mpb);
                MaterialPropertyBlock shadowMPB = null;
                if (prototype.isShadowCasting)
                {
                    shadowMPB = new MaterialPropertyBlock();
                    skinnedMeshRenderer.GetPropertyBlock(shadowMPB);
                }
                AddRenderer(lod, skinnedMeshRenderer.sharedMesh, instanceMaterials, transformOffset, mpb, 
                    skinnedMeshRenderer.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off, skinnedMeshRenderer.gameObject.layer, shadowMPB,
                    skinnedMeshRenderer);
            }

            return true;
        }

        public override void CalculateBounds()
        {
            if (instanceLODs == null || instanceLODs.Count == 0 || instanceLODs[0].renderers == null ||
                instanceLODs[0].renderers.Count == 0)
                return;

            Bounds rendererBounds;
            GameObject tmpGO = new GameObject("TempGO");
            BoxCollider tmpCollider = tmpGO.AddComponent<BoxCollider>();
            for (int lod = 0; lod < instanceLODs.Count; lod++)
            {
                if (instanceLODs[lod].excludeBounds)
                    continue;

                for (int r = 0; r < instanceLODs[lod].renderers.Count; r++)
                {
                    SkinnedMeshRenderer smr = (SkinnedMeshRenderer)instanceLODs[lod].renderers[r].rendererRef;
                    if (smr.rootBone != null)
                    {
                        tmpGO.transform.position = smr.rootBone.position - prototype.prefabObject.transform.position;
                        tmpGO.transform.rotation = smr.rootBone.rotation * Quaternion.Inverse(prototype.prefabObject.transform.rotation);
                        tmpGO.transform.localScale = smr.rootBone.lossyScale;

                        tmpCollider.center = smr.localBounds.center;
                        tmpCollider.size = smr.localBounds.size;

                        rendererBounds = tmpCollider.bounds;
                    }
                    else
                        rendererBounds = smr.bounds;

                    if (lod == 0 && r == 0)
                    {
                        instanceBounds = rendererBounds;
                        continue;
                    }
                    instanceBounds.Encapsulate(rendererBounds);
                }
            }
            GameObject.Destroy(tmpGO);
        }

        public void GetSkinnedMeshRenderersOfTransform(Transform objectTransform, List<SkinnedMeshRenderer> skinnedMeshRenderers)
        {
            SkinnedMeshRenderer skinnedMeshRenderer = objectTransform.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
                skinnedMeshRenderers.Add(skinnedMeshRenderer);

            Transform childTransform;
            for (int i = 0; i < objectTransform.childCount; i++)
            {
                childTransform = objectTransform.GetChild(i);
                if (childTransform.GetComponent<GPUICrowdPrefab>() != null)
                    continue;
                GetSkinnedMeshRenderersOfTransform(childTransform, skinnedMeshRenderers);
            }
        }
    }

    public class GPUICrowdTransition
    {
        public int arrayIndex;
        public float startTime;
        public float totalTime;
        public int transitioningClipCount;
        public Vector4 startWeights;
        public Vector4 endWeights;
        public int endActiveClipCount;

        public GPUICrowdTransition()
        {

        }

        public void SetData(int arrayIndex, float startTime, float totalTime, int transitioningClipCount, 
            Vector4 startWeights, Vector4 endWeights, int endActiveClipCount)
        {
            this.arrayIndex = arrayIndex;
            this.startTime = startTime;
            this.totalTime = totalTime;
            this.transitioningClipCount = transitioningClipCount;
            this.startWeights = startWeights;
            this.endWeights = endWeights;
            this.endActiveClipCount = endActiveClipCount;
        }
    }

    [System.Serializable]
    public class GPUIAnimationEvent : UnityEvent<GPUICrowdPrefab, float, int, string>
    {
        public GPUICrowdPrototype prototype;
        public AnimationClip clip;
        public int eventFrame;
        public float floatParam;
        public int intParam;
        public string stringParam;

        public GPUIAnimationEvent()
        {
        }

        public GPUIAnimationEvent(GPUICrowdPrototype prototype, AnimationClip clip)
        {
            this.prototype = prototype;
            this.clip = clip;
        }
    }
}
#endif //GPU_INSTANCER