using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetScript : MonoBehaviour
{

    void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}
