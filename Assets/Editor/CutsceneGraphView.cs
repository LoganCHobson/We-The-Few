using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class CutsceneGraphView : GraphView
{

    public readonly Vector2 defaultNodeSize = new Vector2(150, 200);

    private NodeSearchWindow searchWindow;
    public Blackboard blackboard;

    public List<ExposedProperty> exposedProperties = new List<ExposedProperty>();

    public CutsceneGraphView(EditorWindow editorWindow)
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
        AddSearchWindow(editorWindow);
    }

    private void AddSearchWindow(EditorWindow window)
    {
       searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
       searchWindow.Init(window, this);

       nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
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

        //node.capabilities &= ~Capabilities.Movable;
        node.capabilities &= ~Capabilities.Deletable; //We can use this syntax to bitwise our nodes and remove functionality.

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
    public void CreateNode(string _nodeName, Vector2 mousePosition)
    {
        AddElement(CreateDialogueNode(_nodeName, mousePosition));
    }

    public DialogueNode CreateDialogueNode(string _nodeName, Vector2 position)
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

        dialogueNode.styleSheets.Add(Resources.Load<StyleSheet>("DialogueColor"));

        Button button = new Button(() => { AddChoicePort(dialogueNode); });
        dialogueNode.titleContainer.Add(button);
        button.text = "New Choice";

        TextField textField = new TextField(string.Empty);
        textField.RegisterValueChangedCallback(evt =>
        {
            dialogueNode.dialogText = evt.newValue;
            //dialogueNode.title = evt.newValue;
        });
        //textField.SetValueWithoutNotify(dialogueNode.title);
        dialogueNode.mainContainer.Add(textField);

        dialogueNode.RefreshExpandedState();
        dialogueNode.RefreshPorts();
        dialogueNode.SetPosition(new Rect(position, defaultNodeSize));

        return dialogueNode;
    }



    public void AddChoicePort(DialogueNode _dialogueNode, string _overridenPortName = "")
    {
        Port port = AddPort(_dialogueNode, Direction.Output);

        Label oldLabel = port.contentContainer.Q<Label>("type"); //Querys a ui elemnt for something apparently? Wild.
        port.contentContainer.Remove(oldLabel);

        int outputPortCount = _dialogueNode.outputContainer.Query("connector").ToList().Count;

        string choicePortName = string.IsNullOrEmpty(_overridenPortName) ? $"Choice {outputPortCount + 1}" : _overridenPortName;

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

    public void ClearBlackboardProperties()
    {
        exposedProperties.Clear();
        blackboard.Clear();
    }
    public void AddPropertyToBlackboard(ExposedProperty _exposedProperty)
    {
        string localPropertyName = _exposedProperty.propertyName;
        string localPropertyValue = _exposedProperty.propertyValue;

        while(exposedProperties.Any(property => property.propertyName == localPropertyName))
        {
            localPropertyName = $"{localPropertyName}(1)";
        }


        ExposedProperty property = new ExposedProperty();
        property.propertyName = localPropertyName;
        property.propertyValue = localPropertyValue;

        exposedProperties.Add(property);

        VisualElement container = new VisualElement();
        BlackboardField blackboardField = new BlackboardField { text = property.propertyName, typeText = property.propertyValue };
        container.Add(blackboardField);

        TextField propertyValueTextField = new TextField("Value: ")
        { 
            value = localPropertyValue, 
        
        };
        propertyValueTextField.RegisterValueChangedCallback(evt =>
        {
            int index = exposedProperties.FindIndex(property => property.propertyName == _exposedProperty.propertyName);
            exposedProperties[index].propertyValue = evt.newValue;
        });
        container.Add(propertyValueTextField);
        BlackboardRow blackBoardValueRow = new BlackboardRow(blackboardField, propertyValueTextField);
        blackboard.Add(blackBoardValueRow);

        blackboard.Add(container);
    }


    #endregion

}
