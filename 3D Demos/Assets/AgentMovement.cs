using System;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class AgentMovement : MonoBehaviour
{
    public Camera mainCamera;

    public string targetTag; 

    public bool canSeek;
    public bool canFlee; 

    public float maxVelocity = 20f;
    public float radius = 20f;

    public float steeringForce = 2f;
    public float maxForce = 50f;

    public float displacementRadius = 3f; 

    float initialVelocityX;
    float initialVelocityZ;

    [HideInInspector]
    public float slowingRadius = 10f;
    [HideInInspector]
    public bool slow = false;
    [HideInInspector]
    public Rigidbody rb;


    private bool completed = false;

    void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }   
    
    void Start()
    {
        // initialVelocityX = UnityEngine.Random.Range(-maxVelocity, maxVelocity);
        //  initialVelocityZ = UnityEngine.Random.Range(-maxVelocity, maxVelocity);

        initialVelocityX = 1f;
        initialVelocityZ = 5f;
    }

    public void Steering()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.tag == targetTag )
            {
                if (canSeek)
                {
                    Seek( hitCollider.gameObject.transform );
                }

                else if (canFlee)
                {
                    Flee( hitCollider.gameObject.transform );
                }

                // Controller player = hitCollider.gameObject.GetComponent<Controller>();
            }
        }
    }

    public void Seek( Transform target)
    {
        // calculate the direction towards the target
        // velocity = normalize(target - position) * max_velocity
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;

        Arrive(target.position);

        Vector3 desiredVelocity = direction * maxVelocity;

        /* 
         * steering = truncate (steering, max_force)
         * steering = steering / mass
         * velocity = truncate (velocity + steering , max_speed)
         * position = position + velocity
         */

        Vector3 steering = (desiredVelocity - rb.velocity) * steeringForce;
        steering = Truncate(steering, maxForce);
        steering = steering / rb.mass;

        Vector3 newVelocity = rb.velocity + steering;

        // truncate the new velocity to ensure it doesn't exceed maxVelocity (normalize) 
        newVelocity = Truncate(newVelocity, maxVelocity);

        // update position
        Vector3 newPosition = transform.position + newVelocity * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        // rotate the "head" of the agent to look at the target
        //transform.LookAt(newPosition); 

        // Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        // transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
    }

    public void Flee( Transform target )
    {
        Vector3 direction = (transform.position - target.position).normalized;
        direction.y = 0;

        // desired_velocity = normalize(target - position) * max_velocity
        Vector3 desiredVelocity = (direction * maxVelocity);

        Vector3 steering = (desiredVelocity - rb.velocity) * steeringForce;
        steering = Truncate(steering, maxForce);
        steering = steering / rb.mass;

        Vector3 newVelocity = (rb.velocity + steering);

        // truncate the new velocity to ensure it doesn't exceed maxVelocity (normalize) 
        newVelocity = Truncate(newVelocity, maxVelocity);

        // update position
        Vector3 newPosition = transform.position + newVelocity * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        // rotate the "head" of the agent to look at the target 
        //transform.LookAt(newPosition); 

        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
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

    public Vector3 Wander()
    {
        Vector3 circleCenter = rb.velocity.normalized * displacementRadius;
        Vector3 displacement = new Vector3(UnityEngine.Random.Range(0f, 1f), 0, UnityEngine.Random.Range(0f, 1f));
        Vector3 wanderForce = circleCenter + displacement;

        float angle = Vector3.Angle(rb.velocity, wanderForce);
        
        float newVectorX = Mathf.Cos(UnityEngine.Random.Range(0, angle));
        float newVectorY = Mathf.Sin(UnityEngine.Random.Range(0, angle));

        float randVectorX = UnityEngine.Random.Range(0f, 1f) * newVectorX - (newVectorX * 0.5f);
        float randVectorY = UnityEngine.Random.Range(0f, 1f) * newVectorY - (newVectorY * 0.5f);

        wanderForce = new Vector3(randVectorX, 0, randVectorY);

        return wanderForce; 

        /*float randomAngle = (UnityEngine.Random.Range(0, angle));
        
        Vector3 result = randomAngle * rb.velocity;

        float usedAngle = UnityEngine.Random.Range(0f, 1f) * angle - (angle * 0.5f); Debug.Log(usedAngle);
        
        // wanderAngle += (Math.random() * ANGLE_CHANGE) - (ANGLE_CHANGE * .5);
        Quaternion randomDir = Quaternion.AngleAxis(UnityEngine.Random.Range(0f, 1f) * angle - (angle * 0.5f), Vector3.up);

        Vector3 randomDire = new Vector3(Mathf.Sin(usedAngle), 0, Mathf.Cos(usedAngle));*/ 


    }

    public Vector3 Truncate(Vector3 vector, float maxLength)
    {
        if (vector.magnitude > maxLength)
        {
            vector = vector.normalized * maxLength;
        }
        return vector;
    }
}
