using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Photon.Pun;

public class VersatileControllerVirtual : MonoBehaviour
{
  public TextMeshProUGUI debug;

  [Tooltip ("Disable this if you want to manually set the position and rotation, using the control input. Otherwise the object this component is attached to will be driven directly by this component")]
  public bool setPose = true;
  
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
  
  [PunRPC]
  public void ControllerStarted (string name)
  {
    initialize ();
    
    this.gameObject.name = name;
    if (!knownControllers.Contains (this.gameObject))
    {
      // A new controller has been started. Unlikely to get duplicates, but checking anyway.
      knownControllers.Add (this.gameObject);
      controllerObjects[this.gameObject] = name;
      Debug.Log ("Controller started: " + name + " " + this.gameObject.name);
      newControllers.Invoke (this.gameObject);
    }
  }
  
  // Event tracking for button presses.

  private bool classInitialized = false;
  
  private void classInitialize ()
  {
    if (!classInitialized)
    {
      buttonDownEvents = new Dictionary <string, UnityEvent <string, VersatileControllerVirtual>> ();
      buttonUpEvents = new Dictionary <string, UnityEvent <string, VersatileControllerVirtual>> ();
      poseEvents = new UnityEvent<GameObject, Quaternion, Vector3> ();
      classInitialized = true;
    }
  }
  
  private Dictionary <string, UnityEvent <string, VersatileControllerVirtual>> buttonDownEvents;
  private Dictionary <string, UnityEvent <string, VersatileControllerVirtual>> buttonUpEvents;
  private UnityEvent<GameObject, Quaternion, Vector3> poseEvents;

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
  
  [PunRPC]
  public void SendButtonDown (string button, string systemID, string controllerID, PhotonMessageInfo info)
  {
    classInitialize ();
    if (buttonDownEvents.ContainsKey (button))
    {
      buttonDownEvents[button].Invoke (button, this);
    }
    Debug.Log("Got button down: " + button + " " + info.Sender.ToString () + " " + systemID + " " + controllerID);
  }
  [PunRPC]
  public void SendButtonUp (string button, PhotonMessageInfo info)
  {
    classInitialize ();
    if (buttonUpEvents.ContainsKey (button))
    {
      buttonUpEvents[button].Invoke (button, this);
    }
    Debug.Log("Got button up: " + button + " " + info.Sender.ToString ());
  }
  
  // Event tracking for pose updates

  public void subscribePose (UnityAction <GameObject, Quaternion, Vector3> call)
  {
    classInitialize ();
    poseEvents.AddListener (call);
  }

//   // Returns true if the provided strings match the settings for this controller.
//   private bool validateSource (string sID, string cID)
//   {
//     return true;
//   }
  
  
  [PunRPC]
  void SendControlInfo (float x, float y, float z, float w, float px, float py, float pz, PhotonMessageInfo info)
  {
    classInitialize ();
    
    Quaternion o = new Quaternion(x, y, z, w);
    Vector3 p = new Vector3 (px, py, pz);
    //         Debug.Log("Got controller info: " + o.ToString ("F5") + " " + info.Sender.ToString ());
    
    poseEvents.Invoke (this.gameObject, o, p);
    
    if (setPose)
    {
      transform.localRotation = o;
      transform.localPosition = p;
    }
  }
}
