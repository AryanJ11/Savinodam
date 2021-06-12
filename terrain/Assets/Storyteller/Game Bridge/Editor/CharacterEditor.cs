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
    [CustomEditor(typeof(Character))]
    [CanEditMultipleObjects]
    public class CharacterEditor : Editor
    {
        public void OnEnable()
        {
            selectedCharacter = (Character)target;

            #region set the icon

            if (!iconSet)
            {
                IconManager.SetIcon(selectedCharacter, IconManager.DaiMangouIcons.CharacterIcon);
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

            targetChararacterIndex = serializedObject.FindProperty("targetChararacterIndex");

            MatchingRouteNumber = serializedObject.FindProperty("MatchingRouteNumber");

            UsesText = serializedObject.FindProperty("UsesText");

            //  UsesStoryboardImages = serializedObject.FindProperty("UsesStoryboardImages");

            UsesVoiceOver = serializedObject.FindProperty("UsesVoiceOver");

            UsesSoundffects = serializedObject.FindProperty("UsesSoundffects");

            UseKeywordFilters = serializedObject.FindProperty("UseKeywordFilters");

            FilterCount = serializedObject.FindProperty("FilterCount");

            ShowKeywordFilterFouldout = serializedObject.FindProperty("ShowKeywordFilterFouldout");

            ShowGeneralSettings = serializedObject.FindProperty("ShowGeneralSettings");

            textDisplayMode = serializedObject.FindProperty("textDisplayMode");

            LastSelectedUID = serializedObject.FindProperty("LastSelectedUID");

            useNameUI = serializedObject.FindProperty("UseNameUI");

            useDialogueTextUI = serializedObject.FindProperty("UseDialogueTextUI");

            useMoveNextButton = serializedObject.FindProperty("UseMoveNextButton");

            useMovePreviousButton = serializedObject.FindProperty("UseMovePreviousButton");

            useRouteButton = serializedObject.FindProperty("UseRouteButton");

            useAutoStartVoiceClip = serializedObject.FindProperty("AutoStartVoiceClip");

            useAutoStartSoundEffectClip = serializedObject.FindProperty("AutoStartSoundEffectClip");

            NameColour = serializedObject.FindProperty("NameColour");

            TextColour = serializedObject.FindProperty("TextColour");

            AutoStartAllConditionsByDefault = serializedObject.FindProperty("AutoStartAllConditionsByDefault");

            ShowNodeSpecificConditionSettings = serializedObject.FindProperty("ShowNodeSpecificConditionSettings");

            UpdateUID = serializedObject.FindProperty("UpdateUID");

            if (selectedCharacter.LastSelectedUID != "")
            {
                MakeSelectionSwitch = true;

                if (selectedCharacter.self == null)
                    selectedCharacter.self =
                        selectedCharacter.sceneData.Characters[selectedCharacter.targetChararacterIndex];
            }
            else
            {
                // lets give you the character s the selection if you have not yet selected any character
                // an error wil probably be thrown if you delete your character  and attempt to select it by selecting the matching gameobject
                if (selectedCharacter.self != null)
                    CurrentStory.Select(selectedCharacter.self.UID);
            }
        }
        

        public override bool RequiresConstantRepaint()
        {
            return true;
        }

        public override void OnInspectorGUI()
        {
            //  DrawDefaultInspector();

            if (edwin.Length == 0)
            {
                Repaint();
                edwin = Resources.FindObjectsOfTypeAll(typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow")) as EditorWindow[];
            }

            ScreenRect.size = new Vector2(edwin[0].position.width, edwin[0].position.height);

            if (MakeSelectionSwitch)
            {
                if (CurrentStory.ActiveStory != null)
                    if (selectedCharacter.sceneData.FullCharacterDialogueSet.Find(n =>
                        n.UID == CurrentStory.ActiveStory.Scenes[CurrentStory.ActiveSceneIndex].NodeElements.Last().UID))
                    {
                        var theCharacterUID = CurrentStory.ActiveStory.Scenes[CurrentStory.ActiveSceneIndex].NodeElements
                            .Last().CallingNode.UID;


                        if (theCharacterUID == selectedCharacter.self.UID)
                            if (CurrentStory.ActiveStory.Scenes[CurrentStory.ActiveSceneIndex].NodeElements.Last().UID !=
                                LastSelectedUID.stringValue)
                                selectedCharacter.LastSelectedUID = LastSelectedUID.stringValue =
                                    CurrentStory.ActiveStory.Scenes[CurrentStory.ActiveSceneIndex].NodeElements.Last().UID;
                    }


                CurrentStory.Select(selectedCharacter.LastSelectedUID);
                MakeSelectionSwitch = false;
            }


            serializedObject.Update();

            #region IMGUI

            #region Character cover image

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Box(ImageLibrary.GBCharacterImage, EditorStyles.inspectorDefaultMargins);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            #endregion



            #region tell users to assign a SceneData Asset

            if (selectedCharacter.sceneData == null)
                EditorGUILayout.HelpBox("Please assign a SceneData Asset in the area below", MessageType.Info);

            selectedCharacter.sceneData =
                (SceneData)EditorGUILayout.ObjectField(selectedCharacter.sceneData, typeof(SceneData), false);

            #endregion


            // if there is still no scene data , than return
            if (selectedCharacter.sceneData == null) return;


            if (showHelpMessage)
            {
                EditorGUILayout.HelpBox(
    "Select a character from this list, this gameObject will become the selected character",
    MessageType.Info);

                EditorGUILayout.HelpBox(
                    "Clicking 'Setup' will update this character with all the necessary data from the scene in which this character exists",
                    MessageType.Info);
            }

            #region character selection and setup
         //   GUILayout.BeginHorizontal();
            #region creae the popup list of characters to choose from

            if (characternames.Count == 0)
                foreach (var character in selectedCharacter.sceneData.Characters)
                    characternames.Add(character.CharacterName);


            selectedCharacter.targetChararacterIndex = targetChararacterIndex.intValue =
                EditorGUILayout.Popup(selectedCharacter.targetChararacterIndex, characternames.ToArray());

            #endregion


            #region if we dont have any node selected then we cant apply modied properties. so we check if it i null here and apply the modified property for the character selection

            /*if (matchingSelectedNodeData == null)
            {
                serializedObject.ApplyModifiedProperties(); 
            }*/

            #endregion

            selectedCharacter.self = selectedCharacter.sceneData.Characters[selectedCharacter.targetChararacterIndex];

            #region trigger setting up the character data

            if (GUILayout.Button("Setup"))
            {
                var targetCharacter = selectedCharacter.sceneData.Characters[selectedCharacter.targetChararacterIndex];

                var sortedList = targetCharacter.NodeDataInMyChain.ToList();


                #region rename the selectedgameObject with Character  script on it

                selectedCharacter.gameObject.name = targetCharacter.CharacterName;

                #endregion


                #region setup General Reflected Data

                #region Generate and setup GeneralReflectedData Parent

                if (selectedCharacter.GeneralReflectedData == null)
                {
                    selectedCharacter.GeneralReflectedDataParent = new GameObject("General Reflected Data");
                    selectedCharacter.GeneralReflectedDataParent.transform.SetParent(selectedCharacter.transform);
                    selectedCharacter.GeneralReflectedDataParent.transform.localPosition = Vector3.zero;
                    selectedCharacter.GeneralReflectedDataParent.hideFlags = HideFlags.HideInHierarchy;
                }
                else
                {
                    selectedCharacter.TempGeneralReflectedDataSet = selectedCharacter.GeneralReflectedData;
                }

                #endregion

                #region create a new instance of ReflectedData as a gameObject and then assign the GeneralReflectedData value at i to the reflected data ID

                var newGeneralReflectedDatagameObject = new GameObject(targetCharacter.name + "General Reflected");
                newGeneralReflectedDatagameObject.transform.SetParent(selectedCharacter.GeneralReflectedDataParent
                    .transform);
                newGeneralReflectedDatagameObject.AddComponent<ReflectedData>();
                var theGeneralReflectedDataComponent = newGeneralReflectedDatagameObject.GetComponent<ReflectedData>();
                theGeneralReflectedDataComponent.CharacterGameObject = selectedCharacter.gameObject;
                theGeneralReflectedDataComponent.character = selectedCharacter;
                theGeneralReflectedDataComponent.self = newGeneralReflectedDatagameObject;
                // we already resized the ReflectedDataSet list to be the same size as the SortedList so we dont use .Add
                selectedCharacter.GeneralReflectedData = theGeneralReflectedDataComponent;
                // it is VERY important that the UIDs match.
                /// selectedCharacter.GeneralReflectedData.General = true;
                selectedCharacter.GeneralReflectedData.UID = targetCharacter.UID;

                #endregion


                #region Add the first general condition

                var newGeneralCondition = new GameObject(newGeneralReflectedDatagameObject.name + "Condition " +
                                                         theGeneralReflectedDataComponent.Conditions.Count);
                newGeneralCondition.AddComponent<Condition>();
                var _generalCondition = newGeneralCondition.GetComponent<Condition>();
                _generalCondition.CharacterGameObject = selectedCharacter.gameObject;
                _generalCondition.character = selectedCharacter;
                _generalCondition.Self = newGeneralCondition;
                newGeneralCondition.transform.SetParent(newGeneralReflectedDatagameObject.transform);
                // newCondition.hideFlags = HideFlags.HideInHierarchy;
                theGeneralReflectedDataComponent.Conditions.Add(newGeneralCondition.GetComponent<Condition>());

                #endregion

                if (selectedCharacter.TempGeneralReflectedDataSet != null)
                {
                    //selectedCharacter.TempGeneralReflectedDataSet = null;
                    // DestroyImmediate(theGeneralReflectedDataComponent, true);

                    var data = selectedCharacter.GeneralReflectedData;
                    if (selectedCharacter.TempGeneralReflectedDataSet.UID == data.UID)
                    {
                        data.CharacterGameObject = selectedCharacter.TempGeneralReflectedDataSet.CharacterGameObject;
                        data.character = selectedCharacter.TempGeneralReflectedDataSet.character;
                        data.characterComponent = selectedCharacter.TempGeneralReflectedDataSet.characterComponent;


                        for (var c = 0; c < data.Conditions.Count; c++)
                        {
                            var conditionToDelete = data.Conditions[c];
                            DestroyImmediate(conditionToDelete.Self);
                            data.Conditions.RemoveAt(c);
                        }

                        // finally we move the condition from TempReflectedDataSet[i] conditions to the datas condition list
                        foreach (var condition in selectedCharacter.TempGeneralReflectedDataSet.Conditions)
                        {
                            condition.Self.transform.SetParent(data.self.transform);
                            data.Conditions.Add(condition);
                        }
                    }

                    DestroyImmediate(selectedCharacter.TempGeneralReflectedDataSet.self);
                    selectedCharacter.TempGeneralReflectedDataSet = null;
                }

                #endregion

                #region generate and setup reflected data parent

                // now we update or crreate the Reflected data gameobject if necessary


                if (selectedCharacter.ReflectedDataSet.Count == 0)
                {
                    selectedCharacter.ReflectedDataSet.Resize(sortedList.Count);


                    selectedCharacter.ReflectedDataParent = new GameObject("Reflected Data");
                    selectedCharacter.ReflectedDataParent.transform.SetParent(selectedCharacter.transform);
                    selectedCharacter.ReflectedDataParent.transform.localPosition = Vector3.zero;
                    selectedCharacter.ReflectedDataParent.hideFlags = HideFlags.HideInHierarchy;


                    var AudioManager = new GameObject("Audio Manager");
                    AudioManager.transform.SetParent(selectedCharacter.transform);
                    AudioManager.transform.localPosition = Vector3.zero;

                    var TypingAudioManager = new GameObject("Typing");
                    TypingAudioManager.transform.SetParent(AudioManager.transform);
                    TypingAudioManager.transform.localPosition = Vector3.zero;
                    TypingAudioManager.AddComponent<AudioSource>();
                    selectedCharacter.TypingAudioSource = TypingAudioManager.GetComponent<AudioSource>();

                    var VoiceAudioManager = new GameObject("Voice");
                    VoiceAudioManager.transform.SetParent(AudioManager.transform);
                    VoiceAudioManager.transform.localPosition = Vector3.zero;
                    VoiceAudioManager.AddComponent<AudioSource>();
                    selectedCharacter.VoiceAudioSource = VoiceAudioManager.GetComponent<AudioSource>();

                    var SoundEffectsAudioManager = new GameObject("Sound Effects");
                    SoundEffectsAudioManager.transform.SetParent(AudioManager.transform);
                    SoundEffectsAudioManager.transform.localPosition = Vector3.zero;
                    SoundEffectsAudioManager.AddComponent<AudioSource>();
                    selectedCharacter.SoundEffectAudioSource = SoundEffectsAudioManager.GetComponent<AudioSource>();
                }
                else
                {
                    // we cache the current set of reflected data in a temporary list and then empty and resize the reflecteddataset list
                    selectedCharacter.TempReflectedDataSet = new List<ReflectedData>();
                    foreach (var capturedData in selectedCharacter.ReflectedDataSet)
                        selectedCharacter.TempReflectedDataSet.Add(capturedData);

                    selectedCharacter.ReflectedDataSet = new List<ReflectedData>();
                    selectedCharacter.ReflectedDataSet.Resize(sortedList.Count);
                }

                #endregion

                #region Setup Reflected Data For all individual nodes

                for (var i = 0; i < sortedList.Count; i++)
                {
                    #region create a new instance of ReflectedData as a gameObject and then assign the sortedList value at i to the reflected data ID

                    var newReflectedDatagameObject = new GameObject(sortedList[i].Name + "Reflected");
                    newReflectedDatagameObject.transform.SetParent(selectedCharacter.ReflectedDataParent.transform);
                    newReflectedDatagameObject.AddComponent<ReflectedData>();
                    var theReflectedDataComponent = newReflectedDatagameObject.GetComponent<ReflectedData>();
                    theReflectedDataComponent.CharacterGameObject = selectedCharacter.gameObject;
                    theReflectedDataComponent.character = selectedCharacter;
                    theReflectedDataComponent.self = newReflectedDatagameObject;
                    // we already resized the ReflectedDataSet list to be the same size as the SortedList so we dont use .Add
                    selectedCharacter.ReflectedDataSet[i] = theReflectedDataComponent;
                    // it is VERY important that the UIDs match.
                    selectedCharacter.ReflectedDataSet[i].UID = sortedList[i].UID;

                    #endregion

                    #region Add the first condition

                    var newCondition = new GameObject(newReflectedDatagameObject.name + "Condition " +
                                                      theReflectedDataComponent.Conditions.Count);
                    newCondition.AddComponent<Condition>();
                    var _condition = newCondition.GetComponent<Condition>();
                    _condition.CharacterGameObject = selectedCharacter.gameObject;
                    _condition.character = selectedCharacter;
                    _condition.Self = newCondition;
                    newCondition.transform.SetParent(newReflectedDatagameObject.transform);
                    // newCondition.hideFlags = HideFlags.HideInHierarchy;
                    theReflectedDataComponent.Conditions.Add(newCondition.GetComponent<Condition>());

                    #endregion

                    #region here we begin checking to see if any UID values we have for reflected data are in the temp reflected data. if so , we destroy their conditions and replace the m with the conditions in the TempReflectedDataSet

                    if (selectedCharacter.TempReflectedDataSet.Count != 0)
                        foreach (var tempData in selectedCharacter.TempReflectedDataSet)
                        {
                            //we can use ReflectedDataSet[i] because the sorted list count and ReflectedDataSet ount are the same
                            var data = selectedCharacter.ReflectedDataSet[i];

                            if (sortedList[i].UID == tempData.UID)
                                if (tempData.UID == data.UID)
                                {
                                    data.CharacterGameObject = tempData.CharacterGameObject;
                                    data.character = tempData.character;
                                    data.characterComponent = tempData.characterComponent;
                                    data.RouteSpecificDataset = tempData.RouteSpecificDataset;
                                    data.ActionSpecificData = tempData.ActionSpecificData;
                                    data.DialogueSpecificData = tempData.DialogueSpecificData;
                                    data.LinkSpecificData = tempData.LinkSpecificData;
                                    data.EndSpecificData = tempData.EndSpecificData;
                                    data.EnvironmentSpecificData = tempData.EnvironmentSpecificData;


                                    foreach (var dataset in data.character.sceneData
                                        .Characters[data.character.targetChararacterIndex].NodeDataInMyChain)
                                        dataset.IsPlayer = tempData.character.sceneData
                                            .Characters[data.character.targetChararacterIndex].IsPlayer;


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
                 //   UpdateUID.stringValue = selectedCharacter.sceneData.UpdateUID;

                    #endregion
                }

                // now destroy all the data in TempReflectedDataSet
                foreach (var item in selectedCharacter.TempReflectedDataSet) DestroyImmediate(item.self);
                selectedCharacter.TempReflectedDataSet.RemoveAll(n => n == null);


                #endregion

                serializedObject.ApplyModifiedProperties();
                return;
            }

          /*  if (!UpdateUID.stringValue.Equals(selectedCharacter.sceneData.UpdateUID))
            {
                EditorGUILayout.HelpBox("Your scene data has been updated. Click the setup button to continue", MessageType.Info);

                return;
            }*/

            selectedCharacter.sceneData.LanguageIndex = EditorGUILayout.Popup(selectedCharacter.sceneData.LanguageIndex, selectedCharacter.sceneData.LanguageNameArray);

            GUILayout.Space(10);

            #endregion
            //GUILayout.EndHorizontal();
            #endregion
            #region route number definition

            if (showHelpMessage)
                EditorGUILayout.HelpBox(
                    "Route numbers are the actual route on which a characters interaction begins on (see tutorial)",
                    MessageType.Info);
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Route Number", GUILayout.Width(100));
            GUILayout.FlexibleSpace();
            MatchingRouteNumber.intValue =
                EditorGUILayout.DelayedIntField(selectedCharacter.MatchingRouteNumber, GUILayout.Width(100));
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            #endregion

            /*  if (selectedCharacter.LastSelectedUID == "")
              {
                  EditorGUILayout.HelpBox("Select any " + selectedCharacter.name + " in the Storyteller Canvas to begin editing its data in this window", MessageType.Warning);
              }*/

            #region TEMP

            /*   if (GUILayout.Button("Show / Hide Hidden Data"))
              {
                  if (selectedCharacter.ReflectedDataParent != null)
                      if (selectedCharacter.ReflectedDataParent.hideFlags == HideFlags.None)
                          selectedCharacter.ReflectedDataParent.hideFlags = HideFlags.HideInHierarchy;
                      else
                          selectedCharacter.ReflectedDataParent.hideFlags = HideFlags.None;

                  if (selectedCharacter.GeneralReflectedDataParent != null)
                      if (selectedCharacter.GeneralReflectedDataParent.hideFlags == HideFlags.None)
                          selectedCharacter.GeneralReflectedDataParent.hideFlags = HideFlags.HideInHierarchy;
                      else
                          selectedCharacter.GeneralReflectedDataParent.hideFlags = HideFlags.None;
              }

             if (GUILayout.Button("Unhide all"))
                  foreach (Transform child in selectedCharacter.transform)
                      child.hideFlags = HideFlags.None;
              */

            #endregion
            if (selectedCharacter.ReflectedDataSet.Count == 0)
                return;

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


            #region General Settings

            ShowGeneralSettings.boolValue = EditorGUILayout.Foldout(selectedCharacter.ShowGeneralSettings,
                "General Settings (for all nodes)");

            if (selectedCharacter.ShowGeneralSettings)
            {
                if (!selectedCharacter.self.IsPlayer)
                {
                    EditorGUILayout.HelpBox(
                        "Only the Player can control these general settings however you can set specific Name and Dialogue text colours here for your non-player characters",
                        MessageType.Info);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Name Colour", GUILayout.Width(100));
                    GUILayout.FlexibleSpace();
                    NameColour.colorValue = EditorGUILayout.ColorField(NameColour.colorValue,GUILayout.Width(100));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Text Colour", GUILayout.Width(100));
                    GUILayout.FlexibleSpace();
                    TextColour.colorValue = EditorGUILayout.ColorField(TextColour.colorValue, GUILayout.Width(100));
                    GUILayout.EndHorizontal();
                }

                EditorGUI.BeginDisabledGroup(!selectedCharacter.self.IsPlayer);

                if (showHelpMessage)
                    EditorGUILayout.HelpBox(
                        "By turning on Use Text you are choosing to use a UI system to display dialogue",
                        MessageType.Info);

                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Use Text");
                GUILayout.FlexibleSpace();
                var usesTextPowerIcon = selectedCharacter.UsesText ? ImageLibrary.PowerOnpro : ImageLibrary.PowerOffpro;
                if (GUILayout.Button(usesTextPowerIcon, GUIStyle.none, GUILayout.Width(15), GUILayout.Height(15)))
                    UsesText.boolValue = !selectedCharacter.UsesText;
                GUILayout.EndHorizontal();
                GUILayout.Space(5);

                if (selectedCharacter.UsesText)
                {
                    selectedCharacter.ShowUIAndTextSettings =
                        EditorGUILayout.Foldout(selectedCharacter.ShowUIAndTextSettings, "UI Settings");

                    if (selectedCharacter.ShowUIAndTextSettings)
                    {
                        #region settings for Name UI

                        /*  GUILayout.Space(120);
                          var area = GUILayoutUtility.GetLastRect().AddRect(-15, 0);
                          var colorWheelsArea = area.ToLowerLeft(ScreenRect.width, 100);
                          var nameColorWheelArea = colorWheelsArea.ToCenterLeft(100, 100,10);                       
                          colourPickerWheel.DrawColorWheel(nameColorWheelArea, NameColour.colorValue);
                          colourPickerWheel.DrawColorWheel(nameColorWheelArea.PlaceToRight(0,0,40), NameColour.colorValue);

                          GUILayout.Space(20);*/

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
                        NameText.objectReferenceValue = (TextMeshProUGUI)EditorGUILayout.ObjectField(selectedCharacter.NameText,
                            typeof(TextMeshProUGUI), true, GUILayout.Height(15), GUILayout.Width(100));
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Name Colour", GUILayout.Width(100));
                        GUILayout.FlexibleSpace();
                        NameColour.colorValue = EditorGUILayout.ColorField(NameColour.colorValue, GUILayout.Width(100));
                        GUILayout.EndHorizontal();

                        EditorGUI.EndDisabledGroup();
                        GUILayout.Space(5);
                        Separator2();

                        #endregion


                        #region settings for Dialogue UI

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
                        DisplayedText.objectReferenceValue = (TextMeshProUGUI)EditorGUILayout.ObjectField(
                            selectedCharacter.DisplayedText, typeof(TextMeshProUGUI), true, GUILayout.Height(15),
                            GUILayout.Width(100));
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Text Colour", GUILayout.Width(100));
                        GUILayout.FlexibleSpace();
                        TextColour.colorValue = EditorGUILayout.ColorField(TextColour.colorValue, GUILayout.Width(100));
                        GUILayout.EndHorizontal();
                        EditorGUI.EndDisabledGroup();
                        GUILayout.Space(5);
                        Separator2();

                        #endregion

                        #region settings for Next Button UI

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
                            selectedCharacter.MoveNextButton, typeof(Button), true, GUILayout.Height(15),
                            GUILayout.Width(100));
                        GUILayout.EndHorizontal();
                        EditorGUI.EndDisabledGroup();
                        GUILayout.Space(5);
                        Separator2();

                        #endregion

                        #region settings for Previous Button UI

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
                            selectedCharacter.MovePreviousButton, typeof(Button), true, GUILayout.Height(15),
                            GUILayout.Width(100));
                        GUILayout.EndHorizontal();
                        EditorGUI.EndDisabledGroup();
                        GUILayout.Space(5);
                        Separator2();

                        #endregion

                        #region settings for Route  UI

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
                            selectedCharacter.RouteParent, typeof(GameObject), true, GUILayout.Height(15),
                            GUILayout.Width(100));
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Route Button");
                        GUILayout.FlexibleSpace();
                        RouteButton.objectReferenceValue = (Button)EditorGUILayout.ObjectField(
                            selectedCharacter.RouteButton, typeof(Button), true, GUILayout.Height(15),
                            GUILayout.Width(100));
                        GUILayout.EndHorizontal();
                        EditorGUI.EndDisabledGroup();
                        GUILayout.Space(5);
                        Separator2();

                        #endregion
                    }


                    if (showHelpMessage)
                        EditorGUILayout.HelpBox(
                            "In text display settings you have the option of usinf typed text or immediately displayed text",
                            MessageType.Info);

                    selectedCharacter.ShowTextDisplayModeSettings =
                        EditorGUILayout.Foldout(selectedCharacter.ShowTextDisplayModeSettings, "Text Display Settings");

                    if (selectedCharacter.ShowTextDisplayModeSettings)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Text Display Mode");
                        textDisplayMode.enumValueIndex =
                            (int)(CharacterTextDisplayMode)EditorGUILayout.EnumPopup(
                                selectedCharacter.textDisplayMode, GUILayout.Height(15), GUILayout.Width(100));
                        GUILayout.EndHorizontal();

                        switch (selectedCharacter.textDisplayMode)
                        {
                            case CharacterTextDisplayMode.Instant:

                                break;
                            case CharacterTextDisplayMode.Typed:
                                GUILayout.Space(5);

                                GUILayout.BeginHorizontal();
                                GUILayout.Label("Typing Speed");
                                GUILayout.FlexibleSpace();
                                TypingSpeed.floatValue = EditorGUILayout.FloatField(selectedCharacter.TypingSpeed,
                                    GUILayout.Height(15), GUILayout.Width(100));
                                GUILayout.EndHorizontal();


                                GUILayout.BeginHorizontal();
                                GUILayout.Label("Typing Sound");
                                GUILayout.FlexibleSpace();
                                TypingAudioCip.objectReferenceValue =
                                    (AudioClip)EditorGUILayout.ObjectField(selectedCharacter.TypingAudioCip,
                                        typeof(AudioClip), false, GUILayout.Height(15), GUILayout.Width(100));
                                GUILayout.EndHorizontal();

                                GUILayout.Space(5);


                                break;
                                /*  case CharacterTextDisplayMode.Custom:

                                      break;*/
                        }
                    }

                    GUILayout.Space(10);
                    Separator();


                    if (showHelpMessage)
                        EditorGUILayout.HelpBox(
                            "With Keyword filters turned on, this Player character can replace any word in dialogue with another word." +
                            " There are four variations of text replacement settings. Replacing static text with dynamic text, dynamic text with static text, ststic text with ststic text and dynamic text with dynamic text",
                            MessageType.Info);

                    var keywordIdentifierCount = selectedCharacter.KeywordFilters.Count;

                    GUILayout.Space(5);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Use Keyword Filters");
                    GUILayout.FlexibleSpace();
                    var usesKeywordFiltersPowerIcon = selectedCharacter.UseKeywordFilters
                        ? ImageLibrary.PowerOnpro
                        : ImageLibrary.PowerOffpro;
                    if (GUILayout.Button(usesKeywordFiltersPowerIcon, GUIStyle.none, GUILayout.Width(15),
                        GUILayout.Height(15)))
                        UseKeywordFilters.boolValue = !selectedCharacter.UseKeywordFilters;
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5);


                    if (selectedCharacter.UseKeywordFilters)
                    {
                        if (showHelpMessage)
                            EditorGUILayout.HelpBox("Keyword filters change marked words into other words",
                                MessageType.Info);

                        ShowKeywordFilterFouldout.boolValue =
                            EditorGUILayout.Foldout(selectedCharacter.ShowKeywordFilterFouldout,
                                "Show Key Word Filters");

                        if (selectedCharacter.ShowKeywordFilterFouldout)
                        {
                            GUILayout.Space(5);
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Amount Of Filters", GUILayout.Width(100));
                            GUILayout.FlexibleSpace();
                            FilterCount.intValue =
                                EditorGUILayout.DelayedIntField(selectedCharacter.FilterCount, GUILayout.Width(100));

                            if (keywordIdentifierCount != selectedCharacter.FilterCount)
                            {
                                selectedCharacter.KeywordFilters.Resize(selectedCharacter.FilterCount);

                            }

                            GUILayout.Space(15);
                            GUILayout.EndHorizontal();
                            GUILayout.Space(5);


                            for (var i = 0; i < selectedCharacter.KeywordFilters.Count; i++)
                            {
                                if (selectedCharacter.KeywordFilters[i] == null)
                                    selectedCharacter.KeywordFilters[i] = new KeywordFilter();

                                var area = GUILayoutUtility.GetLastRect().AddRect(-15, 0, 0, 10);
                                var filterAreHeight =
                                    selectedCharacter.KeywordFilters[i].StaticKeywordMethod &&
                                    selectedCharacter.KeywordFilters[i].StaticReplacementStringMethod
                                        ? 105
                                        : 170;
                                var filterArea = area.PlaceUnder(Screen.width, filterAreHeight);
                                GUI.DrawTexture(filterArea, Textures.DuskLightest);
                                var headerArea = filterArea.ToUpperLeft(0, 3, 0, 15);
                                GUI.DrawTexture(headerArea, Textures.DuskLighter);

                                #region Header Content



                                var moveKeywordFilterUpButtonArea = headerArea.ToCenterLeft(15, 8, 25, -10);
                                if (ClickEvent.Click(1, moveKeywordFilterUpButtonArea, ImageLibrary.upArrow,
                                    "Move this Keyword Filter up by one position"))
                                {
                                    if (i > 0)
                                    {
                                        var KeywordFilterAtTop = selectedCharacter.KeywordFilters[i - 1];

                                        selectedCharacter.KeywordFilters[i - 1] = null;
                                        // selectedCharacter.KeywordFilters[i] = null;

                                        selectedCharacter.KeywordFilters[i - 1] = selectedCharacter.KeywordFilters[i];
                                        selectedCharacter.KeywordFilters[i] = KeywordFilterAtTop;
                                    }
                                }


                                var moveKeywordFilterDownButtonArea =
                                    moveKeywordFilterUpButtonArea.PlaceToRight(15, 0, 20);
                                if (ClickEvent.Click(1, moveKeywordFilterDownButtonArea, ImageLibrary.downArrow,
                                    "Move this keyword Filter down by one position"))
                                {
                                    if (i != selectedCharacter.KeywordFilters.Count - 1)
                                    {
                                        var KeywordFilterAtBottom = selectedCharacter.KeywordFilters[i + 1];

                                        selectedCharacter.KeywordFilters[i + 1] = null;
                                        //selectedCharacter.KeywordFilters[i] = null;

                                        selectedCharacter.KeywordFilters[i + 1] = selectedCharacter.KeywordFilters[i];
                                        selectedCharacter.KeywordFilters[i] = KeywordFilterAtBottom;
                                    }
                                }


                                if (ClickEvent.Click(1, headerArea.ToCenterRight(15, 15, -25, -8),
                                    selectedCharacter.KeywordFilters[i].Disabled
                                        ? ImageLibrary.PowerOffpro
                                        : ImageLibrary.PowerOnpro, "Enable / Disable this Keyword Filter"))
                                    selectedCharacter.KeywordFilters[i].Disabled =
                                        !selectedCharacter.KeywordFilters[i].Disabled;

                                #endregion

                                EditorGUI.BeginDisabledGroup(selectedCharacter.KeywordFilters[i].Disabled);

                                GUILayout.Space(25);
                                GUILayout.BeginHorizontal();
                                GUILayout.BeginVertical();
                                GUILayout.Label("Key Word", GUILayout.Width(100));
                                selectedCharacter.KeywordFilters[i].StaticKeywordMethod =
                                    GUILayout.Toggle(selectedCharacter.KeywordFilters[i].StaticKeywordMethod,
                                        "Static Method", GUILayout.Width(100));
                                GUILayout.EndVertical();
                                GUILayout.FlexibleSpace();
                                GUILayout.BeginVertical();
                                GUILayout.Label("Replacement", GUILayout.Width(100));
                                selectedCharacter.KeywordFilters[i].StaticReplacementStringMethod =
                                    GUILayout.Toggle(selectedCharacter.KeywordFilters[i].StaticReplacementStringMethod,
                                        "Static Method", GUILayout.Width(100));
                                GUILayout.EndVertical();
                                GUILayout.Space(15);
                                GUILayout.EndHorizontal();

                                var keyWordIdentifier = selectedCharacter.KeywordFilters[i];

                                if (keyWordIdentifier == null)
                                    keyWordIdentifier = new KeywordFilter();

                                GUILayout.Space(5);
                                GUILayout.BeginHorizontal();

                                if (keyWordIdentifier.StaticKeywordMethod)
                                {
                                    keyWordIdentifier.KeyWord =
                                         EditorGUILayout.TextField(keyWordIdentifier.KeyWord, GUILayout.Width(100),GUILayout.Height(20));
                                }
                                else
                                {
                                    #region dynamic keyword string

                                    GUILayout.BeginVertical();
                                    GUILayout.BeginHorizontal();
                                    GUILayout.FlexibleSpace();

                                    selectedCharacter.KeywordFilters[i].DynamicKeyword.TargetGameObject =
                                        (GameObject)EditorGUILayout.ObjectField(
                                            selectedCharacter.KeywordFilters[i].DynamicKeyword.TargetGameObject,
                                            typeof(GameObject), true, GUILayout.Height(15));
                                    GUILayout.FlexibleSpace();
                                    GUILayout.EndHorizontal();

                                    if (selectedCharacter.KeywordFilters[i].DynamicKeyword.cachedTargetObject !=
                                        selectedCharacter.KeywordFilters[i].DynamicKeyword.TargetGameObject)
                                    {
                                        selectedCharacter.KeywordFilters[i].DynamicKeyword.Components =
                                            new Component[0];
                                        selectedCharacter.KeywordFilters[i].DynamicKeyword.serializedMethods =
                                            new SerializableMethodInfo[0];

                                        selectedCharacter.KeywordFilters[i].DynamicKeyword.SetComponent(0);
                                        selectedCharacter.KeywordFilters[i].DynamicKeyword.SetMethod(0);
                                        selectedCharacter.KeywordFilters[i].DynamicKeyword.cachedTargetObject =
                                            selectedCharacter.KeywordFilters[i].DynamicKeyword.TargetGameObject;
                                    }


                                    GUILayout.BeginHorizontal();
                                    GUILayout.FlexibleSpace();
                                    GUILayout.Box(ImageLibrary.DownFlowArrow, EditorStyles.label, GUILayout.Width(15),
                                        GUILayout.Height(15));
                                    GUILayout.FlexibleSpace();
                                    GUILayout.EndHorizontal();


                                    var disabledComponents =
                                        selectedCharacter.KeywordFilters[i].DynamicKeyword.TargetGameObject == null;
                                    EditorGUI.BeginDisabledGroup(disabledComponents);

                                    if (disabledComponents &&
                                        (selectedCharacter.KeywordFilters[i].DynamicKeyword.Components.Count() != 0 ||
                                         selectedCharacter.KeywordFilters[i].DynamicKeyword.serializedMethods.Count() !=
                                         0))
                                    {
                                        selectedCharacter.KeywordFilters[i].DynamicKeyword.Components =
                                            new Component[0];
                                        selectedCharacter.KeywordFilters[i].DynamicKeyword.serializedMethods =
                                            new SerializableMethodInfo[0];
                                    }

                                    GUILayout.BeginHorizontal();
                                    GUILayout.FlexibleSpace();
                                    var componentName = !selectedCharacter.KeywordFilters[i].DynamicKeyword.Components
                                        .Any()
                                        ? "None"
                                        : selectedCharacter.KeywordFilters[i].DynamicKeyword
                                            .Components[
                                                selectedCharacter.KeywordFilters[i].DynamicKeyword.ComponentIndex]
                                            .GetType().Name;


                                    if (GUILayout.Button(componentName, EditorStyles.popup, GUILayout.MinWidth(100),
                                        GUILayout.Height(15)))
                                    {
                                        selectedCharacter.KeywordFilters[i].DynamicKeyword.GetGameObjectComponents();
                                        var menu = new GenericMenu();
                                        for (var u = 0;
                                            u < selectedCharacter.KeywordFilters[i].DynamicKeyword.Components.Length;
                                            u++)
                                            menu.AddItem(
                                                new GUIContent(selectedCharacter.KeywordFilters[i].DynamicKeyword
                                                    .Components[u].GetType().Name),
                                                selectedCharacter.KeywordFilters[i].DynamicKeyword.ComponentIndex
                                                    .Equals(u),
                                                selectedCharacter.KeywordFilters[i].DynamicKeyword.SetComponent, u);
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
                                    var methodName = !selectedCharacter.KeywordFilters[i].DynamicKeyword
                                        .serializedMethods.Any()
                                        ? "None"
                                        : selectedCharacter.KeywordFilters[i].DynamicKeyword
                                            .serializedMethods[
                                                selectedCharacter.KeywordFilters[i].DynamicKeyword.MethodIndex]
                                            .methodName;
                                    //  var disabledMethods = condition.TargetGameObject == null;


                                    if (GUILayout.Button(methodName, EditorStyles.popup, GUILayout.MinWidth(100),
                                        GUILayout.Height(15)))
                                    {
                                        selectedCharacter.KeywordFilters[i].DynamicKeyword.GetComponentMethods();
                                        var menu = new GenericMenu();
                                        for (var u = 0;
                                            u < selectedCharacter.KeywordFilters[i].DynamicKeyword.serializedMethods
                                                .Length;
                                            u++)
                                            menu.AddItem(
                                                new GUIContent(selectedCharacter.KeywordFilters[i].DynamicKeyword
                                                    .serializedMethods[u].methodInfo.Name),
                                                selectedCharacter.KeywordFilters[i].DynamicKeyword.MethodIndex
                                                    .Equals(u),
                                                selectedCharacter.KeywordFilters[i].DynamicKeyword.SetMethod, u);
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

                                    selectedCharacter.KeywordFilters[i].DynamicReplacementString.TargetGameObject =
                                        (GameObject)EditorGUILayout.ObjectField(
                                            selectedCharacter.KeywordFilters[i].DynamicReplacementString
                                                .TargetGameObject,
                                            typeof(GameObject), true, GUILayout.Height(15));
                                    GUILayout.FlexibleSpace();
                                    GUILayout.EndHorizontal();

                                    if (selectedCharacter.KeywordFilters[i].DynamicReplacementString
                                            .cachedTargetObject != selectedCharacter.KeywordFilters[i]
                                            .DynamicReplacementString.TargetGameObject)
                                    {
                                        selectedCharacter.KeywordFilters[i].DynamicReplacementString.Components =
                                            new Component[0];
                                        selectedCharacter.KeywordFilters[i].DynamicReplacementString.serializedMethods =
                                            new SerializableMethodInfo[0];

                                        selectedCharacter.KeywordFilters[i].DynamicReplacementString.SetComponent(0);
                                        selectedCharacter.KeywordFilters[i].DynamicReplacementString.SetMethod(0);
                                        selectedCharacter.KeywordFilters[i].DynamicReplacementString.cachedTargetObject
                                            = selectedCharacter.KeywordFilters[i].DynamicReplacementString
                                                .TargetGameObject;
                                    }


                                    GUILayout.BeginHorizontal();
                                    GUILayout.FlexibleSpace();
                                    GUILayout.Box(ImageLibrary.DownFlowArrow, EditorStyles.label, GUILayout.Width(15),
                                        GUILayout.Height(15));
                                    GUILayout.FlexibleSpace();
                                    GUILayout.EndHorizontal();


                                    var disabledComponents =
                                        selectedCharacter.KeywordFilters[i].DynamicReplacementString.TargetGameObject ==
                                        null;
                                    EditorGUI.BeginDisabledGroup(disabledComponents);

                                    if (disabledComponents &&
                                        (selectedCharacter.KeywordFilters[i].DynamicReplacementString.Components
                                             .Count() != 0 || selectedCharacter.KeywordFilters[i]
                                             .DynamicReplacementString.serializedMethods.Count() != 0))
                                    {
                                        selectedCharacter.KeywordFilters[i].DynamicReplacementString.Components =
                                            new Component[0];
                                        selectedCharacter.KeywordFilters[i].DynamicReplacementString.serializedMethods =
                                            new SerializableMethodInfo[0];
                                    }

                                    GUILayout.BeginHorizontal();
                                    GUILayout.FlexibleSpace();
                                    var componentName = !selectedCharacter.KeywordFilters[i].DynamicReplacementString
                                        .Components.Any()
                                        ? "None"
                                        : selectedCharacter.KeywordFilters[i].DynamicReplacementString
                                            .Components[
                                                selectedCharacter.KeywordFilters[i].DynamicReplacementString
                                                    .ComponentIndex].GetType().Name;


                                    if (GUILayout.Button(componentName, EditorStyles.popup, GUILayout.MinWidth(100),
                                        GUILayout.Height(15)))
                                    {
                                        selectedCharacter.KeywordFilters[i].DynamicReplacementString
                                            .GetGameObjectComponents();
                                        var menu = new GenericMenu();
                                        for (var u = 0;
                                            u < selectedCharacter.KeywordFilters[i].DynamicReplacementString.Components
                                                .Length;
                                            u++)
                                            menu.AddItem(
                                                new GUIContent(selectedCharacter.KeywordFilters[i]
                                                    .DynamicReplacementString.Components[u].GetType().Name),
                                                selectedCharacter.KeywordFilters[i].DynamicReplacementString
                                                    .ComponentIndex.Equals(u),
                                                selectedCharacter.KeywordFilters[i].DynamicReplacementString
                                                    .SetComponent, u);
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
                                    var methodName = !selectedCharacter.KeywordFilters[i].DynamicReplacementString
                                        .serializedMethods.Any()
                                        ? "None"
                                        : selectedCharacter.KeywordFilters[i].DynamicReplacementString
                                            .serializedMethods[
                                                selectedCharacter.KeywordFilters[i].DynamicReplacementString
                                                    .MethodIndex].methodName;
                                    //  var disabledMethods = condition.TargetGameObject == null;


                                    if (GUILayout.Button(methodName, EditorStyles.popup, GUILayout.MinWidth(100),
                                        GUILayout.Height(15)))
                                    {
                                        selectedCharacter.KeywordFilters[i].DynamicReplacementString
                                            .GetComponentMethods();
                                        var menu = new GenericMenu();
                                        for (var u = 0;
                                            u < selectedCharacter.KeywordFilters[i].DynamicReplacementString
                                                .serializedMethods.Length;
                                            u++)
                                            menu.AddItem(
                                                new GUIContent(selectedCharacter.KeywordFilters[i]
                                                    .DynamicReplacementString.serializedMethods[u].methodInfo.Name),
                                                selectedCharacter.KeywordFilters[i].DynamicReplacementString.MethodIndex
                                                    .Equals(u),
                                                selectedCharacter.KeywordFilters[i].DynamicReplacementString.SetMethod,
                                                u);
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

                            //    keyWordIdentifier.NewColour = EditorGUILayout.ColorField(keyWordIdentifier.NewColour);

                                GUILayout.FlexibleSpace();
                                GUILayout.Space(5);

                                if (GUILayout.Button(ImageLibrary.deleteConditionIcon, GUIStyle.none,
                                    GUILayout.Width(15)))
                                {
                                    selectedCharacter.KeywordFilters.RemoveAt(i);
                                    selectedCharacter.KeywordFilters.RemoveAll(n => n == null);
                                    selectedCharacter.FilterCount = selectedCharacter.KeywordFilters.Count;
                                }

                                GUILayout.Space(15);
                            }
                        }
                    }
                }

                EditorGUI.EndDisabledGroup();

                Separator();


                /*  GUILayout.Space(5);
                  GUILayout.BeginHorizontal();
                  GUILayout.Label("Use Storyboard Images");
                  GUILayout.FlexibleSpace();
                  var usesStoryboardImagesPowerIcon = selectedCharacter.UsesStoryboardImages ? ImageLibrary.PowerOnpro : ImageLibrary.PowerOffpro;
                  if (GUILayout.Button(usesStoryboardImagesPowerIcon, GUIStyle.none, GUILayout.Width(15), GUILayout.Height(15)))
                      UsesStoryboardImages.boolValue = !selectedCharacter.UsesStoryboardImages;
                  GUILayout.EndHorizontal();
                  GUILayout.Space(5);
  
                  if (selectedCharacter.UsesStoryboardImages)
                  { }
                  Separator();*/

                if (showHelpMessage)
                    EditorGUILayout.HelpBox("The UseVoiceover toggle preps your character to do voice clip playbacks",
                        MessageType.Info);
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Use Voiceover");
                GUILayout.FlexibleSpace();
                var UsesVoiceOverPowerIcon =
                    selectedCharacter.UsesVoiceOver ? ImageLibrary.PowerOnpro : ImageLibrary.PowerOffpro;
                if (GUILayout.Button(UsesVoiceOverPowerIcon, GUIStyle.none, GUILayout.Width(15), GUILayout.Height(15)))
                    UsesVoiceOver.boolValue = !selectedCharacter.UsesVoiceOver;
                GUILayout.EndHorizontal();
                GUILayout.Space(5);

                if (selectedCharacter.UsesVoiceOver)
                {
                    if (showHelpMessage)
                        EditorGUILayout.HelpBox(
                            "With Auto Start turned on, your voice clip will play as soon as the data associated with the voice clip is processed (Using the Condition system to trigger playback is recommended)",
                            MessageType.Info);

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(
                        useAutoStartVoiceClip.boolValue ? ImageLibrary.PowerOnpro : ImageLibrary.PowerOffpro,
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

                if (selectedCharacter.UsesSoundffects)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(
                        useAutoStartSoundEffectClip.boolValue ? ImageLibrary.PowerOnpro : ImageLibrary.PowerOffpro,
                        GUIStyle.none, GUILayout.Width(15), GUILayout.Height(15)))
                        useAutoStartSoundEffectClip.boolValue = !useAutoStartSoundEffectClip.boolValue;
                    GUILayout.Label("Auto start");
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5);
                }
            }


            GUILayout.Space(5);

            #endregion


            #region if scene id == default -1

            if (selectedCharacter.sceneData.SceneID == -1) return;

            #endregion


            // Separator();


            #region check if the scene order has been endited

            if (CurrentStory.SceneOrderEdited)
            {
                var targetScene = tempStoryData.Scenes.Find(s => s.UID == selectedCharacter.sceneData.UID);
                if (targetScene != null)
                {
                    var id = tempStoryData.Scenes.IndexOf(targetScene);
                    selectedCharacter.sceneData.SceneID = id;
                }

                CurrentStory.SceneOrderEdited = false;

            }


            if (selectedCharacter.sceneData.SceneID + 1 > tempStoryData.Scenes.Count)
            {
                EditorGUILayout.HelpBox(
                    "Make sure that this Story project is the correct project. Else check to ensure that the scene was not deleted ",
                    MessageType.Info);
                // if (GUILayout.Button("Attempt Correction"))
                CurrentStory.SceneOrderEdited = true;
                return;
            }

            #endregion

            var scene = tempStoryData.Scenes[selectedCharacter.sceneData.SceneID];

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
                matchingSelectedNodeData = selectedCharacter.sceneData
                    .Characters[selectedCharacter.targetChararacterIndex].NodeDataInMyChain
                    .Find(n => n.UID == selectedNode.UID);
                matchingReflectedData = selectedCharacter.ReflectedDataSet.Find(r => r.UID == selectedNode.UID);

                if (selectedCharacter.GeneralReflectedData != null && selectedNode.CallingNode != null)
                    if (selectedNode.CallingNode.UID == selectedCharacter.GeneralReflectedData.UID)
                        matchingGeneralReflectedData = selectedCharacter.GeneralReflectedData;

                UID = selectedNode.UID;
                LastSelectedUID.stringValue = UID;
            }

            #endregion


            #region if there is no matchingSelectedNodeData

            if (matchingSelectedNodeData == null) return;

            #endregion

            GUILayout.Space(10);


            if (selectedCharacter.GeneralReflectedData == null)
                return;



            Separator2();
            Separator3();

            if (showHelpMessage)
                EditorGUILayout.HelpBox(
                    "General Conditions are processed whenever this character is actively engaged in interaction in which it is carrying out any action",
                    MessageType.Info);

            selectedCharacter.ShowGeneralConditionsSettings =
                EditorGUILayout.Foldout(selectedCharacter.ShowGeneralConditionsSettings,
                    "General Conditions (for this character)");

            GUILayout.Space(10);


            if (selectedCharacter.ShowGeneralConditionsSettings)
                DrawGeneralConditionCreator(selectedCharacter);

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
            //  Separator();

            GUILayout.Space(10);


            #region node specific data

            selectedCharacter.ShowNodeSpecificSettings =
                EditorGUILayout.Foldout(selectedCharacter.ShowNodeSpecificSettings, "Show Node Specific Settings");

            GUILayout.Space(10);

            if (selectedCharacter.ShowNodeSpecificSettings)
            {
                #region Draw Node Specific Data

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

                        EditorUtility.SetDirty(selectedCharacter.sceneData);
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
                { }

                if (matchingSelectedNodeData.type == typeof(ActionNodeData))
                {
                    Separator3();

                    var action = (ActionNodeData)matchingSelectedNodeData;
                    GUILayout.Space(5);
                    action.LocalizedText[selectedCharacter.sceneData.LanguageIndex] = EditorGUILayout.TextArea(action.LocalizedText[selectedCharacter.sceneData.LanguageIndex], Theme.GameBridgeSkin.customStyles[7], GUILayout.Width(ScreenRect.width - 40), GUILayout.Height(100));
                    GUILayout.Space(5);
                    Separator(); 

                    Separator3();

                    if (selectedCharacter.UsesSoundffects)
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
                        action.LocalizedSoundEffects[selectedCharacter.sceneData.LanguageIndex] = (AudioClip)EditorGUILayout.ObjectField("Sound Effect", action.LocalizedSoundEffects[selectedCharacter.sceneData.LanguageIndex],
                            typeof(AudioClip), false);
                        GUILayout.Space(5);
                        Separator();

                        /* GUILayout.Space(5);
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
                         Separator();
                         */

                        EditorGUI.EndDisabledGroup();
                    }

                    Separator3();

                    if (showHelpMessage)
                        EditorGUILayout.HelpBox(
                            "The Start time, Duration and Delay here are a reflection of the start time, duration and delay of the selected node",
                            MessageType.Info);
                    EditorGUILayout.LabelField("Start Time", action.StartTime.ToString());
                    EditorGUILayout.LabelField("Duration", action.Duration.ToString());
                    EditorGUILayout.LabelField("Delay", action.Delay.ToString());
                    if (showHelpMessage)
                        EditorGUILayout.HelpBox(
                            "Realtime delay is set at runtime. it is recommended that you use Realtime dealy instead of Delay",
                            MessageType.Info);
                    EditorGUILayout.LabelField("Realtime Delay", action.RealtimeDelay.ToString());
                    // EditorGUILayout.LabelField("Realtime Delay", action.RealtimeDelay.ToString());

                }

                if (matchingSelectedNodeData.type == typeof(DialogueNodeData))
                {
                    var dialogue = (DialogueNodeData)matchingSelectedNodeData;


                    //  if (selectedCharacter.UsesText)
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
                    dialogue.OverridenCharacterTextDisplayMode = (CharacterTextDisplayMode)EditorGUILayout.EnumPopup(dialogue.OverridenCharacterTextDisplayMode, GUILayout.Height(15), GUILayout.Width(100));
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
                            selectedCharacter.TypingSpeed = EditorGUILayout.IntField(selectedCharacter.TypingSpeed, GUILayout.Height(15), GUILayout.Width(100));
                            GUILayout.EndHorizontal();


                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Delay");
                            GUILayout.FlexibleSpace();
                            selectedCharacter.Delay = EditorGUILayout.FloatField(selectedCharacter.Delay, GUILayout.Height(15), GUILayout.Width(100));
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Typing Sound");
                            GUILayout.FlexibleSpace();
                            selectedCharacter.TypingAudioCip = (AudioClip)EditorGUILayout.ObjectField(selectedCharacter.TypingAudioCip, typeof(AudioClip), false, GUILayout.Height(15), GUILayout.Width(100));
                            GUILayout.EndHorizontal();

                            GUILayout.Space(5);



                            break;
                        case CharacterTextDisplayMode.Custom:

                            break;
                    }
                } */


                    //}

                    Separator3();
                    GUILayout.Space(10);

                    GUILayout.BeginHorizontal();

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent(!Theme.GameBridgeSkin.customStyles[7].richText? "Markup Edit: <color=green>On</color>": "Markup Edit: <color=#ff0033>Off</color>",ImageLibrary.editIcon, "Do a quick markup edit"), Theme.GameBridgeSkin.button, GUILayout.Width(120), GUILayout.Height(20)))
                    {
                        Theme.GameBridgeSkin.customStyles[7].richText = !Theme.GameBridgeSkin.customStyles[7].richText;
                    }

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button(new GUIContent("Open Editor",ImageLibrary.MarkupIcon, "Edit in Markup Editor"),Theme.GameBridgeSkin.button, GUILayout.Width(120), GUILayout.Height(20)))
                    {
                        var markupWindow = EditorWindow.GetWindow<MarkupEditor>();
                        markupWindow.sceneData = selectedCharacter.sceneData;
                        markupWindow.TargetNodeData = dialogue;
                    }

                    GUILayout.FlexibleSpace();

                    GUILayout.EndHorizontal();

                    dialogue.LocalizedText[selectedCharacter.sceneData.LanguageIndex] =
                        EditorGUILayout.TextArea(dialogue.LocalizedText[selectedCharacter.sceneData.LanguageIndex], Theme.GameBridgeSkin.customStyles[7], GUILayout.Width(ScreenRect.width - 40), GUILayout.Height(100));
                    GUILayout.Space(5);
                    Separator();

                    Separator3();
                    if (selectedCharacter.UsesVoiceOver)
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
                        dialogue.LocalizedVoiceRecordings[selectedCharacter.sceneData.LanguageIndex] = (AudioClip)EditorGUILayout.ObjectField("Voice clip",
                            dialogue.LocalizedVoiceRecordings[selectedCharacter.sceneData.LanguageIndex], typeof(AudioClip), false);
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

                    if (selectedCharacter.UsesSoundffects)
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
                        dialogue.LocalizedSoundEffects[selectedCharacter.sceneData.LanguageIndex] = (AudioClip)EditorGUILayout.ObjectField("Sound Effect",
                            dialogue.LocalizedSoundEffects[selectedCharacter.sceneData.LanguageIndex], typeof(AudioClip), false);
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
                         Separator();*/
                        EditorGUI.EndDisabledGroup();
                    }


                    Separator3();

                    GUILayout.Space(5);

                    if (showHelpMessage)
                        EditorGUILayout.HelpBox(
                            "The Start time, Duration and Delay here are a reflection of the start time, duration and delay of the selected node",
                            MessageType.Info);
                    EditorGUILayout.LabelField("Start Time", dialogue.StartTime.ToString());
                    EditorGUILayout.LabelField("Duration", dialogue.Duration.ToString());
                    EditorGUILayout.LabelField("Delay", dialogue.Delay.ToString());
                    if (showHelpMessage)
                        EditorGUILayout.HelpBox(
                            "Realtime delay is set at runtime. it is recommended that you use Realtime dealy instead of Delay",
                            MessageType.Info);
                    EditorGUILayout.LabelField("Realtime Delay", dialogue.RealtimeDelay.ToString());
                    GUILayout.Space(5);
                    Separator();

                    // EditorGUILayout.LabelField("Realtime Delay", dialogue.RealtimeDelay.ToString());


                }

                if (matchingSelectedNodeData.type == typeof(RouteNodeData))
                {
                    var route = (RouteNodeData)matchingSelectedNodeData;


                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Alternate Route Titles");
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

                        if (matchingReflectedData.RouteSpecificDataset.LanguageSpecificData.Count != selectedCharacter.sceneData.Languages.Count)
                            matchingReflectedData.RouteSpecificDataset.LanguageSpecificData.Resize(selectedCharacter.sceneData.Languages.Count);

                        if (matchingReflectedData.RouteSpecificDataset.LanguageSpecificData[selectedCharacter.sceneData.LanguageIndex] == null)
                            matchingReflectedData.RouteSpecificDataset.LanguageSpecificData[selectedCharacter.sceneData.LanguageIndex] = new ReflectedData.LanguageSpecificDataForRouteNodeData();

                        if (matchingReflectedData.RouteSpecificDataset.LanguageSpecificData[selectedCharacter.sceneData.LanguageIndex].RouteTitles.Count !=
                            route.DataIconnectedTo.Count)
                            matchingReflectedData.RouteSpecificDataset.LanguageSpecificData[selectedCharacter.sceneData.LanguageIndex].RouteTitles.Resize(
                                route.DataIconnectedTo.Count);

                        for (var i = 0; i < route.DataIconnectedTo.Count; i++)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(route.DataIconnectedTo[i].Name, GUILayout.Width(100));
                            GUILayout.FlexibleSpace();
                            matchingReflectedData.RouteSpecificDataset.LanguageSpecificData[selectedCharacter.sceneData.LanguageIndex].RouteTitles[i] =
                                EditorGUILayout.DelayedTextField(
                                    matchingReflectedData.RouteSpecificDataset.LanguageSpecificData[selectedCharacter.sceneData.LanguageIndex].RouteTitles[i],
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
                    //   var link = (LinkNodeData)matchingSelectedNodeData;
                }

                if (matchingSelectedNodeData.type == typeof(EndNodeData))
                {
                    //   var end = (EndNodeData)matchingSelectedNodeData;
                }
                #endregion



                #region Draw Node Specific Conditions
                if (matchingSelectedNodeData.type != typeof(CharacterNodeData))
                {
                    Separator3();

                    GUILayout.Space(10);
                    ShowNodeSpecificConditionSettings.boolValue = EditorGUILayout.Foldout(ShowNodeSpecificConditionSettings.boolValue, "Node Spefic Conditions");
                    if (ShowNodeSpecificConditionSettings.boolValue)
                        DrawConditionCreator(selectedCharacter);
                    GUILayout.Space(5);
                    Separator();
                }

                #endregion
            }

            #endregion

            GUILayout.Space(35);

            #endregion



            var helpMessageButtonArea = GUILayoutUtility.GetLastRect().ToLowerLeft(150, 20);
            showHelpMessage = GUI.Toggle(helpMessageButtonArea, showHelpMessage, "Show help messages");

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGeneralConditionCreator(Character character)
        {
            if (matchingGeneralReflectedData == null) return;

            var generalConditionsCount = matchingGeneralReflectedData.Conditions.Count;
            if (GeneralConditionSpecificSpaceing.Count != generalConditionsCount)
                GeneralConditionSpecificSpaceing.Resize(generalConditionsCount);


            for (var c = 0; c < generalConditionsCount; c++)
            {
                var condition = matchingGeneralReflectedData.Conditions[c];

                var conditionSerializedObject = new SerializedObject(condition);
                conditionSerializedObject.Update();

                var area = EditorGUILayout.GetControlRect();

                // GUI.DrawTexture(area.AddRect(0,0,0,60), Textures.Gray);

                #region Backround UI

                var eventCount = matchingGeneralReflectedData.Conditions[c].targetEvent.GetPersistentEventCount();
                var countSpacing = eventCount < 2 ? 0 : (eventCount - 1) * 47;
                /* var conditionBodyArea = area.ToUpperLeft(0, areaHeight + countSpacing);*/
                var conditionBodyArea = area.ToUpperLeft(0, 100 + GeneralConditionSpecificSpaceing[c] + countSpacing);
                GUI.Box(conditionBodyArea, "", Theme.GameBridgeSkin.customStyles[2]);
                GUI.DrawTexture(area.ToUpperLeft(0, 3, 0, 15), Textures.DuskLighter);
                var conditionBodyFooter = conditionBodyArea.PlaceUnder(0, 5);
                GUI.Box(conditionBodyFooter, "", Theme.GameBridgeSkin.customStyles[3]);
                var buttonArea = conditionBodyFooter.ToLowerRight(55, 14, -12, 14);
                GUI.Box(buttonArea, "", Theme.GameBridgeSkin.customStyles[4]);

                var addConditionButtonArea = buttonArea.ToCenterLeft(8, 8, 10);
                if (ClickEvent.Click(4, addConditionButtonArea, ImageLibrary.addConditionIcon))
                {
                    var newCondition =
                        new GameObject("General Condition " + generalConditionsCount);
                    newCondition.AddComponent<Condition>(); //.self = newCondition;
                    var _condition = newCondition.GetComponent<Condition>();
                    _condition.CharacterGameObject = character.gameObject;
                    _condition.character = character;
                    _condition.Self = newCondition;
                    newCondition.transform.SetParent(matchingGeneralReflectedData.transform);
                    // newCondition.hideFlags = HideFlags.HideInHierarchy;
                    //   matchingGeneralReflectedData.Conditions.Add(newCondition.GetComponent<Condition>());
                    matchingGeneralReflectedData.Conditions.Insert(c + 1, newCondition.GetComponent<Condition>());

                    GeneralConditionSpecificSpaceing.Add(0);
                }

                var deleteConditionButtonArea = buttonArea.ToCenterRight(8, 8, -10);
                if (c != 0)
                    if (ClickEvent.Click(4, deleteConditionButtonArea, ImageLibrary.deleteConditionIcon))
                    {
                        DestroyImmediate(matchingGeneralReflectedData.Conditions[c].gameObject);
                        matchingGeneralReflectedData.Conditions.RemoveAt(c);
                        return;
                    }

                #endregion


                var moveConditionUpButtonArea = area.ToCenterLeft(15, 8, 10);
                if (ClickEvent.Click(1, moveConditionUpButtonArea, ImageLibrary.upArrow,
                    "Move this condtion up by one position"))
                {
                    if (c > 0)
                    {
                        var ConditionAtTop = matchingGeneralReflectedData.Conditions[c - 1];

                        matchingGeneralReflectedData.Conditions[c - 1] = null;
                        //  matchingGeneralReflectedData.Conditions[c] = null;

                        matchingGeneralReflectedData.Conditions[c - 1] = condition;
                        matchingGeneralReflectedData.Conditions[c] = ConditionAtTop;
                    }
                }


                var moveConditionDownButtonArea = moveConditionUpButtonArea.PlaceToRight(15, 0, 20);
                if (ClickEvent.Click(1, moveConditionDownButtonArea, ImageLibrary.downArrow,
                    "Move this condtion down by one position"))
                {
                    if (c != generalConditionsCount - 1)
                    {
                        var ConditionAtBottom = matchingGeneralReflectedData.Conditions[c + 1];

                        matchingGeneralReflectedData.Conditions[c + 1] = null;
                        // matchingGeneralReflectedData.Conditions[c] = null;

                        matchingGeneralReflectedData.Conditions[c + 1] = condition;
                        matchingGeneralReflectedData.Conditions[c] = ConditionAtBottom;
                    }
                }

                var copyAllConditionDataButtonArea = moveConditionDownButtonArea.PlaceToRight(12, 12, 40, -2);
                if (ClickEvent.Click(1, copyAllConditionDataButtonArea, ImageLibrary.CopyIcon, "Copy condition data"))
                {
                    ConditionCopy.MakeCopy(condition);
                }

                var copyOnlyEventsButtonArea = copyAllConditionDataButtonArea.PlaceToRight(12, 12, 20);
                if (ClickEvent.Click(1, copyOnlyEventsButtonArea, ImageLibrary.CopyIcon,
                    "copy all event data only (disabled)"))
                {
                    /*var e = System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), condition.targetEvent.GetPersistentTarget(0),
                        condition.targetEvent.GetPersistentMethodName(0)) as UnityEngine.Events.UnityAction;
                    UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(ConditionCopy.TargetEvent, e);*/
                    // ConditionCopy.MakeCopy(condition, true);/
                }


                var pasteDataButtonArea = copyOnlyEventsButtonArea.PlaceToRight(12, 12, 20);
                if (ClickEvent.Click(1, pasteDataButtonArea, ImageLibrary.PasteIcon, "Paste copied data"))
                {
                    Undo.RegisterCompleteObjectUndo(condition, "Condition");
                    ConditionCopy.Paste(condition);
                }

                if (ClickEvent.Click(1, area.ToUpperRight(15, 15, -10),
                    condition.Disabled ? ImageLibrary.PowerOffpro : ImageLibrary.PowerOnpro,
                    "Enable / Disable this condition"))
                    condition.Disabled = !condition.Disabled;

                EditorGUI.BeginDisabledGroup(condition.Disabled);

                var autoStartInfo = condition.AutoStart ? "Auto Start Is On" : "Auto Start Is Off";
                if (GUILayout.Button(autoStartInfo, GUILayout.Height(15)))
                    condition.AutoStart = !condition.AutoStart;

                // starting out 4 pixels must be added to the layout height

                #region

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
                    : condition.Components[condition.ComponentIndex].GetType().Name;


                if (GUILayout.Button(componentName, EditorStyles.popup, GUILayout.MinWidth(100), GUILayout.Height(15)))
                {
                    condition.GetGameObjectComponents();
                    var menu = new GenericMenu();
                    for (var i = 0; i < condition.Components.Length; i++)
                        menu.AddItem(new GUIContent(condition.Components[i].GetType().Name),
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
                //  var disabledMethods = condition.TargetGameObject == null;


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


                GeneralConditionSpecificSpaceing[c] = 184;

                #endregion


                if (matchingSelectedNodeData.type != typeof(EnvironmentNodeData))
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

        private void DrawConditionCreator(Character character)
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
                    _condition.CharacterGameObject = character.gameObject;
                    _condition.character = character;
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
                if (ClickEvent.Click(1, moveConditionUpButtonArea, ImageLibrary.upArrow,
                    "Move this condtion up by one position"))
                {
                    if (c > 0)
                    {
                        var ConditionAtTop = matchingReflectedData.Conditions[c - 1];

                        matchingReflectedData.Conditions[c - 1] = null;
                        // matchingReflectedData.Conditions[c]=null;

                        matchingReflectedData.Conditions[c - 1] = condition;
                        matchingReflectedData.Conditions[c] = ConditionAtTop;
                    }
                }


                var moveConditionDownButtonArea = moveConditionUpButtonArea.PlaceToRight(15, 0, 20);
                if (ClickEvent.Click(1, moveConditionDownButtonArea, ImageLibrary.downArrow,
                    "Move this condtion down by one position"))
                {
                    if (c != condtionsCount - 1)
                    {
                        var ConditionAtBottom = matchingReflectedData.Conditions[c + 1];

                        matchingReflectedData.Conditions[c + 1] = null;
                        //  matchingReflectedData.Conditions[c] = null;

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
                if (ClickEvent.Click(1, copyOnlyEventsButtonArea, ImageLibrary.CopyIcon,
                    "copy all event data only (Disabled)"))
                {
                    //  ConditionCopy.MakeCopy(condition , true);
                }


                var pasteDataButtonArea = copyOnlyEventsButtonArea.PlaceToRight(12, 12, 20);
                if (ClickEvent.Click(1, pasteDataButtonArea, ImageLibrary.PasteIcon, "Paste copied data"))
                {
                    Undo.RegisterCompleteObjectUndo(condition, "Condition");
                    ConditionCopy.Paste(condition);
                }

                if (ClickEvent.Click(1, area.ToUpperRight(15, 15, -10),
                    condition.Disabled ? ImageLibrary.PowerOffpro : ImageLibrary.PowerOnpro,
                    "Disable/ Enable this condition"))
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
               // ConditionSpecificSpaceing[c] = 29;
                if (matchingSelectedNodeData.useTime)
                {
                    GUILayout.BeginHorizontal();
                    var useTimeInfo = condition.UseTime ? "Use Time Is On" : "Use Time Is Off";
                    if (GUILayout.Button(useTimeInfo, GUILayout.Height(15)))
                        condition.UseTime = !condition.UseTime;

                    GUILayout.FlexibleSpace();

                    EditorGUI.BeginDisabledGroup(!condition.UseTime);

                    condition.timeUseMethod = (TimeUseMethod)EditorGUILayout.EnumPopup(condition.timeUseMethod,
                        GUILayout.Width(100), GUILayout.Height(15));

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
                   // ConditionSpecificSpaceing[c] = 194;
                    if (condition.timeUseMethod == TimeUseMethod.Custom)
                    {

                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Custom Time : ", GUILayout.Height(15));
                        condition.CustomWaitTime = EditorGUILayout.DelayedFloatField(condition.CustomWaitTime,
                            GUILayout.Width(115), GUILayout.Height(15));
                        GUILayout.EndHorizontal();
                        // ConditionSpecificSpaceing[c] = 192;
                    }

                }


                #region

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
                    : condition.Components[condition.ComponentIndex].GetType().Name;


                if (GUILayout.Button(componentName, EditorStyles.popup, GUILayout.MinWidth(100), GUILayout.Height(15)))
                {
                    condition.GetGameObjectComponents();
                    var menu = new GenericMenu();
                    for (var i = 0; i < condition.Components.Length; i++)
                        menu.AddItem(new GUIContent(condition.Components[i].GetType().Name),
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
                //  var disabledMethods = condition.TargetGameObject == null;


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
            GUI.DrawTextureWithTexCoords(repeatArea, ImageLibrary.RepeatableStipe,
                new Rect(0, 0, repeatArea.width / 20, 1));
        }


        #region variables

        [SerializeField] public string UID = "";

        [SerializeField] private NodeData matchingSelectedNodeData;

        private ReflectedData matchingGeneralReflectedData;
        private ReflectedData matchingReflectedData;
        private bool iconSet;
        private Rect ScreenRect = new Rect(0, 0, 0, 0);
        private Vector2 scrollView;
        private readonly List<int> GeneralConditionSpecificSpaceing = new List<int>();
        private readonly List<int> ConditionSpecificSpaceing = new List<int>();
        private Vector2 characterSelectionScrollView;
        private readonly List<string> characternames = new List<string>();

        private Story tempStoryData;

        private Character selectedCharacter;

        private bool MakeSelectionSwitch;


        private SerializedProperty NameUI;

        private SerializedProperty NameText;

        private SerializedProperty DisplayedTextUI;

        private SerializedProperty DisplayedText;

        private SerializedProperty moveNextButton;

        private SerializedProperty movePreviousButton;

        private SerializedProperty RouteButton;

        private SerializedProperty RouteParent;

        private SerializedProperty TypingSpeed;

        private SerializedProperty TypingAudioCip;

        private SerializedProperty targetChararacterIndex;

        private SerializedProperty MatchingRouteNumber;

        private SerializedProperty UsesText;

        //    private SerializedProperty UsesStoryboardImages;

        private SerializedProperty UsesVoiceOver;

        private SerializedProperty UsesSoundffects;

        private SerializedProperty UseKeywordFilters;

        private SerializedProperty FilterCount;

        private SerializedProperty ShowKeywordFilterFouldout;

        private SerializedProperty ShowGeneralSettings;

        private SerializedProperty textDisplayMode;

        private SerializedProperty LastSelectedUID;

        private SerializedProperty useNameUI;

        private SerializedProperty useDialogueTextUI;

        private SerializedProperty useMoveNextButton;

        private SerializedProperty useMovePreviousButton;

        private SerializedProperty useRouteButton;

        private SerializedProperty useAutoStartVoiceClip;

        private SerializedProperty useAutoStartSoundEffectClip;

        private SerializedProperty NameColour;

        private SerializedProperty TextColour;

        private SerializedProperty AutoStartAllConditionsByDefault;

        private SerializedProperty ShowNodeSpecificConditionSettings;

        private SerializedProperty UpdateUID;

        #endregion

        bool showHelpMessage = false;
        private EditorWindow[] edwin;
        //  private ColourPickerWheel colourPickerWheel = new ColourPickerWheel();
        //     private ColourPickerWheel colourPickerWheel2 = new ColourPickerWheel();
    }
}