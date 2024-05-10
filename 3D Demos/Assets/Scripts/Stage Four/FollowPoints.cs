using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPoints : MonoBehaviour
{
    Rigidbody rb;

    private float steeringForce = 2f; 
    private float maxVelocity = 2f;
    private float wanderTimer = 0f; 
    
    private int location = 0;
    private bool reverseLocation = false;
    private Vector3 steering = Vector3.zero;
    private Vector3 nextPos = Vector3.zero; 
    
    public List<Transform> pathPoints = new List<Transform>();



    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        nextPos = CalculateNextPosition(pathPoints);
    }

    // Update is called once per frame
    void Update()
    {
        wanderTimer += Time.deltaTime;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        steering += Seek(nextPos) * Time.deltaTime;

        Vector3 tmp = rb.velocity + steering;
        rb.velocity = (rb.velocity + tmp).normalized * maxVelocity;

        Quaternion targetRotation = Quaternion.LookRotation(rb.velocity, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);

        if (Vector3.Distance(transform.position, nextPos) < .5f)
        {
            nextPos = CalculateNextPosition(pathPoints);
            Debug.Log("yeah baby");
        }
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
        steering = Truncate(steering, maxVelocity);
        steering = steering / rb.mass;
        steering.y = 0;

        return steering;
    }

    Vector3 CalculateNextPosition(List<Transform> targetTransforms)
    {
        Vector3 pos = Vector3.zero;

        if (!reverseLocation)
        {
            if (location == targetTransforms.Count - 1)
            {
                reverseLocation = true;
                pos = targetTransforms[location].position;

            }

            else
            {
                pos = targetTransforms[location + 1].position;
                location++;
            }
        }

        if (reverseLocation)
        {
            if (location == 0)
            {
                reverseLocation = false;
                pos = targetTransforms[location].position;

            }

            else
            {
                pos = targetTransforms[location - 1].position;
                location--;
            }
        }

        return pos;
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
