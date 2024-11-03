using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;


public class UnityEventNode : CutsceneNode
{
    [SerializeField] public UnityEvent unityEvent = new UnityEvent();
    public List<string> listenerGuids = new List<string>();
    public List<string> methodNames = new List<string>();
    public string eventName;

    public void ExtractListeners()
    {
        var listeners = unityEvent.GetPersistentEventCount();
        for (int i = 0; i < listeners; i++)
        {
            listenerGuids.Add(unityEvent.GetPersistentTarget(i).GetComponent<GUIDComponent>().GUID);
            methodNames.Add(unityEvent.GetPersistentMethodName(i));
        }
    }

    public void RebuildListeners()
    {
        for (int i = 0; i < listenerGuids.Count; i++)
        {
            GameObject target = GameObject.FindObjectsOfType<GUIDComponent>().FirstOrDefault(g => g.GUID == listenerGuids[i])?.gameObject;
            if (target != null)
            {
                var method = target.GetComponent(target.GetType()).GetType().GetMethod(methodNames[i]);
                UnityAction action = (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), target, method);
                unityEvent.AddListener(action);
            }
        }
    }
}

public class UnityEventNodeWrapper : ScriptableObject
{
    [SerializeField] public UnityEvent unityEvent = new UnityEvent();
    public List<string> listenerGuids = new List<string>();

}


