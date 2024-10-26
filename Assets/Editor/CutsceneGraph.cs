using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class CutsceneGraph : EditorWindow
{
    private CutsceneGraphView graphView;
    private string fileName = "New Narrative";


    [MenuItem("Graph/Cutscene Graph")]
    public static void openGraphWindow()
    {
        CutsceneGraph window = GetWindow<CutsceneGraph>();
        window.titleContent = new GUIContent("Cutscene Graph");
    }

    private void OnEnable()
    {
        ConstructGraph();
        GenerateToolbar();
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(graphView);
    }

    private void ConstructGraph()
    {
        graphView = new CutsceneGraphView
        {
            name = "Cutscene Graph"
        };

        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }

    private void GenerateToolbar()
    {
        Toolbar toolbar = new Toolbar();
        TextField filenametextField = new TextField("File Name:");
        filenametextField.SetValueWithoutNotify(fileName);
        filenametextField.MarkDirtyRepaint();
        filenametextField.RegisterValueChangedCallback(evt => fileName = evt.newValue);
        toolbar.Add(filenametextField);
        toolbar.Add(new Button(() => RequestData(true)) { text = "Save Data" });
        toolbar.Add(new Button(() => RequestData(false)) { text = "Load Data" });

        Button nodeCreateButton = new Button(() =>
        {
            graphView.CreateNode("Dialogue Node");
        });

        nodeCreateButton.text = "Create Node";
        toolbar.Add(nodeCreateButton);

        rootVisualElement.Add(toolbar);
    }

    private void RequestData(bool save)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            EditorUtility.DisplayDialog("Invalid file name!", "Please enter a valid file name.", "OK");
            return;
        }

        GraphSaveUtility saveUtility = GraphSaveUtility.GetInstance(graphView);

        if (save)
        {
            saveUtility.SaveGraph(fileName);
        }
        else
        {
            saveUtility.LoadGraph(fileName);
        }
    }


}
