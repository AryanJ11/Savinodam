using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace DaiMangou.BridgedData
{
    public static class ConditionCopy
    {

        static bool AutoStart;

        static GameObject cachedTargetObject;

        static MethodInfo[] cacheMethods = new MethodInfo[0];

        static int ComponentIndex;

        static Component[] Components = new Component[0];

        static bool Disabled;

        static bool Invoked;

        static int MethodIndex;

        static bool ObjectiveBool;

        static GameObject Self;

        static SerializableMethodInfo[] serializedMethods = new SerializableMethodInfo[0];

        public static UnityEvent TargetEvent = new UnityEvent();

        static GameObject TargetGameObject;

        static TimeUseMethod timeUseMethod = TimeUseMethod.RealtimeDelay;

        static bool UseTime;

        static bool EventsOnly;
        // Copy and past could have all been done i na single function but this is just the setup for testing 
        public static void MakeCopy(Condition condition, bool eventsOnly = false)
        {
            EventsOnly = eventsOnly;

            if (!EventsOnly)
            {
                AutoStart = condition.AutoStart;
                cachedTargetObject = condition.cachedTargetObject;
                ComponentIndex = condition.ComponentIndex;
                Disabled = condition.Disabled;
                Invoked = condition.Invoked;
                MethodIndex = condition.MethodIndex;
                ObjectiveBool = condition.ObjectiveBool;
                Self = condition.Self;
                TargetGameObject = condition.TargetGameObject;
                timeUseMethod = condition.timeUseMethod;
                UseTime = condition.UseTime;

                cacheMethods = new MethodInfo[condition.cacheMethods.Length];
                for (int i = 0; i < condition.cacheMethods.Length; i++)
                    cacheMethods[i] = condition.cacheMethods[i];

                Components = new Component[condition.Components.Length];
                for (int i = 0; i < condition.Components.Length; i++)
                    Components[i] = condition.Components[i];


                serializedMethods = new SerializableMethodInfo[condition.serializedMethods.Length];
                for (int i = 0; i < condition.serializedMethods.Length; i++)
                    serializedMethods[i] = condition.serializedMethods[i];
            }
            else
            {
              //  var e = Delegate.CreateDelegate(typeof(UnityAction), condition.targetEvent.GetPersistentTarget(0),
               //     condition.targetEvent.GetPersistentMethodName(0));

                
//targetEvent.AddListener(condition.targetEvent.GetPersistentTarget(condition.targetEvent.));
            }
          //  targetEvent = condition.targetEvent;




        }

        public static void Paste(Condition condition)
        {
            if (!EventsOnly)
            {
                condition.AutoStart = AutoStart;
                condition.cachedTargetObject = cachedTargetObject;
                condition.ComponentIndex = ComponentIndex;
                condition.Disabled = Disabled;
                condition.Invoked = Invoked;
                condition.MethodIndex = MethodIndex;
                condition.ObjectiveBool = ObjectiveBool;
                condition.Self = Self;
                condition.TargetGameObject = TargetGameObject;
                condition.timeUseMethod = timeUseMethod;
                condition.UseTime = UseTime;

                condition.cacheMethods = new MethodInfo[cacheMethods.Length];
                for (int i = 0; i < cacheMethods.Length; i++)
                    condition.cacheMethods[i] = cacheMethods[i];

                condition.Components = new Component[Components.Length];
                for (int i = 0; i < Components.Length; i++)
                    condition.Components[i] = Components[i];


                condition.serializedMethods = new SerializableMethodInfo[serializedMethods.Length];
                for (int i = 0; i < serializedMethods.Length; i++)
                    condition.serializedMethods[i] = serializedMethods[i];
            }
          /*  else
            {

                condition.targetEvent = TargetEvent;
            }*/
           // 


        }
    }
    public static class BridgeData
    {
        /// <summary>
        ///  flag to check if any nodes are still processing data
        /// </summary>
        public static int ActiveEvents = -1;
        // public static int ActiveIndex;

    }
}