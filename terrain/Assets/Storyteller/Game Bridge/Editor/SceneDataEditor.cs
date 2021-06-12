using System.Collections.Generic;
using System.IO;
using System.Linq;
using DaiMangou.BridgedData;
using DaiMangou.Storyteller;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DaiMangou.GameBridgeEditor
{
    [CustomEditor(typeof(SceneData))]
    [CanEditMultipleObjects]
    public class SceneDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }

}