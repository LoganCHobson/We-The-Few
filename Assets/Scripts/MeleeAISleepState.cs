using UnityEngine;

public class MeleeAISleepState : IState
{
    public bool isSleeping = false;

    private MeleeAIStateMachine stateMachine;
    public void Enter(MeleeAIStateMachine _stateMachine)
    {
        stateMachine = _stateMachine;
        stateMachine.agent.isStopped = true;
        stateMachine.agent.ResetPath();
        isSleeping = true;
    }

    public void Run()
    {
        //Idk man, maybe we do something here.
    }

    public void Exit()
    {
        stateMachine.agent.isStopped = false;
        isSleeping = false;
    }

    
}
