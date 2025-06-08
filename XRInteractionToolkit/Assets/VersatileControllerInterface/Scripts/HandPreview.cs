using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandPreview : MonoBehaviour
{
  public VersatileControllerVirtual controller;
  
  [System.Serializable]
  public class Fingers
  {
    public string name;
    public GameObject armature;
    public Quaternion bendRotationInitial;
    public Quaternion bendRotationFinal;
    
    public Fingers (string n, GameObject g, Quaternion ri, Quaternion rf)
    {
      name = n;
      armature = g;
      bendRotationInitial = ri;
      bendRotationFinal = rf;
    }
  };
  
  public Fingers [] bends;
  
  // Register to receive button press events.
  void Start()
  {
    controller.subscribeNameUpdates (updateName);
    
    // Since each finger can occur multiple times (for each joint),
    // work out the unique names.
    List <string> names = new List <string> ();
    foreach (Fingers f in bends)
    {
      if (!(names.Contains (f.name)))
      {
        names.Add (f.name);
      }
    }
      
    foreach (string name in names)
    {
      controller.subscribeSlider (name, moveFinger);
    }
  }
  
  private void updateName (string n, bool isLeftHanded, string skinName)
  {
  }
  
  public void moveFinger (string name, float value, VersatileControllerVirtual ctl)
  {
    Debug.Log ("Move finger " + name + " " + value);
    foreach (Fingers f in bends)
    {
      if (f.name == name)
      {
        f.armature.transform.localRotation = Quaternion.Lerp (f.bendRotationInitial, f.bendRotationFinal, value);
      }
    }
  }
  
}
