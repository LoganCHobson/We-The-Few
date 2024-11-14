using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class CutsceneNodeData
{
    public NodeType type;
    public string nodeName;
    public string guid;
    public Vector2 position;




    public string dialogText;



    public string cameraGuid;
    public string focusGuid;
    public float cameraZoomLevel;



    [SerializeField] public UnityEvent unityEvent = new UnityEvent();
    public List<ListenerData> listeners;
    public string eventName;


    public float delay;
}

public enum NodeType
{
    Dialogue,
    Camera,
    UnityEvent,
    Delay,
}
[System.Serializable]
public class ListenerData
{
    public string GUID;
    public string methodName;
    public Type parameterType;

    public ListenerData(string guid, string methodName, Type parameterType)
    {
        this.GUID = guid;
        this.methodName = methodName;
        this.parameterType = parameterType;
    }
}

