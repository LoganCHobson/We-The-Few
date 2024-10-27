using System;
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



    public string cameraName;
    public string focusName;
    public float cameraZoomLevel;



    public UnityEvent unityEvent;
    public string eventName;

}

public enum NodeType
{
    Dialogue,
    Camera,
    UnityEvent,
}
