using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using System.Linq;
using Unity.VisualScripting;

public class AgentMovement : MonoBehaviour
{
    public float viewRadius;

    [Range(0, 360)]
    public float viewAngle;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [HideInInspector]
    public bool playerInFOV = false;

    private AlertStage alertStage;

    public Light spotLight; 

    // [Range(0, 100)]
    [HideInInspector]
    public float alertLevel;

    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform>();

    [HideInInspector] public string selectedBehaviorOption = "";
    [HideInInspector] public string selectedTypeOption = "";

    [HideInInspector] public bool isMovementPaused = true;
    [HideInInspector] public Rigidbody rb;

    Vector3 steering = Vector3.zero;
    float wanderTimer = 0f;

    public GameObject player;
    GameObject playerTwo; 

    public float maxVelocity = 20f;

    public float steeringForce = 2f;
    public float maxForce = 50f;

    public float T = 3f;

    [HideInInspector] public float slowingRadius = 10f;
    [HideInInspector] public bool slow = false;

    private bool isBoosted = false;
    
    private GameObject leader;
    private AgentMovement leaderMovement;

    private Vector3 avoidanceForce = Vector3.zero;
    private int neighborCount = 0; 

    Scene scene;

    enum AlertStage
    {
        Peaceful,
        Intrigued,
        Alerted
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        scene = SceneManager.GetActiveScene(); 
        // player = GameObject.FindGameObjectWithTag("Player"); 
    }

    private void Start()
    {
        StartCoroutine("FindTargetsWithDelay", .2f);

        alertStage = AlertStage.Peaceful;
        alertLevel = 0;

        rb.velocity = Vector3.zero;

        if (gameObject.tag == "Follower")
        {
            leader = GameObject.FindWithTag("Leader");
            leaderMovement = leader.GetComponent<AgentMovement>(); 
            selectedBehaviorOption = "Follow"; 
        }

    }

    private void Update()
    {
        UpdateBehavior();
        ComputeAlignment(); 


        if (!isMovementPaused)
        {
            normalizePosition();

            Collider[] targetsInFOV = Physics.OverlapSphere(transform.position, viewRadius);
            playerTwo = null;

            foreach (Collider c in targetsInFOV)
            {
                if (c.CompareTag("Player"))
                {
                    // figuring out if player is within fov angle 
                    float signedAngle = Vector3.Angle(transform.forward, c.transform.position - transform.position);

                    if (Mathf.Abs(signedAngle) < viewAngle / 2)
                    {
                        playerTwo = c.gameObject;
                        playerInFOV = true;
                    }
                }
            }
     
            if (playerTwo == null)
            {
                playerInFOV = false;
            }

            wanderTimer += Time.deltaTime;

            Vector3 tmp = rb.velocity + steering;
            rb.velocity = (rb.velocity + tmp).normalized * maxVelocity;

            Quaternion targetRotation = Quaternion.LookRotation(rb.velocity, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
           
            _UpdateAlertState(playerInFOV);
        }
    }

    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    Vector3 ComputeAlignment()
    {
        neighborCount = 0; 
        Vector3 force = Vector3.zero; 

        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
        List<Collider> targetsInViewAngle = new List<Collider>(); 

        for (int i = 0; i < targetsInViewRadius.Count(); i++ )
        {
            Collider B = targetsInViewRadius[i];

            float signedAngle = Vector3.Angle(transform.forward, B.transform.position - transform.position);

            if (Mathf.Abs(signedAngle) < viewAngle / 2)
            {
                targetsInViewAngle.Add(B); 
            }
        }

        Debug.Log(neighborCount);

        for (int j = 0; j < targetsInViewAngle.Count(); j++)
        {
            Collider B = targetsInViewAngle[j];
            float DistFromB = Vector3.Distance(transform.position, B.gameObject.transform.position); 
            
            if ( DistFromB < 10f )
            {
                Rigidbody agentRB = B.GetComponent<AgentMovement>().rb;
                
                force.x += agentRB.velocity.x;
                force.z += agentRB.velocity.z;

                neighborCount++; 
            }
        }

        return force; 

    }


    void FindVisibleTargets()
    {
        visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    visibleTargets.Add(target);

                    if ( Vector3.Distance(target.position, transform.position) < 10f)
                    {
                        steering += (transform.position - target.position); 
                    }
                }
            }
        }

        avoidanceForce = SeeAhead ( FindMostThreateningObstacle(targetsInViewRadius), 15f);
        // steering += avoidanceForce; 
    }

    private void _UpdateAlertState(bool playerInFOV)
    {
        switch (alertStage)
        {
            case AlertStage.Peaceful:

                if (wanderTimer >= 2f)
                {
                    wanderTimer = 0f;

                    steering = ComputeAlignment();
                }

                if (playerInFOV)
                {
                    alertStage = AlertStage.Intrigued;
                }

                break;

            case AlertStage.Intrigued:
                if (playerInFOV)
                {
                    alertLevel++;
                    if (alertLevel >= 100)
                        alertStage = AlertStage.Alerted;

                    steering = Wander(3f) + DetermineBehavior(selectedBehaviorOption, player) * 0.1f;

                    if (playerInFOV)
                    {
                        alertStage = AlertStage.Intrigued;
                    }
                }

                else
                {
                    alertLevel--;
                    if (alertLevel <= 0)
                        alertStage = AlertStage.Peaceful;
                }
                break;

            case AlertStage.Alerted:

                if (!playerInFOV)
                    alertStage = AlertStage.Intrigued;
                else
                {
                    steering = DetermineBehavior(selectedBehaviorOption, player);
                }

                break;
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

    public Vector3 Seek(Vector3 target)
    {
        // calculate the direction towards the target
        // velocity = normalize(target - position) * max_velocity
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0;

        // Arrive(target);

        Vector3 desiredVelocity = direction * maxVelocity;

        Vector3 steering = (desiredVelocity - rb.velocity) * steeringForce;
        steering = Truncate(steering, maxForce);
        steering = steering / rb.mass;
        steering.y = 0;

        return steering;
    }

    public Vector3 Flee(Vector3 targetPosition)
    {
        Vector3 direction = (transform.position - targetPosition).normalized;
        direction.y = 0;

        // desired_velocity = normalize(target - position) * max_velocity
        Vector3 desiredVelocity = direction * maxVelocity;

        Vector3 steering = (desiredVelocity - rb.velocity) * steeringForce;
        steering = Truncate(steering, maxForce);
        steering = steering / rb.mass;

        Vector3 newVelocity = rb.velocity + steering;

        Vector3 newPosition = transform.position + newVelocity * Time.fixedDeltaTime;
        newPosition.y = 0;

        return newPosition;

    }

    public void Arrive(Vector3 targetPosition)
    {
        Vector3 desiredVelocity = targetPosition - transform.position;
        float distance = Vector3.Distance(targetPosition, transform.position);

        if (distance < slowingRadius)
        {
            desiredVelocity = desiredVelocity.normalized * maxVelocity * (distance / slowingRadius);
        }
        else
        {
            desiredVelocity = desiredVelocity.normalized * maxVelocity;
        }

        Vector3 steering = desiredVelocity - rb.velocity;

        steering = Truncate(steering, maxForce);
        steering /= rb.mass;
        Vector3 newVelocity = rb.velocity + steering;

        newVelocity = Truncate(newVelocity, maxVelocity);

        Vector3 newPosition = transform.position + newVelocity * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        // rotate the agent to look at the target position
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(desiredVelocity.x, 0, desiredVelocity.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
    }

    private Vector3 Wander(float radius)
    {
        Vector3 circleCenter = rb.velocity.normalized * radius;
        Vector3 displacement = new Vector3(UnityEngine.Random.Range(0f, 1f), 0, UnityEngine.Random.Range(0f, 1f)) * radius;
        Vector3 wanderForce = circleCenter + displacement;

        float angle = Vector3.Angle(rb.velocity, wanderForce) * Mathf.Deg2Rad * 0.7f;

        float newVectorX = Mathf.Cos(UnityEngine.Random.Range(0, angle));
        float newVectorY = Mathf.Sin(UnityEngine.Random.Range(0, angle));

        float randVectorX = UnityEngine.Random.Range(0.2f, 1f) * newVectorX - (newVectorX * 0.5f);
        float randVectorY = UnityEngine.Random.Range(0.2f, 1f) * newVectorY - (newVectorY * 0.5f);

        wanderForce = new Vector3(randVectorX, 0, randVectorY);

        return wanderForce;
    }

    public Vector3 Pursuit(GameObject target)
    {
        Rigidbody rbt = target.GetComponent<Rigidbody>();

        T = 2 * Vector3.Distance(target.transform.position, gameObject.transform.position) / maxVelocity;

        Vector3 futurePosition = target.transform.position + rbt.velocity * T;

        Vector3 steering = futurePosition - transform.position;
        steering = Truncate(steering, maxForce);
        steering /= rb.mass;
        Vector3 newVelocity = rb.velocity + steering;
        newVelocity = Truncate(newVelocity, maxVelocity);
        Vector3 newPosition = transform.position + newVelocity * Time.fixedDeltaTime;
        newPosition.y = 0;

        return futurePosition;
    }

    public Vector3 Evade(GameObject target)
    {
        Rigidbody rbt = target.GetComponent<Rigidbody>();

        T = Vector3.Distance(gameObject.transform.position, target.transform.position) / maxVelocity;

        Vector3 distance = target.transform.position - transform.position;
        int updatesAhead = Mathf.RoundToInt(distance.magnitude / maxVelocity);
        Vector3 futurePosition = target.transform.position + rbt.velocity * updatesAhead;
        return futurePosition;
    }

    public Vector3 SeeAhead(Vector3 obstacle, float radius)
    {
        Vector3 ahead = transform.position + rb.velocity.normalized * radius;
        // Vector3 ahead2 = transform.position + rb.velocity.normalized * radius * 0.5f;

        Vector3 avoidanceForce = ahead - obstacle;

        avoidanceForce = avoidanceForce.normalized * radius;

        return avoidanceForce;
    }


    public Vector3 Truncate(Vector3 vector, float maxLength)
    {
        if (vector.magnitude > maxLength)
        {
            vector = vector.normalized * maxLength;
        }
        return vector;
    }

    private Vector3 FindMostThreateningObstacle(Collider[] gameObjects)
    {
        float smallestDistance = 30f;
        float currentDistance = 0f;

        Vector3 threateningObstacle = Vector3.zero;

        foreach (Collider obstacle in gameObjects)
        {
            Transform obs = obstacle.gameObject.transform; 
            currentDistance = Mathf.Abs(Vector3.Distance(obs.position, transform.position));

            if (currentDistance < smallestDistance)
            {
                threateningObstacle = obs.position;
                smallestDistance = currentDistance;
            }
        }

        return threateningObstacle;
    }

    private Vector3 DetermineBehavior(string behavior, GameObject target)
    {
        Vector3 steeringForce = Vector3.zero;

        if (behavior == "Seek")
        {
            steeringForce = Seek(target.transform.position);
        }
        
        else if (behavior == "Pursuit")
        {
            steeringForce = Seek(Pursuit(target));
        }
        
        else if (behavior == "Flee")
        {
            steeringForce = ( Flee(target.transform.position) + Wander(10f) + Wander(5f) ) * 0.3f;
        }
        
        else if (behavior == "Evade")
        {
            steeringForce = Flee(Evade(target));
        }

        steeringForce.y = 0;
        return steeringForce;
    }

    private void normalizePosition()
    {
        rb.velocity = new Vector3 (rb.velocity.x, 0, rb.velocity.z);
        gameObject.transform.Rotate(0, transform.rotation.y, 0);
        
        spotLight.range = viewRadius;

        if (viewAngle > 179)
        {
            spotLight.spotAngle = viewAngle / 2;
        }

        else
        {
            spotLight.spotAngle = viewAngle;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Buff"))
        {
            StartCoroutine(BoostAgentSpeed());
        }
    }

    IEnumerator BoostAgentSpeed()
    {
        isBoosted = true;
        float originalSpeed = maxVelocity;
        maxVelocity *= 1.5f; // Increase speed
        
        yield return new WaitForSeconds(7f);
        maxVelocity = originalSpeed; // Reset speed
        isBoosted = false;
    }

    void UpdateBehavior()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            selectedBehaviorOption = "Flee";
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            selectedBehaviorOption = "Seek";
        }
    }

    float GetSpeed()
    {
        return isBoosted ? maxVelocity * 1.5f : maxVelocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        steering += Wander(10f);
    }

}
