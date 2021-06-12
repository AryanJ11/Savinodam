using UnityEngine;
using UnityEngine.Serialization;

namespace DaiMangou.BridgedData
{
    public class ClickListener : MonoBehaviour
    {
        public Character characterComponent;

        public Dialoguer dialoguerComponent;

        public int indexInList;

        public void SwitchRoute()
        {
            if (dialoguerComponent)
            {
                var route =
                    (RouteNodeData) dialoguerComponent.sceneData.ActiveCharacterDialogueSet[
                        dialoguerComponent.ActiveIndex];
                dialoguerComponent.ReturnPointUID = route.DataIconnectedTo[indexInList].UID;
                route.RuntimeRouteID = indexInList;
                dialoguerComponent.CachedRoute = route;
            }

            if (characterComponent)
            {
                var route =
                    (RouteNodeData) characterComponent.sceneData.ActiveCharacterDialogueSet[
                        characterComponent.ActiveIndex];
                characterComponent.ReturnPointUID = route.DataIconnectedTo[indexInList].UID;
                route.RuntimeRouteID = indexInList;
                characterComponent.CachedRoute = route;
            }
        }
    }
}