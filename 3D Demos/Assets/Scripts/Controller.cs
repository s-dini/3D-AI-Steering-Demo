using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float targetRadius = 5f;

    private bool isBoosted = false;

    [HideInInspector]
    public bool isMovementPaused = true; 


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
        if (!isMovementPaused)
        {
            Vector3 mousePos = viewCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, viewCamera.transform.position.y));

            transform.LookAt(mousePos + Vector3.up * transform.position.y);
            velocity = new Vector3(mousePos.x, 0, mousePos.z).normalized * moveSpeed;

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, targetRadius);
        }
        

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
        if (!isMovementPaused)
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
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
        float originalSpeed = moveSpeed;
        moveSpeed *= 1.5f; 

        yield return new WaitForSeconds(7f);
        moveSpeed = originalSpeed; 
        isBoosted = false;
    }

    float GetSpeed()
    {
        return isBoosted ? moveSpeed * 1.5f : moveSpeed;
    }
}