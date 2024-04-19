using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;

public class AgentMovement : MonoBehaviour
{
    public float viewRadius;
    [Range(0, 360)]
    
    public float viewAngle;

    public LayerMask staticMask;
    public LayerMask obstacleMask;

    public string targetTag;

    public float displacementRadius = 3f; 

    public SteeringManager agent;

    [HideInInspector] 
    public bool staticTargets;
    [HideInInspector] 
    public bool player; 

    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform>();

    [HideInInspector]
    public Vector3 wanderingForce = Vector3.zero;

    [HideInInspector]
    public Rigidbody rb;

    [HideInInspector]
    public string selectedBehaviorOption;

    private Vector3 steering = Vector3.zero; 

    private float wanderTimer;
    private bool isWandering = true;
    private bool targetInView = false;


    void Start()
    {
        // StartCoroutine("FindTargetsWithDelay", .2f);

        Vector3 randomDirection = Random.insideUnitSphere;
        randomDirection.y = 0; 
        float randomSpeed = Random.Range(0, agent.maxVelocity);
        Vector3 initialVelocity = randomDirection * randomSpeed;

        rb.velocity = initialVelocity;
        wanderingForce = agent.Wander(displacementRadius);
    }

    void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    /*IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }*/

    private void FixedUpdate()
    {
        FindVisibleTargets();
        Debug.Log(selectedBehaviorOption);
        Debug.Log(isWandering);

        Debug.Log("Agent velocity: " + rb.velocity.magnitude);

        if (isWandering == true)
        {
            wanderTimer += Time.fixedDeltaTime;

            if (wanderTimer >= 2f)
            {
                wanderTimer = 0f;

                wanderingForce = agent.Wander(displacementRadius);
            }

            rb.velocity = (rb.velocity + wanderingForce).normalized * agent.maxVelocity;

            Quaternion targetRotation = Quaternion.LookRotation(rb.velocity, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
        }

        else
        {
            rb.velocity = (rb.velocity + steering).normalized * agent.maxVelocity;
            Quaternion targetRotation = Quaternion.LookRotation(rb.velocity, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
        }
    }

    void FindVisibleTargets()
    {
        visibleTargets.Clear(); // empties array
        // Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, staticMask);
       
        /*for (int i = 0; i < targetsInViewRadius.Length; i++)
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
        }*/

        Collider[] player = Physics.OverlapSphere(transform.position, viewRadius);
        
        for (int i = 0; i < player.Length; i++)
        {
            foreach (Transform target in visibleTargets)
            {
                isWandering = false;
            }

            if (player[i].gameObject.CompareTag(targetTag))
            {
                Transform target = player[i].transform;
                Vector3 dirToTarget = (target.position - transform.position).normalized;

                if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
                {
                    float dstToTarget = Vector3.Distance(transform.position, target.position);

                    // if the target is within the field of view and not obstructed by obstacles
                    if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                    {
                        isWandering = false; 
                        visibleTargets.Add(target);

                        steering += DetermineBehavior(selectedBehaviorOption, player[i].gameObject);
                        break;
                    }
                }
            }

            if (visibleTargets.Count == 0 && i == player.Length - 1)
            {
                isWandering = true;
            }
        }
    }

    private Vector3 DetermineBehavior( string behavior, GameObject target )
    {
        Vector3 steeringForce = Vector3.zero;
        
        if (behavior == "Seek")
        {
            steeringForce = agent.Seek(target.transform.position);
        }
        else if (behavior == "Pursuit")
        {
            steeringForce = agent.Seek( agent.Pursuit(target) );
        }
        else if (behavior == "Flee")
        {
            steeringForce = agent.Flee(target.transform.position);
            Debug.Log("flee force " + steeringForce);
        }
        else if (behavior == "Evade")
        {
            steeringForce = agent.Flee(agent.Evade(target)); 
        }
        steeringForce.y = 0; 
        return steeringForce; 
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