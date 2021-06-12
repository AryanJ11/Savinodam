using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using TMPro;

namespace DaiMangou.BridgedData
{
    /// <summary>
    ///     choice between typed or instant text
    /// </summary>
    public enum CharacterTextDisplayMode
    {
        Instant = 0,

        Typed = 1
        //   Custom = 2
    }





    [DisallowMultipleComponent]
    public class Character : MonoBehaviour
    {
        /// <summary>
        /// </summary>
        private void Awake()
        {
            if (self == null)
                self = sceneData.Characters[targetChararacterIndex];


            if (!self.IsPlayer) return;
            if (UseMoveNextButton)
                MoveNextButton.onClick.AddListener(MoveNext);

            if (UseMovePreviousButton)
                MovePreviousButton.onClick.AddListener(MovePrevious);


        }

        /// <summary>
        /// </summary>
        public void OnEnable()
        {
            self = sceneData.Characters[targetChararacterIndex];

            if (self.IsPlayer)
                doRefresh += GenerateActiveDialogueSet;
        }

        /// <summary>
        /// </summary>
        public void OnDisable()
        {
            if (self.IsPlayer
            ) // if we dont do this , then all other characters will modify the ActiveCharacterDialogueSet -__-
                doRefresh -= GenerateActiveDialogueSet;
        }

        /// <summary>
        /// 
        /// </summary>
        /* private void Start()
         {
     
         }*/

        IEnumerator Start()
        {
            sceneData.ActiveCharacterDialogueSet = new List<NodeData>();
            while (true)
            {
                Process();
                yield return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void Reset()
        {
            #region check if the reflected data and general reflected data and audio gameobjects already exist , if so , we find them and assign them

            if (!transform.Find("Reflected Data")) return;
            DestroyImmediate(transform.Find("General Reflected Data").gameObject, true);
            DestroyImmediate(transform.Find("Reflected Data").gameObject, true);
            DestroyImmediate(transform.Find("Audio Manager").gameObject, true);

            #endregion
        }

        /// <summary>
        ///     when two or more character interact , the ActiveCharacterDialogueSetWill be generated
        /// </summary>
        public void GenerateActiveDialogueSet()
        {
            // combine the list of nodes in the communicating character node list to characters node list
            var combi = self.NodeDataInMyChain.ToList();
            combi = CommunicatingCharacters.Aggregate(combi,
                (current, communicatingCharacter) => current.Concat(communicatingCharacter.NodeDataInMyChain).ToList());

            // generate the character list and add the communicating characters and this character to it
            Characters = new List<CharacterNodeData>();
            foreach (var character in CommunicatingCharacters)
                Characters.Add(character);

            // we add itself to the character list last
            Characters.Add(self);


            #region here we will populate the  ActiveChracterDalogueSet at runtime, unlike the NodeDataInMyChain list, this list will have all nodes ordered correctly for execution at runtime

            // firstly make ActiveCharacterDialogueSet a new list
            sceneData.ActiveCharacterDialogueSet = new List<NodeData>();

            // add all nodedata to the active character dialogue set if it is not a character node and is not set to pass
            foreach (var data in combi.Where(data => !data.Runtime_Pass).Where(data =>
                data.type != typeof(CharacterNodeData) && data.type != typeof(EnvironmentNodeData)))
            {
                sceneData.ActiveCharacterDialogueSet.Add(data);

            }



            // here we will begin the process of  setting times on all other nodes. Do not break this function.
            foreach (var AllNodesInChainNotSetToPass in Characters.Select(character =>
                character.NodeDataInMyChain.Where(n => !n.Runtime_Pass).OrderBy(st => st.StartTime).ToList()))
            {
                // we will start from the end of the nodeData list and assign start times and delay times in reverse
                for (var i = AllNodesInChainNotSetToPass.Count - 1; i >= 0; i--)
                {
                    if (AllNodesInChainNotSetToPass[i].type == typeof(RouteNodeData))
                    {
                        var route = (RouteNodeData)AllNodesInChainNotSetToPass[i];

                        if (!route.OverrideStartTime)
                            route.StartTime = route.DataIconnectedTo[route.RuntimeRouteID].StartTime - 0.00001f;
                    }

                    if (AllNodesInChainNotSetToPass[i].type == typeof(LinkNodeData))
                    {
                        var link = (LinkNodeData)AllNodesInChainNotSetToPass[i];

                        if (!link.OverrideStartTime)
                        {

                            if (link.Loop)
                                link.StartTime =
                                    link.DataConnectedToMe[0].DataConnectedToMe[0].StartTime + 0.00002f;
                            else
                                link.StartTime = link.DataIconnectedTo[0].StartTime - 0.00001f;
                        }

                        // no else here because we push the overriding start time to the start time during the data push
                    }

                    // end nodes are connected to nothing , so we must look at the entire list again and find the node data with the largest start time in the current chain and use that value as the 
                    // an nodes start time
                    if (AllNodesInChainNotSetToPass[i].type != typeof(EndNodeData)) continue;
                    var end = (EndNodeData)AllNodesInChainNotSetToPass[i];

                    // find all nodedata in the characters nodedatainmychain and add them ro a new list if the nodedata is not set to pass
                    if (!end.OverrideStartTime)
                        end.StartTime = AllNodesInChainNotSetToPass.Max(s => s.StartTime) + 0.00001f;
                }
            }

            sceneData.ActiveCharacterDialogueSet =
                sceneData.ActiveCharacterDialogueSet.OrderBy(t => t.StartTime).ToList();

            var listOfOnlyActionAndDialogue = sceneData.ActiveCharacterDialogueSet.Where(node =>
                node.type == typeof(ActionNodeData) || node.type == typeof(DialogueNodeData)).ToList();
            for (var i = 0; i < listOfOnlyActionAndDialogue.Count; i++)
            {

                var data = listOfOnlyActionAndDialogue[i];
                if (i == 0)
                    data.RealtimeDelay = data.Delay;


                if (i <= 0) continue;
                var previousData = listOfOnlyActionAndDialogue[i - 1];


                var calculatedDelay = data.StartTime - (previousData.StartTime + previousData.Duration);

                if (calculatedDelay > 0)
                {
                    data.RealtimeDelay = calculatedDelay;
                }
                else
                    calculatedDelay = 0;
            }

            // now set the new Active index based on the cachedUID

            if (!CachedUID.Equals(""))
            {
                ActiveIndex = sceneData.ActiveCharacterDialogueSet.FindIndex(i => i.UID == CachedUID);
            }

            if (ReturnPointUID.Equals("")) return;
            {
                ActiveIndex = sceneData.ActiveCharacterDialogueSet.FindIndex(i => i.UID == ReturnPointUID);
                ReturnPointUID = "";
            }

            #endregion


        }



        /// <summary>
        /// 
        /// </summary>
        public void CleanUp()
        {
            if (CommunicatingCharacters.Count == 0 && Characters.Count > 0)
            {
                Characters = new List<CharacterNodeData>();
                sceneData.ActiveCharacterDialogueSet = new List<NodeData>();
                ActiveIndex = 0; // // this resets the conversation. this must be changed
                CachedUID = "";
                ReturnPointUID = "";
                communicatingCharacterCount = CommunicatingCharacters.Count;
            }

            #region run if we have communicating characters and a characters

            if (CommunicatingCharacters.Count <= 0 || Characters.Count <= 0) return;
            if (CommunicatingCharacters.Contains(sceneData.ActiveCharacterDialogueSet[ActiveIndex].CallingNodeData))
            {
                CachedUID = sceneData.ActiveCharacterDialogueSet[ActiveIndex].UID;
            }
            else
            {
                sceneData.ActiveCharacterDialogueSet = new List<NodeData>();
                CommunicatingCharacters = new List<CharacterNodeData>();
                ActiveIndex = 0; // this resets the conversation. this must be changed
                CachedUID = "";
                ReturnPointUID = "";
            }

            doRefresh();

            #endregion
        }

        /// <summary>
        /// </summary>
        private void Process()
        {

            if (!self.IsPlayer) return;



            if (CommunicatingCharacters.Count == 0) return;

            if (CommunicatingCharacters.Count != communicatingCharacterCount)
            {
                communicatingCharacterCount = CommunicatingCharacters.Count;
                doRefresh();
            }

            if (sceneData.ActiveCharacterDialogueSet.Count == 0) return;

            //   Debug.Log(ActiveIndex + "   "+ sceneData.ActiveCharacterDialogueSet[ActiveIndex].name);

            //we are gurrently processing and agrigating so return
            if (BridgeData.ActiveEvents > 0) return;



            ActiveNodeData = sceneData.ActiveCharacterDialogueSet[ActiveIndex];

            // we call processData on the nodedata that is at ActiveInxex in the ActiveCharacterDialogueSet
            //  ActiveNodeData.ProcessData();



            if (TargetReflectedData == null || CachedActiveIndex != ActiveIndex)
            {
                // needs to be optimized to not have to search the list this way
                var tlist = ReflectedDataSet.ToList();
                if (communicatingCharacterCount > 0)
                {
                    tlist = CommunicatingCharacterGameobject.Aggregate(tlist, (current, t) => current
                        .Concat(t.GetComponent<Character>().ReflectedDataSet)
                        .ToList());

                    TargetReflectedData = tlist.Find(r => r.UID == ActiveNodeData.UID);
                }
                //  TargetReflectedData = ReflectedDataSet.Find(r => r.UID == ActiveNodeData.UID);

                CachedActiveIndex = ActiveIndex;
            }

            if (UseNameUI)
            {
                NameText.color = TargetReflectedData.character.NameColour;
                NameText.text = ActiveNodeData.CharacterName;
            }


            // this wont run until we place routes in the ActiveCharacterDialogueSet. we wil do this by cheking each nodes connection at the end of generating the ActiveCharacterDialogueSet
            // and checkng if and connected nodes are routes, if they are , we just put them infront... of course after matching back the ID to make sure it is the correct node 

            foreach (var condition in TargetReflectedData.character.GeneralReflectedData.Conditions.Where(condition =>
                !condition.Disabled))
            {
                // set option to only process conditions if node is a certain type or uses text or something
                if (!ActiveNodeData.useTime && !ActiveNodeData.IsPlayer) continue;
                if (!condition.Invoked)
                    condition.ProcessConditionData();
            }

            // possibly patchwork ?
            if (!ActiveNodeData.IsPlayer)
            {
                TargetReflectedData.character.ActiveNodeData = ActiveNodeData;


                if (TargetReflectedData.character.TargetReflectedData != TargetReflectedData)
                {
                    TargetReflectedData.character.TargetReflectedData = TargetReflectedData;
                    // don't think we need to set other characters active index but 
                    TargetReflectedData.character.ActiveIndex = ActiveIndex;
                }
            }

            if (ActiveNodeData.type == typeof(RouteNodeData))
            {
                var route = (RouteNodeData)ActiveNodeData;

                // here we want to jump forward in the list by adding 1 to Active index if the RouteNodeData at ActiveIndex is from a character other than a player
                // you can increase or reduce the flexibility of the system here 
                if (!route.IsPlayer)
                {


                    if (TargetReflectedData.character.TargetReflectedData.Conditions.Count == 0 &&
                        route.RuntimeRouteID == route.RuntimeTempRouteID)
                        MoveNext();
                }
                else
                {

                    if (UseRouteButton)
                    {
                        if (!SetupRouteButtons) // edit later for possible continued runtime edit
                        {
                            if (UseDialogueTextUI)
                                DisplayedText.text = "";

                            if (route.DataIconnectedTo.Count > RouteButtons.Count)
                            {
                                var amountToAdd = route.DataIconnectedTo.Count - RouteButtons.Count;
                                for (var i = 0; i < amountToAdd; i++)
                                {
                                    var instancedButton = Instantiate(RouteButton, RouteParent.transform);

                                    var clickListener = instancedButton.gameObject.AddComponent<ClickListener>();
                                    clickListener.characterComponent = this;
                                    instancedButton.onClick.AddListener(clickListener.SwitchRoute);

                                    instancedButton.GetComponent<ClickListener>().characterComponent = this;
                                    RouteButtons.Add(instancedButton);
                                }
                            }

                            foreach (var routeButton in RouteButtons) routeButton.gameObject.SetActive(false);

                            for (var i = 0; i < route.DataIconnectedTo.Count; i++)
                                RouteButtons[i].gameObject.SetActive(true);

                            for (var i = 0; i < RouteButtons.Count; i++)
                                // set clicklistener indexInList value here
                                if (RouteButtons[i].gameObject.activeInHierarchy)
                                {
                                    RouteButtons[i].GetComponent<ClickListener>().indexInList = i;


                                    var textchild = RouteButtons[i].transform.GetChild(0).GetComponent<Text>();


                                    textchild.text = TargetReflectedData.RouteSpecificDataset.UseAlternativeRouteTitles
                                        ? TargetReflectedData.RouteSpecificDataset.LanguageSpecificData[sceneData.LanguageIndex].RouteTitles[i]
                                        : route.DataIconnectedTo[i].LocalizedText[sceneData.LanguageIndex];
                                }

                            SetupRouteButtons = true;
                        }
                    }
                }


                if (route.IsPlayer)
                    if (UseMoveNextButton)
                        if (MoveNextButton.gameObject.activeInHierarchy)
                        {
                            MoveNextButton.gameObject.SetActive(false);

                            if (UseMovePreviousButton) // becaue move previou puttons are not so necessary
                                MovePreviousButton.gameObject.SetActive(false);
                        }


                foreach (var condition in TargetReflectedData.Conditions.Where(condition => !condition.Disabled))
                {
                    if (!condition.Invoked)
                        condition.ProcessConditionData();

                }

                route.ProcessData();

                if (CachedRoute != null)
                {
                    foreach (var button in RouteButtons)
                        button.gameObject.SetActive(false);

                    ActiveIndex = sceneData.ActiveCharacterDialogueSet.IndexOf(CachedRoute) + 1;
                    CachedRoute = null;

                    if (UseMoveNextButton)
                        MoveNextButton.gameObject.SetActive(true);

                    if (UseMovePreviousButton)
                        MovePreviousButton.gameObject.SetActive(true);

                    ResetConditions();
                    SetupRouteButtons = false;
                }


                if (!route.IsPlayer && route.RuntimeRouteID == route.RuntimeTempRouteID) // rough patch for now
                    MoveNext(); // this is only called by route nodes that are not player. By this point conditions have been processes and invoked so this is fine.
            }
            else if (ActiveNodeData.type == typeof(LinkNodeData))
            {
                var link = (LinkNodeData)ActiveNodeData;

                if (!link.Loop)
                {
                    MoveNext();
                }
                else // this will be check in the same loop scene
                {
                    link.RuntimeIterationCount += 1;

                    if (link.RuntimeIterationCount == link.LoopValue)
                        link.loopRoute.RuntimeRouteID = link.loopRoute.AutoSwitchValue;



                    ReturnPointUID = link.DataIconnectedTo[0].UID;
                    link.loopRoute.ProcessData();
                    SetupRouteButtons = false;
                    BridgeData.ActiveEvents = 0; // set to 0 so that we can auto refrehs later


                }

                foreach (var condition in TargetReflectedData.Conditions.Where(condition => !condition.Disabled))
                {
                    if (!condition.Invoked)
                        condition.ProcessConditionData();
                }

                link.ProcessData();

                //      if (HasReturnPoint) return;
            }
            else if (ActiveNodeData.type == typeof(EndNodeData))
            {
                var end = (EndNodeData)ActiveNodeData;

                if (!end.IsPlayer) MoveNext();
                foreach (var condition in TargetReflectedData.Conditions.Where(condition => !condition.Disabled))
                {
                    if (!condition.Invoked)
                        condition.ProcessConditionData();
                }

                end.ProcessData();
                // execute any other action using the condition system
            }
            else if (ActiveNodeData.type == typeof(ActionNodeData))
            {
                // if (ActiveNodeData != sceneData.ActiveCharacterDialogueSet[CachedActiveIndex])
                //   DisplayedText.text = "";

                var action = (ActionNodeData)ActiveNodeData;

                if (UseDialogueTextUI && UsesText)
                {
                    DisplayedText.maxVisibleCharacters = action.LocalizedText[sceneData.LanguageIndex].Length;
                    DisplayedText.text = action.LocalizedText[sceneData.LanguageIndex];
                }

                if (action.LocalizedSoundEffects[sceneData.LanguageIndex] != null && UsesSoundffects)
                    if (!TargetReflectedData.ActionSpecificData.OverrideUseSoundEffect)
                        if (!TargetReflectedData.ActionSpecificData.SoundEffectWasPlayed)
                            if (AutoStartSoundEffectClip)
                            {
                                SoundEffectAudioSource.clip = action.LocalizedSoundEffects[sceneData.LanguageIndex];
                                SoundEffectAudioSource.PlayOneShot(SoundEffectAudioSource.clip);
                                TargetReflectedData.ActionSpecificData.SoundEffectWasPlayed = true;
                            }

                foreach (var condition in TargetReflectedData.Conditions.Where(condition => !condition.Disabled))
                {
                    if (!condition.Invoked)
                        condition.ProcessConditionData();
                }

                action.ProcessData();
                // 
            }
            else
            {


                var dialogue = (DialogueNodeData)ActiveNodeData;

                if (UseDialogueTextUI && UsesText)
                {
                    // we push the text into a var because the text can be manipulated at runtime 
                    if (!ActiveUID.Equals(ActiveNodeData.UID))
                    {
                        DisplayedText.color = TargetReflectedData.character.TextColour;
                        TempText = dialogue.LocalizedText[sceneData.LanguageIndex]; //we can now edit this text without destroying the text in the asset
                        ActiveUID = ActiveNodeData.UID;


                        if (UseKeywordFilters)
                            foreach (var keywordFilter in KeywordFilters.Where(keywordFilter => !keywordFilter.Disabled)
                            )
                            {
                                if (TempText.Contains(keywordFilter.KeyWord))
                                {

                                    #region both static keyword and replacement

                                    if (keywordFilter.StaticKeywordMethod &&
                                        keywordFilter.StaticReplacementStringMethod)
                                    {

                                        TempText = TempText.Replace(keywordFilter.KeyWord, keywordFilter.ReplacementString);

                                        //TempText = TempText.Replace(keywordFilter.KeyWord, "<color=#"+ ColorUtility.ToHtmlStringRGB(keywordFilter.NewColour) + ">" + keywordFilter.ReplacementString + "</color>");

                                    }

                                    #endregion

                                    #region static keyword and dynaic replacement string

                                    if (keywordFilter.StaticKeywordMethod &&
                                        !keywordFilter.StaticReplacementStringMethod)
                                    {
                                        var compA = keywordFilter.DynamicReplacementString.Components[
                                            keywordFilter.DynamicReplacementString.ComponentIndex];

                                        var replacementStringDelegate = (DelA)Delegate.CreateDelegate(typeof(DelA),
                                            compA,
                                            keywordFilter.DynamicReplacementString
                                                .serializedMethods[keywordFilter.DynamicReplacementString.MethodIndex]
                                                .methodName);
                                        TempText = TempText.Replace(keywordFilter.KeyWord, replacementStringDelegate());
                                    }

                                    #endregion

                                }

                                if (keywordFilter.DynamicKeyword.TargetGameObject == null) continue;
                                {
                                    #region both non static keyword and replacement

                                    if (!keywordFilter.StaticKeywordMethod &&
                                        !keywordFilter.StaticReplacementStringMethod)
                                    {
                                        var compA = keywordFilter.DynamicReplacementString.Components[
                                            keywordFilter.DynamicReplacementString.ComponentIndex];
                                        var replacementStringDelegate = (DelA)Delegate.CreateDelegate(typeof(DelA),
                                            compA,
                                            keywordFilter.DynamicReplacementString
                                                .serializedMethods[keywordFilter.DynamicReplacementString.MethodIndex]
                                                .methodName);

                                        var compB = keywordFilter.DynamicKeyword.Components[
                                            keywordFilter.DynamicKeyword.ComponentIndex];
                                        var keywordDelegate = (DelB)Delegate.CreateDelegate(typeof(DelB), compB,
                                            keywordFilter.DynamicKeyword
                                                .serializedMethods[keywordFilter.DynamicKeyword.MethodIndex]
                                                .methodName);


                                        TempText = TempText.Replace(keywordDelegate(), replacementStringDelegate());
                                    }

                                    #endregion

                                    #region dynamic keyword and static replacement string

                                    if (keywordFilter.StaticKeywordMethod ||
                                        !keywordFilter.StaticReplacementStringMethod) continue;
                                    {
                                        var compB = keywordFilter.DynamicKeyword.Components[
                                            keywordFilter.DynamicKeyword.ComponentIndex];
                                        var keywordDelegate = (DelB)Delegate.CreateDelegate(typeof(DelB), compB,
                                            keywordFilter.DynamicKeyword
                                                .serializedMethods[keywordFilter.DynamicKeyword.MethodIndex]
                                                .methodName);

                                        TempText = TempText.Replace(keywordDelegate(), keywordFilter.ReplacementString);
                                    }

                                    #endregion
                                }
                            }

                    }

                    if (textDisplayMode == CharacterTextDisplayMode.Typed)
                    {

                        if (!typing)
                        {
                            modifiedTempText = Regex.Replace(TempText, "<.*?>", string.Empty);
                            DisplayedText.maxVisibleCharacters = 0;
                            DisplayedText.text = TempText;
                            TypingCoroutine = StartCoroutine(DoType());
                        }

                    }
                    else
                    {
                        DisplayedText.maxVisibleCharacters = TempText.Length;
                        DisplayedText.text = TempText;
                    }

                }


                if (dialogue.LocalizedVoiceRecordings[sceneData.LanguageIndex] != null && UsesVoiceOver)
                    if (!TargetReflectedData.DialogueSpecificData.OverrideUseVoiceover)
                        if (!TargetReflectedData.DialogueSpecificData.VoiceClipWasPlayed)
                        {


                            if (AutoStartVoiceClip)
                            {
                                VoiceAudioSource.clip = dialogue.LocalizedVoiceRecordings[sceneData.LanguageIndex];
                                VoiceAudioSource.PlayOneShot(VoiceAudioSource.clip);
                                TargetReflectedData.DialogueSpecificData.VoiceClipWasPlayed = true;
                            }
                        }

                if (dialogue.LocalizedSoundEffects[sceneData.LanguageIndex] != null && UsesSoundffects)
                    if (!TargetReflectedData.DialogueSpecificData.OverrideUseSoundEffect)
                        if (!TargetReflectedData.DialogueSpecificData.SoundEffectWasPlayed)
                        {

                            if (AutoStartSoundEffectClip)
                            {

                                SoundEffectAudioSource.clip = dialogue.LocalizedSoundEffects[sceneData.LanguageIndex];
                                SoundEffectAudioSource.PlayOneShot(SoundEffectAudioSource.clip);
                                TargetReflectedData.DialogueSpecificData.SoundEffectWasPlayed = true;
                            }

                        }

                // TEST
                foreach (var condition in TargetReflectedData.Conditions.Where(condition => !condition.Disabled))
                {
                    if (!condition.Invoked)
                        condition.ProcessConditionData();
                }

                dialogue.ProcessData();
            }

            // once there are no more routes processing events then we trigger a refresh
            if (BridgeData.ActiveEvents != 0) return;
            BridgeData.ActiveEvents = -1;
            doRefresh();
        }


        public void PlayVoiceClipNow()
        {

            if (ActiveNodeData.type != typeof(DialogueNodeData)) return;
            VoiceAudioSource.clip = ((DialogueNodeData)ActiveNodeData).LocalizedVoiceRecordings[sceneData.LanguageIndex];


            if (VoiceAudioSource.clip == null || !UsesVoiceOver) return;
            if (TargetReflectedData.DialogueSpecificData.OverrideUseVoiceover) return;
            if (TargetReflectedData.DialogueSpecificData.VoiceClipWasPlayed) return;
            VoiceAudioSource.PlayOneShot(VoiceAudioSource.clip);
            TargetReflectedData.DialogueSpecificData.VoiceClipWasPlayed = true;
        }

        public void PlaySoundEffectNow()
        {


            if (ActiveNodeData.type != typeof(ActionNodeData) &&
                ActiveNodeData.type != typeof(DialogueNodeData)) return;
            SoundEffectAudioSource.clip = ActiveNodeData.type == typeof(DialogueNodeData)
                ? ((DialogueNodeData)ActiveNodeData).LocalizedSoundEffects[sceneData.LanguageIndex]
                : ((ActionNodeData)ActiveNodeData).LocalizedSoundEffects[sceneData.LanguageIndex];

            if (SoundEffectAudioSource.clip == null || !UsesSoundffects) return;
            if (TargetReflectedData.DialogueSpecificData.OverrideUseSoundEffect) return;
            if (TargetReflectedData.DialogueSpecificData.SoundEffectWasPlayed) return;

            SoundEffectAudioSource.PlayOneShot(SoundEffectAudioSource.clip);

            if (ActiveNodeData.type == typeof(DialogueNodeData))
                TargetReflectedData.DialogueSpecificData.SoundEffectWasPlayed = true;
            else
                TargetReflectedData.ActionSpecificData.SoundEffectWasPlayed = true;

        }


        IEnumerator DoType()
        {

            if (TypingAudioSource.clip == null)
                TypingAudioSource.clip = TypingAudioCip;

            typing = true;
            int typedCounter = 0;

            while (typedCounter < modifiedTempText.Length)
            {

                if (modifiedTempText[typedCounter] == ' ')
                {
                    typedCounter++;
                    DisplayedText.maxVisibleCharacters++;
                }

                typedCounter++;

                DisplayedText.maxVisibleCharacters++;


                if (TypingAudioSource)
                {
                    TypingAudioSource.Play();
                    RandomiseVolume();
                }
                yield return new WaitForSeconds(TypingSpeed);



            }

            yield return null;




        }

        private void RandomiseVolume()
        {
            TypingAudioSource.volume = UnityEngine.Random.Range(1 - VolumeVariation, VolumeVariation + 1);
        }


        /// <summary>
        ///     move to next event in chain of events
        /// </summary>
        public void MoveNext()
        {

            // we also deactivate the buttons BEFORE 
            foreach (var button in RouteButtons)
                button.gameObject.SetActive(false);

            // set back to false
            TargetReflectedData.DialogueSpecificData.VoiceClipWasPlayed = false;
            TargetReflectedData.DialogueSpecificData.SoundEffectWasPlayed = false;
            TargetReflectedData.ActionSpecificData.SoundEffectWasPlayed = false;
            VoiceAudioSource.Stop();
            SoundEffectAudioSource.Stop();

            if (ActiveIndex + 1 < sceneData.ActiveCharacterDialogueSet.Count)
            {
                // reset out text length so that typed text can type itself
                // TextLength = 0;
                ActiveIndex += 1;
            }

            if (typing)
            {
                StopCoroutine(TypingCoroutine);
                typing = false;
                TempText = "";
                DisplayedText.text = "";

            }



            ResetConditions();
        }

        /// <summary>
        ///     move to previous evnt in chaing of events
        /// </summary>
        public void MovePrevious()
        {
            //for safety when moving next while typing text
            foreach (var button in RouteButtons)
                button.gameObject.SetActive(false);

            // set back to false
            TargetReflectedData.DialogueSpecificData.VoiceClipWasPlayed = false;
            TargetReflectedData.DialogueSpecificData.SoundEffectWasPlayed = false;
            TargetReflectedData.ActionSpecificData.SoundEffectWasPlayed = false;
            VoiceAudioSource.Stop();
            SoundEffectAudioSource.Stop();

            if (ActiveIndex > 0)
            {
                // reset out text length so that typed text can type itself
                //  TextLength = 0;
                ActiveIndex -= 1;
            }

            if (typing)
            {
                StopCoroutine(TypingCoroutine);
                typing = false;
                TempText = "";
                DisplayedText.text = "";

            }

            ResetConditions();
        }

        /// <summary>
        ///     This requires the public TargetRoute value to be set
        /// </summary>
        public void ContinueOnRoute()
        {
            RouteNodeData route = sceneData.ActiveCharacterDialogueSet[ActiveIndex] as RouteNodeData;
            route.RuntimeRouteID = TargetRoute;
            CachedRoute = route;
            ResetConditions();
        }

        /// <summary>
        /// </summary>
        /// <param name="routeID"></param>
        public void GoToRoute(int routeID)
        {
            RouteNodeData route = sceneData.ActiveCharacterDialogueSet[ActiveIndex] as RouteNodeData;
            route.RuntimeRouteID = routeID;
            CachedRoute = route;
            ResetConditions();
        }

        /// <summary>
        /// </summary>
        /// <param name="conditionIndex"></param>
        public void TurnOnCondition(int conditionIndex)
        {
            TargetReflectedData.Conditions[conditionIndex].Disabled = false;
        }

        public void TurnOffCondition(int conditionIndex)
        {
            TargetReflectedData.Conditions[conditionIndex].Disabled = true;
        }

        public void CancelCondition(int conditionIndex)
        {
            TargetReflectedData.Conditions[conditionIndex]
                .StopCoroutine(TargetReflectedData.Conditions[conditionIndex].coroutine);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ResetConditions()
        {
            // make sure to set invoked back to false to false so that the events can be invoked again if we move back to it

            foreach (var condition in TargetReflectedData.Conditions)
                condition.Invoked = false;

            foreach (var generalCondition in TargetReflectedData.character.GeneralReflectedData.Conditions)
                generalCondition.Invoked = false;


            // TargetReflectedData.Conditions.All(inv => inv.Invoked = false);
        }

        #region variables

        //  public UnityEvent m_MyEvent;


        /// <summary>
        /// </summary>
        [HideInInspector]
        [FormerlySerializedAs("dialogueData")]
        public SceneData sceneData;

        /// <summary>
        /// the value here is set to -1 to start , so that we can select a scene value which will then pass its scene id to this scene id value which will then be serialized
        /// </summary>
        // [HideInInspector]
        // public int SceneID = -1;

        /// <summary>
        /// </summary>
        [HideInInspector]
        public delegate void RefreshCharacterDialogue();

        /// <summary>
        ///     used to re-generate the ActiveCharacterDialogueSet
        /// </summary>
        [HideInInspector] public static RefreshCharacterDialogue doRefresh;

        /// <summary>
        ///     static int flag used to check how many charactrs are speaking at once, if the number inceases the aUI for dialogue
        ///     will be added
        /// </summary>
        [HideInInspector] public static int ActiveDialogues = 0;


        /// <summary>
        ///     this is the value of the data in ActiveNodeData that is currently being processed
        /// </summary>
        [HideInInspector]
        public int ActiveIndex;

        /// <summary>
        /// </summary>
        private int CachedActiveIndex = -1;

        //   public List<UnityEvent> EventTrigger = new List<UnityEvent>();

        // a special set of data with an ID value matching that of each NodeData in the FullCharacterDataSet
        [HideInInspector] public List<CharacterNodeData> Characters = new List<CharacterNodeData>();

        /// <summary>
        /// </summary>
        [HideInInspector] public List<ReflectedData> ReflectedDataSet = new List<ReflectedData>();

        /// <summary>
        /// </summary>
        [HideInInspector] public List<ReflectedData> TempReflectedDataSet = new List<ReflectedData>();

        /// <summary>
        ///     Gameobject that holds all the reflected data as child objects (do not delete)
        /// </summary>
        [HideInInspector] public GameObject ReflectedDataParent;

        /// <summary>
        ///     The currently used reflected data
        /// </summary>
        [HideInInspector] public ReflectedData TargetReflectedData;

        /// <summary>
        /// </summary>
        [HideInInspector] public ReflectedData GeneralReflectedData;

        [HideInInspector] public GameObject GeneralReflectedDataParent;

        [HideInInspector] public ReflectedData TempGeneralReflectedDataSet;

        /// <summary>
        /// </summary>
        [HideInInspector] [NonSerialized] public RouteNodeData CachedRoute;

        /// <summary>
        /// </summary>
        [NonSerialized] public NodeData ActiveNodeData;


        [HideInInspector, Obsolete("Please use NameText ")] public Text NameUI;

        [HideInInspector] public TextMeshProUGUI NameText;

        [HideInInspector, Obsolete("Please use DisplayedText ")] public Text DisplayedTextUI;

        [HideInInspector] public TextMeshProUGUI DisplayedText;

        [HideInInspector]
        [FormerlySerializedAs("moveNextButton")]
        public Button MoveNextButton;

        [HideInInspector]
        [FormerlySerializedAs("movePreviousButton")]
        public Button MovePreviousButton;

        [HideInInspector] public Button RouteButton;

        [HideInInspector] public List<Button> RouteButtons = new List<Button>();

        [HideInInspector] public GameObject RouteParent;

        [HideInInspector] public bool UseNameUI = true;

        [HideInInspector] public bool UseDialogueTextUI = true;

        [HideInInspector] public bool UseMoveNextButton = true;

        [HideInInspector] public bool UseMovePreviousButton = true;

        [HideInInspector] public bool UseRouteButton = true;


        /// <summary>
        /// </summary>
        [HideInInspector] public CharacterTextDisplayMode textDisplayMode = CharacterTextDisplayMode.Instant;


        #region typing stuff

        /// <summary>
        /// </summary>
        [HideInInspector] public float TypingSpeed = 0.05f;

        private string TempText;
        private string modifiedTempText;
        private Coroutine TypingCoroutine;
        /// <summary>
        /// </summary>
        [HideInInspector] public float Timer;

        [HideInInspector] public float startDelay = 0.5f;
        [HideInInspector] public float VolumeVariation = 0.1f;

        [HideInInspector] public AudioSource TypingAudioSource;

        private bool typing;


        [HideInInspector] public AudioClip TypingAudioCip;

        #endregion

        [HideInInspector] public AudioSource VoiceAudioSource;

        [HideInInspector] public AudioSource SoundEffectAudioSource;

        [HideInInspector] public List<CharacterNodeData> CommunicatingCharacters = new List<CharacterNodeData>();

        [HideInInspector] public List<GameObject> CommunicatingCharacterGameobject = new List<GameObject>();

        private int communicatingCharacterCount;

        [HideInInspector] public CharacterNodeData self;

        [HideInInspector] public int targetChararacterIndex;

        [HideInInspector] public string CachedUID = "";

        [HideInInspector] public int TargetRoute;

        [HideInInspector] public int MatchingRouteNumber;


        [HideInInspector] public bool UsesText = true;

        [HideInInspector] public bool UsesStoryboardImages;

        [HideInInspector] public bool UsesVoiceOver = true;

        [HideInInspector] public bool AutoStartVoiceClip;

        [HideInInspector] public bool UsesSoundffects = true;

        [HideInInspector] public bool AutoStartSoundEffectClip;

        [HideInInspector] public List<KeywordFilter> KeywordFilters = new List<KeywordFilter>();

        [HideInInspector] public bool UseKeywordFilters;

        [HideInInspector] public int FilterCount;

        [HideInInspector] public bool ShowGeneralSettings;

        [HideInInspector] public bool ShowKeywordFilterFouldout;

        [HideInInspector] public bool ShowNodeSpecificSettings = true;

        [HideInInspector] public bool SetupRouteButtons;

        [HideInInspector] public string LastSelectedUID = "";

        [HideInInspector] public bool ShowUIAndTextSettings;

        [HideInInspector] public bool ShowTextDisplayModeSettings;

        [HideInInspector] public bool ShowGeneralConditionsSettings = true;

        [HideInInspector] public bool ShowNodeSpecificConditionSettings = true;

        //  private bool HasReturnPoint = false;
        [NonSerialized] public string ReturnPointUID = "";

        #endregion

        /// <summary>
        ///     delegate that we will use to create a delegate method
        /// </summary>
        /// <returns></returns>
        private delegate string DelA();

        private delegate string DelB();

        [HideInInspector] public Color NameColour = Color.white;
        [HideInInspector] public Color TextColour = Color.white;



        private string ActiveUID = "";
        [HideInInspector]
        public bool AutoStartAllConditionsByDefault;
        [HideInInspector]
        public string UpdateUID = "";
    }
}