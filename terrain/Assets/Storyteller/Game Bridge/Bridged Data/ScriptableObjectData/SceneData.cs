using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace DaiMangou.BridgedData
{


    [Serializable]
    public class Language
    {
        public string Name;
        public string CountryCode;
        public bool Active;
        public bool Default;

        public Language()
        { }

        public Language(string languageName, string languageCountryCode, bool isActive, bool isdefault)
        {
            Name = languageName;
            CountryCode = languageCountryCode;
            Active = isActive;
            Default = isdefault;
        }
    }
    /// <summary>
    /// This is the heart an soul of data management for Game Bridge. your Storyteller Data is pushed to the SceneData nd and can be used in game
    /// </summary>
    [Serializable]
    public class SceneData : ScriptableObject
    {
        public string UID = "";

        public string UpdateUID = Guid.NewGuid().ToString();
        /// <summary>
        /// The index value representnting the scene for which this dataset was pushed from
        /// </summary>
        public int SceneID = -1;
        /// <summary>
        /// This is the entire raw dataset as push into the SceneData Asset
        /// </summary>
       // [HideInInspector]
        public List<NodeData> FullCharacterDialogueSet = new List<NodeData>();
        /// <summary>
        /// This is the current list of nodeData currently used in game at any one moment
        /// </summary>
       // [HideInInspector] 
        public List<NodeData> ActiveCharacterDialogueSet = new List<NodeData>();
        /// <summary>
        /// This is the list of characters existing in the FullCharacterDialogueSet
        /// </summary>
        public List<CharacterNodeData> Characters = new List<CharacterNodeData>();

        public List<Language> Languages = new List<Language> { new Language() };
        public int LanguageIndex = 0;
        public string  [] LanguageNameArray = new string[] {""};

        public void OnEnable()
        {
          //  ActiveCharacterDialogueSet = new List<NodeData>();
#if UNITY_EDITOR
            DaiMangou.Storyteller.IconManager.SetIcon(this, DaiMangou.Storyteller.IconManager.DaiMangouIcons.SceneIcon);

#endif
        }
    }
}
