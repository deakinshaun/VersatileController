using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPreview : MonoBehaviour
{
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
  
  // Register to receive button press events.
  void Start()
  {
    foreach (Indicators i in indicators)
    {
      GetComponent <FlexibleControllerVirtual> ().subscribeButtonDown (i.indicatorName, switchLightOn);
      GetComponent <FlexibleControllerVirtual> ().subscribeButtonUp (i.indicatorName, switchLightOff);
    }
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
  
  private void switchLightOn (string name, FlexibleControllerVirtual ctl)
  {
    toggleLight (name, lightOn);
  }
  private void switchLightOff (string name, FlexibleControllerVirtual ctl)
  {
    toggleLight (name, lightOff);
  }
  
}
