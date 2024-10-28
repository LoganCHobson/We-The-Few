using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class UnityEventUtility : MonoBehaviour
{
    public static List<ListenerInfo> GetListenerInfos(UnityEvent unityEvent)
    {
        List<ListenerInfo> listenerInfos = new List<ListenerInfo>();

        var invocationListField = typeof(UnityEventBase)
            .GetField("m_PersistentCalls", BindingFlags.NonPublic | BindingFlags.Instance);

        var persistentCalls = invocationListField.GetValue(unityEvent);
        var callsField = persistentCalls.GetType().GetField("m_Calls", BindingFlags.NonPublic | BindingFlags.Instance);
        var callsList = (IList)callsField.GetValue(persistentCalls);

        foreach (var call in callsList)
        {
            var methodField = call.GetType().GetField("m_MethodName", BindingFlags.NonPublic | BindingFlags.Instance);
            var targetField = call.GetType().GetField("m_Target", BindingFlags.NonPublic | BindingFlags.Instance);

            string methodName = (string)methodField.GetValue(call);
            UnityEngine.Object targetObject = (UnityEngine.Object)targetField.GetValue(call);

            listenerInfos.Add(new ListenerInfo(targetObject, methodName));
        }

        return listenerInfos;
    }
}

[Serializable]
public class ListenerInfo
{
    public UnityEngine.Object Target { get; }
    public string MethodName { get; }

    public ListenerInfo(UnityEngine.Object target, string methodName)
    {
        Target = target;
        MethodName = methodName;
    }
}
