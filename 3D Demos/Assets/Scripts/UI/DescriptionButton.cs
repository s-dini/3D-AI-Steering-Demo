using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DescriptionButton : MonoBehaviour
{
    public Button button;
    public GameObject infoPanel; 

    void Start()
    {
        button = button.GetComponent<Button>();
        button.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        FindObjectOfType<AudioManager>().Play("Click");
        infoPanel.SetActive(true);
    }
}
