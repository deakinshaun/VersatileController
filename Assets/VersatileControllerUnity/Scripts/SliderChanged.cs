using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// This class is a mechanism to translate the onValueChanged events
/// produced by the slider class, into named events - so the same
/// function can be called by all sliders, and each source identified.
/// Add this to the slider widget, and fill in the sliderName field. Direct
/// the onValueChanged event to the onValueChanged method in this class.
/// Then direct the namedOnValueChanged event to a function that takes
/// two parameters: a string name for the slider (from the sliderName
/// field), and a float representing the current slider value.
public class SliderChanged : MonoBehaviour
{
  public string sliderName;
  public UnityEvent<string, float> namedOnValueChanged;
    
  public void onValueChanged (float value)
  {
    namedOnValueChanged.Invoke (sliderName, value);
  }
}
