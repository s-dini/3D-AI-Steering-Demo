using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour
{
    public float moveSpeed = 6f;

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
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }
}