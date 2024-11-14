using UnityEngine;
using UnityEngine.AI;

public abstract class AIBase : MonoBehaviour
{
    public NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if(agent == null )
        {
            agent = GetComponentInParent<NavMeshAgent>();
        }
    }
    public NavMeshPathStatus GetPathStatus(Vector3 targetPosition)
    {
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path);
        return path.status;
    }

    public void MoveToTarget(Vector3 targetPosition)
    {
        
        if (agent.pathPending /* || agent.remainingDistance <= agent.stoppingDistance*/) return;

        agent.SetDestination(targetPosition);
        Debug.Log("Moving");
        
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction.magnitude > 0.1f) 
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * agent.angularSpeed);
        }
    }
}
