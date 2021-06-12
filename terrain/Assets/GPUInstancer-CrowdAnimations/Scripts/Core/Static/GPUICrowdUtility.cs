#if GPU_INSTANCER
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GPUInstancer.CrowdAnimations
{
    public static class GPUICrowdUtility
    {
        #region Set Animation Data
        public static void SetAnimationData(GPUICrowdRuntimeData runtimeData)
        {
            GPUICrowdPrototype prototype = runtimeData.prototype as GPUICrowdPrototype;
            if (prototype.animationData != null && prototype.animationData.animationTexture != null)
            {
                if (runtimeData.animationData == null || runtimeData.animationData.Length != runtimeData.bufferSize * 2)
                {
                    Vector4[] previousAnimationDataArray = null;
                    if (runtimeData.animationData != null)
                        previousAnimationDataArray = runtimeData.animationData;
                    runtimeData.animationData = new Vector4[runtimeData.bufferSize * 2];
                    if (previousAnimationDataArray != null)
                        Array.Copy(previousAnimationDataArray, runtimeData.animationData, previousAnimationDataArray.Length);
                }
                if (runtimeData.animationDataBuffer == null || runtimeData.animationDataBuffer.count != runtimeData.bufferSize * 2)
                {
                    if (runtimeData.animationDataBuffer != null)
                        runtimeData.animationDataBuffer.Release();
                    runtimeData.animationDataBuffer = new ComputeBuffer(runtimeData.bufferSize * 2, GPUInstancerConstants.STRIDE_SIZE_FLOAT4);
                    runtimeData.animationDataBuffer.SetData(runtimeData.animationData);
                }
                if (runtimeData.animationBakeBuffer == null || runtimeData.animationBakeBuffer.count != prototype.animationData.totalBoneCount * runtimeData.bufferSize)
                {
                    if (runtimeData.animationBakeBuffer != null)
                        runtimeData.animationBakeBuffer.Release();
                    runtimeData.animationBakeBuffer = new ComputeBuffer(prototype.animationData.totalBoneCount * runtimeData.bufferSize, GPUInstancerConstants.STRIDE_SIZE_MATRIX4X4);
                }
                    

                if (prototype.animationData.useCrowdAnimator)
                {
                    if (runtimeData.crowdAnimatorControllerBuffer == null || runtimeData.crowdAnimatorControllerBuffer.count != runtimeData.bufferSize * 4)
                    {
                        Vector4[] previousControllerDataArray = null;
                        if (runtimeData.animationData != null)
                            previousControllerDataArray = runtimeData.crowdAnimatorControllerData;
                        runtimeData.crowdAnimatorControllerData = new Vector4[runtimeData.bufferSize * 4];
                        if (previousControllerDataArray != null)
                            Array.Copy(previousControllerDataArray, runtimeData.crowdAnimatorControllerData, previousControllerDataArray.Length);


                        if (runtimeData.crowdAnimatorControllerBuffer != null)
                            runtimeData.crowdAnimatorControllerBuffer.Release();
                        runtimeData.crowdAnimatorControllerBuffer = new ComputeBuffer(runtimeData.bufferSize * 4, GPUInstancerConstants.STRIDE_SIZE_FLOAT4);
                        runtimeData.crowdAnimatorControllerBuffer.SetData(runtimeData.crowdAnimatorControllerData);
                    }
                }

                runtimeData.animationClipDataDict = new Dictionary<int, GPUIAnimationClipData>();
                foreach (GPUIAnimationClipData clipData in prototype.animationData.clipDataList)
                    runtimeData.animationClipDataDict.Add(clipData.animationClip.GetHashCode(), clipData);
                runtimeData.animatorStateDict = new Dictionary<int, GPUIAnimatorState>();
                foreach (GPUIAnimatorState state in prototype.animationData.states)
                    runtimeData.animatorStateDict.Add(state.hashCode, state);
            }
        }

        public static void SetAppendBuffers(GPUICrowdRuntimeData runtimeData)
        {
            GPUICrowdPrototype prototype = runtimeData.prototype as GPUICrowdPrototype;

            if (prototype.animationData == null || prototype.animationData.animationTexture == null)
            {
                Debug.LogError("Prototype has no baked animations: " + prototype.prefabObject.name);
                return;
            }

            foreach (GPUInstancerPrototypeLOD gpuiLod in runtimeData.instanceLODs)
            {
                foreach (GPUInstancerRenderer renderer in gpuiLod.renderers)
                {
                    GPUISkinnedMeshData smd = prototype.animationData.GetSkinnedMeshDataByName(renderer.rendererRef.gameObject.name);
                    if (prototype.animationData != null && prototype.animationData.animationTexture != null)
                    {
                        renderer.mpb.SetBuffer(GPUICrowdConstants.CrowdKernelPoperties.ANIMATION_BUFFER, runtimeData.animationBakeBuffer);
                        renderer.mpb.SetFloat(GPUICrowdConstants.CrowdKernelPoperties.TOTAL_NUMBER_OF_BONES, prototype.animationData.totalBoneCount);
                        if (smd.hasBindPoseOffset)
                        {
                            for (int i = 0; i < renderer.materials.Count; i++)
                            {
                                renderer.materials[i].EnableKeyword(GPUICrowdConstants.KEYWORD_GPUI_CA_BINDPOSEOFFSET);
                            }
                            renderer.mpb.SetMatrix(GPUICrowdConstants.CrowdKernelPoperties.BINDPOSE_OFFSET, smd.bindPoseOffset);
                        }
                    }
                    if (runtimeData.hasShadowCasterBuffer)
                    {
                        if (prototype.animationData != null && prototype.animationData.animationTexture != null)
                        {
                            renderer.shadowMPB.SetBuffer(GPUICrowdConstants.CrowdKernelPoperties.ANIMATION_BUFFER, runtimeData.animationBakeBuffer);
                            renderer.shadowMPB.SetFloat(GPUICrowdConstants.CrowdKernelPoperties.TOTAL_NUMBER_OF_BONES, prototype.animationData.totalBoneCount);
                            if (smd.hasBindPoseOffset)
                            {
                                renderer.shadowMPB.SetMatrix(GPUICrowdConstants.CrowdKernelPoperties.BINDPOSE_OFFSET, smd.bindPoseOffset);
                            }
                        }
                    }
                }
            }
        }

        public static void SetMeshUVs(GPUICrowdRuntimeData runtimeData)
        {
            GPUICrowdPrototype prototype = runtimeData.prototype as GPUICrowdPrototype;
            if (prototype.animationData == null || prototype.animationData.skinnedMeshDataList == null || runtimeData.isUVsSet)
                return;
            foreach (GPUInstancerPrototypeLOD gpuiLod in runtimeData.instanceLODs)
            {
                for (int i = 0; i < gpuiLod.renderers.Count; i++)
                {
                    if (prototype.animationData.skinnedMeshDataList.Count <= i)
                        break;
                    GPUInstancerRenderer renderer = gpuiLod.renderers[i];
                    GPUISkinnedMeshData smd = prototype.animationData.GetSkinnedMeshDataByName(renderer.rendererRef.gameObject.name);
                    renderer.mesh = GenerateMeshWithUVs(renderer.mesh, smd);
                }
            }
            runtimeData.isUVsSet = true;
        }

        public static Mesh GenerateMeshWithUVs(Mesh originalMesh, GPUISkinnedMeshData smd)
        {
            List<Vector4> boneIndexes = new List<Vector4>();
            List<Vector4> boneWeights = new List<Vector4>();
            Vector4 boneIndexVector = Vector4.zero;
            Vector4 boneWeightVector = Vector4.zero;
            foreach (BoneWeight boneWeight in originalMesh.boneWeights)
            {
                boneIndexVector.x = smd.boneIndexes[boneWeight.boneIndex0];
                boneIndexVector.y = smd.boneIndexes[boneWeight.boneIndex1];
                boneIndexVector.z = smd.boneIndexes[boneWeight.boneIndex2];
                boneIndexVector.w = smd.boneIndexes[boneWeight.boneIndex3];
                boneIndexes.Add(boneIndexVector);

                boneWeightVector.x = boneWeight.weight0;
                boneWeightVector.y = boneWeight.weight1;
                boneWeightVector.z = boneWeight.weight2;
                boneWeightVector.w = boneWeight.weight3;
                boneWeights.Add(boneWeightVector);
            }
            Mesh mesh = new Mesh();
            mesh.name = originalMesh.name + "_GPUI_CA";
            mesh.subMeshCount = originalMesh.subMeshCount;
            mesh.vertices = originalMesh.vertices;

            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                mesh.SetTriangles(originalMesh.GetTriangles(i), i);
            }

            mesh.normals = originalMesh.normals;
            mesh.tangents = originalMesh.tangents;
            mesh.colors = originalMesh.colors;
            mesh.uv = originalMesh.uv;
            mesh.SetUVs(2, boneIndexes);
            mesh.SetUVs(3, boneWeights);

            return mesh;
        }
        #endregion Set Animation Data

        #region Create Crowd Prototypes
        public static void SetCrowdPrefabPrototypes(GameObject gameObject, List<GPUInstancerPrototype> prototypeList, List<GameObject> prefabList, bool forceNew)
        {
            if (prefabList == null)
                return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                Undo.RecordObject(gameObject, "Prefab prototypes changed");

            bool changed = false;
            if (forceNew)
            {
                foreach (GPUICrowdPrototype prototype in prototypeList)
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(prototype));
                    changed = true;
                }
            }
            else
            {
                foreach (GPUICrowdPrototype prototype in prototypeList)
                {
                    if (!prefabList.Contains(prototype.prefabObject))
                    {
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(prototype));
                        changed = true;
                    }
                }
            }
            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
#endif

            foreach (GameObject go in prefabList)
            {
                if (!forceNew && prototypeList.Exists(p => p.prefabObject == go))
                    continue;

                prototypeList.Add(GenerateCrowdPrototype(go, forceNew));
            }

#if UNITY_EDITOR
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (!Application.isPlaying)
            {
                GPUICrowdPrefab[] prefabInstances = GameObject.FindObjectsOfType<GPUICrowdPrefab>();
                for (int i = 0; i < prefabInstances.Length; i++)
                {
#if UNITY_2018_2_OR_NEWER
                    UnityEngine.Object prefabRoot = PrefabUtility.GetCorrespondingObjectFromSource(prefabInstances[i].gameObject);
#else
                    UnityEngine.Object prefabRoot = PrefabUtility.GetPrefabParent(prefabInstances[i].gameObject);
#endif
                    if (prefabRoot != null && ((GameObject)prefabRoot).GetComponent<GPUICrowdPrefab>() != null && prefabInstances[i].prefabPrototype != ((GameObject)prefabRoot).GetComponent<GPUICrowdPrefab>().prefabPrototype)
                    {
                        Undo.RecordObject(prefabInstances[i], "Changed GPUInstancer Prefab Prototype " + prefabInstances[i].gameObject + i);
                        prefabInstances[i].prefabPrototype = ((GameObject)prefabRoot).GetComponent<GPUICrowdPrefab>().prefabPrototype;
                    }
                }
            }
#endif
        }

        public static GPUICrowdPrototype GenerateCrowdPrototype(GameObject go, bool forceNew, bool attachScript = true, GPUICrowdPrototype copySettingsFrom = null)
        {
            GPUICrowdPrefab prefabScript = go.GetComponent<GPUICrowdPrefab>();
            if (attachScript && prefabScript == null)
#if UNITY_2018_3_OR_NEWER && UNITY_EDITOR
                prefabScript = GPUInstancerUtility.AddComponentToPrefab<GPUICrowdPrefab>(go);
#else
                prefabScript = go.AddComponent<GPUICrowdPrefab>();
#endif
            if (attachScript && prefabScript == null)
                return null;

            GPUICrowdPrototype prototype = null;

            if (prefabScript != null)
                prototype = (GPUICrowdPrototype)prefabScript.prefabPrototype;
            if (prototype == null)
            {
                prototype = copySettingsFrom != null ? ScriptableObject.Instantiate(copySettingsFrom) : ScriptableObject.CreateInstance<GPUICrowdPrototype>();
                if (prefabScript != null)
                    prefabScript.prefabPrototype = prototype;
                prototype.prefabObject = go;
                prototype.name = go.name + "_" + go.GetInstanceID();
                prototype.useOriginalShaderForShadow = true;
                if (go.GetComponent<Rigidbody>() != null)
                {
                    prototype.enableRuntimeModifications = true;
                    prototype.autoUpdateTransformData = true;
                }
                prototype.hasNoAnimator = go.GetComponent<Animator>() == null;

                // if SRP use original shader for shadow
                if (!prototype.useOriginalShaderForShadow)
                {
                    MeshRenderer[] renderers = go.GetComponentsInChildren<MeshRenderer>();
                    foreach (MeshRenderer rdr in renderers)
                    {
                        foreach (Material mat in rdr.sharedMaterials)
                        {
                            if (mat.shader.name.Contains("HDRenderPipeline") || mat.shader.name.Contains("LWRenderPipeline") || mat.shader.name.Contains("Lightweight Render Pipeline"))
                            {
                                prototype.useOriginalShaderForShadow = true;
                                break;
                            }
                        }
                        if (prototype.useOriginalShaderForShadow)
                            break;
                    }
                }

                //GPUInstancerUtility.GenerateInstancedShadersForGameObject(prototype, gpuiSettings);

#if UNITY_EDITOR
                if (!Application.isPlaying)
                    EditorUtility.SetDirty(go);
#endif
            }
#if UNITY_EDITOR
            if (!Application.isPlaying && string.IsNullOrEmpty(AssetDatabase.GetAssetPath(prototype)))
            {
                string assetPath = GPUInstancerConstants.GetDefaultPath() + GPUICrowdConstants.PROTOTYPES_CROWD_PATH + prototype.name + ".asset";

                if (!System.IO.Directory.Exists(GPUInstancerConstants.GetDefaultPath() + GPUICrowdConstants.PROTOTYPES_CROWD_PATH))
                {
                    System.IO.Directory.CreateDirectory(GPUInstancerConstants.GetDefaultPath() + GPUICrowdConstants.PROTOTYPES_CROWD_PATH);
                }

                AssetDatabase.CreateAsset(prototype, assetPath);
            }

#if UNITY_2018_3_OR_NEWER
            if (!Application.isPlaying && prefabScript != null && prefabScript.prefabPrototype != prototype)
            {
                GameObject prefabContents = GPUInstancerUtility.LoadPrefabContents(go);
                prefabContents.GetComponent<GPUICrowdPrefab>().prefabPrototype = prototype;
                GPUInstancerUtility.UnloadPrefabContents(go, prefabContents);
            }
#endif
#endif
            return prototype;
        }

        public static GPUICrowdAnimationData GenerateCrowdAnimationData(GPUICrowdPrototype prototype)
        {
            if (prototype.animationData == null)
                prototype.animationData = ScriptableObject.CreateInstance<GPUICrowdAnimationData>();

#if UNITY_EDITOR
            if (!Application.isPlaying && string.IsNullOrEmpty(AssetDatabase.GetAssetPath(prototype.animationData)))
            {
                string assetPath = GPUInstancerConstants.GetDefaultPath() + GPUICrowdConstants.PROTOTYPES_CROWD__ANIMATION_DATA_PATH + prototype.name + "_AnimationData.asset";

                if (!System.IO.Directory.Exists(GPUInstancerConstants.GetDefaultPath() + GPUICrowdConstants.PROTOTYPES_CROWD__ANIMATION_DATA_PATH))
                {
                    System.IO.Directory.CreateDirectory(GPUInstancerConstants.GetDefaultPath() + GPUICrowdConstants.PROTOTYPES_CROWD__ANIMATION_DATA_PATH);
                }

                AssetDatabase.CreateAsset(prototype.animationData, assetPath);
            }
#endif
            return prototype.animationData;
        }
        #endregion Create Crowd Prototypes

        #region Shader Functions
        public static bool IsShaderInstanced(Shader shader)
        {
#if UNITY_EDITOR
            string originalAssetPath = AssetDatabase.GetAssetPath(shader);
            string originalShaderText = "";
            try
            {
                originalShaderText = System.IO.File.ReadAllText(originalAssetPath);
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                return false;
            }
            if (!string.IsNullOrEmpty(originalShaderText))
                return originalShaderText.Contains("GPUICrowdInclude.cginc");
#endif
            return false;
        }

        public static void GenerateInstancedShadersForGameObject(GPUInstancerPrototype prototype)
        {
            if (prototype.prefabObject == null)
                return;

            SkinnedMeshRenderer[] skinnedMeshRenderers = prototype.prefabObject.GetComponentsInChildren<SkinnedMeshRenderer>();

#if UNITY_EDITOR
            string warnings = "";
#endif

            foreach (SkinnedMeshRenderer mr in skinnedMeshRenderers)
            {
                Material[] mats = mr.sharedMaterials;

                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] == null || mats[i].shader == null)
                        continue;
                    if (GPUInstancerConstants.gpuiSettings.shaderBindings.IsShadersInstancedVersionExists(mats[i].shader.name, GPUICrowdConstants.GPUI_EXTENSION_CODE))
                    {
                        GPUInstancerConstants.gpuiSettings.AddShaderVariantToCollection(mats[i], GPUICrowdConstants.GPUI_EXTENSION_CODE);
                        continue;
                    }

                    if (!Application.isPlaying)
                    {
                        if (IsShaderInstanced(mats[i].shader))
                        {
                            GPUInstancerConstants.gpuiSettings.shaderBindings.AddShaderInstance(mats[i].shader.name, mats[i].shader, true, GPUICrowdConstants.GPUI_EXTENSION_CODE);
                            GPUInstancerConstants.gpuiSettings.AddShaderVariantToCollection(mats[i], GPUICrowdConstants.GPUI_EXTENSION_CODE);
                        }
#if UNITY_EDITOR
                        else
                        {
                            if (!warnings.Contains(mats[i].shader.name))
                                warnings += "Can not find instanced version for shader: " + mats[i].shader.name + ". Standard Shader will be used instead.";
                        }
#endif
                    }
                }
            }

            if (prototype.useGeneratedBillboard && prototype.billboard != null)
            {
                GPUInstancerConstants.gpuiSettings.AddShaderVariantToCollection(GPUInstancerUtility.GetBillboardShaderName(prototype), GPUICrowdConstants.GPUI_EXTENSION_CODE);
            }

#if UNITY_EDITOR
            if (string.IsNullOrEmpty(warnings))
            {
                if (prototype.warningText != null)
                {
                    prototype.warningText = null;
                    EditorUtility.SetDirty(prototype);
                }
            }
            else
            {
                if (prototype.warningText != warnings)
                {
                    prototype.warningText = warnings;
                    EditorUtility.SetDirty(prototype);
                }
            }
#endif
        }
        #endregion

    }
}
#endif //GPU_INSTANCER