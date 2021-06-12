using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

#if GPU_INSTANCER
namespace GPUInstancer.CrowdAnimations
{
    [CustomEditor(typeof(GPUICrowdManager))]
    [CanEditMultipleObjects]
    public class GPUICrowdManagerEditor : GPUInstancerPrefabManagerEditor
    {
        private GPUICrowdManager _crowdManager;
        private GPUICrowdAnimationBaker _animationBaker;
        private List<int> boneOpenFoldouts;

        private ReorderableList _clipReorderableList;
        private int _clipListSelectedIndex;

        private bool _optionalRenderersFoldout;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (GPUICrowdConstants.gpuiCrowdSettings == null)
                GPUICrowdConstants.gpuiCrowdSettings = GPUICrowdSettings.GetDefaultGPUICrowdSettings();

            wikiHash = "#The_Crowd_Manager";
            versionNo = GPUICrowdEditorConstants.GPUI_CA_VERSION;

            _crowdManager = (target as GPUICrowdManager);
            if (_animationBaker == null)
                _animationBaker = new GPUICrowdAnimationBaker();
            boneOpenFoldouts = new List<int>();

            _clipReorderableList = new ReorderableList(new List<AnimationClip>(), typeof(AnimationClip), true, true, false, true);
            _clipReorderableList.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "Clip List", GPUInstancerEditorConstants.Styles.boldLabel);
            };
            _clipReorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                AnimationClip clip = (AnimationClip)_clipReorderableList.list[index];
                rect.y += 2;
                rect.height -= 4;
                EditorGUI.LabelField(rect, clip == null ? "" : clip.name);
            };
            _clipReorderableList.onCanRemoveCallback = (ReorderableList l) => {
                return l.list.Count > 0;
            };
            _clipReorderableList.onSelectCallback = (ReorderableList l) => {
                _clipListSelectedIndex = l.index;
                EditorGUIUtility.PingObject((AnimationClip)l.list[l.index]);
            };
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!Application.isPlaying && Event.current.type == EventType.ExecuteCommand && Event.current.commandName == "ObjectSelectorClosed")
            {
                if (EditorGUIUtility.GetObjectPickerControlID() == 123)
                {
                    Object pickerObject = EditorGUIUtility.GetObjectPickerObject();
                    if(pickerObject != null && pickerObject is AnimationClip && _crowdManager.selectedPrototypeList != null && _crowdManager.selectedPrototypeList.Count > 0
                        && _crowdManager.selectedPrototypeList[0] != null)
                    {
                        GPUICrowdPrototype prototype = (GPUICrowdPrototype)_crowdManager.selectedPrototypeList[0];
                        AnimationClip animationClip = (AnimationClip)pickerObject;
                        if (!prototype.clipList.Contains(animationClip))
                            prototype.clipList.Add(animationClip);
                    }
                }
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (_animationBaker == null)
                _animationBaker.ClearBakingData();
        }

        public override void AddPickerObject(UnityEngine.Object pickerObject, GPUInstancerPrototype overridePrototype = null)
        {
            if (overridePrototype != null && GPUInstancerDefines.previewCache != null)
            {
                GPUInstancerDefines.previewCache.RemovePreview(overridePrototype);
            }

            AddPickerObjectCrowd(_crowdManager, pickerObject, overridePrototype);
        }

        public static void AddPickerObjectCrowd(GPUICrowdManager _crowdManager, UnityEngine.Object pickerObject, GPUInstancerPrototype overridePrototype = null)
        {
            if (pickerObject == null)
                return;

            if (pickerObject is GPUICrowdPrototype)
            {
                GPUICrowdPrototype crowdPrototype = (GPUICrowdPrototype)pickerObject;
                if (crowdPrototype.prefabObject != null)
                {
                    pickerObject = crowdPrototype.prefabObject;
                }
            }

            if (!(pickerObject is GameObject))
            {
#if UNITY_2018_3_OR_NEWER
                if (PrefabUtility.GetPrefabAssetType(pickerObject) == PrefabAssetType.Model)
#else
                if (PrefabUtility.GetPrefabType(pickerObject) == PrefabType.ModelPrefab)
#endif
                    EditorUtility.DisplayDialog(GPUInstancerConstants.TEXT_PREFAB_TYPE_WARNING_TITLE, GPUInstancerConstants.TEXT_PREFAB_TYPE_WARNING_3D, GPUInstancerConstants.TEXT_OK);
                else
                    EditorUtility.DisplayDialog(GPUInstancerConstants.TEXT_PREFAB_TYPE_WARNING_TITLE, GPUInstancerConstants.TEXT_PREFAB_TYPE_WARNING, GPUInstancerConstants.TEXT_OK);
                return;
            }

            GameObject prefabObject = (GameObject)pickerObject;

#if UNITY_2018_3_OR_NEWER
            PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(pickerObject);

            if (prefabType == PrefabAssetType.Regular || prefabType == PrefabAssetType.Variant)
            {
                if (PrefabUtility.IsPartOfNonAssetPrefabInstance(prefabObject))
                    prefabObject = GPUInstancerUtility.GetOutermostPrefabAssetRoot(prefabObject);
            }
            else
            {
                if (prefabType == PrefabAssetType.Model)
                    EditorUtility.DisplayDialog(GPUInstancerConstants.TEXT_PREFAB_TYPE_WARNING_TITLE, GPUInstancerConstants.TEXT_PREFAB_TYPE_WARNING_3D, GPUInstancerConstants.TEXT_OK);
                else
                    EditorUtility.DisplayDialog(GPUInstancerConstants.TEXT_PREFAB_TYPE_WARNING_TITLE, GPUInstancerConstants.TEXT_PREFAB_TYPE_WARNING, GPUInstancerConstants.TEXT_OK);
                return;
            }
            if (prefabType == PrefabAssetType.Variant)
            {
                if (prefabObject.GetComponent<GPUInstancerPrefab>() == null &&
                    !EditorUtility.DisplayDialog("Variant Prefab Warning",
                        "Prefab is a Variant. Do you wish to add the Variant as a prototype or the corresponding Prefab?" +
                        "\n\nIt is recommended to add the Prefab, if you do not have different renderers for the Variant.",
                        "Add Variant",
                        "Add Prefab"))
                {
                    prefabObject = GPUInstancerUtility.GetCorrespongingPrefabOfVariant(prefabObject);
                }
            }
#else
            PrefabType prefabType = PrefabUtility.GetPrefabType(pickerObject);

            if (prefabType != PrefabType.Prefab)
            {
                bool instanceFound = false;
                if (prefabType == PrefabType.PrefabInstance)
                {
#if UNITY_2018_2_OR_NEWER
                    GameObject newPrefabObject = (GameObject)PrefabUtility.GetCorrespondingObjectFromSource(prefabObject);
#else
                    GameObject newPrefabObject = (GameObject)PrefabUtility.GetPrefabParent(prefabObject);
#endif
                    if (PrefabUtility.GetPrefabType(newPrefabObject) == PrefabType.Prefab)
                    {
                        while (newPrefabObject.transform.parent != null)
                            newPrefabObject = newPrefabObject.transform.parent.gameObject;
                        prefabObject = newPrefabObject;
                        instanceFound = true;
                    }
                }
                if (!instanceFound)
                {
                    if (prefabType == PrefabType.ModelPrefab)
                        EditorUtility.DisplayDialog(GPUInstancerConstants.TEXT_PREFAB_TYPE_WARNING_TITLE, GPUInstancerConstants.TEXT_PREFAB_TYPE_WARNING_3D, GPUInstancerConstants.TEXT_OK);
                    else
                        EditorUtility.DisplayDialog(GPUInstancerConstants.TEXT_PREFAB_TYPE_WARNING_TITLE, GPUInstancerConstants.TEXT_PREFAB_TYPE_WARNING, GPUInstancerConstants.TEXT_OK);
                    return;
                }
            }
#endif

            if (prefabObject.GetComponentInChildren<SkinnedMeshRenderer>() == null)
            {
                EditorUtility.DisplayDialog(GPUInstancerConstants.TEXT_PREFAB_TYPE_WARNING_TITLE, GPUICrowdConstants.TEXT_PREFAB_NO_SKINNEDMESH, GPUInstancerConstants.TEXT_OK);
                return;
            }

            if (prefabObject.GetComponent<Animator>() == null)
            {
                Animator[] animators = prefabObject.GetComponentsInChildren<Animator>();
                if (animators != null && animators.Length > 0)
                {
                    EditorUtility.DisplayDialog(GPUInstancerConstants.TEXT_PREFAB_TYPE_WARNING_TITLE, GPUICrowdConstants.TEXT_PREFAB_WITH_CHILD_ANIMATOR, GPUInstancerConstants.TEXT_OK);
                    return;
                }
            }

            if (_crowdManager.prefabList.Contains(prefabObject))
                return;

            GPUICrowdPrefab prefabScript = prefabObject.GetComponent<GPUICrowdPrefab>();
            if (prefabScript != null && prefabScript.prefabPrototype != null && prefabScript.prefabPrototype.prefabObject != prefabObject)
            {
#if UNITY_2018_3_OR_NEWER
                GPUInstancerUtility.RemoveComponentFromPrefab<GPUICrowdPrefab>(prefabObject);
#else
                DestroyImmediate(prefabScript, true);
#endif
                prefabScript = null;
            }

            if (prefabScript == null)
            {
#if UNITY_2018_3_OR_NEWER
                prefabScript = GPUInstancerUtility.AddComponentToPrefab<GPUICrowdPrefab>(prefabObject);
#else
                prefabScript = prefabObject.AddComponent<GPUICrowdPrefab>();
#endif
            }
            if (prefabScript == null)
                return;

            if (prefabScript.prefabPrototype != null && _crowdManager.prototypeList.Contains(prefabScript.prefabPrototype))
                return;

            Undo.RecordObject(_crowdManager, "Add prototype");

            if (!_crowdManager.prefabList.Contains(prefabObject))
            {
                _crowdManager.prefabList.Add(prefabObject);
                _crowdManager.GeneratePrototypes();
            }

            if (prefabScript.prefabPrototype != null)
            {
                if (_crowdManager.registeredPrefabs == null)
                    _crowdManager.registeredPrefabs = new List<RegisteredPrefabsData>();

                RegisteredPrefabsData data = _crowdManager.registeredPrefabs.Find(d => d.prefabPrototype == prefabScript.prefabPrototype);
                if (data == null)
                {
                    data = new RegisteredPrefabsData(prefabScript.prefabPrototype);
                    _crowdManager.registeredPrefabs.Add(data);
                }

                GPUICrowdPrefab[] scenePrefabInstances = FindObjectsOfType<GPUICrowdPrefab>();
                foreach (GPUICrowdPrefab prefabInstance in scenePrefabInstances)
                    if (prefabInstance.prefabPrototype == prefabScript.prefabPrototype)
                        data.registeredPrefabs.Add(prefabInstance);
            }
        }

        public override void DrawGPUInstancerPrototypeBillboardSettings(GPUInstancerPrototype selectedPrototype)
        {

        }

        public override void DrawGPUInstancerPrototypeAdvancedActions()
        {

        }

        public override bool DrawGPUInstancerPrototypeBeginningInfo(List<GPUInstancerPrototype> selectedPrototypeList)
        {
            GPUICrowdPrototype prototype0 = (GPUICrowdPrototype)selectedPrototypeList[0];
            bool hasChanged = base.DrawGPUInstancerPrototypeBeginningInfo(selectedPrototypeList);
            bool isFrameRateMixed = false;
            int frameRate = prototype0.frameRate;
            for (int i = 1; i < selectedPrototypeList.Count; i++)
            {
                GPUICrowdPrototype prototype = (GPUICrowdPrototype)selectedPrototypeList[i];
                if (!isFrameRateMixed && frameRate != prototype.frameRate)
                    isFrameRateMixed = true;
            }


            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            EditorGUILayout.BeginVertical(GPUInstancerEditorConstants.Styles.box);
            GPUInstancerEditorConstants.DrawCustomLabel(GPUICrowdEditorConstants.TEXT_animationBakeData, GPUInstancerEditorConstants.Styles.boldLabel);
            hasChanged |= MultiIntSlider(selectedPrototypeList, GPUICrowdEditorConstants.TEXT_frameRate, frameRate, 15, 90, isFrameRateMixed, (p, v) => ((GPUICrowdPrototype)p).frameRate = v);
            EditorGUILayout.EndVertical();
            EditorGUI.EndDisabledGroup();

            return hasChanged;
        }

        public override void DrawGPUInstancerPrototypeBeginningInfo(GPUInstancerPrototype selectedPrototype)
        {
            base.DrawGPUInstancerPrototypeBeginningInfo(selectedPrototype);
            
            EditorGUI.BeginDisabledGroup(Application.isPlaying);

            GPUICrowdPrototype prototype = (GPUICrowdPrototype)selectedPrototype;

            GPUICrowdPrefab gpuiPrefab = prototype.prefabObject.GetComponent<GPUICrowdPrefab>();
            if (gpuiPrefab == null)
                return;
            Animator prefabAnimator = gpuiPrefab.GetAnimator();

            bool hasAnimator = prefabAnimator != null;
            prototype.hasNoAnimator = !hasAnimator;

            if (prototype.clipList == null)
                prototype.clipList = new List<AnimationClip>();

            if (hasAnimator)
                prototype.animatorCullingMode = prefabAnimator.cullingMode;

            #region Animation Bake Data
            EditorGUILayout.BeginVertical(GPUInstancerEditorConstants.Styles.box);
            GPUInstancerEditorConstants.DrawCustomLabel(GPUICrowdEditorConstants.TEXT_animationBakeData, GPUInstancerEditorConstants.Styles.boldLabel);

            prototype.animationData = (GPUICrowdAnimationData)EditorGUILayout.ObjectField(GPUICrowdEditorConstants.TEXT_animationDataAsset, prototype.animationData, typeof(GPUICrowdAnimationData), false);
            EditorGUI.BeginDisabledGroup(true);
            if (prototype.animationData != null)
                EditorGUILayout.ObjectField(GPUICrowdEditorConstants.TEXT_prefabAnimationTexture, prototype.animationData.animationTexture, typeof(GameObject), false);
            EditorGUI.EndDisabledGroup();
            prototype.frameRate = EditorGUILayout.IntSlider(GPUICrowdEditorConstants.TEXT_frameRate, prototype.frameRate, 15, 90);
            if (!hasAnimator)
            {
                if (prototype.clipList == null || prototype.clipList.Count == 0)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox(GPUICrowdEditorConstants.HELPTEXT_noAnimator, MessageType.Warning);
                }
                _clipReorderableList.list = prototype.clipList;
                _clipReorderableList.index = _clipListSelectedIndex;

                EditorGUILayout.Space();
                _clipReorderableList.DoLayoutList();
                Rect rect = EditorGUILayout.GetControlRect(false, 25);
                GPUInstancerEditorConstants.DrawColoredButton(GPUICrowdEditorConstants.Contents.addClip, GPUInstancerEditorConstants.Colors.lightBlue, Color.white, FontStyle.Bold, rect,
                () =>
                {
                    EditorGUIUtility.ShowObjectPicker<AnimationClip>(null, false, "", 123);
                },
                true, true,
                (o) =>
                {
                    if (o != null && o is AnimationClip)
                    {
                        AnimationClip animationClip = (AnimationClip)o;
                        if (!prototype.clipList.Contains(animationClip))
                            prototype.clipList.Add(animationClip);
                    }
                });
                EditorGUILayout.Space();
            }

            if (hasAnimator && !prefabAnimator.hasTransformHierarchy)
            {
                EditorGUILayout.HelpBox(GPUICrowdEditorConstants.TEXT_modelPrefabInfo, MessageType.Info);
                GameObject modelImporterGO = null;
                if (string.IsNullOrEmpty(prototype.modelPrefabPath))
                {
                    if (prefabAnimator.avatar != null)
                    {
                        string modelAssetPath = AssetDatabase.GetAssetPath(prefabAnimator.avatar);
                        AssetImporter assetImporter = AssetImporter.GetAtPath(modelAssetPath);
                        if (assetImporter != null && assetImporter is ModelImporter)
                        {
                            prototype.modelPrefabPath = modelAssetPath;
                            modelImporterGO = AssetDatabase.LoadAssetAtPath<GameObject>(prototype.modelPrefabPath);
                        }
                    }
                }
                else
                    modelImporterGO = AssetDatabase.LoadAssetAtPath<GameObject>(prototype.modelPrefabPath);
                if (modelImporterGO == null)
                    prototype.modelPrefabPath = null;

                GameObject newModelImporterGO = (GameObject)EditorGUILayout.ObjectField(GPUICrowdEditorConstants.TEXT_modelPrefabReference, modelImporterGO, typeof(GameObject), false);
                if (modelImporterGO != null && newModelImporterGO != modelImporterGO)
                {
                    string modelAssetPath = AssetDatabase.GetAssetPath(newModelImporterGO);
                    AssetImporter assetImporter = AssetImporter.GetAtPath(modelAssetPath);
                    if (assetImporter != null && assetImporter is ModelImporter)
                        prototype.modelPrefabPath = modelAssetPath;
                    else
                    {
                        Debug.LogError(GPUICrowdEditorConstants.TEXT_modelPrefabReferenceError);
                    }
                }
            }

            if (!Application.isPlaying)
            {
                GUILayout.Space(10);
                if (prototype.animationData == null || prototype.animationData.animationTexture == null)
                {
                    EditorGUILayout.HelpBox(GPUICrowdEditorConstants.TEXT_bakeAnimationInfo, MessageType.Info);
                }
                EditorGUILayout.BeginHorizontal();
                if (hasAnimator && !prefabAnimator.hasTransformHierarchy && string.IsNullOrEmpty(prototype.modelPrefabPath))
                    EditorGUI.BeginDisabledGroup(true);
                GPUInstancerEditorConstants.DrawColoredButton(GPUICrowdEditorConstants.Contents.regenerateAnimationData, GPUInstancerEditorConstants.Colors.darkBlue, Color.white, FontStyle.Bold, Rect.zero,
                () =>
                {
                    _animationBaker.SkinnedMeshBakeAnimations(prototype);
                });
                if (hasAnimator && !prefabAnimator.hasTransformHierarchy && string.IsNullOrEmpty(prototype.modelPrefabPath))
                    EditorGUI.EndDisabledGroup();

                if (prototype.animationData != null && prototype.animationData.animationTexture != null)
                {
                    GPUInstancerEditorConstants.DrawColoredButton(new GUIContent(GPUICrowdEditorConstants.TEXT_testAnimations), GPUInstancerEditorConstants.Colors.lightBlue, Color.white, FontStyle.Bold, Rect.zero,
                    () =>
                    {
                        _animationBaker.GenerateTestInstance(prototype);
                    });
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5);
            }

            EditorGUILayout.EndVertical();
            #endregion Animation Bake Data

            if (prototype.animationData != null && prototype.animationData.animationTexture != null)
            {
                #region Animator Settings
                EditorGUILayout.BeginVertical(GPUInstancerEditorConstants.Styles.box);
                GPUInstancerEditorConstants.DrawCustomLabel(GPUICrowdEditorConstants.TEXT_prefabAnimatorSettings, GPUInstancerEditorConstants.Styles.boldLabel);

                GUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                Rect rect = GUILayoutUtility.GetRect(100, 20, GPUInstancerEditorConstants.Styles.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
                GUIStyle selectedStyle = new GUIStyle("button");
                selectedStyle.normal = selectedStyle.active;

                if (prototype.hasNoAnimator)
                    prototype.animationData.useCrowdAnimator = true;

                EditorGUI.BeginDisabledGroup(prototype.hasNoAnimator);
                GPUInstancerEditorConstants.DrawColoredButton(GPUICrowdEditorConstants.Contents.mecanimAnimator,
                    prototype.animationData.useCrowdAnimator ? GPUInstancerEditorConstants.Colors.dimgray : GPUInstancerEditorConstants.Colors.green,
                    Color.white, FontStyle.Bold, rect,
                () =>
                {
                    prototype.animationData.useCrowdAnimator = false;
                },
                false, false, null, prototype.animationData.useCrowdAnimator ? null : selectedStyle);

                rect = GUILayoutUtility.GetRect(100, 20, GPUInstancerEditorConstants.Styles.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
                GPUInstancerEditorConstants.DrawColoredButton(GPUICrowdEditorConstants.Contents.crowdAnimator,
                    prototype.animationData.useCrowdAnimator ? GPUInstancerEditorConstants.Colors.green : GPUInstancerEditorConstants.Colors.dimgray,
                    Color.white, FontStyle.Bold, rect,
                () =>
                {
                    prototype.animationData.useCrowdAnimator = true;
                },
                false, false, null, prototype.animationData.useCrowdAnimator ? selectedStyle : null);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5);

                #region Crowd Animator Settings
                List<string> clipNames = new List<string>();
                GUIContent[] optionsList = new GUIContent[prototype.animationData.clipDataList.Count];
                if (prototype.animationData.useCrowdAnimator)
                {
                    EditorGUILayout.HelpBox(GPUICrowdEditorConstants.TEXT_crowdAnimatorInfo, MessageType.Info);
                    DrawHelpText(GPUICrowdEditorConstants.HELPTEXT_crowdAnimator);
                    if (prototype.animationData.clipDataList != null && prototype.animationData.clipDataList.Count > 0)
                    {
                        for (int i = 0; i < optionsList.Length; i++)
                        {
                            string clipName = prototype.animationData.clipDataList[i].animationClip.name;
                            int clipNameCounter = 1;
                            while (clipNames.Contains(clipName))
                            {
                                clipName = prototype.animationData.clipDataList[i].animationClip.name + " " + clipNameCounter;
                                clipNameCounter++;
                            }
                            optionsList[i] = new GUIContent(clipName);
                            clipNames.Add(clipName);
                        }
                        prototype.animationData.crowdAnimatorDefaultClip = EditorGUILayout.Popup(GPUICrowdEditorConstants.Contents.defaultClip, 
                            prototype.animationData.crowdAnimatorDefaultClip, optionsList);
                    }
                }
                #endregion Crowd Animator Settings

                prototype.animationData.applyRootMotion = EditorGUILayout.Toggle(GPUICrowdEditorConstants.TEXT_applyRootMotion, prototype.animationData.applyRootMotion);

                #region Optimize Game Objects
                if (prototype.animationData.animationTexture != null && hasAnimator && prefabAnimator.hasTransformHierarchy)
                {
                    if (!prototype.animationData.useCrowdAnimator)
                    {
                        prototype.animationData.optimizeGameObjects = EditorGUILayout.Toggle(GPUICrowdEditorConstants.TEXT_removeBoneGOs, prototype.animationData.optimizeGameObjects);
                        if (prototype.animationData.optimizeGameObjects)
                        {
                            GPUInstancerEditorConstants.DrawCustomLabel(GPUICrowdEditorConstants.TEXT_exposedBones, GPUInstancerEditorConstants.Styles.boldLabel, false);
                            EditorGUILayout.BeginVertical(GPUInstancerEditorConstants.Styles.box);
                            List<GPUIBone> rootBones = prototype.animationData.bones.FindAll(b => b.isRoot == true);
                            bool hasChanged = false;
                            foreach (GPUIBone bone in rootBones)
                            {
                                hasChanged |= DrawBone(prototype, bone, 12);
                            }
                            EditorGUILayout.EndVertical();
                            if (hasChanged)
                                SetExposedTransforms(prototype);
                        }
                    }
                }
                else
                {
                    prototype.animationData.optimizeGameObjects = false;
                }
                #endregion Optimize Game Objects

                #region Optional Renderers
                if (prototype.animationData.useCrowdAnimator && prototype.animationData.animationTexture != null && prototype.animationData.skinnedMeshDataList.Count > 1 && prototype.prefabObject.GetComponent<LODGroup>() == null)
                {
                    EditorGUILayout.Space();
                    prototype.hasOptionalRenderers = EditorGUILayout.Toggle("Has Optional Renderers", prototype.hasOptionalRenderers);
                    if (prototype.hasOptionalRenderers)
                    {
                        Rect contentRect = GUILayoutUtility.GetRect(4, 20, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
                        contentRect.x += 12;
                        _optionalRenderersFoldout = EditorGUI.Foldout(contentRect, _optionalRenderersFoldout, "Optional Renderers");
                        if (_optionalRenderersFoldout)
                        {
                            foreach (GPUISkinnedMeshData smd in prototype.animationData.skinnedMeshDataList)
                            {
                                smd.isOptional = EditorGUILayout.Toggle(smd.transformName, smd.isOptional);
                            }
                        }
                    }
                    DrawHelpText(GPUICrowdEditorConstants.HELPTEXT_optionalRenderers);
                }
                else
                    prototype.hasOptionalRenderers = false;
                #endregion Optional Renderers

                EditorGUILayout.EndVertical();
                #endregion Animator Settings

                #region Clip Settings
                if (prototype.animationData.useCrowdAnimator)
                {
                    if (prototype.animationData.clipDataList != null && prototype.animationData.clipDataList.Count > 0)
                    {
                        if (_crowdManager.selectedClipIndex >= prototype.animationData.clipDataList.Count)
                            _crowdManager.selectedClipIndex = 0;
                        EditorGUILayout.BeginVertical(GPUInstancerEditorConstants.Styles.box);
                        GPUInstancerEditorConstants.DrawCustomLabel("Clip Settings", GPUInstancerEditorConstants.Styles.boldLabel);

                        DrawHelpText(GPUICrowdEditorConstants.HELPTEXT_clipSettings);

                        _crowdManager.selectedClipIndex = EditorGUILayout.Popup(_crowdManager.selectedClipIndex, optionsList);
                        GPUIAnimationClipData selectedClipData = prototype.animationData.clipDataList[_crowdManager.selectedClipIndex];
                            

                        EditorGUILayout.Space();
                        selectedClipData.isLoopDisabled = !EditorGUILayout.Toggle("Is Looping", !selectedClipData.isLoopDisabled);

                        #region Animation Events
                        EditorGUILayout.Space();
                        Rect foldoutRect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
                        foldoutRect.x += 12;
                        _crowdManager.showEventsFoldout = EditorGUI.Foldout(foldoutRect, _crowdManager.showEventsFoldout, new GUIContent("Animation Events"));
                        if (_crowdManager.showEventsFoldout)
                        {
                            if (_crowdManager.animationEvents != null && _crowdManager.animationEvents.Count > 0)
                            {
                                for (int i = 0; i < _crowdManager.animationEvents.Count; i++)
                                {
                                    GPUIAnimationEvent animationEvent = _crowdManager.animationEvents[i];
                                    if (animationEvent.prototype != prototype || animationEvent.clip != selectedClipData.animationClip)
                                        continue;
                                        
                                    //((GPUICrowdPrototype)_crowdManager.prototypeList[0]).animationData.clipDataList[0].animationEvents[0];
                                    EditorGUILayout.BeginVertical(GPUInstancerEditorConstants.Styles.box);
                                    GPUInstancerEditorConstants.DrawCustomLabel(selectedClipData.animationClip.name + " Event at Frame " + animationEvent.eventFrame, GPUInstancerEditorConstants.Styles.boldLabel);

                                    animationEvent.eventFrame = EditorGUILayout.IntSlider("Event Frame", animationEvent.eventFrame, 0, selectedClipData.clipFrameCount - 1);
                                    animationEvent.floatParam = EditorGUILayout.FloatField("Float Parameter", animationEvent.floatParam);
                                    animationEvent.intParam = EditorGUILayout.IntField("Int Parameter", animationEvent.intParam);
                                    animationEvent.stringParam = EditorGUILayout.TextField("String Parameter", animationEvent.stringParam);
                                    SerializedProperty serializedProperty = serializedObject.FindProperty("animationEvents");

                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("animationEvents").GetArrayElementAtIndex(i),
                                        new GUIContent("Event at Frame " + animationEvent.eventFrame));

                                    GPUInstancerEditorConstants.DrawColoredButton(new GUIContent("Remove Event at Frame " + animationEvent.eventFrame), GPUInstancerEditorConstants.Colors.darkred, Color.white, FontStyle.Bold, Rect.zero,
                                    () =>
                                    {
                                        _crowdManager.animationEvents.Remove(animationEvent);
                                    });

                                    EditorGUILayout.EndVertical();
                                    EditorGUILayout.Space();
                                }
                            }
                            GPUInstancerEditorConstants.DrawColoredButton(new GUIContent("Add Event"), GPUInstancerEditorConstants.Colors.lightBlue, Color.white, FontStyle.Bold, Rect.zero,
                            () =>
                            {
                                if (_crowdManager.animationEvents == null)
                                    _crowdManager.animationEvents = new List<GPUIAnimationEvent>();
                                _crowdManager.animationEvents.Add(new GPUIAnimationEvent(prototype, selectedClipData.animationClip));
                            });
                            EditorGUILayout.Space();
                        }

                        #endregion Animation Events

                        EditorGUILayout.EndVertical();
                    }
                }
                #endregion Clip Settings
            }

            EditorGUI.EndDisabledGroup();
        }

        private bool DrawBone(GPUICrowdPrototype prototype, GPUIBone currentBone, int indent)
        {
            bool hasChanged = false;
            bool hasChildren = currentBone.childBoneIndexes != null && currentBone.childBoneIndexes.Count > 0;
            Rect contentRect;
            bool isFoldoutOpen = hasChildren && boneOpenFoldouts.Contains(currentBone.boneIndex);
            bool dontDestroy = currentBone.dontDestroy;
            bool newDontDestroy = currentBone.dontDestroy;
            if (hasChildren)
            {
                EditorGUILayout.BeginHorizontal();
                contentRect = GUILayoutUtility.GetRect(4, 20, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
                contentRect.x += indent;
                isFoldoutOpen = EditorGUI.Foldout(contentRect, isFoldoutOpen, "", true, GPUInstancerEditorConstants.Styles.foldout);
                contentRect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
                contentRect.x += indent;
                newDontDestroy = EditorGUI.ToggleLeft(contentRect, currentBone.boneTransformName, currentBone.dontDestroy);
                EditorGUILayout.EndHorizontal();

                if (isFoldoutOpen)
                {
                    for (int i = 0; i < currentBone.childBoneIndexes.Count; i++)
                    {
                        hasChanged |= DrawBone(prototype, prototype.animationData.bones[currentBone.childBoneIndexes[i]], indent + 12);
                    }
                }
            }
            else
            {
                contentRect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
                contentRect.x += indent;
                newDontDestroy = EditorGUI.ToggleLeft(contentRect, currentBone.boneTransformName, currentBone.dontDestroy);
            }
            if (isFoldoutOpen && !boneOpenFoldouts.Contains(currentBone.boneIndex))
            {
                boneOpenFoldouts.Add(currentBone.boneIndex);
            }
            else if (!isFoldoutOpen && boneOpenFoldouts.Contains(currentBone.boneIndex))
            {
                boneOpenFoldouts.Remove(currentBone.boneIndex);
            }
            if (dontDestroy != newDontDestroy)
            {
                hasChanged = true;
                SetBoneDontDestroy(prototype, currentBone, newDontDestroy, Event.current.alt);
                EditorUtility.SetDirty(prototype);
            }
            return hasChanged;
        }

        private void SetBoneDontDestroy(GPUICrowdPrototype prototype, GPUIBone currentBone, bool dontDestroy, bool applyToChildren)
        {
            if (dontDestroy != currentBone.dontDestroy)
            {
                currentBone.dontDestroy = dontDestroy;
                if (applyToChildren)
                {
                    for (int i = 0; i < currentBone.childBoneIndexes.Count; i++)
                    {
                        SetBoneDontDestroy(prototype, prototype.animationData.bones[currentBone.childBoneIndexes[i]], dontDestroy, applyToChildren);
                    }
                }
            }
        }

        public void SetExposedTransforms(GPUICrowdPrototype prototype)
        {
            List<string> result = new List<string>();
            foreach (GPUIBone bone in prototype.animationData.bones)
            {
                if (bone.dontDestroy)
                    result.Add(bone.boneTransformName);
            }
            prototype.animationData.exposedTransforms = result.ToArray();
        }

        public override void DrawPrototypeBoxButtons()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.BeginHorizontal();
                GPUInstancerEditorConstants.DrawColoredButton(GPUICrowdEditorConstants.Contents.bakeAll, GPUInstancerEditorConstants.Colors.darkBlue, Color.white, FontStyle.Bold, Rect.zero,
                () =>
                {
                    if (EditorUtility.DisplayDialog("Bake Animations", "All prototypes' animations will be baked. Do you wish to continue?", "Continue", "Cancel"))
                    {
                        _animationBaker.SkinnedMeshBakeAnimations(_crowdManager.prototypeList);
                    }
                });

                List<GPUInstancerPrototype> missingList = new List<GPUInstancerPrototype>();
                foreach (GPUICrowdPrototype crowdPrototype in _crowdManager.prototypeList)
                {
                    if (crowdPrototype.animationData == null || crowdPrototype.animationData.animationTexture == null)
                        missingList.Add(crowdPrototype);
                }
                if (missingList.Count > 0)
                {
                    GPUInstancerEditorConstants.DrawColoredButton(GPUICrowdEditorConstants.Contents.bakeMissing, GPUInstancerEditorConstants.Colors.darkBlue, Color.white, FontStyle.Bold, Rect.zero,
                    () =>
                    {
                        if (EditorUtility.DisplayDialog("Bake Animations", "Prototypes with no baked animations will be baked. Do you wish to continue?", "Continue", "Cancel"))
                        {
                            _animationBaker.SkinnedMeshBakeAnimations(missingList);
                        }
                    });
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
#else //GPU_INSTANCER

namespace GPUInstancer.CrowdAnimations
{
    [CustomEditor(typeof(GPUICrowdManager))]
    [CanEditMultipleObjects]
    public class GPUICrowdManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(GPUICrowdConstants.ERROR_GPUI_Dependency, MessageType.Error);
            Debug.LogError(GPUICrowdConstants.ERROR_GPUI_Dependency);
        }
    }
}
#endif //GPU_INSTANCER