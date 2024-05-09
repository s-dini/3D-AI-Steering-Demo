using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class FollowerScript : MonoBehaviour
{
    enum AlertStage
    {
        Peaceful,
        Intrigued,
        Alerted
    }

    [Range(0, 360)] public float viewAngle;
    public float viewRadius;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [HideInInspector] public Light spotLight;
    [HideInInspector] public List<Transform> visibleTargets = new List<Transform>();

    public float maxVelocity = 20f;
    public float steeringForce = 2f;
    public float maxForce = 50f;
    public float T = 3f;

    [HideInInspector] public float slowingRadius = 10f;
    [HideInInspector] public bool slow = false;
    [HideInInspector] public bool isMovementPaused = true;

    [HideInInspector] public string selectedBehaviorOption = "";

    private GameObject player; 
    private GameObject leader;
    
    private bool playerInFOV;
    private bool leaderInFOV; 

    private int alertLevel;  

    private bool isBoosted = false;
    private GameObject playerTwo;
    private GameObject leaderTwo;
    private AlertStage alertStage;

    private AgentMovement leaderScript;
    private float wanderTimer; 

    Rigidbody rb;

    Vector3 steering = Vector3.zero;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        spotLight = GetComponent<Light>();
        
        leader = GameObject.FindWithTag("Leader");
        player = GameObject.FindGameObjectWithTag("Player"); 
    }

    private void Start()
    {
        StartCoroutine("FindTargetsWithDelay", .2f);  
        leaderScript = leader.GetComponent<AgentMovement>();
    }

    private void Update()
    {
        UpdateBehavior();

        if (!isMovementPaused)
        {
            normalizePosition();

            Collider[] targetsInFOV = Physics.OverlapSphere(transform.position, viewRadius);
            playerTwo = null; leaderTwo = null; 

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

                else if (c.CompareTag("Leader"))
                {
                    float signedAngle = Vector3.Angle(transform.forward, c.transform.position - transform.position);

                    if (Mathf.Abs(signedAngle) < viewAngle / 2)
                    {
                        leaderTwo = c.gameObject;
                        leaderInFOV = true;
                    }
                }
            }

            if (playerTwo == null)
            {
                playerInFOV = false;
            }

            if (leaderTwo == null)
            {
                leaderInFOV = false; 
            }

            wanderTimer += Time.deltaTime;

            Vector3 tmp = rb.velocity + steering;
            rb.velocity = (rb.velocity + tmp).normalized * maxVelocity;

            Quaternion targetRotation = Quaternion.LookRotation(rb.velocity, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);

            _UpdateAlertState();
        }
    }

    public void _UpdateAlertState()
    {
        switch (alertStage)
        {
            case AlertStage.Peaceful:

                if (wanderTimer >= 2f)
                {
                    wanderTimer = 0f;
                    steering = Wander(3f);
                }

                if (playerInFOV || leaderInFOV)
                {
                    alertStage = AlertStage.Intrigued;
                }

                break;

            case AlertStage.Intrigued: 
                
                if (leaderInFOV)
                {
                    // alertLevel++;
                    if (alertLevel >= 100)
                    {
                        alertStage = AlertStage.Alerted;
                    }

                    Vector3 seekLocation = FollowLeader(leader); 
                    steering = Wander(3f) + Seek(seekLocation) * 0.1f;
                    alertStage = AlertStage.Intrigued;
                }

                else if (playerInFOV)
                {
                    alertLevel++; 

                    if (alertLevel >= 100)
                    {
                        alertStage = AlertStage.Alerted; 
                    }
                }

                else
                {
                    alertLevel--;
                    if (alertLevel <= 1)
                        alertStage = AlertStage.Peaceful;
                }

                break;

            case AlertStage.Alerted:

                if (!playerInFOV)
                    alertStage = AlertStage.Intrigued;
                
                else
                {
                    leaderScript.alertLevel = 100;
                    leaderScript.playerInFOV = true;

                    steering = DetermineBehavior(selectedBehaviorOption, leader);
                }

                break;
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

    private Vector3 Wander(float radius)
    {
        Vector3 circleCenter = rb.velocity.normalized * radius;
        Vector3 displacement = new Vector3(UnityEngine.Random.Range(0f, 1f), 0, UnityEngine.Random.Range(0f, 1f)) * radius;
        Vector3 wanderForce = circleCenter + displacement;

        float angle = Vector3.Angle(rb.velocity, wanderForce) * Mathf.Deg2Rad * 0.7f;

        float newVectorX = Mathf.Cos(UnityEngine.Random.Range(0, angle));
        float newVectorY = Mathf.Sin(UnityEngine.Random.Range(0, angle));

        float randVectorX = UnityEngine.Random.Range(0.5f, 1f) * newVectorX - (newVectorX * 0.5f);
        float randVectorY = UnityEngine.Random.Range(0.5f, 1f) * newVectorY - (newVectorY * 0.5f);

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

        Vector3 ahead2 = transform.position + rb.velocity.normalized * radius * 0.5f;

        Vector3 avoidanceForce = ahead - obstacle;

        avoidanceForce = avoidanceForce.normalized * radius;

        return avoidanceForce;
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

    public Vector3 Truncate(Vector3 vector, float maxLength)
    {
        if (vector.magnitude > maxLength)
        {
            vector = vector.normalized * maxLength;
        }
        return vector;
    }

    private Vector3 FindMostThreateningObstacle(List<Collider> gameObjects)
    {
        float smallestDistance = viewRadius;
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

    private void normalizePosition()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        gameObject.transform.Rotate(0, transform.rotation.y, 0);

        spotLight.range = viewRadius * 3f;

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

    float GetSpeed()
    {
        return isBoosted ? maxVelocity * 1.5f : maxVelocity;
    }

    private Vector3 FollowLeader(GameObject leader)
    {
        Vector3 leaderPos = leader.gameObject.transform.position;
        Vector3 offset = transform.position - leaderPos;
        offset.y = 0f;

        Vector3 force = -leaderPos + offset; 

        // Vector3 force = leader.transform.position + new Vector3(10f, 0f, 10f);

        return force;
    }

    private Vector3 Arrive(Vector3 targetPosition)
    {
        Vector3 desiredVelocity = targetPosition - transform.position;
        float distance = desiredVelocity.magnitude;

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

        Quaternion targetRotation = Quaternion.LookRotation(desiredVelocity);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);

        return steering;
    }

    private Vector3 Separation()
    {
        Vector3 force = Vector3.zero;
        int neighborCount = 0;

        Collider[] colliders = Physics.OverlapSphere(transform.position, 10f);
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject != gameObject && collider.CompareTag("Follower"))
            {
                Vector3 direction = transform.position - collider.transform.position;
                force += direction.normalized;
                neighborCount++;
            }
        }

        if (neighborCount != 0)
        {
            force /= neighborCount;
            force *= 10f;
        }

        return force;
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
            steeringForce = (Flee(target.transform.position) + Wander(10f) + Wander(5f)) * 0.3f;
        }

        else if (behavior == "Evade")
        {
            steeringForce = Flee(Evade(target));
        }

        steeringForce.y = 0;
        return steeringForce;
    }

}
