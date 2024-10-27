using UnityEngine;
using UnityEngine.Events;


public class UnityEventNode : CutsceneNode
{
    [SerializeField]
    public UnityEvent unityEvent = new UnityEvent();
    public string eventName;
}

public class UnityEventNodeWrapper : ScriptableObject
{
    public UnityEvent unityEvent = new UnityEvent();
}
