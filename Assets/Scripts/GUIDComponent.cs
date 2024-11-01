using System;
using UnityEngine;

[ExecuteInEditMode]
public class GUIDComponent : MonoBehaviour
{
     public string guid;

    private void Awake()
    {
        if (string.IsNullOrEmpty(guid))
        {
            guid = Guid.NewGuid().ToString();
            Debug.Log($"Generated new GUID: {guid}");
        }
    }
}