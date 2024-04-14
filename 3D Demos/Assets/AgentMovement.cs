using System;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class AgentMovement : MonoBehaviour
{
    public Camera mainCamera;
    public Transform player;

    public bool canSeek;
    public bool canFlee; 

    public float maxVelocity = 20f;
    public float radius;

    public float steeringForce = 2f;
    public float maxForce = 50f;

    float initialVelocityX;
    float initialVelocityZ; 


    private Rigidbody rb;

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

    void FixedUpdate()
    {
        rb.velocity = new Vector3(initialVelocityX, 0f, initialVelocityZ);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.tag == "Player")
            {
                if (canSeek)
                {
                    Seek();
                }

                else if (canFlee)
                {
                    Flee();
                }
            }
        }
    }

    private void Seek()
    {
        // calculate the direction towards the player
        // velocity = normalize(target - position) * max_velocity
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        // desired_velocity = normalize(target - position) * max_velocity
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

        // rotate the "head" of the agent to look at the player
        //transform.LookAt(newPosition); 

        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
    }

    private void Flee()
    {
        Vector3 direction = (transform.position - player.position).normalized;
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

        // rotate the "head" of the agent to look at the player
        //transform.LookAt(newPosition); 

        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
    }

    private Vector3 Truncate(Vector3 vector, float maxLength)
    {
        if (vector.magnitude > maxLength)
        {
            vector = vector.normalized * maxLength;
        }
        return vector;
    }

}
