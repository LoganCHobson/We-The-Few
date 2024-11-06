using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
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
    public void CreateNode(string _nodeName, Vector2 _mousePosition, NodeType type = NodeType.Dialogue)
    {
        switch (type)
        {
            case NodeType.Dialogue:
                AddElement(CreateDialogueNode(_nodeName, _mousePosition, ""));
                break;
            case NodeType.Camera:
                AddElement(CreateCameraNode(_nodeName, _mousePosition, null, null));
                break;
            case NodeType.UnityEvent:
                AddElement(CreateUnityEventNode(_nodeName, _mousePosition, null));
                break;
            case NodeType.Delay:
                AddElement(CreateDelayNode(_nodeName, _mousePosition, 0f));
                break;
            default:
                AddElement(CreateDialogueNode(_nodeName, _mousePosition, ""));
                break;
        }

    }

    public DialogueNode CreateDialogueNode(string _nodeName, Vector2 _position, string _dialogText)
    {
        DialogueNode dialogueNode = new DialogueNode()
        {
            type = NodeType.Dialogue,
            name = _nodeName,
            title = _nodeName,
            dialogText = _dialogText,
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
        textField.SetValueWithoutNotify(dialogueNode.dialogText);
        dialogueNode.mainContainer.Add(textField);

        dialogueNode.RefreshExpandedState();
        dialogueNode.RefreshPorts();
        dialogueNode.SetPosition(new Rect(_position, defaultNodeSize));

        return dialogueNode;
    }

    public CameraNode CreateCameraNode(string _nodeName, Vector2 _position, Camera _camera, GameObject _focus)
    {
        CameraNode cameraNode = new CameraNode()
        {
            cameraZoomLevel = 0f,
            type = NodeType.Camera,
            name = _nodeName,
            title = _nodeName,
            guid = Guid.NewGuid().ToString(),
            camera = _camera,
            focus = _focus

        };


        ObjectField cameraField = new ObjectField
        {
            objectType = typeof(Camera),
            allowSceneObjects = true,
            value = cameraNode.camera
        };
        cameraField.RegisterValueChangedCallback(evt => cameraNode.camera = (Camera)evt.newValue);
        cameraField.label = "Camera";
        cameraNode.mainContainer.Add(cameraField);

        ObjectField focusField = new ObjectField
        {
            objectType = typeof(GameObject),
            allowSceneObjects = true,
            value = cameraNode.focus
        };
        focusField.RegisterValueChangedCallback(evt => cameraNode.focus = (GameObject)evt.newValue);
        focusField.label = "focus";
        cameraNode.mainContainer.Add(focusField);


        Port imputPort = AddPort(cameraNode, Direction.Input, Port.Capacity.Multi);
        imputPort.portName = "Input";
        cameraNode.inputContainer.Add(imputPort);
        Port outputPort = AddPort(cameraNode, Direction.Output, Port.Capacity.Multi);
        outputPort.portName = "Output";
        cameraNode.outputContainer.Add(outputPort);
        cameraNode.styleSheets.Add(Resources.Load<StyleSheet>("CameraColor"));

        cameraNode.RefreshPorts();
        cameraNode.RefreshExpandedState();
        cameraNode.SetPosition(new Rect(_position, defaultNodeSize));

        return cameraNode;
    }
    public UnityEventNode CreateUnityEventNode(string _nodeName, Vector2 _position, UnityEvent _unityEvent, List<string> listenerGuids = null, List<string> methodNames = null)
    {
        UnityEventNode unityEventNode = new UnityEventNode
        {
            eventName = "",
            type = NodeType.UnityEvent,
            name = _nodeName,
            title = _nodeName,
            guid = Guid.NewGuid().ToString(),
            unityEvent = _unityEvent ?? new UnityEvent(), // Ensure we pass the correct instance
            listenerGuids = listenerGuids ?? new List<string>(),
            methodNames = methodNames ?? new List<string>(),
        };

        if (_unityEvent == null)
        {
            _unityEvent = new UnityEvent();
        }


        UnityEventNodeWrapper unityEvent = ScriptableObject.CreateInstance<UnityEventNodeWrapper>();
        unityEvent.unityEvent = _unityEvent;
        unityEventNode.unityEvent = _unityEvent;

        
        var eventField = new PropertyField();
        eventField.bindingPath = "unityEvent";
        eventField.Bind(new SerializedObject(wrapper));
        unityEventNode.mainContainer.Add(eventField);

        Port inputPort = AddPort(unityEventNode, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";
        unityEventNode.inputContainer.Add(inputPort);

        Port outputPort = AddPort(unityEventNode, Direction.Output, Port.Capacity.Multi);
        outputPort.portName = "Output";
        unityEventNode.outputContainer.Add(outputPort);

        unityEventNode.styleSheets.Add(Resources.Load<StyleSheet>("UnityEventColor"));

        unityEventNode.RefreshPorts();
        unityEventNode.RefreshExpandedState();
        unityEventNode.SetPosition(new Rect(_position, defaultNodeSize));

        return unityEventNode;
    }


    public DelayNode CreateDelayNode(string _nodeName, Vector2 _position, float _delay)
    {
        DelayNode delayNode = new DelayNode()
        {
            type = NodeType.Delay,
            name = _nodeName,
            title = _nodeName,
            delay = _delay,
            guid = Guid.NewGuid().ToString(),

        };

        Port imputPort = AddPort(delayNode, Direction.Input, Port.Capacity.Multi);
        imputPort.portName = "Input";
        delayNode.inputContainer.Add(imputPort);
        Port outputPort = AddPort(delayNode, Direction.Output, Port.Capacity.Multi);
        outputPort.portName = "Output";
        delayNode.outputContainer.Add(outputPort);
        delayNode.styleSheets.Add(Resources.Load<StyleSheet>("DelayColor"));

       

        TextField textField = new TextField(string.Empty);
        textField.RegisterValueChangedCallback(evt =>
        {
            if (float.TryParse(evt.newValue, out float result))
            {
                delayNode.delay = result; 
            }
            else
            {
                
                textField.value = delayNode.delay.ToString(); 
                Debug.LogWarning("Invalid input. Please enter a valid number.");
            }
        });
        textField.SetValueWithoutNotify(delayNode.delay.ToString());
        delayNode.mainContainer.Add(textField);

        delayNode.RefreshExpandedState();
        delayNode.RefreshPorts();
        delayNode.SetPosition(new Rect(_position, defaultNodeSize));

        return delayNode;
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

        while (exposedProperties.Any(property => property.propertyName == localPropertyName))
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
