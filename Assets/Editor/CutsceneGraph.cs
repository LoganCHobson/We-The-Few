using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
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
        GenerateMinimap();
        GenerateBlackBoard();
    }

    private void GenerateBlackBoard()
    {
        Blackboard blackboard = new Blackboard(graphView);
        blackboard.Add(new BlackboardSection { title = "Exposed Properties" });
        blackboard.addItemRequested = blackboard =>
        {
            graphView.AddPropertyToBlackboard(new ExposedProperty());
        };
        blackboard.editTextRequested = (blackboard1, element, newValue) =>
        {
            string oldPropertyName = ((BlackboardField)element).text;
            if (graphView.exposedProperties.Any(property => property.propertyName == newValue))
            {
                EditorUtility.DisplayDialog("Error", "This property name already exists, please choose another one!", "OK");
                return;
            }

            int index = graphView.exposedProperties.FindIndex(property => property.propertyName == oldPropertyName);
            graphView.exposedProperties[index].propertyName = newValue;

            ((BlackboardField)element).text = newValue;
        };

        blackboard.SetPosition(new Rect(10, 30, 200, 300));
        graphView.Add(blackboard);
        graphView.blackboard = blackboard;

    }

    private void GenerateMinimap() //Adds that cool minimap at the bottom right of the graph editor.
    {
        MiniMap miniMap = new MiniMap { anchored = true };
        //10px offset form left side.
        Vector2 coords = graphView.contentViewContainer.WorldToLocal(new Vector2(this.maxSize.x - 10, 30));
        miniMap.SetPosition(new Rect(1900, 30, 200, 140));
        graphView.Add(miniMap);
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(graphView);
    }

    private void ConstructGraph()
    {
        graphView = new CutsceneGraphView(this)
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
