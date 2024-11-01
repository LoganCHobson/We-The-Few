using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class SceneRuleGuid
{
    static SceneRuleGuid()
    {
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
    }

    private static void OnHierarchyChanged()
    {
        GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();

        foreach (var gameObject in allGameObjects)
        {
            
            if (gameObject.GetComponent<GUIDComponent>() == null)
            {
                gameObject.AddComponent<GUIDComponent>();
                Debug.Log($"Added GUID to {gameObject.name}");
            }
        }
    }
}