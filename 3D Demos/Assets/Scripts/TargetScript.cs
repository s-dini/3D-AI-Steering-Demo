using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetScript : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Platform"))
        {
            FindObjectOfType<AudioManager>().Play("Obtained");
            Destroy(gameObject, 0.1f);
        }
    }
}
