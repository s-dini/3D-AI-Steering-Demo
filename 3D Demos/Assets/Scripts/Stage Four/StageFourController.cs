using UnityEngine;
using System.Collections;

public class StageFourController : MonoBehaviour
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
        Quaternion target = Quaternion.Euler(0f, transform.rotation.y, 0f);

        if (!isMovementPaused)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 targetPoint = hit.point;
                Vector3 moveDirection = (targetPoint - transform.position).normalized;
                rb.MovePosition(transform.position + moveDirection * moveSpeed * Time.deltaTime);
            }
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