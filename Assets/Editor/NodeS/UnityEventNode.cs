using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;


public class UnityEventNode : CutsceneNode
{
    [SerializeField] public UnityEvent unityEvent = new UnityEvent();
    public List<string> listenerGuids = new List<string>();
    public List<string> methodNames = new List<string>();
    public string eventName;


    public UnityEventNode()
    {
        if (unityEvent == null)
        {
            unityEvent = new UnityEvent();
        }
    }
    public void ExtractListeners()
    {
        if (unityEvent == null)
        {
            Debug.LogError("UnityEvent is null");
            return;
        }

        listenerGuids.Clear();
        methodNames.Clear();
        int listeners = unityEvent.GetPersistentEventCount();
        Debug.Log($"Listeners count: {listeners}");

        for (int i = 0; i < listeners; i++)
        {
            var target = unityEvent.GetPersistentTarget(i);
            if (target != null)
            {
                var guidComponent = target.GetComponent<GUIDComponent>();
                if (guidComponent != null)
                {
                    listenerGuids.Add(guidComponent.GUID);
                    methodNames.Add(unityEvent.GetPersistentMethodName(i));
                    Debug.Log($"Added listener: {guidComponent.GUID} - {unityEvent.GetPersistentMethodName(i)}");
                }
                else
                {
                    Debug.LogWarning($"GUIDComponent missing on target: {target.name}");
                }
            }
            else
            {
                Debug.LogWarning($"Target is null for listener at index {i}");
            }
        }
    }

    public void RebuildListeners()
    {
        unityEvent.RemoveAllListeners();

        for (int i = 0; i < listenerGuids.Count; i++)
        {
            GameObject target = GameObject.FindObjectsOfType<GUIDComponent>().FirstOrDefault(g => g.GUID == listenerGuids[i])?.gameObject;
            if (target != null)
            {
                bool methodFound = false;

                // Check GameObject methods first
                if (methodNames[i] == "SetActive")
                {
                    UnityAction<bool> action = new UnityAction<bool>(target.SetActive);
                    unityEvent.AddListener(() => action(true));  // Or false, depending on the use case
                    methodFound = true;
                    Debug.Log($"Listener fixed: GameObject.SetActive on {target.name}");
                    continue;
                }

                foreach (var component in target.GetComponents<Component>())
                {
                    var methodInfo = component.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                              .FirstOrDefault(m => m.Name == methodNames[i]);

                    if (methodInfo != null)
                    {
                        var action = (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), component, methodInfo);
                        if (action != null)
                        {
                            unityEvent.AddListener(action);
                            methodFound = true;
                            Debug.Log($"Listener fixed: {component.GetType().Name}.{methodNames[i]} on {target.name}");
                            break;
                        }
                        else
                        {
                            Debug.LogWarning($"Failed to create delegate for method: {methodNames[i]} on {component.GetType().Name}");
                        }
                    }
                }

                if (!methodFound)
                {
                    Debug.LogWarning($"Method not found: {methodNames[i]} on any component of {target.name}");
                }
            }
            else
            {
                Debug.LogWarning($"Target not found for GUID: {listenerGuids[i]}");
            }
        }
    }



}

public class UnityEventNodeWrapper : ScriptableObject
{
    [SerializeField] public UnityEvent unityEvent = new UnityEvent();
    public List<string> listenerGuids = new List<string>();

}


