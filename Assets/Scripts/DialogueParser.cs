using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class DialogueParser : MonoBehaviour
{
    [SerializeField] private CutsceneNodeContainer container;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button choicePrefab;
    [SerializeField] private Transform buttonContainer;

    private void Start()
    {
        NodeLinkData narrativeData = container.nodeLinks.First(); //Entrypoint node
        ProceedToNarrative(narrativeData.targetNodeGuid);
    }

    private void ProceedToNarrative(string _narrativeDataGUID)
    {

        var nodeData = container.cutsceneNodeData.Find(x => x.guid == _narrativeDataGUID);

        if (nodeData == null)
        {
            Debug.LogError($"Node with GUID {_narrativeDataGUID} not found.");
            return;
        }

        switch (nodeData.type)
        {
            case NodeType.Dialogue:
                DialogueNodeRun(_narrativeDataGUID, nodeData);
                break;
            case NodeType.Camera:
                // Handle Camera node functionality
                CameraNodeRun(_narrativeDataGUID, nodeData);
                break;
            case NodeType.UnityEvent:
                // Handle UnityEvent functionality
                UnityEventNodeRun(_narrativeDataGUID, nodeData);
                break;
            default:
                Debug.LogError($"Unknown NodeType: {nodeData.type}");
                break;
        }


    }



    private void DialogueNodeRun(string _narrativeDataGUID, CutsceneNodeData _nodeData)
    {
        string text = container.cutsceneNodeData.Find(x => x.guid == _narrativeDataGUID).dialogText;

        var choices = container.nodeLinks.Where(x => x.baseNodeGuid == _narrativeDataGUID);
        dialogueText.text = ProcessProperties(text);
        Button[] buttons = buttonContainer.GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            Destroy(buttons[i].gameObject);
        }

        foreach (NodeLinkData choice in choices)
        {
            Button button = Instantiate(choicePrefab, buttonContainer);
            button.GetComponentInChildren<TMP_Text>().text = ProcessProperties(choice.portName);

            button.onClick.AddListener(() => ProceedToNarrative(choice.targetNodeGuid));
        }
    }

    private void CameraNodeRun(string _narrativeDataGUID, CutsceneNodeData _nodeData)
    {
        Camera camera = UnityEngine.Object.FindObjectsOfType<GUIDComponent>().FirstOrDefault(g => g.guid == _nodeData.cameraGuid)?.GetComponent<Camera>();
        GameObject focus = UnityEngine.Object.FindObjectsOfType<GUIDComponent>().FirstOrDefault(g => g.guid == _nodeData.focusGuid)?.gameObject;
        camera.transform.LookAt(focus.transform);
        var choices = container.nodeLinks.Where(x => x.baseNodeGuid == _narrativeDataGUID);
        foreach (NodeLinkData choice in choices)
        {
            ProceedToNarrative(choice.targetNodeGuid);
        }
    }

    private void UnityEventNodeRun(string _narrativeDataGUID, CutsceneNodeData _nodeData)
    {
        _nodeData.unityEvent.Invoke();

        var choices = container.nodeLinks.Where(x => x.baseNodeGuid == _narrativeDataGUID);
        foreach (NodeLinkData choice in choices)
        {
            ProceedToNarrative(choice.targetNodeGuid);
        }
    }

    private string ProcessProperties(string text)
    {
        foreach (ExposedProperty exposedProperty in container.exposedProperties)
        {
            text = text.Replace($"[{exposedProperty.propertyName}]", exposedProperty.propertyValue);
        }
        return text;
    }
}
