using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI; 

public class RangeSlider : MonoBehaviour
{
    public Slider rangeSlider;
    public AgentMovement agent; 

    void Start()
    {
        agent.viewRadius = rangeSlider.value;
        rangeSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    void OnSliderValueChanged(float value)
    {
        agent.viewRadius = value;
    }
}
