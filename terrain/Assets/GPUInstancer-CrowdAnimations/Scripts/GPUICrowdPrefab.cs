#if GPU_INSTANCER
using System.Collections.Generic;
using UnityEngine;

namespace GPUInstancer.CrowdAnimations
{
    public class GPUICrowdPrefab : GPUInstancerPrefab
    {
        public GPUICrowdRuntimeData runtimeData;
        public Animator animatorRef;

        public GPUIMecanimAnimator mecanimAnimator;
        public GPUICrowdAnimator crowdAnimator;

        public List<GPUICrowdPrefab> childCrowdPrefabs;
        public GPUICrowdPrefab parentCrowdPrefab;

        public override void SetupPrefabInstance(GPUInstancerRuntimeData runtimeData, bool forceNew = false)
        {
            this.runtimeData = (GPUICrowdRuntimeData)runtimeData;
            if (animatorRef == null)
                animatorRef = GetAnimator();

            GPUICrowdPrototype prototype = (GPUICrowdPrototype)prefabPrototype;
            if (parentCrowdPrefab != null)
            {
                crowdAnimator = parentCrowdPrefab.crowdAnimator;
                crowdAnimator.UpdateIndex(this.runtimeData, gpuInstancerID - 1);
                return;
            }

            if (prototype.animationData.useCrowdAnimator)
            {
                if (crowdAnimator == null)
                    crowdAnimator = new GPUICrowdAnimator();
                if (crowdAnimator.activeClipCount == 0)
                    crowdAnimator.StartAnimation(this.runtimeData, gpuInstancerID - 1, prototype.animationData.clipDataList[prototype.animationData.crowdAnimatorDefaultClip].animationClip);
                else
                    crowdAnimator.UpdateIndex(this.runtimeData, gpuInstancerID - 1);
            }
            else
            {
                mecanimAnimator = new GPUIMecanimAnimator(GetAnimator());
                if (animatorRef != null)
                {
                    animatorRef.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    animatorRef.applyRootMotion = prototype.animationData.applyRootMotion;
                }
            }

            if (prototype.hasOptionalRenderers && prototype.childPrototypes != null && prototype.childPrototypes.Count > 0)
            {
                foreach (string key in prototype.childPrototypes.Keys)
                {
                    Transform child = transform.Find(key);
                    if (child != null)
                    {
                        GPUICrowdPrefab childCrowdPrefab = child.gameObject.GetComponent<GPUICrowdPrefab>();
                        if (childCrowdPrefab == null)
                        {
                            childCrowdPrefab = child.gameObject.AddComponent<GPUICrowdPrefab>();
                            childCrowdPrefab.prefabPrototype = prototype.childPrototypes[key];

                            childCrowdPrefab.parentCrowdPrefab = this;
                            if (childCrowdPrefabs == null)
                                childCrowdPrefabs = new List<GPUICrowdPrefab>();
                            childCrowdPrefabs.Add(childCrowdPrefab);

                            child.gameObject.AddComponent<GPUInstancerPrefabRuntimeHandler>();
                        }
                    }
                }
            }
        }

        public Animator GetAnimator()
        {
            return GetComponent<Animator>();
        }

        public override Transform GetInstanceTransform(bool forceNew = false)
        {
            if (parentCrowdPrefab != null)
                return parentCrowdPrefab.GetInstanceTransform(forceNew);
            if (!_isTransformSet || forceNew)
            {
                _instanceTransform = transform;
                _instanceTransform.hasChanged = false;
                _isTransformSet = true;
            }
            return _instanceTransform;
        }

        #region Crowd Animator Workflow
        public void StartAnimation(AnimationClip animationClip, float startTime = -1.0f, float speed = 1.0f, float transitionTime = 0)
        {
            if (parentCrowdPrefab != null)
            {
                Debug.LogError("Can not change animations on a child prototype.");
                return;
            }
            crowdAnimator.StartAnimation(runtimeData, gpuInstancerID - 1, animationClip, startTime, speed, transitionTime);
            UpdateChildren();
        }

        public void StartBlend(Vector4 animationWeights,
            AnimationClip animationClip1, AnimationClip animationClip2, AnimationClip animationClip3 = null, AnimationClip animationClip4 = null,
            float[] animationTimes = null, float[] animationSpeeds = null, float transitionTime = 0)
        {
            if (parentCrowdPrefab != null)
            {
                Debug.LogError("Can not change animations on a child prototype.");
                return;
            }
            crowdAnimator.StartBlend(runtimeData, gpuInstancerID - 1, animationWeights, animationClip1, animationClip2, animationClip3, animationClip4, animationTimes, animationSpeeds, transitionTime);
            UpdateChildren();
        }

        public void SetAnimationWeights(Vector4 animationWeights)
        {
            if (parentCrowdPrefab != null)
            {
                Debug.LogError("Can not change animations on a child prototype.");
                return;
            }
            crowdAnimator.SetAnimationWeights(runtimeData, gpuInstancerID - 1, animationWeights);
            UpdateChildren();
        }

        public void SetAnimationSpeed(float animationSpeed)
        {
            if (parentCrowdPrefab != null)
            {
                Debug.LogError("Can not change animations on a child prototype.");
                return;
            }
            crowdAnimator.SetAnimationSpeed(runtimeData, gpuInstancerID - 1, animationSpeed);
            UpdateChildren();
        }

        public void SetAnimationSpeeds(float[] animationSpeeds)
        {
            if (parentCrowdPrefab != null)
            {
                Debug.LogError("Can not change animations on a child prototype.");
                return;
            }
            crowdAnimator.SetAnimationSpeeds(runtimeData, gpuInstancerID - 1, animationSpeeds);
            UpdateChildren();
        }

        public float GetAnimationTime(AnimationClip animationClip)
        {
            return crowdAnimator.GetClipTime(runtimeData, animationClip);
        }

        public void SetAnimationTime(AnimationClip animationClip, float time)
        {
            if (parentCrowdPrefab != null)
            {
                Debug.LogError("Can not change animations on a child prototype.");
                return;
            }
            crowdAnimator.SetClipTime(runtimeData, gpuInstancerID - 1, animationClip, time);
            UpdateChildren();
        }

        public void UpdateChildren()
        {
            if (childCrowdPrefabs != null)
            {
                foreach (GPUICrowdPrefab childPrefab in childCrowdPrefabs)
                {
                    if (childPrefab != null && childPrefab.state == PrefabInstancingState.Instanced && childPrefab.runtimeData != null && childPrefab.gpuInstancerID > 0)
                        crowdAnimator.UpdateIndex(childPrefab.runtimeData, childPrefab.gpuInstancerID - 1);
                }
            }
        }
        #endregion Crowd Animator Workflow
    }
}
#endif //GPU_INSTANCER