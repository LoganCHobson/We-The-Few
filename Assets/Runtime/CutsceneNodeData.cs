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
    public List<string> listenerGuids = new List<string>();
    public List<string> methodNames = new List<string>();
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
