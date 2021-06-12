using UnityEngine;
using UnityEngine.Events;

namespace DaiMangou.BridgedData
{
    [DisallowMultipleComponent]
    public class NPCTrigger : MonoBehaviour
    {
        [HideInInspector]
        public bool added;

        public bool InteractingwithPlayer
        {
            get
            {
                return added;
            }
        }

        public Character character;
        public UnityEvent OnEnterEvent = new UnityEvent();
        public UnityEvent OnExitEvent = new UnityEvent();
    }
}