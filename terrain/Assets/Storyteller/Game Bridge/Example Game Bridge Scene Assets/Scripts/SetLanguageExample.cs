using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DaiMangou.BridgedData
{
    public class SetLanguageExample : MonoBehaviour
    {
        public Dialoguer dialoguer;
        public Character character;

        public int index;
        public int languageIndex
        {
            get
            {
                return index;
            }
            set
            {
                index = value;
            }
        }

        void Update()
        {
            if(dialoguer!= null)
            {
                dialoguer.sceneData.LanguageIndex = languageIndex;
            }

            if(character!= null)
            {
                character.sceneData.LanguageIndex = languageIndex;
            }
        }
    }
}