using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class FlexibleControllerPhysical : MonoBehaviourPun
{
  public TextMeshProUGUI debug;
  
  [Header ("Default Controller Widgets")]
  [Tooltip ("Canvas for this set of widgets, so they can be switched on and off as a group")]
  public Canvas defaultControls;
  public TextMeshProUGUI systemID;
  public TextMeshProUGUI controllerID;
  
  private Quaternion restOrientation = Quaternion.identity;
  private Vector3 restPosition = Vector3.zero;
  
  // Set current pose as the "zero" state.
  public void recenter ()
  {
    restOrientation = getOrientation ();
    restPosition = getPosition ();
  }
  
  public void sendButtonDown (string button)
  {
    GetComponent<PhotonView>().RPC ("SendButtonDown", RpcTarget.All, button, systemID.text, controllerID.text);
  }
  public void sendButtonUp (string button)
  {
    GetComponent<PhotonView>().RPC ("SendButtonUp", RpcTarget.All, button);
  }
  
  // Just stubs, since physical controllers don't need to process these.
  [PunRPC]
  public void ControllerStarted (string name) {}
  [PunRPC]
  public void SendButtonDown (string button, string systemID, string controllerID, PhotonMessageInfo info) {}
  [PunRPC]
  public void SendButtonUp (string button, PhotonMessageInfo info) {}
  [PunRPC]
  void SendControlInfo (float x, float y, float z, float w, float px, float py, float pz, PhotonMessageInfo info) {}
  
  private float announcementTimer = 0.0f;
  private float announcementLimit = 2.0f; // The time delay between new announcements of this controller.
  private void announceController ()
  {
    announcementTimer += Time.deltaTime;
    
    if (announcementTimer > announcementLimit)
    {
      if (photonView.IsMine == true || PhotonNetwork.IsConnected == false)
      {
        GetComponent<PhotonView>().RPC ("ControllerStarted", RpcTarget.All, controllerID.text);
      }
      announcementTimer = 0.0f;
    }
  }
  
  private void Start()
  {
    Input.gyro.enabled = true;
    announceController ();
  }
  
  private Quaternion getOrientation ()
  {
    return Quaternion.Euler (90, 0, 90) * Input.gyro.attitude * Quaternion.Euler (180, 180, 0);
  }
  
  private Vector3 getPosition ()
  {
    return Vector3.zero;
  }
  
  void Update()   
  {
    announceController ();
    if (SystemInfo.supportsGyroscope)
    {
//       Debug.Log ("Gyro active, " + photonView.IsMine + " " + PhotonNetwork.IsConnected);
      if (photonView.IsMine == true || PhotonNetwork.IsConnected == false)
      {
        
        // Convert android to unity coordinates.
        Quaternion orientation = getOrientation () * Quaternion.Inverse (restOrientation);
        Vector3 position = getPosition () - restPosition;
        //    debug.text = orientation.ToString ("F5");
        
        GetComponent<PhotonView>().RPC("SendControlInfo", RpcTarget.All, 
                                       orientation.x, orientation.y, orientation.z, orientation.w,
                                       position.x, position.y, position.z);
      }
    }
  }
  
}
