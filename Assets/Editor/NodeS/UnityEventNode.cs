using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;


public class UnityEventNode : CutsceneNode
{
    [SerializeField]
    public UnityEvent unityEvent = new UnityEvent();
    public List<ListenerData> listeners = new List<ListenerData>();
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
        listeners.Clear();

        if (unityEvent == null)
        {
            Debug.LogError("UnityEvent is null");
            return;
        }

        var listenerCount = unityEvent.GetPersistentEventCount();
        Debug.Log($"Listeners count: {listenerCount}");

        for (int i = 0; i < listenerCount; i++)
        {
            var target = unityEvent.GetPersistentTarget(i);
            if (target == null)
            {
                Debug.LogWarning($"Target is null for listener at index {i}");
                continue;
            }

            var guidComponent = target.GetComponent<GUIDComponent>();
            if (guidComponent == null)
            {
                Debug.LogWarning($"GUIDComponent missing on target: {target.name}");
                continue;
            }

            var methods = target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                    .Where(m => m.Name == unityEvent.GetPersistentMethodName(i))
                                    .ToArray();

            Debug.Log($"Methods found for {unityEvent.GetPersistentMethodName(i)} on {target.name}: {methods.Length}");

            foreach (var methodInfo in methods)
            {
                if (methodInfo != null)
                {
                    var parameterType = methodInfo.GetParameters().FirstOrDefault()?.ParameterType ?? typeof(void);
                    listeners.Add(new ListenerData(guidComponent.GUID, unityEvent.GetPersistentMethodName(i), parameterType));
                    break;  // Only add the first matching method
                }
            }

            if (!listeners.Any(l => l.GUID == guidComponent.GUID && l.methodName == unityEvent.GetPersistentMethodName(i)))
            {
                Debug.LogWarning($"Method not found: {unityEvent.GetPersistentMethodName(i)} on {target.name}");
                listeners.Add(new ListenerData(guidComponent.GUID, unityEvent.GetPersistentMethodName(i), typeof(void)));
            }
        }

        if (listeners.Count != listenerCount)
        {
            Debug.LogError("ExtractListeners: List count does not match listener count. Please check the listener extraction logic.");
            Debug.LogError($"listenerCount: {listenerCount}, listeners.Count: {listeners.Count}");
        }
    }


    public void RebuildListeners()
    {
        unityEvent.RemoveAllListeners();

        foreach (var listenerData in listeners)
        {
            GameObject target = FindGameObjectByGUID(listenerData.GUID);

            if (target != null)
            {
                bool methodFound = false;

                // Ensure we're searching for the correct method
                foreach (var component in target.GetComponents<Component>())
                {
                    // Get all methods from the component type
                    MethodInfo[] methods = component.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    // Attempt to find a matching method based on the method name
                    foreach (var method in methods)
                    {
                        if (method.Name.Equals(listenerData.methodName))
                        {
                            // Get method parameters
                            ParameterInfo[] parameters = method.GetParameters();

                            // Handle methods with no parameters (like AudioSource.Play)
                            if (parameters.Length == 0)
                            {
                                // Special case for AudioSource.Play() (no parameters)
                                if (method.Name == "Play" && component is AudioSource)
                                {
                                    // Create a delegate for the method and add the listener
                                    Delegate action = Delegate.CreateDelegate(typeof(UnityAction), component, method);
                                    unityEvent.AddListener(() => action.DynamicInvoke());
                                    methodFound = true;
                                    break;
                                }
                            }
                            // Handle methods with one parameter
                            else if (parameters.Length == 1 && listenerData.parameterType != null)
                            {
                                Type parameterType = parameters[0].ParameterType;
                                if (parameterType == listenerData.parameterType)
                                {
                                    Delegate action = Delegate.CreateDelegate(typeof(UnityAction<>).MakeGenericType(parameterType), component, method);
                                    unityEvent.AddListener(() => action.DynamicInvoke(listenerData.parameterType));
                                    methodFound = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (methodFound)
                    {
                        Debug.Log($"Listener fixed: {listenerData.methodName} on {target.name}");
                        break;
                    }
                }

                if (!methodFound)
                {
                    Debug.LogWarning($"Method '{listenerData.methodName}' not found on any component of {target.name}");
                }
            }
            else
            {
                Debug.LogWarning($"Target GameObject with GUID {listenerData.GUID} not found.");
            }
        }
    }


    private GameObject FindGameObjectByGUID(string guid)
    {
        var guidComponent = GameObject.FindObjectsOfType<GUIDComponent>().FirstOrDefault(g => g.GUID == guid);
        return guidComponent?.gameObject;
    }







    private void DynamicInvoke(Delegate action, object[] args)
    {
        action.DynamicInvoke(args);
    }

    private object GetDefaultValue(Type type)
    {
        if (type == typeof(string))
        {
            return "";  // Return default empty string for string parameters
        }
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }




}



public class UnityEventNodeWrapper : ScriptableObject
{
    [SerializeField] public UnityEvent unityEvent = new UnityEvent();
    public List<string> listenerGuids = new List<string>();

}
