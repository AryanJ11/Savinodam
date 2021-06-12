using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if GPU_INSTANCER
namespace GPUInstancer.CrowdAnimations
{
    [ExecuteInEditMode]
    public class GPUICrowdManager : GPUInstancerPrefabManager
    {
        protected List<AnimatorClipInfo> _animatorClipInfos = new List<AnimatorClipInfo>();

        private ComputeShader _skinnedMeshAnimateComputeShader;
        private int _animateBonesKernelID;
        private int _animateBonesLerpedKernelID;
        private int _fixWeightsKernelID;

        private ComputeShader _crowdAnimatorComputeShader;
        private int _crowdAnimatorKernelID;

        private float _lastRootMotionUpdateTime;
        //private static float _rootMotionFrequency = 1.0f / 90.0f;

        private float _lastTransitionUpdateTime;
        private static float _transitionFrequency = 1.0f / 60.0f;

        private float _lastAnimateTime;

        public List<GPUIAnimationEvent> animationEvents;
#if UNITY_EDITOR
        public int selectedClipIndex;
        public bool showEventsFoldout = true;
#endif

        #region MonoBehavior Methods

        public override void Awake()
        {
            if (GPUICrowdConstants.gpuiCrowdSettings == null)
                GPUICrowdConstants.gpuiCrowdSettings = GPUICrowdSettings.GetDefaultGPUICrowdSettings();

            base.Awake();

            if (!GPUInstancerConstants.gpuiSettings.shaderBindings.HasExtension(GPUICrowdConstants.GPUI_EXTENSION_CODE))
            {
                GPUInstancerConstants.gpuiSettings.shaderBindings.AddExtension(new GPUICrowdShaderBindings());
            }
        }

        public override void Start()
        {
            base.Start();

            _skinnedMeshAnimateComputeShader = (ComputeShader)Resources.Load(GPUICrowdConstants.COMPUTE_SKINNED_MESH_ANIMATE_PATH);
            _animateBonesKernelID = _skinnedMeshAnimateComputeShader.FindKernel(GPUICrowdConstants.COMPUTE_ANIMATE_BONES_KERNEL);
            _animateBonesLerpedKernelID = _skinnedMeshAnimateComputeShader.FindKernel(GPUICrowdConstants.COMPUTE_ANIMATE_BONES_LERPED_KERNEL);
            _fixWeightsKernelID = _skinnedMeshAnimateComputeShader.FindKernel(GPUICrowdConstants.COMPUTE_FIX_WEIGHTS_KERNEL);

            _crowdAnimatorComputeShader = (ComputeShader)Resources.Load(GPUICrowdConstants.COMPUTE_CROWD_ANIMATOR_PATH);
            _crowdAnimatorKernelID = _crowdAnimatorComputeShader.FindKernel(GPUICrowdConstants.COMPUTE_CROWD_ANIMATOR_KERNEL);
        }

        public override void OnEnable()
        {
            if (Application.isPlaying)
            {
                int count = prefabList.Count;
                for (int i = 0; i < count; i++)
                {
                    GPUICrowdPrototype crowdPrototype = (GPUICrowdPrototype)prototypeList[i];
                    if (crowdPrototype.hasOptionalRenderers && crowdPrototype.animationData != null && crowdPrototype.animationData.skinnedMeshDataList != null)
                    {
                        SkinnedMeshRenderer[] skinnedMeshRenderers = crowdPrototype.prefabObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                        foreach (GPUISkinnedMeshData smd in crowdPrototype.animationData.skinnedMeshDataList)
                        {
                            if (smd.isOptional && (crowdPrototype.childPrototypes == null || !crowdPrototype.childPrototypes.ContainsKey(smd.transformName)))
                            {
                                foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
                                {
                                    if (skinnedMeshRenderer.gameObject.name.Equals(smd.transformName))
                                    {
                                        GPUICrowdPrototype generatedPrototype = DefineGameObjectAsCrowdPrototypeAtRuntime(skinnedMeshRenderer.gameObject, crowdPrototype.animationData, false,
                                            crowdPrototype);
                                        generatedPrototype.hasOptionalRenderers = false;

                                        generatedPrototype.enableRuntimeModifications = true;
                                        generatedPrototype.addRemoveInstancesAtRuntime = true;
                                        generatedPrototype.isChildPrototype = true;
                                        generatedPrototype.autoUpdateTransformData = false;

                                        if (crowdPrototype.childPrototypes == null)
                                            crowdPrototype.childPrototypes = new Dictionary<string, GPUICrowdPrototype>();
                                        crowdPrototype.childPrototypes.Add(smd.transformName, generatedPrototype);

                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            base.OnEnable();
            
            if (Application.isPlaying && animationEvents != null)
            {
                foreach (GPUIAnimationEvent animationEvent in animationEvents)
                {
                    if (animationEvent.prototype.animationData != null && animationEvent.prototype.animationData.useCrowdAnimator)
                        GPUICrowdAPI.AddAnimationEvent(this, animationEvent);
                }
            }
        }

        public override void Update()
        {
            ClearCompletedThreads();
            while (threadStartQueue.Count > 0 && activeThreads.Count < maxThreads)
            {
                GPUIThreadData threadData = threadStartQueue.Dequeue();
                threadData.thread.Start(threadData.parameter);
                activeThreads.Add(threadData.thread);
            }
            if (threadQueue.Count > 0)
            {
                Action action = threadQueue.Dequeue();
                if (action != null)
                    action.Invoke();
            }
            //_rootMotionFrequency = 1.0f / 120.0f * Time.timeScale;
            _transitionFrequency = 1.0f / 60.0f * Time.timeScale;

            float currentTime = Time.time;
            //bool calculateRootMotion = currentTime - _lastRootMotionUpdateTime > _rootMotionFrequency;
            bool calculateTransitions = currentTime - _lastTransitionUpdateTime > _transitionFrequency;

            if (runtimeDataList != null)
            {
                foreach (GPUICrowdRuntimeData runtimeData in runtimeDataList)
                {
                    GPUICrowdPrototype crowdPrototype = (GPUICrowdPrototype)runtimeData.prototype;

                    if (crowdPrototype.animationData == null)
                        continue;

                    if (crowdPrototype.autoUpdateTransformData || (crowdPrototype.animationData.applyRootMotion && !crowdPrototype.animationData.useCrowdAnimator))
                    {
                        List<GPUInstancerPrefab> prefabInstanceList = _registeredPrefabsRuntimeData[runtimeData.prototype];
                        Transform instanceTransform;
                        foreach (GPUICrowdPrefab prefabInstance in prefabInstanceList)
                        {
                            if (!prefabInstance)
                                continue;
                            instanceTransform = prefabInstance.GetInstanceTransform();
                            if (instanceTransform.hasChanged && prefabInstance.state == PrefabInstancingState.Instanced)
                            {
                                instanceTransform.hasChanged = false;
                                runtimeData.instanceDataArray[prefabInstance.gpuInstancerID - 1] = instanceTransform.localToWorldMatrix;
                                runtimeData.transformDataModified = true;
                                if (prefabInstance.childCrowdPrefabs != null)
                                {
                                    foreach (GPUICrowdPrefab childPrefabInstance in prefabInstance.childCrowdPrefabs)
                                    {
                                        if (childPrefabInstance != null && childPrefabInstance.runtimeData != null)
                                        {
                                            childPrefabInstance.runtimeData.instanceDataArray[childPrefabInstance.gpuInstancerID - 1] = instanceTransform.localToWorldMatrix;
                                            childPrefabInstance.runtimeData.transformDataModified = true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Crowd Animator Root Motion
                    if (crowdPrototype.animationData.useCrowdAnimator && runtimeData.instanceCount > 0)
                    {
                        if (crowdPrototype.animationData.applyRootMotion /*&& calculateRootMotion*/)
                        {
#if UNITY_EDITOR
                            UnityEngine.Profiling.Profiler.BeginSample("GPUI Crowd Animator Root Motion");
#endif
                            List<GPUInstancerPrefab> prefabInstanceList = _registeredPrefabsRuntimeData[crowdPrototype];
                            float lerpAmount = (currentTime - _lastRootMotionUpdateTime) * crowdPrototype.frameRate;
                            runtimeData.transformDataModified = true;
                            foreach (GPUICrowdPrefab prefabInstance in prefabInstanceList)
                            {
                                if (prefabInstance != null && prefabInstance.state == PrefabInstancingState.Instanced)
                                {
                                    prefabInstance.crowdAnimator.ApplyRootMotion(runtimeData, prefabInstance.gpuInstancerID - 1, prefabInstance.GetInstanceTransform(), currentTime, lerpAmount);
                                }
                            }
#if UNITY_EDITOR
                            UnityEngine.Profiling.Profiler.EndSample();
#endif
                        }
                        if (calculateTransitions)
                        {
                            int transitionCount = runtimeData.transitioningAnimators.Count;
                            if (transitionCount > 0)
                            {
                                for (int i = 0; i < transitionCount; i++)
                                {
                                    if (!runtimeData.transitioningAnimators[i].ApplyTransition(runtimeData))
                                    {
                                        runtimeData.transitioningAnimators.RemoveAt(i);
                                        i--;
                                        transitionCount--;
                                    }
                                }
                            }
                        }
                        if (runtimeData.hasEvents)
                        {
#if UNITY_EDITOR
                            UnityEngine.Profiling.Profiler.BeginSample("GPUI Crowd Animator Events");
#endif
                            List<GPUInstancerPrefab> prefabInstanceList = _registeredPrefabsRuntimeData[crowdPrototype];
                            foreach (GPUICrowdPrefab prefabInstance in prefabInstanceList)
                            {
                                if (prefabInstance.state == PrefabInstancingState.Instanced)
                                {
                                    prefabInstance.crowdAnimator.ApplyAnimationEvents(runtimeData, prefabInstance, currentTime, Time.deltaTime);
                                }
                            }
#if UNITY_EDITOR
                            UnityEngine.Profiling.Profiler.EndSample();
#endif
                        }
                    }

                    if (runtimeData.transformDataModified)
                    {
                        runtimeData.transformationMatrixVisibilityBuffer.SetData(runtimeData.instanceDataArray);
                        runtimeData.transformDataModified = false;
                    }
                }
            }

            //if (calculateRootMotion)
                _lastRootMotionUpdateTime = currentTime;

            if (calculateTransitions)
                _lastTransitionUpdateTime = currentTime;
        }

        public override void LateUpdate()
        {
            if (Application.isPlaying && runtimeDataList != null)
            {
                foreach (GPUICrowdRuntimeData runtimeData in runtimeDataList)
                {
                    UpdateAnimatorsData(runtimeData); // calculate baked animation
                    // Can inject code here to modify bone matrix buffers
                }
                _lastAnimateTime = Time.time;
            }
            base.LateUpdate(); // GPUI core rendering
        }
        #endregion MonoBehavior Methods

        public override void GeneratePrototypes(bool forceNew = false)
        {
            if (GPUICrowdConstants.gpuiCrowdSettings == null)
                GPUICrowdConstants.gpuiCrowdSettings = GPUICrowdSettings.GetDefaultGPUICrowdSettings();

            ClearInstancingData();

            if (forceNew || prototypeList == null)
                prototypeList = new List<GPUInstancerPrototype>();
            else
                prototypeList.RemoveAll(p => p == null);

            GPUInstancerConstants.gpuiSettings.SetDefultBindings();

            GPUICrowdUtility.SetCrowdPrefabPrototypes(gameObject, prototypeList, prefabList, forceNew);
        }

#if UNITY_EDITOR
        public override void CheckPrototypeChanges()
        {
            if (GPUICrowdConstants.gpuiCrowdSettings == null)
                GPUICrowdConstants.gpuiCrowdSettings = GPUICrowdSettings.GetDefaultGPUICrowdSettings();

            if (!GPUInstancerConstants.gpuiSettings.shaderBindings.HasExtension(GPUICrowdConstants.GPUI_EXTENSION_CODE))
            {
                GPUInstancerConstants.gpuiSettings.shaderBindings.AddExtension(new GPUICrowdShaderBindings());
            }

            if (prototypeList == null)
                GeneratePrototypes();
            else
                prototypeList.RemoveAll(p => p == null);

            if (GPUInstancerConstants.gpuiSettings != null && GPUInstancerConstants.gpuiSettings.shaderBindings != null)
            {
                GPUInstancerConstants.gpuiSettings.shaderBindings.ClearEmptyShaderInstances();
                foreach (GPUInstancerPrototype prototype in prototypeList)
                {
                    if (prototype.prefabObject != null)
                    {
                        GPUICrowdUtility.GenerateInstancedShadersForGameObject(prototype);
                        if (string.IsNullOrEmpty(prototype.warningText))
                        {
                            if (prototype.prefabObject.GetComponentInChildren<SkinnedMeshRenderer>() == null)
                            {
                                prototype.warningText = "Prefab object does not contain any Skinned Mesh Renderers.";
                            }
                        }
                    }
                }
            }
            if (GPUInstancerConstants.gpuiSettings != null && GPUInstancerConstants.gpuiSettings.billboardAtlasBindings != null)
            {
                GPUInstancerConstants.gpuiSettings.billboardAtlasBindings.ClearEmptyBillboardAtlases();
            }

            if (prefabList == null)
                prefabList = new List<GameObject>();

            prefabList.RemoveAll(p => p == null);
            prefabList.RemoveAll(p => p.GetComponent<GPUICrowdPrefab>() == null);
            prototypeList.RemoveAll(p => p == null);
            prototypeList.RemoveAll(p => !prefabList.Contains(p.prefabObject));

            if (prefabList.Count != prototypeList.Count)
                GeneratePrototypes();

            registeredPrefabs.RemoveAll(rpd => !prototypeList.Contains(rpd.prefabPrototype));
            foreach (GPUInstancerPrefabPrototype prototype in prototypeList)
            {
                if (!registeredPrefabs.Exists(rpd => rpd.prefabPrototype == prototype))
                    registeredPrefabs.Add(new RegisteredPrefabsData(prototype));
            }
        }
#endif // UNITY_EDITOR


        public override void InitializeRuntimeDataAndBuffers(bool forceNew = true)
        {
            base.InitializeRuntimeDataAndBuffers(forceNew);

            foreach (GPUICrowdPrototype prototype in _registeredPrefabsRuntimeData.Keys)
            {
                if (!prototype.animationData)
                    continue;
                GPUICrowdRuntimeData runtimeData = (GPUICrowdRuntimeData)GetRuntimeData(prototype);
                foreach (GPUICrowdPrefab instance in _registeredPrefabsRuntimeData[prototype])
                {
                    if (instance != null)
                    {
                        instance.SetupPrefabInstance(runtimeData, forceNew);
                    }
                }
            }
        }

        public override GPUInstancerRuntimeData InitializeRuntimeDataForPrefabPrototype(GPUInstancerPrefabPrototype p, int additionalBufferSize = 0)
        {
            GPUInstancerRuntimeData runtimeData = GetRuntimeData(p);
            if (runtimeData == null)
            {
                runtimeData = new GPUICrowdRuntimeData(p);
                if (!runtimeData.CreateRenderersFromGameObject(p))
                    return null;
                runtimeDataList.Add(runtimeData);
                runtimeDataDictionary.Add(p, runtimeData);
                if (p.isShadowCasting)
                {
                    runtimeData.hasShadowCasterBuffer = true;
                    if (!p.useOriginalShaderForShadow)
                    {
                        runtimeData.shadowCasterMaterial = new Material(Shader.Find(GPUInstancerConstants.SHADER_GPUI_SHADOWS_ONLY));
                    }
                }
            }

            return base.InitializeRuntimeDataForPrefabPrototype(p, additionalBufferSize);
        }

        public override void SetRenderersEnabled(GPUInstancerPrefab prefabInstance, bool enabled)
        {
            if (!prefabInstance || !prefabInstance.prefabPrototype || !prefabInstance.prefabPrototype.prefabObject)
                return;

            GPUICrowdPrototype prototype = (GPUICrowdPrototype)prefabInstance.prefabPrototype;

            if (!prototype.animationData)
                return;

            GPUICrowdPrefab crowdPrefabInstance = (GPUICrowdPrefab)prefabInstance;
            if (crowdPrefabInstance.animatorRef == null)
                crowdPrefabInstance.animatorRef = crowdPrefabInstance.GetAnimator();
            LODGroup lodGroup = prefabInstance.GetComponent<LODGroup>();

            if (lodGroup != null)
                lodGroup.enabled = enabled;

            if (crowdPrefabInstance.animatorRef != null && enabled && prototype.animationData.IsOptimizeGameObjects())
                AnimatorUtility.DeoptimizeTransformHierarchy(crowdPrefabInstance.animatorRef.gameObject);

            if (enabled && lodGroup != null && prototype.animationData.IsOptimizeGameObjects())
            {
                LOD[] lods = lodGroup.GetLODs();
                LOD lod;
                for (int l = 0; l < lodGroup.lodCount; l++)
                {
                    lod = lods[l];
                    for (int r = 0; r < lod.renderers.Length; r++)
                    {
                        if (lod.renderers[r] is SkinnedMeshRenderer)
                        {
                            if (l > 0)
                                ((SkinnedMeshRenderer)lod.renderers[r]).bones = ((SkinnedMeshRenderer)lods[0].renderers[r]).bones;
                            if (GPUInstancerUtility.IsInLayer(layerMask, lod.renderers[r].gameObject.layer))
                                lod.renderers[r].enabled = enabled;
                        }
                    }
                }
            }
            else
            {
                SkinnedMeshRenderer[] skinnedMeshRenderers = prefabInstance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                if (skinnedMeshRenderers != null && skinnedMeshRenderers.Length > 0)
                {
                    for (int mr = 0; mr < skinnedMeshRenderers.Length; mr++)
                    {
                        SkinnedMeshRenderer skinnedMeshRenderer = skinnedMeshRenderers[mr];
                        if (GPUInstancerUtility.IsInLayer(layerMask, skinnedMeshRenderer.gameObject.layer))
                            skinnedMeshRenderer.enabled = enabled;
                    }
                }
            }

            if (crowdPrefabInstance.animatorRef != null)
            {
                if (!enabled && prototype.animationData.IsOptimizeGameObjects())
                    AnimatorUtility.OptimizeTransformHierarchy(crowdPrefabInstance.animatorRef.gameObject, prototype.animationData.exposedTransforms);

                if (prototype.animationData.useCrowdAnimator)
                    crowdPrefabInstance.animatorRef.enabled = enabled;
                if (enabled)
                    crowdPrefabInstance.animatorRef.cullingMode = prototype.animatorCullingMode;
            }
        }

        public void UpdateAnimatorsData(GPUICrowdRuntimeData runtimeData)
        {
            if (runtimeData.instanceCount == 0)
                return;

            GPUICrowdPrototype prototype = (GPUICrowdPrototype)runtimeData.prototype;

            if (prototype.animationData != null && runtimeData.animationDataBuffer != null)
            {
                #region Crowd Animator
                if (prototype.animationData.useCrowdAnimator)
                {
                    _crowdAnimatorComputeShader.SetBuffer(_crowdAnimatorKernelID, GPUICrowdConstants.CrowdKernelPoperties.ANIMATION_DATA, runtimeData.animationDataBuffer);
                    _crowdAnimatorComputeShader.SetBuffer(_crowdAnimatorKernelID, GPUICrowdConstants.CrowdKernelPoperties.CROWD_ANIMATOR_CONTROLLER, runtimeData.crowdAnimatorControllerBuffer);
                    _crowdAnimatorComputeShader.SetInt(GPUICrowdConstants.CrowdKernelPoperties.INSTANCE_COUNT, runtimeData.instanceCount);
                    _crowdAnimatorComputeShader.SetFloat(GPUICrowdConstants.CrowdKernelPoperties.CURRENT_TIME, Time.time);
                    _crowdAnimatorComputeShader.SetInt(GPUICrowdConstants.CrowdKernelPoperties.FRAME_RATE, prototype.frameRate);

                    _crowdAnimatorComputeShader.Dispatch(_crowdAnimatorKernelID,
                        Mathf.CeilToInt(runtimeData.instanceCount / GPUInstancerConstants.COMPUTE_SHADER_THREAD_COUNT), 1, 1);
                }
                #endregion Crowd Animator

                #region Mecanim Animator
                else
                {
#if UNITY_EDITOR
                    UnityEngine.Profiling.Profiler.BeginSample("GPUI UpdateAnimatorsData");
#endif
                    
                    foreach (GPUICrowdPrefab prefabInstance in _registeredPrefabsRuntimeData[runtimeData.prototype])
                    {
                        if (prefabInstance.state == PrefabInstancingState.Instanced)
                        {
                            prefabInstance.mecanimAnimator.UpdateDataFromMecanimAnimator(runtimeData, prefabInstance.gpuInstancerID - 1, prefabInstance.animatorRef, _animatorClipInfos);
                        }
                    }
                    runtimeData.animationDataBuffer.SetData(runtimeData.animationData);

#if UNITY_EDITOR
                    UnityEngine.Profiling.Profiler.EndSample();
#endif
                }
                #endregion Mecanim Animator

                #region Fix Weights
                // Fix weights
                _skinnedMeshAnimateComputeShader.SetBuffer(_fixWeightsKernelID, GPUICrowdConstants.CrowdKernelPoperties.ANIMATION_DATA, runtimeData.animationDataBuffer);
                _skinnedMeshAnimateComputeShader.SetInt(GPUICrowdConstants.CrowdKernelPoperties.INSTANCE_COUNT, runtimeData.instanceCount);

                _skinnedMeshAnimateComputeShader.Dispatch(_fixWeightsKernelID,
                    Mathf.CeilToInt(runtimeData.instanceCount / GPUInstancerConstants.COMPUTE_SHADER_THREAD_COUNT), 1, 1);
                #endregion Fix Weights

                #region Apply Bone Transforms
                int kernelID = _animateBonesLerpedKernelID;
                if (runtimeData.disableFrameLerp)
                {
                    kernelID = _animateBonesKernelID;
                    runtimeData.disableFrameLerp = false;
                }
                // Apply bone transforms
                _skinnedMeshAnimateComputeShader.SetBuffer(kernelID, GPUICrowdConstants.CrowdKernelPoperties.ANIMATION_DATA, runtimeData.animationDataBuffer);
                _skinnedMeshAnimateComputeShader.SetBuffer(kernelID, GPUICrowdConstants.CrowdKernelPoperties.ANIMATION_BUFFER, runtimeData.animationBakeBuffer);
                _skinnedMeshAnimateComputeShader.SetTexture(kernelID, GPUICrowdConstants.CrowdKernelPoperties.ANIMATION_TEXTURE, prototype.animationData.animationTexture);
                _skinnedMeshAnimateComputeShader.SetInt(GPUICrowdConstants.CrowdKernelPoperties.ANIMATION_TEXTURE_SIZE_X, prototype.animationData.textureSizeX);
                _skinnedMeshAnimateComputeShader.SetInt(GPUICrowdConstants.CrowdKernelPoperties.TOTAL_NUMBER_OF_FRAMES, prototype.animationData.totalFrameCount);
                _skinnedMeshAnimateComputeShader.SetInt(GPUICrowdConstants.CrowdKernelPoperties.TOTAL_NUMBER_OF_BONES, prototype.animationData.totalBoneCount);
                _skinnedMeshAnimateComputeShader.SetInt(GPUICrowdConstants.CrowdKernelPoperties.INSTANCE_COUNT, runtimeData.instanceCount);
                _skinnedMeshAnimateComputeShader.SetFloat(GPUICrowdConstants.CrowdKernelPoperties.DELTA_TIME, Time.time - _lastAnimateTime);
                _skinnedMeshAnimateComputeShader.SetInt(GPUICrowdConstants.CrowdKernelPoperties.FRAME_RATE, prototype.frameRate);

                _skinnedMeshAnimateComputeShader.Dispatch(kernelID,
                    Mathf.CeilToInt(runtimeData.instanceCount / GPUInstancerConstants.COMPUTE_SHADER_THREAD_COUNT_2D),
                    Mathf.CeilToInt(prototype.animationData.totalBoneCount / GPUInstancerConstants.COMPUTE_SHADER_THREAD_COUNT_2D),
                    1);

                #endregion Apply Bone Transforms
            }
        }

        public GPUICrowdPrototype DefineGameObjectAsCrowdPrototypeAtRuntime(GameObject prototypeGameObject, GPUICrowdAnimationData animationData, bool attachScript = true, 
            GPUICrowdPrototype copySettingsFrom = null)
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("DefineGameObjectAsCrowdPrototypeAtRuntime method is designed to use at runtime. Prototype generation canceled.");
                return null;
            }

            if (prefabList == null)
                prefabList = new List<GameObject>();
            GPUICrowdPrototype crowdPrototype = GPUICrowdUtility.GenerateCrowdPrototype(prototypeGameObject, false, attachScript, copySettingsFrom);
            if (!prototypeList.Contains(crowdPrototype))
                prototypeList.Add(crowdPrototype);
            if (!prefabList.Contains(prototypeGameObject))
                prefabList.Add(prototypeGameObject);
            if (crowdPrototype.minCullingDistance < minCullingDistance)
                crowdPrototype.minCullingDistance = minCullingDistance;
            crowdPrototype.animationData = animationData;

            return crowdPrototype;
        }

        public override void DeletePrototype(GPUInstancerPrototype prototype, bool removeSO = true)
        {
            GPUICrowdPrototype crowdPrototype = (GPUICrowdPrototype)prototype;
#if UNITY_EDITOR
            if (crowdPrototype.animationData != null && EditorUtility.DisplayDialog("Remove Animation Data", "Do you wish to delete baked Animation data?", "Yes", "No"))
            {
                if (crowdPrototype.animationData.animationTexture != null)
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(crowdPrototype.animationData.animationTexture));
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(crowdPrototype.animationData));
            }
#endif

            base.DeletePrototype(prototype, removeSO);
        }
    }
}
#else //GPU_INSTANCER
namespace GPUInstancer.CrowdAnimations
{
    public class GPUICrowdManager : MonoBehaviour
    {
    }
}
#endif //GPU_INSTANCER