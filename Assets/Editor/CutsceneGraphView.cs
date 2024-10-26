using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class CutsceneGraphView : GraphView
{

    public readonly Vector2 defaultNodeSize = new Vector2(150, 200);
    public CutsceneGraphView()
    {
        styleSheets.Add(Resources.Load<StyleSheet>("CutsceneGraphDesign"));
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        GridBackground grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();

        AddElement(GenerateEntryPointNode());
    }

    public override List<Port> GetCompatiblePorts(Port _startPort, NodeAdapter _adapter)
    {
        var compatiblePorts = new List<Port>();

        ports.ForEach(port =>
        {
            if (_startPort != port && _startPort.node != port.node)
            {
                compatiblePorts.Add(port);
            }
        });

        return compatiblePorts;
    }

    private CutsceneNode GenerateEntryPointNode()
    {
        CutsceneNode node = new CutsceneNode()
        {
            title = "Start",
            guid = Guid.NewGuid().ToString(),

            entryPoint = true,

        };

        Port port = AddPort(node, Direction.Output);
        port.portName = "Next";
        node.outputContainer.Add(port);

        node.RefreshExpandedState();
        node.RefreshPorts(); //These need to be added when we change the containers.

        node.SetPosition(new Rect(100, 200, 100, 150));
        return node;
    }

    private Port AddPort(CutsceneNode _node, Direction _portDir, Port.Capacity _capacity = Port.Capacity.Single)
    {
        return _node.InstantiatePort(Orientation.Horizontal, _portDir, _capacity, typeof(float));
    }
    private void RemovePort(CutsceneNode node, Port port)
    {
        var targetEdge = edges.ToList().Where(edge => edge.output.portName == port.portName && edge.output.node == port.node);
        if (targetEdge.Any())
        {
            Edge edge = targetEdge.First();
            edge.input.Disconnect(edge);
            RemoveElement(targetEdge.First());
        }

        node.outputContainer.Remove(port);
        node.RefreshPorts();
        node.RefreshExpandedState();
        Debug.Log("Deleted a port");
    }

    //A bunch of logic regaridng how specific nodes are made.
    #region DialogueNode 
    public void CreateNode(string _nodeName)
    {
        AddElement(CreateDialogueNode(_nodeName));
    }

    public DialogueNode CreateDialogueNode(string _nodeName)
    {
        var dialogueNode = new DialogueNode()
        {
            name = _nodeName,
            title = _nodeName,
            dialogText = _nodeName,
            guid = Guid.NewGuid().ToString(),

        };

        Port imputPort = AddPort(dialogueNode, Direction.Input, Port.Capacity.Multi);
        imputPort.portName = "Input";
        dialogueNode.inputContainer.Add(imputPort);

        var button = new Button(() => { AddChoicePort(dialogueNode); });
        dialogueNode.titleContainer.Add(button);
        button.text = "New Choice";

        dialogueNode.RefreshExpandedState();
        dialogueNode.RefreshPorts();
        dialogueNode.SetPosition(new Rect(Vector2.zero, defaultNodeSize));

        return dialogueNode;
    }



    public void AddChoicePort(DialogueNode _dialogueNode, string overridenPortName = "")
    {
        Port port = AddPort(_dialogueNode, Direction.Output);

        Label oldLabel = port.contentContainer.Q<Label>("type"); //Querys a ui elemnt for something apparently? Wild.
        port.contentContainer.Remove(oldLabel);

        int outputPortCount = _dialogueNode.outputContainer.Query("connector").ToList().Count;

        string choicePortName = string.IsNullOrEmpty(overridenPortName) ? $"Choice {outputPortCount + 1}" : overridenPortName;

        TextField textField = new TextField
        {
            name = string.Empty,
            value = choicePortName

        };
        textField.RegisterValueChangedCallback(evt => port.portName = evt.newValue);
        port.contentContainer.Add(new Label("  "));
        port.contentContainer.Add(textField);
        Button deleteButton = new Button(() => RemovePort(_dialogueNode, port))
        {
            text = "x"
        };

        port.contentContainer.Add(deleteButton);



        port.portName = choicePortName;
        _dialogueNode.outputContainer.Add(port);
        _dialogueNode.RefreshPorts();
        _dialogueNode.RefreshExpandedState();
    }


    #endregion

}
