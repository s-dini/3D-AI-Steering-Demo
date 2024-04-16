using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float targetRadius = 5f;

    Rigidbody rb;
    Camera viewCamera;
    Vector3 velocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        viewCamera = Camera.main;
    }

    void Update()
    {
        Vector3 mousePos = viewCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, viewCamera.transform.position.y));
        
        transform.LookAt(mousePos + Vector3.up * transform.position.y);
        velocity = new Vector3(mousePos.x, 0, mousePos.z).normalized * moveSpeed;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, targetRadius); 

        /*foreach (var collider in hitColliders)
        {
            if (collider.gameObject.CompareTag("Agent"))
            {
                AgentMovement agent = collider.gameObject.GetComponent<AgentMovement>();
                agent.slow = true; 
            }
        }*/
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }
}