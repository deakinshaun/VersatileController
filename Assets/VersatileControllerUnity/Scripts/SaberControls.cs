using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaberControls : MonoBehaviour
{
  public Image saberImage;
  
  public Sprite saberOff;
  public Sprite saberOn;
  
  public VersatileControllerPhysical controller;
  
  private bool on = false;
  
  public void toggleSaber ()
  {
    on = !on;
    
    if (on)
    {
      saberImage.sprite = saberOn;
      controller.sendButtonDown ("Saber");
    }
    else
    {
      saberImage.sprite = saberOff;
      controller.sendButtonUp ("Saber");
    }
  }
}
