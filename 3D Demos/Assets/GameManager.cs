using UnityEngine;

public class GameManager : MonoBehaviour
{
    private bool isMovementPaused = true;
    private float lastClickTime = 0f;
    private float doubleClickThreshold = 0.3f; // Adjust as needed

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Check if the time since the last click is within the double click threshold
            if (Time.time - lastClickTime < doubleClickThreshold)
            {
                // Double click detected
                isMovementPaused = !isMovementPaused;
                HandleMovementPause();
            }
            else
            {
                // Single click detected, update last click time
                lastClickTime = Time.time;
            }
        }
    }

    void HandleMovementPause()
    {
        AgentMovement[] agents = FindObjectsOfType<AgentMovement>();
        foreach (AgentMovement agent in agents)
        {
            agent.isMovementPaused = isMovementPaused;
        }

        FollowerScript[] followers = FindObjectsOfType<FollowerScript>();
        foreach (FollowerScript follower in followers)
        {
            follower.isMovementPaused = isMovementPaused;
        }

        StageFourController[] controllers = FindObjectsOfType<StageFourController>();
        foreach (StageFourController controller in controllers)
        {
            controller.isMovementPaused = isMovementPaused;
        }; 
    }
}
