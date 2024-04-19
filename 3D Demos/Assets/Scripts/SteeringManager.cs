using UnityEngine;

public class SteeringManager : MonoBehaviour
{
    public float maxVelocity = 20f;

    public float steeringForce = 2f;
    public float maxForce = 50f;

    public float T = 3f; 

    [HideInInspector]
    public float slowingRadius = 10f;
    [HideInInspector]
    public bool slow = false;
    [HideInInspector]
    public Rigidbody rb;


    void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>(); 
    }

    public Vector3 Seek(Vector3 target)
    {
        // calculate the direction towards the target
        // velocity = normalize(target - position) * max_velocity
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0;

        // Arrive(target);

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
        steering.y = 0;

        Debug.Log(steering);

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

    public Vector3 Wander(float radius)
    {
        Vector3 circleCenter = rb.velocity.normalized * radius;
        Vector3 displacement = new Vector3(UnityEngine.Random.Range(0f, 1f), 0, UnityEngine.Random.Range(0f, 1f)) * radius;
        Vector3 wanderForce = circleCenter + displacement;

        float angle = Vector3.Angle(rb.velocity, wanderForce) * Mathf.Deg2Rad * 0.7f;

        float newVectorX = Mathf.Cos(UnityEngine.Random.Range(0, angle));
        float newVectorY = Mathf.Sin(UnityEngine.Random.Range(0, angle));

        float randVectorX = UnityEngine.Random.Range(0.1f, 1f) * newVectorX - (newVectorX * 0.5f);
        float randVectorY = UnityEngine.Random.Range(0.1f, 1f) * newVectorY - (newVectorY * 0.5f);

        wanderForce = new Vector3(randVectorX, 0, randVectorY);

        return wanderForce;
    }

    public Vector3 Pursuit(GameObject target)
    {
        Rigidbody rbt = target.GetComponent<Rigidbody>(); 

        Vector3 futurePosition = target.transform.position + rbt.velocity * T;

        Vector3 steering = futurePosition - transform.position;
        steering = Truncate(steering, maxForce);
        steering /= rb.mass;
        Vector3 newVelocity = rb.velocity + steering;
        newVelocity = Truncate(newVelocity, maxVelocity);
        Vector3 newPosition = transform.position + newVelocity * Time.fixedDeltaTime;
        newPosition.y = 0;

        return Seek(newPosition); 
    }

    public Vector3 Evade(GameObject target)
    {
        Rigidbody rbt = target.GetComponent<Rigidbody>();
        
        Vector3 distance = target.transform.position - transform.position;
        int updatesAhead = Mathf.RoundToInt(distance.magnitude / maxVelocity);
        Vector3 futurePosition = target.transform.position + rbt.velocity * updatesAhead;
        return Flee(futurePosition);
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