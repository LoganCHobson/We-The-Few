using System;
using UnityEngine;

[ExecuteInEditMode]
public class GUIDComponent : MonoBehaviour
{
    [SerializeField] private string guid;
    public string GUID => guid;

    private void Awake()
    {
        if (string.IsNullOrEmpty(guid))
        {
            guid = Guid.NewGuid().ToString();
            //Debug.Log($"Generated new GUID: {guid}");
        }
    }
}