using UnityEngine;

public class MeleeAI : AIBase
{

    public Transform target;
    void Start()
    {
        MoveToTarget(target.position);
    }

    // Update is called once per frame
    void Update()
    {
       
    }
}
