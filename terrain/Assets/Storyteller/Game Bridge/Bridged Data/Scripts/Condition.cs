using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace DaiMangou.BridgedData
{


    public enum TimeUseMethod
    {

        RealtimeDelay = 0,
        Delay = 1,
        Duration,
        Custom

    }
    /// <summary>
    ///     the condition system allows foe any  accessible function to be called once a condition is met
    /// </summary>
    [Serializable]
    public class Condition : MonoBehaviour
    {
        /// <summary>
        ///     a flag to check if the target ent is to be invoked automatically onve a condition is met or if the nodedata is
        ///     being processed
        /// </summary>
        public bool AutoStart;

        /// <summary>
        ///     helps to determine the state of the Condition editor (do not edit)
        /// </summary>
        public GameObject cachedTargetObject;

        /// <summary>
        ///     This is an array of all the public methods of a Component
        /// </summary>
        public MethodInfo[] cacheMethods = new MethodInfo[0];

        /// <summary>
        ///     this is the index value of the component in the Components array
        /// </summary>
        public int ComponentIndex;

        /// <summary>
        ///     this is an array of all the components on a TargetGameObject
        /// </summary>
        public Component[] Components = new Component[0];

        /// <summary>
        /// </summary>
        private bool ConditionTimerStarted;

        public bool Disabled;

        /// <summary>
        ///     flag to check i the target event is invoked
        /// </summary>
        // [NonSerialized]
        public bool Invoked;

        /// <summary>
        ///     this is the index value of the target Method in the serializedMethods array
        /// </summary>
        public int MethodIndex;

        /// <summary>
        ///     once this bool is equal to a bool value you decide upon the condition system will be activated
        /// </summary>
        public bool ObjectiveBool;

        /// <summary>
        ///     This is the gameobject which this Condition Component is atached to
        /// </summary>
        public GameObject Self;

        /// <summary>
        ///     here we use a speial class which allows us to Serialize methodInfo
        /// </summary>
        public SerializableMethodInfo[] serializedMethods = new SerializableMethodInfo[0];

        /// <summary>
        ///     this unity event only act as a proxy for an unity event in the ReflectedData
        /// </summary>
        public UnityEvent targetEvent = new UnityEvent();

        /// <summary>
        ///     This is the gameobject whose mono scrits we wish to analuze for public methods
        /// </summary>
        public GameObject TargetGameObject;

        /// <summary>
        ///     he delegate method
        /// </summary>
        private Del theDelegate;

        public TimeUseMethod timeUseMethod = TimeUseMethod.RealtimeDelay;

        // public bool PlaySoundEffect;
        //  public bool PlayVoiceClip;
        /// <summary>
        ///     if the node data uses time then the
        /// </summary>
        public bool UseTime;

        public float CustomWaitTime = 0;

       // public IEnumerator ConditionTimerCoroutine;
        public Coroutine coroutine;

       // public void Awake()
      //  {
      //  }

      //  public void Start()
      //  {
         //   ConditionTimerCoroutine = ConditionTimer();
           
        //}

        /// <summary>
        /// </summary>
        public void GetGameObjectComponents()
        {
            Components = TargetGameObject.GetComponents(typeof(MonoBehaviour)); // Component
        }

        /// <summary>
        /// </summary>
        /// <param name="index"></param>
        public void SetComponent(object index)
        {
            ComponentIndex = (int)index;
        }

        /// <summary>
        /// </summary>
        public void GetComponentMethods()
        {
            var theNamespace = Components[ComponentIndex].GetType().Namespace == ""
                ? ""
                : Components[ComponentIndex].GetType().Namespace + ".";

            //  Debug.Log(Type.GetType(Components[ComponentIndex].GetType().Name));
            cacheMethods = Type.GetType(theNamespace + Components[ComponentIndex].GetType().Name, false)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            var boolMethods = new List<MethodInfo>();
            foreach (var b in cacheMethods)
                if (b.ReturnType == typeof(bool))
                    boolMethods.Add(b);

            cacheMethods = boolMethods.ToArray();

            serializedMethods = new SerializableMethodInfo[cacheMethods.Length];

            for (var i = 0; i < cacheMethods.Length; i++)
                serializedMethods[i] = new SerializableMethodInfo(cacheMethods[i]);
        }

        /// <summary>
        /// </summary>
        /// <param name="index"></param>
        public void SetMethod(object index)
        {
            MethodIndex = (int)index;
        }


        /// <summary>
        /// </summary>
        public void ProcessConditionData()
        {


            if (AutoStart)
                if (!Invoked)
                {
                    targetEvent.Invoke();
                    Invoked = true;
                    // in the next update we will let users set a invoke amount 
                }

            if (UseTime)
            {
                // setup the elapse timer and determine if we will delay by duration 
                if (!ConditionTimerStarted && !Invoked)
                {
                  
                  Invoked =  ConditionTimerStarted = true;
                    coroutine = StartCoroutine(ConditionTimer());
                }
            }

            if (TargetGameObject == null) return;
            var comp = Components[ComponentIndex];


            if (theDelegate == null)
                theDelegate =
                    (Del)Delegate.CreateDelegate(typeof(Del), comp, serializedMethods[MethodIndex].methodName);

            if (theDelegate() != ObjectiveBool) return;
            if (!Invoked)
                targetEvent.Invoke();
            Invoked = true;

        }


        /// <summary>
        /// </summary>
        /// <returns></returns>
        public IEnumerator ConditionTimer()
        {

            if (dialoguer)
            {
                //  var waitTime = UseDelay ? dialoguer.ActiveNodeData.Delay : dialoguer.ActiveNodeData.Duration;

                var waitTime = 0f;
                switch (timeUseMethod)
                {
                    case TimeUseMethod.RealtimeDelay:
                        waitTime = dialoguer.ActiveNodeData.Delay;
                        break;
                    case TimeUseMethod.Delay:
                        waitTime = dialoguer.ActiveNodeData.Delay;
                        break;
                    case TimeUseMethod.Duration:
                        waitTime = dialoguer.ActiveNodeData.Duration;
                        break;
                    case TimeUseMethod.Custom:
                        waitTime = CustomWaitTime;
                        break;
                }

                yield return new WaitForSeconds(waitTime);
                ConditionTimerStarted = false;
                targetEvent.Invoke();
              //  Invoked = false;
            }
            else
            {
                //    var waitTime = UseDelay ? character.ActiveNodeData.Delay : character.ActiveNodeData.Duration;
                var waitTime = 0f;
                switch (timeUseMethod)
                {
                    case TimeUseMethod.RealtimeDelay:
                        waitTime = character.ActiveNodeData.Delay;
                        break;
                    case TimeUseMethod.Delay:
                        waitTime = character.ActiveNodeData.Delay;
                        break;
                    case TimeUseMethod.Duration:
                        waitTime = character.ActiveNodeData.Duration;
                        break;
                    case TimeUseMethod.Custom:
                        waitTime = CustomWaitTime;
                        break;
                }
                yield return new WaitForSeconds(waitTime);
                ConditionTimerStarted = false;
                targetEvent.Invoke();
              //  Invoked = false;
            }
        }

        /// <summary>
        ///     delegate that we will use to create a delegate method
        /// </summary>
        /// <returns></returns>
        private delegate bool Del();

        #region Dialoguer Specific

        /// <summary>
        ///     If the refelected data is generated under a gameboject with a Dialoguer Component then the gameobjects is set here
        /// </summary>
        public GameObject DialoguerGameObject;

        /// <summary>
        ///     value is set by get set
        /// </summary>
        public Dialoguer dialoguer;

        /// <summary>
        ///     If the refelected data is generated under a gameboject with a Dialoguer then the dialogue scriptreference is set
        ///     here
        /// </summary>
        public Dialoguer dialoguerComponent
        {
            get
            {
                if (dialoguer == null) dialoguer = DialoguerGameObject.GetComponent<Dialoguer>();

                return dialoguer;
            }
            set { dialoguer = value; }
        }

        #endregion

        #region Character Specific 

        /// <summary>
        ///     If the refelected data is generated under a gameboject with a Character Component then the gameobjects is set here
        /// </summary>
        public GameObject CharacterGameObject;

        /// <summary>
        ///     value is set by get set
        /// </summary>
        public Character character;

        /// <summary>
        ///     If the refelected data is generated under a gameboject with a Character Component then the dialogue scriptreference
        ///     is set here
        /// </summary>
        public Character characterComponent
        {
            get
            {
                if (character == null) character = CharacterGameObject.GetComponent<Character>();

                return character;
            }
            set { character = value; }
        }

        #endregion
    }
}