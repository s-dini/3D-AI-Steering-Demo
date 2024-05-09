using UnityEngine;

public class ExitGame : MonoBehaviour
{
    KeyCode exitKey = KeyCode.Escape;

    void Update()
    {
        if (Input.GetKey(exitKey))
        {
            Application.Quit();
        }
    }
}
