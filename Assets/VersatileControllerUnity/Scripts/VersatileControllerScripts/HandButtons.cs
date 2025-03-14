using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Convert hand quick buttons, into slider actions.
public class HandButtons : MonoBehaviour
{
  public Slider littleFinger;
  public Slider ringFinger;
  public Slider middleFinger;
  public Slider indexFinger;
  public Slider thumb;
  
  public void setFingers (string config)
  {
    switch (config)
    {
      case "HandFlat": 
        littleFinger.value = 0.0f;
        ringFinger.value = 0.0f;
        middleFinger.value = 0.0f;
        indexFinger.value = 0.0f;
        thumb.value = 0.0f;
        break;
      case "HandFist": 
        littleFinger.value = 1.0f;
        ringFinger.value = 1.0f;
        middleFinger.value = 1.0f;
        indexFinger.value = 1.0f;
        thumb.value = 1.0f;
        break;
      case "HandPoint": 
        littleFinger.value = 1.0f;
        ringFinger.value = 1.0f;
        middleFinger.value = 1.0f;
        indexFinger.value = 0.0f;
        thumb.value = 1.0f;
        break;
      case "HandThumb": 
        littleFinger.value = 1.0f;
        ringFinger.value = 1.0f;
        middleFinger.value = 1.0f;
        indexFinger.value = 1.0f;
        thumb.value = 0.0f;
        break;
      default:
        Debug.Log ("Unrecognized hand config: " + config + " in HandButtons");
        break;
    }
  }
}
