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

    private void ProceedToNarrative(string narrativeDataGUID)
    {
        string text = container.cutsceneNodeData.Find(x => x.guid == narrativeDataGUID).dialogText;

        var choices = container.nodeLinks.Where(x => x.baseNodeGuid == narrativeDataGUID);
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

    private string ProcessProperties(string text)
    {
        foreach (ExposedProperty exposedProperty in container.exposedProperties)
        {
            text = text.Replace($"[{exposedProperty.propertyName}]", exposedProperty.propertyValue);
        }
        return text;
    }
}
