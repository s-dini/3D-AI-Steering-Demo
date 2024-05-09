using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ClickOut : MonoBehaviour
{
    public Button exitButton;
    public GameObject infoPanel;

    // Start is called before the first frame update
    void Start()
    {
        exitButton = exitButton.GetComponent<Button>();
        exitButton.onClick.AddListener(TaskOnClick);
    }

    // Update is called once per frame
    void TaskOnClick()
    {
        FindObjectOfType<AudioManager>().Play("Click");
        infoPanel.SetActive(false); 
    }
}
