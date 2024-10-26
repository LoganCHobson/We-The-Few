using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

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
            container.cutsceneNodeData.Add(new CutsceneNodeData
            {
                nodeName = node.name,
                guid = node.guid,
                position = node.GetPosition().position,
            });
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

    private void CreateNodes() //Reminder to go back and fix this to work with all cutscene nodes.
    {
        foreach (CutsceneNodeData node in containerCache.cutsceneNodeData)
        {
            DialogueNode tempNode = targetGraphView.CreateDialogueNode(node.nodeName, Vector2.zero);
            tempNode.guid = node.guid;
            targetGraphView.AddElement(tempNode);

            var nodePorts = containerCache.nodeLinks.Where(node1 => node1.baseNodeGuid == node.guid).ToList();
            nodePorts.ForEach(port => targetGraphView.AddChoicePort(tempNode, port.portName));
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
}
