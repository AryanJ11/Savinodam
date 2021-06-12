using System.Collections.Generic;
using DaiMangou.BridgedData;
using DaiMangou.Storyteller;
using UnityEditor;
using UnityEngine;

namespace DaiMangou.GameBridgeEditor
{
    [InitializeOnLoad]
    internal class HierarchyIcon
    {
        private static List<int> markedObjects;

        static HierarchyIcon()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCB;
        }

        private static void HierarchyItemCB(int instanceID, Rect rect)
        {
            var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

            if (go != null && go.GetComponent<Dialoguer>())
                GUI.DrawTexture(rect.ToCenterLeft(16, 16, -30), ImageLibrary.chatIcon);

            if (go != null && go.GetComponent<Character>())
                GUI.DrawTexture(rect.ToCenterLeft(16, 16, -30), ImageLibrary.CharacterIcon);
        }
    }
}