using UnityEngine;
using UnityEngine.AI;

public abstract class AIBase : MonoBehaviour
{
    [HideInInspector]
    public NavMeshAgent agent;
    [HideInInspector]
    public object stateMachine; 

    private Vector3 lastTargetPosition;

    private void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>();

    }
    public NavMeshPathStatus GetPathStatus(Vector3 targetPosition)
    {
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path);
        return path.status;
    }

    public void MoveToTarget(Vector3 targetPosition)
    {
        if (targetPosition != lastTargetPosition)
        {
            agent.SetDestination(targetPosition);
            lastTargetPosition = targetPosition;
        }
        else if (agent.remainingDistance <= agent.stoppingDistance) return;
        

        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * agent.angularSpeed);
        }
    }
}
