using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Photon.Pun;

public class VersatileControllerVirtual : MonoBehaviour
{
  [System.Serializable]
  public class Skins
  {
    public string name;
    public VersatileControllerPhysical.Handedness whichHand;
    public GameObject [] parts;
  }
  
  public TextMeshProUGUI debug;

  [Tooltip ("Disable this if you want to manually set the position and rotation, using the control input. Otherwise the object this component is attached to will be driven directly by this component")]
  public bool setPose = true;
  
  public Skins [] skins;
  
  // Event for tracking when new controllers are added.
  private static UnityEvent<GameObject> newControllers;
  private static List<GameObject> knownControllers;
  private static Dictionary <GameObject, string> controllerObjects;
  
  private static bool initialized = false;
  private static void initialize ()
  {
    if (!initialized)
    {
      newControllers = new UnityEvent<GameObject> ();
      knownControllers = new List<GameObject> ();
      controllerObjects = new Dictionary <GameObject, string> ();
      initialized = true;
    }
  }
  
  public static void subscribeNewControllers (UnityAction<GameObject> call)
  {
    initialize ();
    newControllers.AddListener (call);
    
    // Inform of all controllers that have already connected.
    foreach (GameObject g in knownControllers)
    {
      newControllers.Invoke (g);
    }
  }
  
  private void setSkin (string skinName, bool isLeftHanded)
  {
    // Switch off all skins
    foreach (Skins s in skins)
    {
      foreach (GameObject g in s.parts)
      {
        g.SetActive (false);
      }
    }
    
    // Enable the active skin.
    foreach (Skins s in skins)
    {
      if ((s.name == skinName) && 
          ((s.whichHand == VersatileControllerPhysical.Handedness.BothHands) || 
          ((s.whichHand == VersatileControllerPhysical.Handedness.LeftHanded) == isLeftHanded)))
      {
        foreach (GameObject g in s.parts)
        {
          g.SetActive (true);
        }
      }
    }
  }
  
  [PunRPC]
  public void ControllerStarted (string name, bool isLeftHanded, string skinName)
  {
    initialize ();
    classInitialize ();
    
    setSkin (skinName, isLeftHanded);
    
    this.gameObject.name = name;
    if (!knownControllers.Contains (this.gameObject))
    {
      // A new controller has been started. Unlikely to get duplicates, but checking anyway.
      knownControllers.Add (this.gameObject);
      controllerObjects[this.gameObject] = name;
      newControllers.Invoke (this.gameObject);
    }
    nameUpdates.Invoke (name, isLeftHanded, skinName);
  }
  
  // Event tracking for button presses.

  private bool classInitialized = false;
  
  private void classInitialize ()
  {
    if (!classInitialized)
    {
      buttonDownEvents = new Dictionary <string, UnityEvent <string, VersatileControllerVirtual>> ();
      buttonUpEvents = new Dictionary <string, UnityEvent <string, VersatileControllerVirtual>> ();
      sliderEvents = new Dictionary <string, UnityEvent <string, float, VersatileControllerVirtual>> ();
      poseEvents = new UnityEvent<GameObject, Quaternion, Vector3> ();
      nameUpdates = new UnityEvent<string, bool, string> ();
      classInitialized = true;
    }
  }
  
  private Dictionary <string, UnityEvent <string, VersatileControllerVirtual>> buttonDownEvents;
  private Dictionary <string, UnityEvent <string, VersatileControllerVirtual>> buttonUpEvents;
  private Dictionary <string, UnityEvent <string, float, VersatileControllerVirtual>> sliderEvents;
  private UnityEvent<GameObject, Quaternion, Vector3> poseEvents;
  private UnityEvent<string, bool, string> nameUpdates;

  public void subscribeNameUpdates (UnityAction <string, bool, string> call)
  {
    classInitialize ();
    nameUpdates.AddListener (call);
  }
  
  // Use this to receive call backs whenever the named button is pressed. 
  // The callback provides the name of the button, so that the callback
  // can be used to subscribe to multiple buttons.
  public void subscribeButtonDown (string button, UnityAction <string, VersatileControllerVirtual> call)
  {
    classInitialize ();
    if (!buttonDownEvents.ContainsKey (button))
    {
      buttonDownEvents[button] = new UnityEvent <string, VersatileControllerVirtual> ();
    }
    buttonDownEvents[button].AddListener (call);
  }
  
  // Use this to receive call backs whenever the named button is released.
  public void subscribeButtonUp (string button, UnityAction <string, VersatileControllerVirtual> call)
  {
    classInitialize ();
    if (!buttonUpEvents.ContainsKey (button))
    {
      buttonUpEvents[button] = new UnityEvent <string, VersatileControllerVirtual> ();
    }
    buttonUpEvents[button].AddListener (call);
  }
  
  public void subscribeSlider (string slider, UnityAction <string, float, VersatileControllerVirtual> call)
  {
    classInitialize ();
    if (!sliderEvents.ContainsKey (slider))
    {
      sliderEvents[slider] = new UnityEvent <string, float, VersatileControllerVirtual> ();
    }
    sliderEvents[slider].AddListener (call);
  }
  
  [PunRPC]
  public void SendButtonDown (string button, string systemID, string controllerID, PhotonMessageInfo info)
  {
    classInitialize ();
    if (buttonDownEvents.ContainsKey (button))
    {
      buttonDownEvents[button].Invoke (button, this);
    }
  }
  
  [PunRPC]
  public void SendButtonUp (string button, string systemID, string controllerID, PhotonMessageInfo info)
  {
    classInitialize ();
    if (buttonUpEvents.ContainsKey (button))
    {
      buttonUpEvents[button].Invoke (button, this);
    }
  }
  [PunRPC]
  public void SendSliderChanged (string slider, float value, string systemID, string controllerID, PhotonMessageInfo info)
  {
    Debug.Log ("Slider: " + slider + " " + value);
    classInitialize ();
    if (sliderEvents.ContainsKey (slider))
    {
      sliderEvents[slider].Invoke (slider, value, this);
    }
  }
  
  // Event tracking for pose updates

  public void subscribePose (UnityAction <GameObject, Quaternion, Vector3> call)
  {
    classInitialize ();
    poseEvents.AddListener (call);
  }

  [PunRPC]
  void SendControlInfo (float x, float y, float z, float w, float px, float py, float pz, PhotonMessageInfo info)
  {
    classInitialize ();
    
    Quaternion o = new Quaternion(x, y, z, w);
    Vector3 p = new Vector3 (px, py, pz);
    
    poseEvents.Invoke (this.gameObject, o, p);
    
    if (setPose)
    {
      transform.localRotation = o;
      transform.localPosition = p;
    }
  }
}
