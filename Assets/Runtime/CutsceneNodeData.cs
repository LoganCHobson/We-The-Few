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

}

[Serializable]
public class DialogueNodeData : CutsceneNodeData
{
    public string dialogText;
}

[Serializable]
public class CameraNodeData : CutsceneNodeData
{
    public Camera camera;
    public GameObject focus;
    public float cameraZoomLevel; 
}

[Serializable]
public class UnityEventNodeData : CutsceneNodeData
{
    public UnityEvent unityEvent;
    public string eventName; 
}

public enum NodeType
{
    Dialogue,
    Camera,
    UnityEvent,
}
