using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SliderChanged : MonoBehaviour
{
  public string sliderName;
  public UnityEvent<string, float> namedOnValueChanged;
    
  public void onValueChanged (float value)
  {
    namedOnValueChanged.Invoke (sliderName, value);
  }
}
