using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FieldOfView : MonoBehaviour
{

    public float viewRadius;
    [Range(0, 360)]
    public float viewAngle;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public AgentMovement agent; 

    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform>();

    [HideInInspector]
    public Vector3 wanderingForce = Vector3.zero;

    [HideInInspector]
    public Rigidbody rb;

    void Start()
    {
        StartCoroutine("FindTargetsWithDelay", .2f);
        StartCoroutine("Wander", 1f); 
    }

    void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    IEnumerator Wander(float delay)
    {
        wanderingForce = agent.Wander();

        while (true)
        {
            yield return new WaitForSeconds(delay);
            wanderingForce = agent.Wander(); 
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = (rb.velocity += wanderingForce).normalized * agent.maxVelocity;

        Quaternion targetRotation = Quaternion.LookRotation(rb.velocity, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
    }

    void FindVisibleTargets()
    {
        visibleTargets.Clear(); // empties array
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            // checks if the angle between the forward direction of the gameObject and the direction to the target is within half of the view angle 
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);

                // if the target is within the field of view and not obstructed by obstacles
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    visibleTargets.Add(target);
                }
            }
        }
    }


    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}