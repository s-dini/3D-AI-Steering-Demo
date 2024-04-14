using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerA_Movement : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 5.0f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        rb.MovePosition(transform.position + moveDirection * moveSpeed * Time.deltaTime);
    }

    void Update()
    {
        Vector3 moveDirection = gameObject.transform.position;
        
        if (moveDirection != Vector3.zero)
        {
            // arctan function returns an angle
            // atan TWO is used to prevent ambiguity when returning a value 
            // float angle = Mathf.Atan2(moveDirection.z, moveDirection.x) * Mathf.Rad2Deg;
            // transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);

            // Quaternion rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            // transform.rotation = rotation; 
        }
    }
}
