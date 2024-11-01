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



    public UnityEvent unityEvent;
    public string eventName;
    public List<ListenerClass> listenerGuids;
}
[Serializable]
public class ListenerClass
{
    public string targetGuid;
}


public enum NodeType
{
    Dialogue,
    Camera,
    UnityEvent,
}
