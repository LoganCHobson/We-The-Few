using UnityEngine;

public class MeleeAIMove : IState
{
    public Transform target;
    public void Enter(MeleeAIStateMachine stateMachine)
    {
        stateMachine.agent.SetDestination(target.position);
    }

    public void Run()
    {

    }

    public void Exit()
    {
        
    }
}
