using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class SceneRuleGuid
{
    static SceneRuleGuid()
    {
        //EditorApplication.hierarchyChanged += OnHierarchyChanged; Re add this when working in cutscene scenes. Will add something more perma later.
    }

    private static void OnHierarchyChanged()
    {
        if (EditorApplication.isPlaying)
            return;
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