using UnityEngine;
using UnityEngine.Events;

namespace DaiMangou.BridgedData
{
    [DisallowMultipleComponent]
    public class PlayerTrigger : MonoBehaviour
    {
        public Character PlayerCharacterComponent;
        public UnityEvent OnEnterEvent = new UnityEvent();
        public UnityEvent OnExitEvent = new UnityEvent();

        public string TargetTag = "";

        public void Reset()
        {
            if (PlayerCharacterComponent != null) return;
            if (gameObject.GetComponent<Character>() != null)
                PlayerCharacterComponent = gameObject.GetComponent<Character>();
        }

        public void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(TargetTag)) return;
            var npcTrigger = other.GetComponent<NPCTrigger>();
            var npcCharacter = other.transform.parent.GetComponent<Character>();

            if (npcTrigger.added) return;
            //  var PlayerCharacterComponent = gameObject.GetComponent<Character>();
            PlayerCharacterComponent.CommunicatingCharacters.Add(npcTrigger.character.self);
            PlayerCharacterComponent.CommunicatingCharacterGameobject.Add(npcTrigger.character.gameObject);
            PlayerCharacterComponent.TargetRoute = npcCharacter.MatchingRouteNumber;

            npcTrigger.added = true;
            PlayerCharacterComponent.GenerateActiveDialogueSet();

            npcTrigger.OnEnterEvent.Invoke();
            OnEnterEvent.Invoke();
        }

        public void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(TargetTag)) return;
            var npcTrigger = other.GetComponent<NPCTrigger>();
            if (!npcTrigger.added) return;
            PlayerCharacterComponent.ResetConditions();
            // var PlayerCharacterComponent = gameObject.GetComponent<Character>();
            PlayerCharacterComponent.CommunicatingCharacters.Remove(other.GetComponent<NPCTrigger>().character
                .self);
            PlayerCharacterComponent.CommunicatingCharacterGameobject.Remove(other.GetComponent<NPCTrigger>()
                .character.gameObject);
            npcTrigger.added = false;
            PlayerCharacterComponent.CleanUp();

            npcTrigger.OnExitEvent.Invoke();
            OnExitEvent.Invoke();
        }
    }
}