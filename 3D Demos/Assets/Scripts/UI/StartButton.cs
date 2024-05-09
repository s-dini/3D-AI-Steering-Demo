using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 

public class StartButton : MonoBehaviour
{
    public Button start;
    public int sceneIndex; 

    void Start()
    {
        start = start.GetComponent<Button>();
        start.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        FindObjectOfType<AudioManager>().Play("Click");
        SceneManager.LoadScene(sceneIndex); 
    }
}
