using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CutsceneNodeContainer : ScriptableObject
{
    public List<NodeLinkData> nodeLinks = new List<NodeLinkData>();
    public List<CutsceneNodeData> cutsceneNodeData = new List<CutsceneNodeData>();
}
