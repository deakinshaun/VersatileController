using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Photon.Pun;

public class FlexibleControllerVirtual : MonoBehaviour
{
    public TextMeshProUGUI debug;

  // Event for tracking when new controllers are added.
  private static UnityEvent<GameObject> newControllers;

  private static bool initialized = false;
  private static void initialize ()
  {
    if (!initialized)
    {
      newControllers = new UnityEvent<GameObject> ();
      initialized = true;
    }
  }
    
  public static void subscribeNewControllers (UnityAction<GameObject> call)
  {
    initialize ();
    newControllers.AddListener (call);
  }
    
    // Returns true if the provided strings match the settings for this controller.
    private bool validateSource (string sID, string cID)
    {
      return true;
    }

    [PunRPC]
    public void ControllerStarted (string name)
    {
      this.gameObject.name = name;
      Debug.Log ("Controller started: " + name + " " + this.gameObject.name);
      newControllers.Invoke (this.gameObject);
    }
    
    [PunRPC]
    public void SendButtonDown (string button, string systemID, string controllerID, PhotonMessageInfo info)
    {
        Debug.Log("Got button down: " + button + " " + info.Sender.ToString () + " " + systemID + " " + controllerID);
    }
    [PunRPC]
    public void SendButtonUp (string button, PhotonMessageInfo info)
    {
        Debug.Log("Got button up: " + button + " " + info.Sender.ToString ());
    }
    
    [PunRPC]
    void SendControlInfo (float x, float y, float z, float w, PhotonMessageInfo info)
    {
        Quaternion o = new Quaternion(x, y, z, w);
//         Debug.Log("Got controller info: " + o.ToString ("F5") + " " + info.Sender.ToString ());
        transform.rotation = o;
    }
}
