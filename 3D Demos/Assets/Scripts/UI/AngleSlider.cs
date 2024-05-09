using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class AngleSlider : MonoBehaviour
{
    public Slider angleSlider;
    public AgentMovement agent;

    void Start()
    {
        agent.viewAngle = angleSlider.value;
        angleSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    void OnSliderValueChanged(float value)
    {
        agent.viewAngle = value;
    }
}
