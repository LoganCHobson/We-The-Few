using NUnit.Framework;
using SolarStudios;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IState 
{
    void Enter(MeleeAIStateMachine _stateMachine);
    void Run();
    void Exit();

}

public class MeleeAIStateMachine : AIBase
{
    [HideInInspector]
    public IState currentState; //DONT TOUCH 

    public List<IState> states;

    private void Awake()
    {
       states = GetComponents<IState>().ToList<IState>();
    }
    private void Start()
    {
        stateMachine = this;

        SetState(gameObject.GetComponent<MeleeAISleepState>()); 
    }

    private void Update()
    {
        if (currentState != null)
        {
            currentState.Run();
        }
        else
        {
            SetState(gameObject.GetComponent<MeleeAISleepState>());
        }

    }

    public void SetState(IState newState)
    {
        if (currentState != null)
        {
            currentState.Exit();
        }

        currentState = newState;
        currentState.Enter(this);
    }
}

