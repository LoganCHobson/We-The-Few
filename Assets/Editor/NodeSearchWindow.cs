using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
{
    private CutsceneGraphView graphView;
    private EditorWindow window;
    private Texture2D indentation;

    public void Init(EditorWindow _window, CutsceneGraphView _graphView)
    {
        window = _window;
        graphView = _graphView;

        indentation = new Texture2D(1, 1);
        indentation.SetPixel(0, 0, new Color()); //Legit just to fix a minor indentation bug in the search window. Unity's fault.
        indentation.Apply();
    }
    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        List<SearchTreeEntry> tree = new List<SearchTreeEntry>()
        {
            new SearchTreeGroupEntry(new GUIContent("Create Elements"), 0),
            new SearchTreeGroupEntry(new GUIContent("Dialogue"), 1),
            new SearchTreeEntry(new GUIContent("Dialogue Node", indentation))
            {
                userData = new DialogueNode(), level = 2
            },
            new SearchTreeGroupEntry(new GUIContent("Camera"), 1),
            new SearchTreeEntry(new GUIContent("Camera Node", indentation))
            {
                userData = new CameraNode(), level = 2
            },
            new SearchTreeGroupEntry(new GUIContent("Events"), 1),
            new SearchTreeEntry(new GUIContent("UnityEvent Node", indentation))
            {
                userData = new UnityEventNode(), level = 2
            },
            new SearchTreeGroupEntry(new GUIContent("Utility"), 1),
            new SearchTreeEntry(new GUIContent("Delay Node", indentation))
            {
                userData = new DelayNode(), level = 2
            }

        };
        return tree;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        Vector2 worldMousePosition = window.rootVisualElement.ChangeCoordinatesTo(window.rootVisualElement.parent, context.screenMousePosition - window.position.position);
        Vector2 localMousePosition = graphView.contentContainer.WorldToLocal(worldMousePosition);
        switch(SearchTreeEntry.userData) 
        {
            case DialogueNode dialogueNode:
                graphView.CreateNode("Dialogue Node", localMousePosition, NodeType.Dialogue);
                return true;

            case CameraNode cameraNode:
                graphView.CreateNode("Camera Node", localMousePosition, NodeType.Camera);
                return true;

            case UnityEventNode unityEventNode:
                graphView.CreateNode("UnityEvent Node", localMousePosition, NodeType.UnityEvent);
                return true;
            case DelayNode delayNode:
                graphView.CreateNode("Delay Node", localMousePosition, NodeType.Delay);
                return true;


            default: return false;
        
         
        }
    }
}
