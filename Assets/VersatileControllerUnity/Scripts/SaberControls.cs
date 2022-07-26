using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaberControls : MonoBehaviour
{
  public Image saberImage;
  
  public Sprite saberOff;
  public Sprite saberOn;
  
  private bool on = false;
  
  public void toggleSaber ()
  {
    on = !on;
    
    if (on)
    {
      saberImage.sprite = saberOn;
    }
    else
    {
      saberImage.sprite = saberOff;
    }
  }
}
