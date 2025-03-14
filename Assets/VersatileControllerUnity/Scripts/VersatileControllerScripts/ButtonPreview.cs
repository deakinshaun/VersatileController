using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ButtonPreview : MonoBehaviour
{
  [Tooltip ("Label, to use to name the controller")]
  public TextMeshPro controllerName;
  
  [Tooltip ("Colour to use to indicate light is off")]
  public Material lightOff;
  [Tooltip ("Colour to use to indicate light is on")]
  public Material lightOn;
  
  [System.Serializable]
  public class Indicators
  {
    public GameObject indicator;
    public string indicatorName;
  }
  
  public Indicators [] indicators;
  
  public VersatileControllerVirtual controller;
  
  // Register to receive button press events.
  void Start()
  {
    controller.subscribeNameUpdates (updateName);
    foreach (Indicators i in indicators)
    {
      controller.subscribeButtonDown (i.indicatorName, switchLightOn);
      controller.subscribeButtonUp (i.indicatorName, switchLightOff);
    }
  }
  
  private void updateName (string n, bool isLeftHanded, string skinName)
  {
    controllerName.text = n;
  }
  
  // Set the light to the given colour (material)
  private void toggleLight (string name, Material m)
  {
    foreach (Indicators i in indicators)
    {
      if (i.indicatorName == name)
      {
        i.indicator.GetComponent <MeshRenderer> ().material = m;
      }
    }
  }
  
  private void switchLightOn (string name, VersatileControllerVirtual ctl)
  {
    toggleLight (name, lightOn);
  }
  private void switchLightOff (string name, VersatileControllerVirtual ctl)
  {
    toggleLight (name, lightOff);
  }
  
}
