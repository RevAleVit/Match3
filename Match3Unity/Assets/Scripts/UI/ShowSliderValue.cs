using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowSliderValue : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Text text;

    public void ChangeValue()
    {
        text.text = slider.value + "X" + slider.value;
    }
}
