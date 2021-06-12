using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Reflection;

namespace DaiMangou.BridgedData
{
    [Serializable]
    public class KeywordFilter
    {
        public string KeyWord = "[*]";
        public string ReplacementString = "";

        public bool StaticKeywordMethod = true;
        public bool StaticReplacementStringMethod = true;

        public DynamicMethod DynamicKeyword = new DynamicMethod();
        public DynamicMethod DynamicReplacementString = new DynamicMethod();
        public bool Disabled;
        public Color NewColour = Color.white;
        /// <summary>
        /// represents the current active nodes UID
        /// </summary>
       // [NonSerialized]
      //  public string CID="";
 
    }
}
