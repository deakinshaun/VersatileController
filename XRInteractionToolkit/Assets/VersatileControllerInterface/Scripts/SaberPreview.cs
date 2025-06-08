using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaberPreview : MonoBehaviour
{
  public GameObject beam;
  public VersatileControllerVirtual controller;
  
  // Register to receive button press events.
  void Start()
  {
    controller.subscribeButtonDown ("Saber", activateSaber);
    controller.subscribeButtonUp ("Saber", deactivateSaber);
  }
  
  public void activateSaber (string name, VersatileControllerVirtual ctl)
  {
    beam.SetActive (true);
  }
  public void deactivateSaber (string name, VersatileControllerVirtual ctl)
  {
    beam.SetActive (false);
  }
}
