using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class UnityEventNode : CutsceneNode
{
    [SerializeField]
    public UnityEvent unityEvent = new UnityEvent();
    public string eventName;
    public List<ListenerClass> listenerNames = new List<ListenerClass>();
}

[SerializeField]
public class UnityEventNodeWrapper : ScriptableObject
{
    public UnityEvent unityEvent;
}
