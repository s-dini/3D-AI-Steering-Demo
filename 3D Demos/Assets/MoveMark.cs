using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveMark : MonoBehaviour
{
    public Transform agent; 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(agent.position.x, agent.position.y + 2f, agent.position.z);
    }
}
