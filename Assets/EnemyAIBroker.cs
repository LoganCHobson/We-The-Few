using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAIBroker : MonoBehaviour
{
    public List<AIBase> enemies = new List<AIBase>();
    public GameObject player;
    public float aiWakeRange;

    public float squaredRange;

    private void Start()
    {
        squaredRange = aiWakeRange * aiWakeRange; //Trust me bruh.
    }

    private void Update()
    {
        float distance = (player.transform.position - transform.position).sqrMagnitude; //Cheaper than Distance()
        if (distance <= squaredRange)
        {
            foreach (AIBase enemy in enemies)
            {
                //enemy.stateMachine.SetState();

            }
        }
    }

}
