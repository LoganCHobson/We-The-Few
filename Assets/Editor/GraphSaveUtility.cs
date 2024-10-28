using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using static CutsceneNodeData;

public class GraphSaveUtility
{
    private CutsceneGraphView targetGraphView;
    private CutsceneNodeContainer containerCache;
    private List<Edge> edges => targetGraphView.edges.ToList();
    private List<CutsceneNode> nodes => targetGraphView.nodes.ToList().Cast<CutsceneNode>().ToList();
    public static GraphSaveUtility GetInstance(CutsceneGraphView _targetGraphView)
    {
        return new GraphSaveUtility
        {
            targetGraphView = _targetGraphView
        };
    }

    public void SaveGraph(string _fileName)
    {
        CutsceneNodeContainer container = ScriptableObject.CreateInstance<CutsceneNodeContainer>();
        if (!SaveNodes(container))
        {
            return;
        }

        SaveExposedProperties(container);


        //Auto creation of folder if it dont exist.
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        AssetDatabase.CreateAsset(container, $"Assets/Resources/{_fileName}.asset");
        AssetDatabase.SaveAssets();
    }

    private void SaveExposedProperties(CutsceneNodeContainer container)
    {
        container.exposedProperties.AddRange(targetGraphView.exposedProperties);
    }

    private bool SaveNodes(CutsceneNodeContainer container)
    {
        if (!edges.Any()) return false; //If no connections, dont bother saving.


        Edge[] connectedPorts = edges.Where(edge => edge.input.node != null).ToArray();

        for (int i = 0; i < connectedPorts.Length; i++)
        {
            CutsceneNode outputNode = connectedPorts[i].output.node as CutsceneNode;
            CutsceneNode inputNode = connectedPorts[i].input.node as CutsceneNode;

            container.nodeLinks.Add(new NodeLinkData
            {
                baseNodeGuid = outputNode.guid,
                portName = connectedPorts[i].output.portName,
                targetNodeGuid = inputNode.guid,
            });
        }

        foreach (CutsceneNode node in nodes.Where(node => !node.entryPoint))
        {
            CutsceneNodeData nodeData = node.type switch
            {
                NodeType.Dialogue => new CutsceneNodeData
                {
                    type = node.type,
                    nodeName = node.name,
                    guid = node.guid,
                    position = node.GetPosition().position,
                    dialogText = (node as DialogueNode).dialogText 
                }
                ,
                NodeType.Camera => new CutsceneNodeData
                {
                    type = node.type,
                    nodeName = node.name,
                    guid = node.guid,
                    position = node.GetPosition().position,
                    cameraZoomLevel = (node as CameraNode).cameraZoomLevel,
                    cameraName = (node as CameraNode).camera?.name, // Save only the name
                    focusName = (node as CameraNode).focus.name,
                },

                NodeType.UnityEvent => new CutsceneNodeData
                {
                    type = node.type,
                    nodeName = node.name,
                    guid = node.guid,
                    position = node.GetPosition().position,
                    eventName = (node as UnityEventNode).eventName,
                    unityEvent = (node as UnityEventNode).unityEvent,
                    listenerNames = (node as UnityEventNode).listenerNames
                    


                },
                _ => null
            };

            container.cutsceneNodeData.Add(nodeData);


        }

        return true;
    }

    public void LoadGraph(string _fileName)
    {
        containerCache = Resources.Load<CutsceneNodeContainer>(_fileName);
        if (containerCache == null)
        {
            EditorUtility.DisplayDialog("File Not Found", "Target cutscene graph file does not exist!", "OK");
            return;
        }

        ClearGraph();
        CreateNodes();
        ConnectNodes();
        CreateExposedProperties();
    }

    private void CreateExposedProperties()
    {
        targetGraphView.ClearBlackboardProperties();

        foreach (ExposedProperty property in containerCache.exposedProperties)
        {
            targetGraphView.AddPropertyToBlackboard(property);
        }
    }

    private void ConnectNodes()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            var connections = containerCache.nodeLinks.Where(node => node.baseNodeGuid == nodes[i].guid).ToList();
            for (int j = 0; j < connections.Count; j++)
            {
                string targetNodeGuid = connections[j].targetNodeGuid;
                CutsceneNode targetNode = nodes.First(node => node.guid == targetNodeGuid);
                LinkNodes(nodes[i].outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);

                targetNode.SetPosition(new Rect(containerCache.cutsceneNodeData.First(node => node.guid == targetNodeGuid).position, targetGraphView.defaultNodeSize));

            }
        }
    }

    private void LinkNodes(Port _output, Port _input)
    {
        Edge tempEdge = new Edge
        {
            output = _output,
            input = _input
        };

        tempEdge?.input.Connect(tempEdge);
        tempEdge?.output.Connect(tempEdge);
        targetGraphView.Add(tempEdge);
    }

    private void CreateNodes()
    {
        foreach (CutsceneNodeData nodeData in containerCache.cutsceneNodeData)
        {
            CutsceneNode tempNode = nodeData.type switch
            {
                NodeType.Dialogue => targetGraphView.CreateDialogueNode(nodeData.nodeName, Vector2.zero, nodeData.dialogText),
                NodeType.Camera => targetGraphView.CreateCameraNode(nodeData.nodeName, Vector2.zero, GameObject.Find(nodeData.cameraName).GetComponent<Camera>(), GameObject.Find(nodeData.focusName)),
                NodeType.UnityEvent => targetGraphView.CreateUnityEventNode(nodeData.nodeName, Vector2.zero, nodeData.unityEvent),
                _ => null
            };

            if (tempNode != null)
            {
                tempNode.guid = nodeData.guid;
                targetGraphView.AddElement(tempNode);

                if (nodeData.type == NodeType.Dialogue)
                {
                    var nodePorts = containerCache.nodeLinks
                                    .Where(link => link.baseNodeGuid == nodeData.guid)
                                    .ToList();

                    nodePorts.ForEach(port => targetGraphView.AddChoicePort((DialogueNode)tempNode, port.portName));
                }
            }
        }
    }

    private void ClearGraph()
    {
        //Set entry points guid back from the save. Discard existing guid. Entry point is always there, gotta clean it.
        nodes.Find(node => node.entryPoint).guid = containerCache.nodeLinks[0].baseNodeGuid;

        foreach (CutsceneNode node in nodes)
        {
            if (node.entryPoint)
            {
                continue;
            }

            //Remove edges connected to this node
            edges.Where(edge => edge.input.node == node).ToList().ForEach(edge => targetGraphView.RemoveElement(edge));

            //Then remove the node.
            targetGraphView.RemoveElement(node);
        }
    }
    //This is to fix the issue where we loose the target of the Unity events. Unfortunatly Unity is insanely dumb and we gotta do it this way.
    public UnityEvent ListenerFixer(List<ListenerClass> listenerNames, UnityEventNode unityEventNode)
    {
        List<ListenerInfo> listeners = UnityEventUtility.GetListenerInfos(unityEventNode.unityEvent); //Get all listeners on the event

        unityEventNode.unityEvent.RemoveAllListeners(); //Then get rid of em
        unityEventNode.listenerNames.Clear();
        foreach (ListenerInfo listenerInfo in listeners)
        {
            MethodInfo methodInfo = listenerInfo.Target?.GetType().GetMethod(listenerInfo.MethodName); //Get method info

            
            if (listenerInfo.Target == null && methodInfo != null) //If we are missing the target, find it one.
            {
                foreach (ListenerClass name in listenerNames)
                {
                    GameObject targetObject = GameObject.Find(name.targetName);
                    if (targetObject != null)
                    {
                        
                        UnityAction action = (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), targetObject, methodInfo);
                        unityEventNode.unityEvent.AddListener(action);
                    }
                }
            }
            else if (methodInfo != null) 
            {
                UnityAction action = (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), listenerInfo.Target, methodInfo);
                unityEventNode.unityEvent.AddListener(action);
            }
        }

        return unityEventNode.unityEvent;
    }

}