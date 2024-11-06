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
    [SerializeField] public UnityEvent unityEvent;
    public List<string> listenerGuids = new List<string>();
    public List<string> methodNames = new List<string>();
    public List<Type[]> parameterTypes = new List<Type[]>();
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
        listenerGuids.Clear();
        methodNames.Clear();
        parameterTypes.Clear();

        if (unityEvent == null)
        {
            Debug.LogError("UnityEvent is null");
            return;
        }

        var listeners = unityEvent.GetPersistentEventCount();
        Debug.Log($"Listeners count: {listeners}");

        for (int i = 0; i < listeners; i++)
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

            listenerGuids.Add(guidComponent.GUID);
            methodNames.Add(unityEvent.GetPersistentMethodName(i));

            
            var methodInfo = target.GetType().GetMethod(
                unityEvent.GetPersistentMethodName(i),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                Type.EmptyTypes,  
                null
            );
            if (methodInfo != null)
            {
                parameterTypes.Add(methodInfo.GetParameters().Select(p => p.ParameterType).ToArray());
            }
            else
            {
                parameterTypes.Add(Type.EmptyTypes);  
                Debug.LogWarning($"Method not found: {unityEvent.GetPersistentMethodName(i)} on {target.name}");
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

                foreach (var component in target.GetComponents<Component>())
                {
                    var methods = component.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                            .Where(m => m.Name == methodNames[i] && m.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes[i]))
                                            .ToArray();

                    foreach (var methodInfo in methods)
                    {
                        if (methodInfo != null)
                        {
                            var parameters = methodInfo.GetParameters();
                            var delegateType = parameters.Length == 0
                                                ? typeof(UnityAction)
                                                : Expression.GetActionType(parameters.Select(p => p.ParameterType).Append(typeof(void)).ToArray());

                            var action = Delegate.CreateDelegate(delegateType, component, methodInfo);

                            if (action != null)
                            {
                                if (parameters.Length == 0)
                                {
                                    unityEvent.AddListener((UnityAction)action);
                                }
                                else
                                {
                                    unityEvent.AddListener(() => DynamicInvoke(action, parameters.Select(p => (object)null).ToArray())); // Provide the actual arguments here
                                }

                                methodFound = true;
                                Debug.Log($"Listener fixed: {component.GetType().Name}.{methodNames[i]} on {target.name}");
                                break;
                            }
                        }
                    }

                    if (methodFound) break;
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

    private void DynamicInvoke(Delegate action, object[] args)
    {
        action.DynamicInvoke(args);
    }


}


public class UnityEventNodeWrapper : ScriptableObject
{
    [SerializeField] public UnityEvent unityEvent = new UnityEvent();
    public List<string> listenerGuids = new List<string>();

}


