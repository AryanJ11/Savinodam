using System.Collections.Generic;
using System.IO;
using System.Linq;
using DaiMangou.BridgedData;
using DaiMangou.Storyteller;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DaiMangou.GameBridgeEditor
{
    [CustomEditor(typeof(Dialoguer))]
    public class DialoguerEditor : Editor
    {

        public void OnEnable()
        {
            selectedDialoguer = (Dialoguer)target;

            #region set the icon

            if (!iconSet)
            {
                IconManager.SetIcon(selectedDialoguer, IconManager.DaiMangouIcons.ChatIcon);
                iconSet = true;
            }

            #endregion

            edwin = Resources.FindObjectsOfTypeAll(typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow")) as EditorWindow[];


            NameUI = serializedObject.FindProperty("NameUI");

            NameText = serializedObject.FindProperty("NameText");

            DisplayedTextUI = serializedObject.FindProperty("DisplayedTextUI");

DisplayedText = serializedObject.FindProperty("DisplayedText");

            moveNextButton = serializedObject.FindProperty("MoveNextButton");

            movePreviousButton = serializedObject.FindProperty("MovePreviousButton");

            RouteButton = serializedObject.FindProperty("RouteButton");

            RouteParent = serializedObject.FindProperty("RouteParent");

            TypingSpeed = serializedObject.FindProperty("TypingSpeed");

            TypingAudioCip = serializedObject.FindProperty("TypingAudioCip");

            UsesText = serializedObject.FindProperty("UsesText");

            // UsesStoryboardImages = serializedObject.FindProperty("UsesStoryboardImages");

            UsesVoiceOver = serializedObject.FindProperty("UsesVoiceOver");

            UsesSoundffects = serializedObject.FindProperty("UsesSoundffects");

            UseKeywordFilters = serializedObject.FindProperty("UseKeywordFilters");

            FilterCount = serializedObject.FindProperty("FilterCount");

            ShowKeywordFilterFouldout = serializedObject.FindProperty("ShowKeywordFilterFouldout");

            ShowGeneralSettings = serializedObject.FindProperty("ShowGeneralSettings");

            textDisplayMode = serializedObject.FindProperty("textDisplayMode");

            //  sceneData = serializedObject.FindProperty("sceneData");

            useNameUI = serializedObject.FindProperty("UseNameUI");

            useDialogueTextUI = serializedObject.FindProperty("UseDialogueTextUI");

            useMoveNextButton = serializedObject.FindProperty("UseMoveNextButton");

            useMovePreviousButton = serializedObject.FindProperty("UseMovePreviousButton");

            useRouteButton = serializedObject.FindProperty("UseRouteButton");

            useAutoStartVoiceClip = serializedObject.FindProperty("AutoStartVoiceClip");

            useAutoStartSoundEffectClip = serializedObject.FindProperty("AutoStartSoundEffectClip");

            AutoStartAllConditionsByDefault = serializedObject.FindProperty("AutoStartAllConditionsByDefault");

            ShowNodeSpecificConditionSettings = serializedObject.FindProperty("ShowNodeSpecificConditionSettings");

            UpdateUID = serializedObject.FindProperty("UpdateUID");

            VolumeVariation = serializedObject.FindProperty("VolumeVariation");
            //   NameColour = serializedObject.FindProperty("NameColour");

            //   TextColour = serializedObject.FindProperty("TextColour");
        }

        public override bool RequiresConstantRepaint()
        {
            return true;
        }

        public override void OnInspectorGUI()
        {
            //    DrawDefaultInspector();

           
            if (edwin.Length == 0)
            {
                Repaint();
                edwin = Resources.FindObjectsOfTypeAll(typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow")) as EditorWindow[];
            }

            ScreenRect.size = new Vector2(edwin[0].position.width, edwin[0].position.height);

            serializedObject.Update();


            #region dialoguer cover image

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Box(ImageLibrary.GBDialogueImage, EditorStyles.inspectorDefaultMargins);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            #endregion


            #region tell users to assign a SceneData Asset

            if (selectedDialoguer.sceneData == null)
                EditorGUILayout.HelpBox("Please assign a SceneData Asset in the area below", MessageType.Info);

            selectedDialoguer.sceneData =
                (SceneData)EditorGUILayout.ObjectField(selectedDialoguer.sceneData, typeof(SceneData), false);

            #endregion

            if (selectedDialoguer.sceneData == null)
                return;


            if (showHelpMessage)
                EditorGUILayout.HelpBox("This will update the Dialoguer with all the necessary data from the scene necessary for the dialoguer", MessageType.Info);

            if (GUILayout.Button("Setup"))
            {
                if (selectedDialoguer.ReflectedDataSet.Count == 0)
                {
                    selectedDialoguer.ReflectedDataSet.Resize(
                        selectedDialoguer.sceneData.FullCharacterDialogueSet.Count);

                    selectedDialoguer.ReflectedDataParent = new GameObject("Reflected Data");
                    selectedDialoguer.ReflectedDataParent.transform.SetParent(selectedDialoguer.transform);
                    selectedDialoguer.ReflectedDataParent.transform.localPosition = Vector3.zero;
                    selectedDialoguer.ReflectedDataParent.hideFlags = HideFlags.HideInHierarchy;


                    var AudioManager = new GameObject("Audio Manager");
                    AudioManager.transform.SetParent(selectedDialoguer.transform);
                    AudioManager.transform.localPosition = Vector3.zero;

                    var TypingAudioManager = new GameObject("Typing");
                    TypingAudioManager.transform.SetParent(AudioManager.transform);
                    TypingAudioManager.transform.localPosition = Vector3.zero;
                    TypingAudioManager.AddComponent<AudioSource>();
                    selectedDialoguer.TypingAudioSource = TypingAudioManager.GetComponent<AudioSource>();

                    var VoiceAudioManager = new GameObject("Voice");
                    VoiceAudioManager.transform.SetParent(AudioManager.transform);
                    VoiceAudioManager.transform.localPosition = Vector3.zero;
                    VoiceAudioManager.AddComponent<AudioSource>();
                    selectedDialoguer.VoiceAudioSource = VoiceAudioManager.GetComponent<AudioSource>();

                    var SoundEffectsAudioManager = new GameObject("Sound Effects");
                    SoundEffectsAudioManager.transform.SetParent(AudioManager.transform);
                    SoundEffectsAudioManager.transform.localPosition = Vector3.zero;
                    SoundEffectsAudioManager.AddComponent<AudioSource>();
                    selectedDialoguer.SoundEffectAudioSource = SoundEffectsAudioManager.GetComponent<AudioSource>();
                }
                else
                {
                    // we cache the current set of reflected data in a temporary list and then empry and resize the reflecteddataset list
                    selectedDialoguer.TempReflectedDataSet = new List<ReflectedData>();
                    
                    foreach (var capturedData in selectedDialoguer.ReflectedDataSet)
                        selectedDialoguer.TempReflectedDataSet.Add(capturedData);

                    selectedDialoguer.ReflectedDataSet = new List<ReflectedData>();
                    selectedDialoguer.ReflectedDataSet.Resize(
                        selectedDialoguer.sceneData.FullCharacterDialogueSet.Count);
                }


                // loop through the sorted list
                for (var i = 0; i < selectedDialoguer.sceneData.FullCharacterDialogueSet.Count; i++)
                {
                    #region create a new instance of ReflectedData as a gameObject and then assign the FullCharacterDialogueSet value at i to the reflected data ID

                    var newReflectedDatagameObject =
                        new GameObject(selectedDialoguer.sceneData.FullCharacterDialogueSet[i].Name + "Reflected");
                    newReflectedDatagameObject.transform.SetParent(selectedDialoguer.ReflectedDataParent.transform);
                    newReflectedDatagameObject.AddComponent<ReflectedData>();
                    var theReflectedDataComponent = newReflectedDatagameObject.GetComponent<ReflectedData>();
                    theReflectedDataComponent.DialoguerGameObject = selectedDialoguer.gameObject;
                    theReflectedDataComponent.dialoguer = selectedDialoguer;
                    theReflectedDataComponent.self = newReflectedDatagameObject;

                    // we already resized the ReflectedDataSet list to be the same size as the FullCharacterDialogueSet so we dont use .Add
                    selectedDialoguer.ReflectedDataSet[i] = theReflectedDataComponent;
                    // it is VERY important that the UIDs match.
                    selectedDialoguer.ReflectedDataSet[i].UID =
                        selectedDialoguer.sceneData.FullCharacterDialogueSet[i].UID;

                    #endregion

                    #region Add the first conditin

                    var newCondition = new GameObject(newReflectedDatagameObject.name + "Condition " +
                                                      theReflectedDataComponent.Conditions.Count);
                    newCondition.AddComponent<Condition>();
                    var _condition = newCondition.GetComponent<Condition>();
                    _condition.DialoguerGameObject = selectedDialoguer.gameObject;
                    _condition.dialoguer = selectedDialoguer;
                    _condition.Self = newCondition;
                    newCondition.transform.SetParent(newReflectedDatagameObject.transform);
                    // newCondition.hideFlags = HideFlags.HideInHierarchy;
                    theReflectedDataComponent.Conditions.Add(newCondition.GetComponent<Condition>());

                    #endregion

                    #region here we begin checking to see if any UID values we have for reflected data in the temp reflected data. if so , we destroy their conditions and replace the m with the conditions in the TempReflectedDataSet

                    if (selectedDialoguer.TempReflectedDataSet.Count != 0)
                        foreach (var tempData in selectedDialoguer.TempReflectedDataSet)
                        {
                            //we can use ReflectedDataSet[i] because the sorted list count and ReflectedDataSet ount are the same
                            var data = selectedDialoguer.ReflectedDataSet[i];

                            if (selectedDialoguer.sceneData.FullCharacterDialogueSet[i].UID == tempData.UID)
                            {
                                data.DialoguerGameObject = tempData.DialoguerGameObject;
                                data.dialoguer = tempData.dialoguer;
                                data.dialoguerComponent = tempData.dialoguerComponent;
                                data.RouteSpecificDataset = tempData.RouteSpecificDataset;
                                data.ActionSpecificData = tempData.ActionSpecificData;
                                data.DialogueSpecificData = tempData.DialogueSpecificData;
                                data.LinkSpecificData = tempData.LinkSpecificData;
                                data.EndSpecificData = tempData.EndSpecificData;
                                data.EnvironmentSpecificData = tempData.EnvironmentSpecificData;

                                /* foreach (var dataset in data.dialoguer.sceneData.Characters[data.dialoguer.].NodeDataInMyChain)
                                 {
                                     dataset.IsPlayer  = tempData.dialoguer.sceneData.Characters[data.dialoguer.targetChararacterIndex].IsPlayer;
                                 }*/


                                for (var c = 0; c < data.Conditions.Count; c++)
                                {
                                    var conditionToDelete = data.Conditions[c];
                                    DestroyImmediate(conditionToDelete.Self);
                                    data.Conditions.RemoveAt(c);
                                }

                                // finally we move the condition from TempReflectedDataSet[i] conditions to the datas condition list
                                foreach (var condition in tempData.Conditions)
                                {
                                    condition.Self.transform.SetParent(data.self.transform);
                                    data.Conditions.Add(condition);
                                }
                            }
                        }


                    #endregion
                }



                // now destroy all the data in TempReflectedDataSet
                foreach (var item in selectedDialoguer.TempReflectedDataSet) DestroyImmediate(item.self);
                selectedDialoguer.TempReflectedDataSet.RemoveAll(n => n == null);

                //UpdateUID.stringValue = selectedDialoguer.sceneData.UpdateUID;

            }

            GUILayout.Space(5);

         /*   if(!UpdateUID.stringValue.Equals(selectedDialoguer.sceneData.UpdateUID))
            {

                EditorGUILayout.HelpBox("Your scene data has been updated. Click the setup button to continue", MessageType.Info);
                
                
                return;
            }*/

            selectedDialoguer.sceneData.LanguageIndex = EditorGUILayout.Popup(selectedDialoguer.sceneData.LanguageIndex, selectedDialoguer.sceneData.LanguageNameArray);

            GUILayout.Space(10);

             /*  if (GUILayout.Button("Show / Hide Hidden Data"))
               {
                   if (selectedDialoguer.ReflectedDataParent != null)
                       if (selectedDialoguer.ReflectedDataParent.hideFlags == HideFlags.None)
                           selectedDialoguer.ReflectedDataParent.hideFlags = HideFlags.HideInHierarchy;
                       else
                           selectedDialoguer.ReflectedDataParent.hideFlags = HideFlags.None;


               }*/
               

            Separator2();
            Separator3();

            #region begin checking and setting the tempStory data reference
            if (tempStoryData == null)
            {
                if (File.Exists(Application.dataPath + "/TempStoryData.asset"))
                {
                    var data = AssetDatabase.LoadAllAssetsAtPath("Assets/TempStoryData.asset");
                    foreach (var asset in data)
                        if (asset.GetType() == typeof(Story))
                            tempStoryData = asset as Story;
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "It seems that you have no Storyteller project open. Please open your storyteller project ",
                        MessageType.Info);
                    return;
                }
            }
            #endregion

            if (selectedDialoguer.ReflectedDataParent == null) return;


            #region General Settings



            ShowGeneralSettings.boolValue = EditorGUILayout.Foldout(selectedDialoguer.ShowGeneralSettings,
                "General Settings (for all nodes)");

            if (selectedDialoguer.ShowGeneralSettings)
            {
                if (showHelpMessage)
                    EditorGUILayout.HelpBox("By turning on Use Text you are choosing to use a UI system to display dialogue", MessageType.Info);

                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Use Text");
                GUILayout.FlexibleSpace();
                var usesTextPowerIcon = selectedDialoguer.UsesText ? ImageLibrary.PowerOnpro : ImageLibrary.PowerOffpro;
                if (GUILayout.Button(usesTextPowerIcon, GUIStyle.none, GUILayout.Width(15), GUILayout.Height(15)))
                    UsesText.boolValue = !selectedDialoguer.UsesText;
                GUILayout.EndHorizontal();
                GUILayout.Space(5);

                if (selectedDialoguer.UsesText)
                {
                    selectedDialoguer.ShowUIAndTextSettings =
                        EditorGUILayout.Foldout(selectedDialoguer.ShowUIAndTextSettings, "UI Settings");

                    if (selectedDialoguer.ShowUIAndTextSettings)
                    {
                        #region sdettings for Name UI

                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button(useNameUI.boolValue ? ImageLibrary.PowerOnpro : ImageLibrary.PowerOffpro,
                            GUIStyle.none, GUILayout.Width(15), GUILayout.Height(15)))
                            useNameUI.boolValue = !useNameUI.boolValue;
                        GUILayout.Label("Use a name UI");
                        GUILayout.EndHorizontal();
                        GUILayout.Space(5);

                        EditorGUI.BeginDisabledGroup(!useNameUI.boolValue);
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Name UI");
                        GUILayout.FlexibleSpace();
                        NameText.objectReferenceValue = (TextMeshProUGUI)EditorGUILayout.ObjectField(selectedDialoguer.NameText,
                            typeof(TextMeshProUGUI), true, GUILayout.Height(15), GUILayout.Width(115));
                        GUILayout.EndHorizontal();
                        EditorGUI.EndDisabledGroup();
                        GUILayout.Space(5);
                        Separator2();

                        #endregion


                        #region sdettings for Dialogue UI

                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button(
                            useDialogueTextUI.boolValue ? ImageLibrary.PowerOnpro : ImageLibrary.PowerOffpro,
                            GUIStyle.none, GUILayout.Width(15), GUILayout.Height(15)))
                            useDialogueTextUI.boolValue = !useDialogueTextUI.boolValue;
                        GUILayout.Label("Use dialogue text UI");
                        GUILayout.EndHorizontal();
                        GUILayout.Space(5);

                        EditorGUI.BeginDisabledGroup(!useDialogueTextUI.boolValue);
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Dialogue Text UI");
                        GUILayout.FlexibleSpace();
                      /*  DisplayedTextUI.objectReferenceValue = (Text)EditorGUILayout.ObjectField(
                            selectedDialoguer.DisplayedTextUI, typeof(Text), true, GUILayout.Height(15),
                            GUILayout.Width(115));*/
 DisplayedText.objectReferenceValue = (TextMeshProUGUI)EditorGUILayout.ObjectField(
                            selectedDialoguer.DisplayedText, typeof(TextMeshProUGUI), true, GUILayout.Height(15),
                            GUILayout.Width(115));
                        GUILayout.EndHorizontal();
                        EditorGUI.EndDisabledGroup();
                        GUILayout.Space(5);
                        Separator2();

                        #endregion

                        #region sdettings for Next Button UI

                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button(
                            useMoveNextButton.boolValue ? ImageLibrary.PowerOnpro : ImageLibrary.PowerOffpro,
                            GUIStyle.none, GUILayout.Width(15), GUILayout.Height(15)))
                            useMoveNextButton.boolValue = !useMoveNextButton.boolValue;
                        GUILayout.Label("Use next button UI");
                        GUILayout.EndHorizontal();
                        GUILayout.Space(5);

                        EditorGUI.BeginDisabledGroup(!useMoveNextButton.boolValue);
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Button Next");
                        GUILayout.FlexibleSpace();
                        moveNextButton.objectReferenceValue = (Button)EditorGUILayout.ObjectField(
                            selectedDialoguer.MoveNextButton, typeof(Button), true, GUILayout.Height(15),
                            GUILayout.Width(115));
                        GUILayout.EndHorizontal();
                        EditorGUI.EndDisabledGroup();
                        GUILayout.Space(5);
                        Separator2();

                        #endregion


                        #region sdettings for Previous Button UI

                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button(
                            useMovePreviousButton.boolValue ? ImageLibrary.PowerOnpro : ImageLibrary.PowerOffpro,
                            GUIStyle.none, GUILayout.Width(15), GUILayout.Height(15)))
                            useMovePreviousButton.boolValue = !useMovePreviousButton.boolValue;
                        GUILayout.Label("Use previous button UI");
                        GUILayout.EndHorizontal();
                        GUILayout.Space(5);

                        EditorGUI.BeginDisabledGroup(!useMovePreviousButton.boolValue);
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Button Previous");
                        GUILayout.FlexibleSpace();
                        movePreviousButton.objectReferenceValue = (Button)EditorGUILayout.ObjectField(
                            selectedDialoguer.MovePreviousButton, typeof(Button), true, GUILayout.Height(15),
                            GUILayout.Width(115));
                        GUILayout.EndHorizontal();
                        EditorGUI.EndDisabledGroup();
                        GUILayout.Space(5);
                        Separator2();

                        #endregion


                        #region sdettings for Route  UI

                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button(
                            useRouteButton.boolValue ? ImageLibrary.PowerOnpro : ImageLibrary.PowerOffpro,
                            GUIStyle.none, GUILayout.Width(15), GUILayout.Height(15)))
                            useRouteButton.boolValue = !useRouteButton.boolValue;
                        GUILayout.Label("Use route button UI");
                        GUILayout.EndHorizontal();
                        GUILayout.Space(5);

                        EditorGUI.BeginDisabledGroup(!useRouteButton.boolValue);
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Route Parent");
                        GUILayout.FlexibleSpace();
                        RouteParent.objectReferenceValue = (GameObject)EditorGUILayout.ObjectField(
                            selectedDialoguer.RouteParent, typeof(GameObject), true, GUILayout.Height(15),
                            GUILayout.Width(115));
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Route Button");
                        GUILayout.FlexibleSpace();
                        RouteButton.objectReferenceValue = (Button)EditorGUILayout.ObjectField(
                            selectedDialoguer.RouteButton, typeof(Button), true, GUILayout.Height(15),
                            GUILayout.Width(115));
                        GUILayout.EndHorizontal();
                        EditorGUI.EndDisabledGroup();
                        GUILayout.Space(5);
                        Separator2();

                        #endregion
                    }

                    if (showHelpMessage)
                        EditorGUILayout.HelpBox("In text display settings you have the option of using typed text or immediately displayed text", MessageType.Info);

                    selectedDialoguer.ShowTextDisplayModeSettings =
                        EditorGUILayout.Foldout(selectedDialoguer.ShowTextDisplayModeSettings, "Text Display Settings");

                    if (selectedDialoguer.ShowTextDisplayModeSettings)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Text Display Mode");
                        textDisplayMode.enumValueIndex =
                            (int)(CharacterTextDisplayMode)EditorGUILayout.EnumPopup(
                                selectedDialoguer.textDisplayMode, GUILayout.Height(15), GUILayout.Width(115));
                        GUILayout.EndHorizontal();

                        switch (selectedDialoguer.textDisplayMode)
                        {
                            case DialoguerTextDisplayMode.Instant:

                                break;
                            case DialoguerTextDisplayMode.Typed:
                                GUILayout.Space(5);

                                GUILayout.BeginHorizontal();
                                GUILayout.Label("Typing Speed");
                                GUILayout.FlexibleSpace();
                                TypingSpeed.floatValue = EditorGUILayout.FloatField(selectedDialoguer.TypingSpeed,
                                    GUILayout.Height(15), GUILayout.Width(115));
                                GUILayout.EndHorizontal();


                                GUILayout.BeginHorizontal();
                                GUILayout.Label("Typing Sound");
                                GUILayout.FlexibleSpace();
                                TypingAudioCip.objectReferenceValue =
                                    (AudioClip)EditorGUILayout.ObjectField(selectedDialoguer.TypingAudioCip,
                                        typeof(AudioClip), false, GUILayout.Height(15), GUILayout.Width(115));
                                GUILayout.EndHorizontal();

                                GUILayout.Space(5);


                                break;
                                /*    case DialoguerTextDisplayMode.Custom:

                                        break;*/
                        }
                    }

                    GUILayout.Space(10);
                    Separator();


                    if (showHelpMessage)
                        EditorGUILayout.HelpBox("With Keyword filters turned on, this Player character can replace any word in dialogue with another word." +
                            " There are four variations of text replacement settings. Replacing static text with dynamic text, dynamic text with static text, ststic text with ststic text and dynamic text with dynamic text", MessageType.Info);


                    var keywordIdentifierCount = selectedDialoguer.KeywordFilters.Count;

                    GUILayout.Space(5);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Use Keyword Filters");
                    GUILayout.FlexibleSpace();
                    var usesKeywordFiltersPowerIcon = selectedDialoguer.UseKeywordFilters
                        ? ImageLibrary.PowerOnpro
                        : ImageLibrary.PowerOffpro;
                    if (GUILayout.Button(usesKeywordFiltersPowerIcon, GUIStyle.none, GUILayout.Width(15),
                        GUILayout.Height(15)))
                        UseKeywordFilters.boolValue = !selectedDialoguer.UseKeywordFilters;
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5);


                    if (selectedDialoguer.UseKeywordFilters)
                    {
                        if (showHelpMessage)
                            EditorGUILayout.HelpBox("Keyword filters change marked words into other words",
                            MessageType.Info);


                        ShowKeywordFilterFouldout.boolValue =
                            EditorGUILayout.Foldout(selectedDialoguer.ShowKeywordFilterFouldout,
                                "Show Key Word Filters");

                        if (selectedDialoguer.ShowKeywordFilterFouldout)
                        {
                            GUILayout.Space(5);
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Amount Of Filters", GUILayout.Width(115));
                            GUILayout.FlexibleSpace();
                            FilterCount.intValue =
                                EditorGUILayout.DelayedIntField(selectedDialoguer.FilterCount, GUILayout.Width(100));
                            if (keywordIdentifierCount != selectedDialoguer.FilterCount)
                                selectedDialoguer.KeywordFilters.Resize(selectedDialoguer.FilterCount);
                            GUILayout.Space(15);
                            GUILayout.EndHorizontal();
                            GUILayout.Space(5);

                            for (var i = 0; i < selectedDialoguer.KeywordFilters.Count; i++)
                            {
                                if (selectedDialoguer.KeywordFilters[i] == null)
                                    selectedDialoguer.KeywordFilters[i] = new KeywordFilter();


                                var area = GUILayoutUtility.GetLastRect().AddRect(-15, 0, 0, 10);
                                var filterAreHeight = selectedDialoguer.KeywordFilters[i].StaticKeywordMethod && selectedDialoguer.KeywordFilters[i].StaticReplacementStringMethod ? 105 : 170;
                                var filterArea = area.PlaceUnder(Screen.width, filterAreHeight);
                                GUI.DrawTexture(filterArea, Textures.DuskLightest);
                                var headerArea = filterArea.ToUpperLeft(0, 3, 0, 15);
                                GUI.DrawTexture(headerArea, Textures.DuskLighter);

                                #region Header Content



                                var moveKeywordFilterUpButtonArea = headerArea.ToCenterLeft(15, 8, 25, -10);
                                if (ClickEvent.Click(1, moveKeywordFilterUpButtonArea, ImageLibrary.upArrow, "Move this Keyword Filter up by one position"))
                                {
                                    if (i > 0)
                                    {
                                        var KeywordFilterAtTop = selectedDialoguer.KeywordFilters[i - 1];

                                        selectedDialoguer.KeywordFilters[i - 1] = null;
                                        // selectedDialoguer.KeywordFilters[i] = null;

                                        selectedDialoguer.KeywordFilters[i - 1] = selectedDialoguer.KeywordFilters[i];
                                        selectedDialoguer.KeywordFilters[i] = KeywordFilterAtTop;
                                    }
                                }


                                var moveKeywordFilterDownButtonArea = moveKeywordFilterUpButtonArea.PlaceToRight(15, 0, 20);
                                if (ClickEvent.Click(1, moveKeywordFilterDownButtonArea, ImageLibrary.downArrow, "Move this keyword Filter down by one position"))
                                {
                                    if (i != selectedDialoguer.KeywordFilters.Count - 1)
                                    {
                                        var KeywordFilterAtBottom = selectedDialoguer.KeywordFilters[i + 1];

                                        selectedDialoguer.KeywordFilters[i + 1] = null;
                                        //selectedDialoguer.KeywordFilters[i] = null;

                                        selectedDialoguer.KeywordFilters[i + 1] = selectedDialoguer.KeywordFilters[i];
                                        selectedDialoguer.KeywordFilters[i] = KeywordFilterAtBottom;
                                    }
                                }



                                if (ClickEvent.Click(1, headerArea.ToCenterRight(15, 15, -25, -8),
    selectedDialoguer.KeywordFilters[i].Disabled ? ImageLibrary.PowerOffpro : ImageLibrary.PowerOnpro, "Enable / Disable this Keyword Filter"))
                                    selectedDialoguer.KeywordFilters[i].Disabled = !selectedDialoguer.KeywordFilters[i].Disabled;
                                #endregion

                                EditorGUI.BeginDisabledGroup(selectedDialoguer.KeywordFilters[i].Disabled);

                                GUILayout.Space(25);
                                GUILayout.BeginHorizontal();
                                GUILayout.BeginVertical();
                                GUILayout.Label("Key Word", GUILayout.Width(100));
                                selectedDialoguer.KeywordFilters[i].StaticKeywordMethod = GUILayout.Toggle(selectedDialoguer.KeywordFilters[i].StaticKeywordMethod, "Static Method", GUILayout.Width(100));
                                GUILayout.EndVertical();
                                GUILayout.FlexibleSpace();
                                GUILayout.BeginVertical();
                                GUILayout.Label("Replacement", GUILayout.Width(100));
                                selectedDialoguer.KeywordFilters[i].StaticReplacementStringMethod = GUILayout.Toggle(selectedDialoguer.KeywordFilters[i].StaticReplacementStringMethod, "Static Method", GUILayout.Width(100));
                                GUILayout.EndVertical();
                                GUILayout.Space(15);
                                GUILayout.EndHorizontal();

                                var keyWordIdentifier = selectedDialoguer.KeywordFilters[i];

                                if (keyWordIdentifier == null)
                                    keyWordIdentifier = new KeywordFilter();

                                GUILayout.Space(5);
                                GUILayout.BeginHorizontal();

                                if (keyWordIdentifier.StaticKeywordMethod)
                                {
                                    keyWordIdentifier.KeyWord =
                                    EditorGUILayout.TextField(keyWordIdentifier.KeyWord, GUILayout.Width(100), GUILayout.Height(20));
                                }
                                else
                                {
                                    #region dynamic keyword string

                                    GUILayout.BeginVertical();
                                    GUILayout.BeginHorizontal();
                                    GUILayout.FlexibleSpace();

                                    selectedDialoguer.KeywordFilters[i].DynamicKeyword.TargetGameObject = (GameObject)EditorGUILayout.ObjectField(selectedDialoguer.KeywordFilters[i].DynamicKeyword.TargetGameObject,
                                       typeof(GameObject), true, GUILayout.Height(15));
                                    GUILayout.FlexibleSpace();
                                    GUILayout.EndHorizontal();

                                    if (selectedDialoguer.KeywordFilters[i].DynamicKeyword.cachedTargetObject != selectedDialoguer.KeywordFilters[i].DynamicKeyword.TargetGameObject)
                                    {
                                        selectedDialoguer.KeywordFilters[i].DynamicKeyword.Components = new Component[0];
                                        selectedDialoguer.KeywordFilters[i].DynamicKeyword.serializedMethods = new SerializableMethodInfo[0];

                                        selectedDialoguer.KeywordFilters[i].DynamicKeyword.SetComponent(0);
                                        selectedDialoguer.KeywordFilters[i].DynamicKeyword.SetMethod(0);
                                        selectedDialoguer.KeywordFilters[i].DynamicKeyword.cachedTargetObject = selectedDialoguer.KeywordFilters[i].DynamicKeyword.TargetGameObject;
                                    }


                                    GUILayout.BeginHorizontal();
                                    GUILayout.FlexibleSpace();
                                    GUILayout.Box(ImageLibrary.DownFlowArrow, EditorStyles.label, GUILayout.Width(15),
                                        GUILayout.Height(15));
                                    GUILayout.FlexibleSpace();
                                    GUILayout.EndHorizontal();


                                    var disabledComponents = selectedDialoguer.KeywordFilters[i].DynamicKeyword.TargetGameObject == null;
                                    EditorGUI.BeginDisabledGroup(disabledComponents);

                                    if (disabledComponents &&
                                        (selectedDialoguer.KeywordFilters[i].DynamicKeyword.Components.Count() != 0 || selectedDialoguer.KeywordFilters[i].DynamicKeyword.serializedMethods.Count() != 0))
                                    {
                                        selectedDialoguer.KeywordFilters[i].DynamicKeyword.Components = new Component[0];
                                        selectedDialoguer.KeywordFilters[i].DynamicKeyword.serializedMethods = new SerializableMethodInfo[0];
                                    }

                                    GUILayout.BeginHorizontal();
                                    GUILayout.FlexibleSpace();
                                    var componentName = !selectedDialoguer.KeywordFilters[i].DynamicKeyword.Components.Any()
                                        ? "None"
                                        : selectedDialoguer.KeywordFilters[i].DynamicKeyword.Components[selectedDialoguer.KeywordFilters[i].DynamicKeyword.ComponentIndex].GetType().Name;


                                    if (GUILayout.Button(componentName, EditorStyles.popup, GUILayout.MinWidth(100), GUILayout.Height(15)))
                                    {
                                        selectedDialoguer.KeywordFilters[i].DynamicKeyword.GetGameObjectComponents();
                                        var menu = new GenericMenu();
                                        for (var u = 0; u < selectedDialoguer.KeywordFilters[i].DynamicKeyword.Components.Length; u++)
                                            menu.AddItem(new GUIContent(selectedDialoguer.KeywordFilters[i].DynamicKeyword.Components[u].GetType().Name),
                                                   selectedDialoguer.KeywordFilters[i].DynamicKeyword.ComponentIndex.Equals(u), selectedDialoguer.KeywordFilters[i].DynamicKeyword.SetComponent, u);
                                        menu.ShowAsContext();
                                    }

                                    GUILayout.FlexibleSpace();
                                    GUILayout.EndHorizontal();

                                    GUILayout.BeginHorizontal();
                                    GUILayout.FlexibleSpace();
                                    GUILayout.Box(ImageLibrary.DownFlowArrow, EditorStyles.label, GUILayout.Width(15),
                                        GUILayout.Height(15));
                                    GUILayout.FlexibleSpace();
                                    GUILayout.EndHorizontal();

                                    GUILayout.BeginHorizontal();
                                    GUILayout.FlexibleSpace();
                                    var methodName = !selectedDialoguer.KeywordFilters[i].DynamicKeyword.serializedMethods.Any()
                                        ? "None"
                                        : selectedDialoguer.KeywordFilters[i].DynamicKeyword.serializedMethods[selectedDialoguer.KeywordFilters[i].DynamicKeyword.MethodIndex].methodName;
                                    //  var disabledMethods = condition.TargetGameObject == null;


                                    if (GUILayout.Button(methodName, EditorStyles.popup, GUILayout.MinWidth(100), GUILayout.Height(15)))
                                    {
                                        selectedDialoguer.KeywordFilters[i].DynamicKeyword.GetComponentMethods();
                                        var menu = new GenericMenu();
                                        for (var u = 0; u < selectedDialoguer.KeywordFilters[i].DynamicKeyword.serializedMethods.Length; u++)
                                            menu.AddItem(new GUIContent(selectedDialoguer.KeywordFilters[i].DynamicKeyword.serializedMethods[u].methodInfo.Name),
                                                   selectedDialoguer.KeywordFilters[i].DynamicKeyword.MethodIndex.Equals(u), selectedDialoguer.KeywordFilters[i].DynamicKeyword.SetMethod, u);
                                        menu.ShowAsContext();
                                    }

                                    GUILayout.FlexibleSpace();
                                    GUILayout.EndHorizontal();



                                    EditorGUI.EndDisabledGroup();



                                    GUILayout.EndVertical();

                                    #endregion
                                }
                                GUILayout.FlexibleSpace();

                                if (keyWordIdentifier.StaticReplacementStringMethod)
                                {
                                    keyWordIdentifier.ReplacementString =
                                        EditorGUILayout.TextField(keyWordIdentifier.ReplacementString, GUILayout.Width(100),GUILayout.Height(20));
                                }
                                else
                                {
                                    #region dynamic replacement String

                                    GUILayout.BeginVertical();
                                    GUILayout.BeginHorizontal();
                                    GUILayout.FlexibleSpace();

                                    selectedDialoguer.KeywordFilters[i].DynamicReplacementString.TargetGameObject = (GameObject)EditorGUILayout.ObjectField(selectedDialoguer.KeywordFilters[i].DynamicReplacementString.TargetGameObject,
                                       typeof(GameObject), true, GUILayout.Height(15));
                                    GUILayout.FlexibleSpace();
                                    GUILayout.EndHorizontal();

                                    if (selectedDialoguer.KeywordFilters[i].DynamicReplacementString.cachedTargetObject != selectedDialoguer.KeywordFilters[i].DynamicReplacementString.TargetGameObject)
                                    {
                                        selectedDialoguer.KeywordFilters[i].DynamicReplacementString.Components = new Component[0];
                                        selectedDialoguer.KeywordFilters[i].DynamicReplacementString.serializedMethods = new SerializableMethodInfo[0];

                                        selectedDialoguer.KeywordFilters[i].DynamicReplacementString.SetComponent(0);
                                        selectedDialoguer.KeywordFilters[i].DynamicReplacementString.SetMethod(0);
                                        selectedDialoguer.KeywordFilters[i].DynamicReplacementString.cachedTargetObject = selectedDialoguer.KeywordFilters[i].DynamicReplacementString.TargetGameObject;
                                    }


                                    GUILayout.BeginHorizontal();
                                    GUILayout.FlexibleSpace();
                                    GUILayout.Box(ImageLibrary.DownFlowArrow, EditorStyles.label, GUILayout.Width(15),
                                        GUILayout.Height(15));
                                    GUILayout.FlexibleSpace();
                                    GUILayout.EndHorizontal();


                                    var disabledComponents = selectedDialoguer.KeywordFilters[i].DynamicReplacementString.TargetGameObject == null;
                                    EditorGUI.BeginDisabledGroup(disabledComponents);

                                    if (disabledComponents &&
                                        (selectedDialoguer.KeywordFilters[i].DynamicReplacementString.Components.Count() != 0 || selectedDialoguer.KeywordFilters[i].DynamicReplacementString.serializedMethods.Count() != 0))
                                    {
                                        selectedDialoguer.KeywordFilters[i].DynamicReplacementString.Components = new Component[0];
                                        selectedDialoguer.KeywordFilters[i].DynamicReplacementString.serializedMethods = new SerializableMethodInfo[0];
                                    }

                                    GUILayout.BeginHorizontal();
                                    GUILayout.FlexibleSpace();
                                    var componentName = !selectedDialoguer.KeywordFilters[i].DynamicReplacementString.Components.Any()
                                        ? "None"
                                        : selectedDialoguer.KeywordFilters[i].DynamicReplacementString.Components[selectedDialoguer.KeywordFilters[i].DynamicReplacementString.ComponentIndex].GetType().Name;


                                    if (GUILayout.Button(componentName, EditorStyles.popup, GUILayout.MinWidth(100), GUILayout.Height(15)))
                                    {
                                        selectedDialoguer.KeywordFilters[i].DynamicReplacementString.GetGameObjectComponents();
                                        var menu = new GenericMenu();
                                        for (var u = 0; u < selectedDialoguer.KeywordFilters[i].DynamicReplacementString.Components.Length; u++)
                                            menu.AddItem(new GUIContent(selectedDialoguer.KeywordFilters[i].DynamicReplacementString.Components[u].GetType().Name),
                                                   selectedDialoguer.KeywordFilters[i].DynamicReplacementString.ComponentIndex.Equals(u), selectedDialoguer.KeywordFilters[i].DynamicReplacementString.SetComponent, u);
                                        menu.ShowAsContext();
                                    }

                                    GUILayout.FlexibleSpace();
                                    GUILayout.EndHorizontal();

                                    GUILayout.BeginHorizontal();
                                    GUILayout.FlexibleSpace();
                                    GUILayout.Box(ImageLibrary.DownFlowArrow, EditorStyles.label, GUILayout.Width(15),
                                        GUILayout.Height(15));
                                    GUILayout.FlexibleSpace();
                                    GUILayout.EndHorizontal();

                                    GUILayout.BeginHorizontal();
                                    GUILayout.FlexibleSpace();
                                    var methodName = !selectedDialoguer.KeywordFilters[i].DynamicReplacementString.serializedMethods.Any()
                                        ? "None"
                                        : selectedDialoguer.KeywordFilters[i].DynamicReplacementString.serializedMethods[selectedDialoguer.KeywordFilters[i].DynamicReplacementString.MethodIndex].methodName;
                                    //  var disabledMethods = condition.TargetGameObject == null;


                                    if (GUILayout.Button(methodName, EditorStyles.popup, GUILayout.MinWidth(100), GUILayout.Height(15)))
                                    {
                                        selectedDialoguer.KeywordFilters[i].DynamicReplacementString.GetComponentMethods();
                                        var menu = new GenericMenu();
                                        for (var u = 0; u < selectedDialoguer.KeywordFilters[i].DynamicReplacementString.serializedMethods.Length; u++)
                                            menu.AddItem(new GUIContent(selectedDialoguer.KeywordFilters[i].DynamicReplacementString.serializedMethods[u].methodInfo.Name),
                                                   selectedDialoguer.KeywordFilters[i].DynamicReplacementString.MethodIndex.Equals(u), selectedDialoguer.KeywordFilters[i].DynamicReplacementString.SetMethod, u);
                                        menu.ShowAsContext();
                                    }

                                    GUILayout.FlexibleSpace();
                                    GUILayout.EndHorizontal();



                                    EditorGUI.EndDisabledGroup();



                                    GUILayout.EndVertical();

                                    #endregion
                                }

                                EditorGUI.EndDisabledGroup();

                                GUILayout.EndHorizontal();

                                GUILayout.Space(5);

                                if (GUILayout.Button(ImageLibrary.deleteConditionIcon, GUIStyle.none,
                                    GUILayout.Width(15)))
                                {
                                    selectedDialoguer.KeywordFilters.RemoveAt(i);
                                    selectedDialoguer.KeywordFilters.RemoveAll(n => n == null);
                                    selectedDialoguer.FilterCount = selectedDialoguer.KeywordFilters.Count;
                                }
                                GUILayout.Space(15);
                            }
                        }
                    }
                }

                Separator();


                /* GUILayout.Space(5);
                 GUILayout.BeginHorizontal();
                 GUILayout.Label("Use Storyboard Images");
                 GUILayout.FlexibleSpace();
                 var usesStoryboardImagesPowerIcon = selectedDialoguer.UsesStoryboardImages ? ImageLibrary.PowerOnpro : ImageLibrary.PowerOffpro;
                 if (GUILayout.Button(usesStoryboardImagesPowerIcon, GUIStyle.none, GUILayout.Width(15), GUILayout.Height(15)))
                     UsesStoryboardImages.boolValue = !selectedDialoguer.UsesStoryboardImages;
                 GUILayout.EndHorizontal();
                 GUILayout.Space(5);
 
                 if (selectedDialoguer.UsesStoryboardImages)
                 { }
                 Separator();*/

                if (showHelpMessage)
                    EditorGUILayout.HelpBox("The UseVoiceover toggle preps your character to do voice clip playbacks", MessageType.Info);

                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Use Voiceover");
                GUILayout.FlexibleSpace();
                var UsesVoiceOverPowerIcon =
                    selectedDialoguer.UsesVoiceOver ? ImageLibrary.PowerOnpro : ImageLibrary.PowerOffpro;
                if (GUILayout.Button(UsesVoiceOverPowerIcon, GUIStyle.none, GUILayout.Width(15), GUILayout.Height(15)))
                    UsesVoiceOver.boolValue = !selectedDialoguer.UsesVoiceOver;
                GUILayout.EndHorizontal();
                GUILayout.Space(5);

                if (selectedDialoguer.UsesVoiceOver)
                {
                    if (showHelpMessage)
                        EditorGUILayout.HelpBox("With Auto Start turned on, your voice clip will play as soon as the data associated with the voice clip is processed (Using the Condition system to trigger playback is recommended)", MessageType.Info);

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(useAutoStartVoiceClip.boolValue ? ImageLibrary.PowerOnpro : ImageLibrary.PowerOffpro,
                        GUIStyle.none, GUILayout.Width(15), GUILayout.Height(15)))
                        useAutoStartVoiceClip.boolValue = !useAutoStartVoiceClip.boolValue;
                    GUILayout.Label("Auto start");
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5);
                }

                Separator();

                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Use Sound Effects");
                GUILayout.FlexibleSpace();
                var usesSoundffectsPowerIcon = UsesSoundffects.boolValue
                    ? ImageLibrary.PowerOnpro
                    : ImageLibrary.PowerOffpro;
                if (GUILayout.Button(usesSoundffectsPowerIcon, GUIStyle.none, GUILayout.Width(15),
                    GUILayout.Height(15)))
                    UsesSoundffects.boolValue = !UsesSoundffects.boolValue;
                GUILayout.EndHorizontal();
                GUILayout.Space(2);

                if (selectedDialoguer.UsesSoundffects)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(useAutoStartSoundEffectClip.boolValue ? ImageLibrary.PowerOnpro : ImageLibrary.PowerOffpro,
                        GUIStyle.none, GUILayout.Width(15), GUILayout.Height(15)))
                        useAutoStartSoundEffectClip.boolValue = !useAutoStartSoundEffectClip.boolValue;
                    GUILayout.Label("Auto start");
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5);
                }
            }


            GUILayout.Space(5);

            #endregion





            #region if no scenes are in the project

            if (tempStoryData.Scenes.Count == 0) return;

            #endregion

            if (CurrentStory.SceneOrderEdited)
            {
                var targetScene = tempStoryData.Scenes.Find(s => s.UID == selectedDialoguer.sceneData.UID);
                if (targetScene != null)
                {
                    var id = tempStoryData.Scenes.IndexOf(targetScene);
                    selectedDialoguer.sceneData.SceneID = id;
                }
                CurrentStory.SceneOrderEdited = false;

            }

            if (selectedDialoguer.sceneData.SceneID + 1 > tempStoryData.Scenes.Count)
            {
                EditorGUILayout.HelpBox(
                    "Make sure that this Story project is the correct project. Else check to ensure that the scene was not deleted ",
                    MessageType.Info);
                // if (GUILayout.Button("Attempt Correction"))
                CurrentStory.SceneOrderEdited = true;

                return;
            }

            var scene = tempStoryData.Scenes[selectedDialoguer.sceneData.SceneID];

            #region if no nodes are in the scene

            if (scene.NodeElements.Count == 0)
            {
                EditorGUILayout.HelpBox("There is nothing in this Storyteller scene", MessageType.Info);
                return;
            }

            #endregion

            var selectedNode = scene.NodeElements.Last();


            #region check if we made a selection of a diferent node

            if (UID != selectedNode.UID)
            {
                matchingSelectedNodeData =
                    selectedDialoguer.sceneData.FullCharacterDialogueSet.Find(n => n.UID == selectedNode.UID);
                matchingReflectedData = selectedDialoguer.ReflectedDataSet.Find(r => r.UID == selectedNode.UID);

                UID = selectedNode.UID;


                //  matchingNodeDataSerializedObject = null;
            }

            #endregion

            #region if there is no matchingSelectedNodeData

            if (matchingSelectedNodeData == null) return;

            #endregion


           

            if (matchingSelectedNodeData.useTime)
            {
                GUILayout.Space(15);
                Separator2();
                Separator3();



                GUILayout.BeginHorizontal();
                GUILayout.Label(" Auto Start All Conditions By Default");
                GUILayout.FlexibleSpace();
                var AutoStartAllConditionsByDefaultIcon = AutoStartAllConditionsByDefault.boolValue
                    ? ImageLibrary.PowerOnpro
                    : ImageLibrary.PowerOffpro;
                if (GUILayout.Button(AutoStartAllConditionsByDefaultIcon, GUIStyle.none, GUILayout.Width(15),
                    GUILayout.Height(15)))
                {
                    AutoStartAllConditionsByDefault.boolValue = !AutoStartAllConditionsByDefault.boolValue;
                    foreach (var condition in matchingReflectedData.Conditions)
                    {
                        Undo.RegisterCompleteObjectUndo(condition, "Condition");
                        condition.AutoStart = AutoStartAllConditionsByDefault.boolValue;
                    }
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.Space(15);
            Separator2();
            Separator3();

            #region Draw Selected Node name

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(matchingSelectedNodeData.CharacterName, Theme.GameBridgeSkin.customStyles[5]);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(matchingSelectedNodeData.Name, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            #endregion

            GUILayout.Space(5);


            GUILayout.Space(10);


            #region node specific data

            selectedDialoguer.ShowNodeSpecificSettings =
                EditorGUILayout.Foldout(selectedDialoguer.ShowNodeSpecificSettings, "Show Node Specific Settings");

            GUILayout.Space(10);

            if (selectedDialoguer.ShowNodeSpecificSettings)
            {
                // we check if the matchingSelectedNodeData is a charactrNodeData, if it is , we show the option for setting the IsPlayer value
                if (matchingSelectedNodeData.type == typeof(CharacterNodeData))
                {
                    var character = (CharacterNodeData)matchingSelectedNodeData;

                    GUILayout.BeginHorizontal();

                    GUILayout.FlexibleSpace();
                    var state = character.IsPlayer ? "Is Player,Turn Off Player ?" : "Is Not Player,Turn On Player ?";
                    if (GUILayout.Button(state, GUILayout.Height(15)))
                    {
                        character.IsPlayer = !character.IsPlayer;
                        foreach (var dataset in character.NodeDataInMyChain)
                            dataset.IsPlayer = character.IsPlayer;
                        EditorUtility.SetDirty(selectedDialoguer.sceneData);
             
                    }

                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Box(character.IsPlayer ? ImageLibrary.CrownOn : ImageLibrary.CrownOff,
                        EditorStyles.inspectorDefaultMargins);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }


                if (matchingSelectedNodeData.type == typeof(EnvironmentNodeData))
                {
                }

                if (matchingSelectedNodeData.type == typeof(ActionNodeData))
                {
                    Separator3();

                    var action = (ActionNodeData)matchingSelectedNodeData;
                    GUILayout.Space(5);
                    action.LocalizedText[selectedDialoguer.sceneData.LanguageIndex] = EditorGUILayout.TextArea(action.LocalizedText[selectedDialoguer.sceneData.LanguageIndex], Theme.GameBridgeSkin.customStyles[7], GUILayout.Width(ScreenRect.width - 40), GUILayout.Height(100));
                    GUILayout.Space(5);
                    Separator();

                    Separator3();

                    if (selectedDialoguer.UsesSoundffects)
                    {
                        GUILayout.Space(5);
                        GUILayout.BeginHorizontal();
                        var overrideUseDurationLengthForSoundEffectsPowerIcon =
                            matchingReflectedData.ActionSpecificData.OverrideUseSoundEffect
                                ? ImageLibrary.PowerOnpro
                                : ImageLibrary.PowerOffRed;
                        if (GUILayout.Button(overrideUseDurationLengthForSoundEffectsPowerIcon, GUIStyle.none,
                            GUILayout.Width(15), GUILayout.Height(15)))
                            matchingReflectedData.ActionSpecificData.OverrideUseSoundEffect =
                                !matchingReflectedData.ActionSpecificData.OverrideUseSoundEffect;
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("Override Sound Effect");
                        GUILayout.EndHorizontal();
                        GUILayout.Space(5);

                        if (matchingReflectedData.ActionSpecificData.OverrideUseSoundEffect)
                            EditorGUILayout.HelpBox(
                                "The only override is to not use sound effects for this node in game",
                                MessageType.Warning);


                        EditorGUI.BeginDisabledGroup(matchingReflectedData.ActionSpecificData.OverrideUseSoundEffect);

                        GUILayout.Space(5);
                        action.LocalizedSoundEffects[selectedDialoguer.sceneData.LanguageIndex] = (AudioClip)EditorGUILayout.ObjectField("Sound Effect", action.LocalizedSoundEffects[selectedDialoguer.sceneData.LanguageIndex],
                            typeof(AudioClip), false);
                        GUILayout.Space(5);
                        Separator();

                        /*   GUILayout.Space(5);
                           GUILayout.BeginHorizontal();
                           GUILayout.Label("Use Duration Length For Soundeffects");
                           GUILayout.FlexibleSpace();
                           var useDurationLengthForSoundEffectsPowerIcon = action.useDurationLengthForSoundEffects ? ImageLibrary.PowerOnpro : ImageLibrary.PowerOffpro;
                           if (GUILayout.Button(useDurationLengthForSoundEffectsPowerIcon, GUIStyle.none, GUILayout.Width(15), GUILayout.Height(15)))
                           {
                               action.useDurationLengthForSoundEffects = !action.useDurationLengthForSoundEffects;
                               action.useSoundEffectLength = !action.useSoundEffectLength;
                           }
                           GUILayout.EndHorizontal();
                           GUILayout.Space(5);
                           Separator2();
   
                           GUILayout.Space(5);
                           GUILayout.BeginHorizontal();
                           GUILayout.Label("Use Sound Effect Length");
                           GUILayout.FlexibleSpace();
                           var useSoundEffectLengthPowerIcon = action.useSoundEffectLength ? ImageLibrary.PowerOnpro : ImageLibrary.PowerOffpro;
                           if (GUILayout.Button(useSoundEffectLengthPowerIcon, GUIStyle.none, GUILayout.Width(15), GUILayout.Height(15)))
                           {
                               action.useSoundEffectLength = !action.useSoundEffectLength;
                               action.useDurationLengthForSoundEffects = !action.useDurationLengthForSoundEffects;
   
                           }
                           GUILayout.EndHorizontal();
                           GUILayout.Space(5);
   
                           if (action.useSoundEffectLength)
                           {
                               if (action.SoundEffect != null)
                               {
                                   Separator2();
                                   GUILayout.Space(5);
                                   GUILayout.BeginHorizontal();
                                   GUILayout.Label("Soundeffect Duration");
                                   GUILayout.FlexibleSpace();
                                   GUILayout.Label(action.SoundEffect.length.ToString(), GUILayout.Width(100));
                                   GUILayout.EndHorizontal();
                                   GUILayout.Space(5);
                               }
                               else
                               {
                                   EditorGUILayout.HelpBox("This node data does not use any voice recordings", MessageType.Info);
                               }
                           }
                           Separator();*/

                        EditorGUI.EndDisabledGroup();
                    }

                    Separator3();

                    if (showHelpMessage)
                        EditorGUILayout.HelpBox("The Start time, Duration and Delay here are a reflection of the start time, duration and delay of the selected node", MessageType.Info);
                    EditorGUILayout.LabelField("Start Time", action.StartTime.ToString());
                    EditorGUILayout.LabelField("Duration", action.Duration.ToString());
                    EditorGUILayout.LabelField("Delay", action.Delay.ToString());
                    if (showHelpMessage)
                        EditorGUILayout.HelpBox("Realtime delay is set at runtime. it is recommended that you use Realtime dealy instead of Delay", MessageType.Info);
                    EditorGUILayout.LabelField("Realtime Delay", action.RealtimeDelay.ToString());
                    EditorGUILayout.HelpBox("Realtime delay is set at runtime. it is recommended that you use Realtime dealy instead of Delay", MessageType.Info);
                    // EditorGUILayout.LabelField("Realtime Delay", action.RealtimeDelay.ToString());
                }

                if (matchingSelectedNodeData.type == typeof(DialogueNodeData))
                {
                    var dialogue = (DialogueNodeData)matchingSelectedNodeData;


                    // if (selectedDialoguer.UsesText)
                    //   {
                    /*  GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                var overrideTextDisplayMethodPowerIcon = dialogue.OverrideTextDisplayMethod ? ImageLibrary.PowerOnpro : ImageLibrary.PowerOffRed;
                if (GUILayout.Button(overrideTextDisplayMethodPowerIcon, GUIStyle.none, GUILayout.Width(15), GUILayout.Height(15)))
                    dialogue.OverrideTextDisplayMethod = !dialogue.OverrideTextDisplayMethod;

                GUILayout.FlexibleSpace();
                GUILayout.Label("Override Text Setting");
                GUILayout.EndHorizontal();
                GUILayout.Space(5);

          if (dialogue.OverrideTextDisplayMethod)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Text Display Mode");
                    dialogue.OverridenCharacterTextDisplayMode = (CharacterTextDisplayMode)EditorGUILayout.EnumPopup(dialogue.OverridenCharacterTextDisplayMode, GUILayout.Height(15), GUILayout.Width(150));
                    GUILayout.EndHorizontal();

                    switch (dialogue.OverridenCharacterTextDisplayMode)
                    {
                        case CharacterTextDisplayMode.Instant:

                            break;
                        case CharacterTextDisplayMode.Typed:
                            GUILayout.Space(5);

                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Typing Speed");
                            GUILayout.FlexibleSpace();
                            selectedDialoguer.TypingSpeed = EditorGUILayout.IntField(selectedDialoguer.TypingSpeed, GUILayout.Height(15), GUILayout.Width(150));
                            GUILayout.EndHorizontal();


                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Delay");
                            GUILayout.FlexibleSpace();
                            selectedDialoguer.Delay = EditorGUILayout.FloatField(selectedDialoguer.Delay, GUILayout.Height(15), GUILayout.Width(150));
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Typing Sound");
                            GUILayout.FlexibleSpace();
                            selectedDialoguer.TypingAudioCip = (AudioClip)EditorGUILayout.ObjectField(selectedDialoguer.TypingAudioCip, typeof(AudioClip), false, GUILayout.Height(15), GUILayout.Width(150));
                            GUILayout.EndHorizontal();

                            GUILayout.Space(5);



                            break;
                        case CharacterTextDisplayMode.Custom:

                            break;
                    }
                } */


                    //  }

                    Separator3();
                    GUILayout.Space(10);

                    GUILayout.BeginHorizontal();

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent(!Theme.GameBridgeSkin.customStyles[7].richText ? "Markup Edit: <color=green>On</color>" : "Markup Edit: <color=#ff0033>Off</color>", ImageLibrary.editIcon, "Do a quick markup edit"), Theme.GameBridgeSkin.button, GUILayout.Width(120), GUILayout.Height(20)))
                    {
                        Theme.GameBridgeSkin.customStyles[7].richText = !Theme.GameBridgeSkin.customStyles[7].richText;
                    }

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button(new GUIContent("Open Editor", ImageLibrary.MarkupIcon, "Edit in Markup Editor"), Theme.GameBridgeSkin.button, GUILayout.Width(120), GUILayout.Height(20)))
                    {
                        var markupWindow = EditorWindow.GetWindow<MarkupEditor>();
                        markupWindow.sceneData = selectedDialoguer.sceneData;
                        markupWindow.TargetNodeData = dialogue;

                    }

                    GUILayout.FlexibleSpace();

                    GUILayout.EndHorizontal();


                    dialogue.LocalizedText[selectedDialoguer.sceneData.LanguageIndex] =
                        EditorGUILayout.TextArea(dialogue.LocalizedText[selectedDialoguer.sceneData.LanguageIndex], Theme.GameBridgeSkin.customStyles[7], GUILayout.Width(ScreenRect.width - 40), GUILayout.Height(100));
                    GUILayout.Space(5);
                        Separator();
                    Separator3();

                    if (selectedDialoguer.UsesVoiceOver)
                    {
                        GUILayout.Space(5);
                        GUILayout.BeginHorizontal();
                        var overrideUseDurationLengthForVoiceOverPowerIcon =
                            matchingReflectedData.DialogueSpecificData.OverrideUseVoiceover
                                ? ImageLibrary.PowerOnpro
                                : ImageLibrary.PowerOffRed;
                        if (GUILayout.Button(overrideUseDurationLengthForVoiceOverPowerIcon, GUIStyle.none,
                            GUILayout.Width(15), GUILayout.Height(15)))
                            matchingReflectedData.DialogueSpecificData.OverrideUseVoiceover =
                                !matchingReflectedData.DialogueSpecificData.OverrideUseVoiceover;
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("Override Voiceover");
                        GUILayout.EndHorizontal();
                        GUILayout.Space(5);

                        if (matchingReflectedData.DialogueSpecificData.OverrideUseVoiceover)
                            EditorGUILayout.HelpBox("The only override is to not use voice for this node in game",
                                MessageType.Warning);


                        EditorGUI.BeginDisabledGroup(matchingReflectedData.DialogueSpecificData.OverrideUseVoiceover);
                        GUILayout.Space(5);
                        dialogue.LocalizedVoiceRecordings[selectedDialoguer.sceneData.LanguageIndex] = (AudioClip)EditorGUILayout.ObjectField("Voice clip",
                            dialogue.LocalizedVoiceRecordings[selectedDialoguer.sceneData.LanguageIndex], typeof(AudioClip), false);
                        GUILayout.Space(5);
                        Separator();


                        /* GUILayout.Space(5);
                         GUILayout.BeginHorizontal();
                         GUILayout.Label("Use Duration Length For Voice Over");
                         GUILayout.FlexibleSpace();
                         var useDurationLengthForVoiceOverPowerIcon = dialogue.useDurationLengthForVoiceOver ? ImageLibrary.PowerOnpro : ImageLibrary.PowerOffpro;
                         if (GUILayout.Button(useDurationLengthForVoiceOverPowerIcon, GUIStyle.none, GUILayout.Width(15), GUILayout.Height(15)))
                         {
                             dialogue.useDurationLengthForVoiceOver = !dialogue.useDurationLengthForVoiceOver;
                             dialogue.useVoiceOverLength = !dialogue.useVoiceOverLength;
 
 
                         }
                         GUILayout.EndHorizontal();
                         GUILayout.Space(5);
                         Separator2();
 
 
                         GUILayout.Space(5);
                         GUILayout.BeginHorizontal();
                         GUILayout.Label("Use Voiceover Length");
                         GUILayout.FlexibleSpace();
                         var useVoiceOverLengthPowerIcon = dialogue.useVoiceOverLength ? ImageLibrary.PowerOnpro : ImageLibrary.PowerOffpro;
                         if (GUILayout.Button(useVoiceOverLengthPowerIcon, GUIStyle.none, GUILayout.Width(15), GUILayout.Height(15)))
                         {
                             dialogue.useVoiceOverLength = !dialogue.useVoiceOverLength;
                             dialogue.useDurationLengthForVoiceOver = !dialogue.useDurationLengthForVoiceOver;
 
                         }
                         GUILayout.EndHorizontal();
                         GUILayout.Space(5);
 
                         if (dialogue.useVoiceOverLength)
                         {
                             if (dialogue.VoicedDialogue != null)
                             {
                                 Separator2();
                                 GUILayout.Space(5);
                                 GUILayout.BeginHorizontal();
                                 GUILayout.Label("Voiceover Duration");
                                 GUILayout.FlexibleSpace();
                                 GUILayout.Label(dialogue.VoicedDialogue.length.ToString(), GUILayout.Width(100));
                                 GUILayout.EndHorizontal();
                                 GUILayout.Space(5);
                             }
                             else
                             {
                                 EditorGUILayout.HelpBox("This node data does not use any voice recordings", MessageType.Info);
                             }
                         }
 
                         Separator();*/

                        EditorGUI.EndDisabledGroup();
                    }


                    // GUILayout.Box(AssetPreview.GetAssetPreview(dialogue.VoicedDialogue),EditorStyles.inspectorDefaultMargins);

                    if (selectedDialoguer.UsesSoundffects)
                    {
                        GUILayout.Space(5);
                        GUILayout.BeginHorizontal();
                        var overrideUseDurationLengthForSoundEffectsPowerIcon =
                            matchingReflectedData.DialogueSpecificData.OverrideUseSoundEffect
                                ? ImageLibrary.PowerOnpro
                                : ImageLibrary.PowerOffRed;
                        if (GUILayout.Button(overrideUseDurationLengthForSoundEffectsPowerIcon, GUIStyle.none,
                            GUILayout.Width(15), GUILayout.Height(15)))
                            matchingReflectedData.DialogueSpecificData.OverrideUseSoundEffect =
                                !matchingReflectedData.DialogueSpecificData.OverrideUseSoundEffect;
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("Override Sound Effect");
                        GUILayout.EndHorizontal();
                        GUILayout.Space(5);

                        if (matchingReflectedData.DialogueSpecificData.OverrideUseSoundEffect)
                            EditorGUILayout.HelpBox(
                                "The only override is to not use sound effects for this node in game",
                                MessageType.Warning);

                        EditorGUI.BeginDisabledGroup(matchingReflectedData.DialogueSpecificData.OverrideUseSoundEffect);

                        GUILayout.Space(5);
                        dialogue.LocalizedSoundEffects[selectedDialoguer.sceneData.LanguageIndex] = (AudioClip)EditorGUILayout.ObjectField("Sound Effect",
                            dialogue.LocalizedSoundEffects[selectedDialoguer.sceneData.LanguageIndex], typeof(AudioClip), false);
                        GUILayout.Space(5);
                        Separator();

                        /* GUILayout.Space(5);
                         GUILayout.BeginHorizontal();
                         GUILayout.Label("Use Duration Length For Soundeffects");
                         GUILayout.FlexibleSpace();
                         var useDurationLengthForSoundEffectsPowerIcon = dialogue.useDurationLengthForSoundEffects ? ImageLibrary.PowerOnpro : ImageLibrary.PowerOffpro;
                         if (GUILayout.Button(useDurationLengthForSoundEffectsPowerIcon, GUIStyle.none, GUILayout.Width(15), GUILayout.Height(15)))
                         {
                             dialogue.useDurationLengthForSoundEffects = !dialogue.useDurationLengthForSoundEffects;
                             dialogue.useSoundEffectLength = !dialogue.useSoundEffectLength;
                         }
                         GUILayout.EndHorizontal();
                         GUILayout.Space(5);
                         Separator2();
 
                         GUILayout.Space(5);
                         GUILayout.BeginHorizontal();
                         GUILayout.Label("Use Sound Effect Length");
                         GUILayout.FlexibleSpace();
                         var useSoundEffectLengthPowerIcon = dialogue.useSoundEffectLength ? ImageLibrary.PowerOnpro : ImageLibrary.PowerOffpro;
                         if (GUILayout.Button(useSoundEffectLengthPowerIcon, GUIStyle.none, GUILayout.Width(15), GUILayout.Height(15)))
                         {
                             dialogue.useSoundEffectLength = !dialogue.useSoundEffectLength;
                             dialogue.useDurationLengthForSoundEffects = !dialogue.useDurationLengthForSoundEffects;
 
                         }
                         GUILayout.EndHorizontal();
                         GUILayout.Space(5);
 
                         if (dialogue.useSoundEffectLength)
                         {
                             if (dialogue.SoundEffect != null)
                             {
                                 Separator2();
                                 GUILayout.Space(5);
                                 GUILayout.BeginHorizontal();
                                 GUILayout.Label("Soundeffect Duration");
                                 GUILayout.FlexibleSpace();
                                 GUILayout.Label(dialogue.SoundEffect.length.ToString(), GUILayout.Width(100));
                                 GUILayout.EndHorizontal();
                                 GUILayout.Space(5);
                             }
                             else
                             {
                                 EditorGUILayout.HelpBox("This node data does not use any voice recordings", MessageType.Info);
                             }
                         }
                         Separator();
                         */
                        EditorGUI.EndDisabledGroup();
                    }

                    Separator3();

                    GUILayout.Space(5);
                    if (showHelpMessage)
                        EditorGUILayout.HelpBox("The Start time, Duration and Delay here are a reflection of the start time, duration and delay of the selected node", MessageType.Info);
                    EditorGUILayout.LabelField("Start Time", dialogue.StartTime.ToString());
                    EditorGUILayout.LabelField("Duration", dialogue.Duration.ToString());
                    EditorGUILayout.LabelField("Delay", dialogue.Delay.ToString());
                    EditorGUILayout.LabelField("Realtime Delay", dialogue.RealtimeDelay.ToString());
                    if (showHelpMessage)
                        EditorGUILayout.HelpBox("Realtime delay is set at runtime. it is recommended that you use Realtime dealy instead of Delay", MessageType.Info);
                    GUILayout.Space(5);
                    Separator();

                    // EditorGUILayout.LabelField("Realtime Delay", dialogue.RealtimeDelay.ToString());


                }

                if (matchingSelectedNodeData.type == typeof(RouteNodeData))
                {
                    var route = (RouteNodeData)matchingSelectedNodeData;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Alternate Route Titles (" + selectedDialoguer.sceneData.Languages[selectedDialoguer.sceneData.LanguageIndex].Name + ")");
                    GUILayout.FlexibleSpace();
                    var usesRouteTitlesIcon = matchingReflectedData.RouteSpecificDataset.UseAlternativeRouteTitles
                        ? ImageLibrary.PowerOnpro
                        : ImageLibrary.PowerOffpro;
                    if (GUILayout.Button(usesRouteTitlesIcon, GUIStyle.none, GUILayout.Width(15),
                        GUILayout.Height(15)))
                        matchingReflectedData.RouteSpecificDataset.UseAlternativeRouteTitles =
                            !matchingReflectedData.RouteSpecificDataset.UseAlternativeRouteTitles;
                    GUILayout.EndHorizontal();


                    GUILayout.Space(5);


                    if (matchingReflectedData.RouteSpecificDataset.UseAlternativeRouteTitles)
                    {


                        if (matchingReflectedData.RouteSpecificDataset.LanguageSpecificData.Count != selectedDialoguer.sceneData.Languages.Count)
                            matchingReflectedData.RouteSpecificDataset.LanguageSpecificData.Resize(selectedDialoguer.sceneData.Languages.Count);

                        if (matchingReflectedData.RouteSpecificDataset.LanguageSpecificData[selectedDialoguer.sceneData.LanguageIndex] == null)
                            matchingReflectedData.RouteSpecificDataset.LanguageSpecificData[selectedDialoguer.sceneData.LanguageIndex] = new ReflectedData.LanguageSpecificDataForRouteNodeData();

                        if (matchingReflectedData.RouteSpecificDataset.LanguageSpecificData[selectedDialoguer.sceneData.LanguageIndex].RouteTitles.Count !=
                            route.DataIconnectedTo.Count)
                            matchingReflectedData.RouteSpecificDataset.LanguageSpecificData[selectedDialoguer.sceneData.LanguageIndex].RouteTitles.Resize(
                                route.DataIconnectedTo.Count);

                        for (var i = 0; i < route.DataIconnectedTo.Count; i++)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(route.DataIconnectedTo[i].Name, GUILayout.Width(100));
                            GUILayout.FlexibleSpace();
                            matchingReflectedData.RouteSpecificDataset.LanguageSpecificData[selectedDialoguer.sceneData.LanguageIndex].RouteTitles[i] =
                                EditorGUILayout.DelayedTextField(
                                    matchingReflectedData.RouteSpecificDataset.LanguageSpecificData[selectedDialoguer.sceneData.LanguageIndex].RouteTitles[i],
                                    GUILayout.Height(15));
                            GUILayout.Space(2);
                            GUILayout.EndHorizontal();
                        }
                    }

                    GUILayout.Space(5);
                    Separator();


                  
                }

                if (matchingSelectedNodeData.type == typeof(LinkNodeData))
                {

                }
                    //   var link = (LinkNodeData)matchingSelectedNodeData;

                if (matchingSelectedNodeData.type == typeof(EndNodeData))
                    {

                    }
                    //   var end = (EndNodeData)matchingSelectedNodeData;

                 
                #region Draw Node Specific Conditions
               if (matchingSelectedNodeData.type != typeof(CharacterNodeData))
                {
                    Separator3();

                    GUILayout.Space(10);
                    ShowNodeSpecificConditionSettings.boolValue = EditorGUILayout.Foldout(ShowNodeSpecificConditionSettings.boolValue, "Node Spefic Conditions");
                    if (ShowNodeSpecificConditionSettings.boolValue)
                        DrawConditionCreator(selectedDialoguer);
                    GUILayout.Space(5);
                    Separator();
                }

                #endregion
            }


            #endregion

            GUILayout.Space(35);

        

            var helpMessageButtonArea = GUILayoutUtility.GetLastRect().ToLowerLeft(150, 20);
            showHelpMessage = GUI.Toggle(helpMessageButtonArea, showHelpMessage, "Show help messages");

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawConditionCreator(Dialoguer dialoguer)
        {
            if (matchingReflectedData == null) return;

            var condtionsCount = matchingReflectedData.Conditions.Count;
            if (ConditionSpecificSpaceing.Count != condtionsCount)
                ConditionSpecificSpaceing.Resize(condtionsCount);


            for (var c = 0; c < condtionsCount; c++)
            {
                var condition = matchingReflectedData.Conditions[c];

                var conditionSerializedObject = new SerializedObject(condition);
                conditionSerializedObject.Update();

                var area = EditorGUILayout.GetControlRect();
                // GUI.DrawTexture(area.AddRect(0,0,0,60), Textures.Gray);

                #region Backround UI

                var eventCount = matchingReflectedData.Conditions[c].targetEvent.GetPersistentEventCount();
                var countSpacing = eventCount < 2 ? 0 : (eventCount - 1) * 47;
                /* var conditionBodyArea = area.ToUpperLeft(0, areaHeight + countSpacing);*/
                var conditionBodyArea = area.ToUpperLeft(0, 100 + ConditionSpecificSpaceing[c] + countSpacing);
                GUI.Box(conditionBodyArea, "", Theme.GameBridgeSkin.customStyles[2]);
                GUI.DrawTexture(area.ToUpperLeft(0, 3, 0, 15), Textures.DuskLighter);
                var conditionBodyFooter = conditionBodyArea.PlaceUnder(0, 5);
                GUI.Box(conditionBodyFooter, "", Theme.GameBridgeSkin.customStyles[3]);
                var buttonArea = conditionBodyFooter.ToLowerRight(55, 14, -12, 14);
                GUI.Box(buttonArea, "", Theme.GameBridgeSkin.customStyles[4]);

                var addConditionButtonArea = buttonArea.ToCenterLeft(8, 8, 10);
                if (ClickEvent.Click(4, addConditionButtonArea, ImageLibrary.addConditionIcon))
                {
                    var newCondition = new GameObject(matchingSelectedNodeData.name + "Condition " +
                                                      matchingReflectedData.Conditions.Count);
                    newCondition.AddComponent<Condition>(); //.self = newCondition;
                    var _condition = newCondition.GetComponent<Condition>();
                    _condition.DialoguerGameObject = dialoguer.gameObject;
                    _condition.dialoguer = dialoguer;
                    _condition.Self = newCondition;
                    newCondition.transform.SetParent(matchingReflectedData.transform);
                    // newCondition.hideFlags = HideFlags.HideInHierarchy;
                    //   matchingReflectedData.Conditions.Add(newCondition.GetComponent<Condition>());
                    matchingReflectedData.Conditions.Insert(c + 1, newCondition.GetComponent<Condition>());

                    ConditionSpecificSpaceing.Add(0);
                }

                var deleteConditionButtonArea = buttonArea.ToCenterRight(8, 8, -10);
                if (c != 0)
                    if (ClickEvent.Click(4, deleteConditionButtonArea, ImageLibrary.deleteConditionIcon))
                    {
                        DestroyImmediate(matchingReflectedData.Conditions[c].gameObject);
                        matchingReflectedData.Conditions.RemoveAt(c);
                        return;
                    }

                #endregion

                var moveConditionUpButtonArea = area.ToCenterLeft(15, 8, 10);
                if (ClickEvent.Click(1, moveConditionUpButtonArea, ImageLibrary.upArrow, "Move this condtion up by one position"))
                {
                    if (c > 0)
                    {
                        var ConditionAtTop = matchingReflectedData.Conditions[c - 1];

                        matchingReflectedData.Conditions[c - 1] = null;
                        matchingReflectedData.Conditions[c] = null;

                        matchingReflectedData.Conditions[c - 1] = condition;
                        matchingReflectedData.Conditions[c] = ConditionAtTop;
                    }
                }


                var moveConditionDownButtonArea = moveConditionUpButtonArea.PlaceToRight(15, 0, 20);
                if (ClickEvent.Click(1, moveConditionDownButtonArea, ImageLibrary.downArrow, "Move this condition down by one position"))
                {
                    if (c != condtionsCount - 1)
                    {
                        var ConditionAtBottom = matchingReflectedData.Conditions[c + 1];

                        matchingReflectedData.Conditions[c + 1] = null;
                        matchingReflectedData.Conditions[c] = null;

                        matchingReflectedData.Conditions[c + 1] = condition;
                        matchingReflectedData.Conditions[c] = ConditionAtBottom;
                    }
                }

                var copyAllConditionDataButtonArea = moveConditionDownButtonArea.PlaceToRight(12, 12, 40, -2);
                if (ClickEvent.Click(1, copyAllConditionDataButtonArea, ImageLibrary.CopyIcon, "Copy  condition data"))
                {
                    ConditionCopy.MakeCopy(condition);
                }

                var copyOnlyEventsButtonArea = copyAllConditionDataButtonArea.PlaceToRight(12, 12, 20);
                if (ClickEvent.Click(1, copyOnlyEventsButtonArea, ImageLibrary.CopyIcon, "copy all event data only (Disabled)"))
                {
                    //  ConditionCopy.MakeCopy(condition, true);
                }


                var pasteDataButtonArea = copyOnlyEventsButtonArea.PlaceToRight(12, 12, 20);
                if (ClickEvent.Click(1, pasteDataButtonArea, ImageLibrary.PasteIcon, "Paste copied data"))
                {
                    Undo.RegisterCompleteObjectUndo(condition, "Condition");
                    ConditionCopy.Paste(condition);
                }

                if (ClickEvent.Click(1, area.ToUpperRight(15, 15, -10),
                    condition.Disabled ? ImageLibrary.PowerOffpro : ImageLibrary.PowerOnpro, "Disable/ Enable this condition"))
                    condition.Disabled = !condition.Disabled;

                EditorGUI.BeginDisabledGroup(condition.Disabled);

                var autoStartInfo = condition.AutoStart ? "Auto Start Is On" : "Auto Start Is Off";
                if (GUILayout.Button(autoStartInfo, GUILayout.Height(15)))
                    condition.AutoStart = !condition.AutoStart;

                /*  if (GUILayout.Button("get condtion methods", GUILayout.Height(15)))
                  {
                      condition.processMethods();
                  }*/

                // starting out 4 pixels must be added to the layout height
               // ConditionSpecificSpaceing[c] = 19;
                if (matchingSelectedNodeData.useTime)
                {


                    GUILayout.BeginHorizontal();
                    var useTimeInfo = condition.UseTime ? "Use Time Is On" : "Use Time Is Off";

                    if (GUILayout.Button(useTimeInfo, GUILayout.Height(15)))
                        condition.UseTime = !condition.UseTime;

                    GUILayout.FlexibleSpace();
                    EditorGUI.BeginDisabledGroup(!condition.UseTime);



                    condition.timeUseMethod = (TimeUseMethod)EditorGUILayout.EnumPopup(condition.timeUseMethod, GUILayout.Width(115), GUILayout.Height(15));



                    /*var titleofTimBeingUsed = condition.UseDelay ? "Using Delay" : "Using Duration";
                    if (GUILayout.Button(titleofTimBeingUsed, GUILayout.Width(100), GUILayout.Height(15)))
                    {
                        condition.UseDuration = !condition.UseDuration;
                        condition.UseDelay = !condition.UseDelay;
                    }
                    */


                    EditorGUI.EndDisabledGroup();
                    GUILayout.EndHorizontal();
                    // from here only 3 must be added to evey height value
                  //  ConditionSpecificSpaceing[c] = 172;

                    if (condition.timeUseMethod == TimeUseMethod.Custom)
                    {

                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Custom Time : ", GUILayout.Height(15));
                        condition.CustomWaitTime = EditorGUILayout.DelayedFloatField(condition.CustomWaitTime, GUILayout.Width(115), GUILayout.Height(15));
                        GUILayout.EndHorizontal();
                        // ConditionSpecificSpaceing[c] = 192;
                    }

                }

                #region

                // all the content inside here 2 added the y
                // all the content inside here 2 added the y
                GUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();
                GUILayout.Label("If", GUILayout.Height(15));
                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();


                //  condition.ComponentIndex = EditorGUILayout.Popup(condition.ComponentIndex,condition.Components, GUILayout.Height(15));


                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                condition.TargetGameObject = (GameObject)EditorGUILayout.ObjectField(condition.TargetGameObject,
                    typeof(GameObject), true, GUILayout.Height(15));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                if (condition.cachedTargetObject != condition.TargetGameObject)
                {
                    condition.Components = new Component[0];
                    condition.serializedMethods = new SerializableMethodInfo[0];

                    condition.SetComponent(0);
                    condition.SetMethod(0);
                    condition.cachedTargetObject = condition.TargetGameObject;
                }


                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Box(ImageLibrary.DownFlowArrow, EditorStyles.label, GUILayout.Width(15),
                    GUILayout.Height(15));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();


                var disabledComponents = condition.TargetGameObject == null;
                EditorGUI.BeginDisabledGroup(disabledComponents);

                if (disabledComponents &&
                    (condition.Components.Count() != 0 || condition.serializedMethods.Count() != 0))
                {
                    condition.Components = new Component[0];
                    condition.serializedMethods = new SerializableMethodInfo[0];
                }

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                var componentName = !condition.Components.Any()
                    ? "None"
                    : condition.Components[condition.ComponentIndex].GetType().ToString();


                if (GUILayout.Button(componentName, EditorStyles.popup, GUILayout.MinWidth(100), GUILayout.Height(15)))
                {
                    condition.GetGameObjectComponents();
                    var menu = new GenericMenu();
                    for (var i = 0; i < condition.Components.Length; i++)
                        menu.AddItem(new GUIContent(condition.Components[i].GetType().ToString()),
                            condition.ComponentIndex.Equals(i), condition.SetComponent, i);
                    menu.ShowAsContext();
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Box(ImageLibrary.DownFlowArrow, EditorStyles.label, GUILayout.Width(15),
                    GUILayout.Height(15));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                var methodName = !condition.serializedMethods.Any()
                    ? "None"
                    : condition.serializedMethods[condition.MethodIndex].methodName;
                // var disabledMethods = condition.TargetGameObject == null;


                if (GUILayout.Button(methodName, EditorStyles.popup, GUILayout.MinWidth(100), GUILayout.Height(15)))
                {
                    condition.GetComponentMethods();
                    var menu = new GenericMenu();
                    for (var i = 0; i < condition.serializedMethods.Length; i++)
                        menu.AddItem(new GUIContent(condition.serializedMethods[i].methodInfo.Name),
                            condition.MethodIndex.Equals(i), condition.SetMethod, i);
                    menu.ShowAsContext();
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Box(ImageLibrary.DownEqualSign, EditorStyles.label, GUILayout.Width(15),
                    GUILayout.Height(15));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                var buttonState = condition.ObjectiveBool ? "True" : "False";
                if (GUILayout.Button(buttonState, GUILayout.Height(15)))
                    condition.ObjectiveBool = !condition.ObjectiveBool;
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                EditorGUI.EndDisabledGroup();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Box(ImageLibrary.DownFlowArrow, EditorStyles.label, GUILayout.Width(15),
                    GUILayout.Height(15));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();


                if (matchingSelectedNodeData.useTime)
                {
                    ConditionSpecificSpaceing[c] = 203;
                    if (condition.timeUseMethod == TimeUseMethod.Custom)
                        ConditionSpecificSpaceing[c] = 221;
                }
                else
                    ConditionSpecificSpaceing[c] = 184;

                #endregion


                if (matchingSelectedNodeData.type != typeof(EnvironmentNodeData) &&
                    matchingSelectedNodeData.type != typeof(CharacterNodeData))
                {
                    EditorGUILayout.PropertyField(conditionSerializedObject.FindProperty("targetEvent"),
                        new GUIContent(condition.name));

                    conditionSerializedObject.ApplyModifiedProperties();
                }

                GUILayout.Space(40);

                EditorGUI.EndDisabledGroup();
            }

            // final spacing
            GUILayout.Space(5);
        }

        private void Separator()
        {
            var area = GUILayoutUtility.GetLastRect().AddRect(-15, 0);

            GUI.DrawTexture(area.ToLowerLeft(Screen.width, 1), Textures.DuskLightest);
        }

        private void Separator2()
        {
            var area = GUILayoutUtility.GetLastRect().AddRect(-15, 0);

            GUI.DrawTexture(area.ToLowerLeft(Screen.width, 1), Textures.DuskLight);
        }

        private void Separator3()
        {
            GUILayout.Space(20);
            var area = GUILayoutUtility.GetLastRect().AddRect(-15, 0);

            var repeatArea = area.ToLowerLeft(Screen.width, 20);
            GUI.DrawTextureWithTexCoords(repeatArea, ImageLibrary.RepeatableStipe, new Rect(0, 0, repeatArea.width / 20, 1));
        }

        #region variables

        [SerializeField] public string UID = "";

        [SerializeField] private NodeData matchingSelectedNodeData;

        private ReflectedData matchingReflectedData;
        private bool iconSet;
        private Rect ScreenRect = new Rect(0, 0, 0, 0);
        private Vector2 scrollView;
        private readonly List<int> ConditionSpecificSpaceing = new List<int>();

        private Story tempStoryData;

        private Dialoguer selectedDialoguer;

        private SerializedProperty DisplayedTextUI;

        private SerializedProperty DisplayedText;

        private SerializedProperty FilterCount;

        private SerializedProperty moveNextButton;

        private SerializedProperty movePreviousButton;


        private SerializedProperty NameUI;

        private SerializedProperty NameText;

        private SerializedProperty RouteButton;

        private SerializedProperty RouteParent;

        //   private SerializedProperty sceneData;

        private SerializedProperty ShowGeneralSettings;

        private SerializedProperty ShowKeywordFilterFouldout;

        private SerializedProperty textDisplayMode;

        private SerializedProperty TypingAudioCip;

        private SerializedProperty TypingSpeed;

        private SerializedProperty useDialogueTextUI;

        private SerializedProperty UseKeywordFilters;

        private SerializedProperty useMoveNextButton;

        private SerializedProperty useMovePreviousButton;

        private SerializedProperty useNameUI;

        private SerializedProperty useRouteButton;

        private SerializedProperty UsesSoundffects;

        //  private SerializedProperty UsesStoryboardImages;

        private SerializedProperty UsesText;

        private SerializedProperty UsesVoiceOver;

        private SerializedProperty useAutoStartVoiceClip;

        private SerializedProperty useAutoStartSoundEffectClip;

        private SerializedProperty AutoStartAllConditionsByDefault;

        private SerializedProperty ShowNodeSpecificConditionSettings;

        private SerializedProperty VolumeVariation;

    private SerializedProperty UpdateUID;
        //  private SerializedProperty NameColour;

        //   private SerializedProperty TextColour;

        bool showHelpMessage = false;

        private EditorWindow[] edwin;

        #endregion
    }
}