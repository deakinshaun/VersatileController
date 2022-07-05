using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class VersatileControllerPhysical : MonoBehaviourPun
{
  [Tooltip ("Use ARCore on supported devices for orientation and position tracking")]
  public bool useAR = false;
  public string ARTrackableName;
  
  [Header ("Default Controller Widgets")]
  [Tooltip ("Canvas for this set of widgets, so they can be switched on and off as a group")]
  public Canvas defaultControls;
  public TMP_InputField systemID;
  public TMP_InputField controllerID;
  [Tooltip ("Status display")]
  public TextMeshProUGUI statusText;
  
  private Quaternion restOrientation = Quaternion.identity;
  private Vector3 restPosition = Vector3.zero;
  
  private PhotonManagerPhysical photonManager;

  private GameObject ARTrackable;
  
  // Called to initialize controller interface, with details of the system and 
  // controller IDs used.
  public void setPhotonManager (PhotonManagerPhysical pm, string sid, string cid)
  {
    photonManager = pm;
    systemID.text = sid;
    controllerID.text = cid;
  }
  
  // Set current pose as the "zero" state.
  public void recenter ()
  {
    restOrientation = getOrientation ();
    restPosition = getPosition ();
  }
  
  public void sendButtonDown (string button)
  {
    if (photonView.IsMine == true || PhotonNetwork.IsConnected == false)
    {
      GetComponent<PhotonView>().RPC ("SendButtonDown", RpcTarget.All, button, systemID.text, controllerID.text);
    }
  }
  public void sendButtonUp (string button)
  {
    if (photonView.IsMine == true || PhotonNetwork.IsConnected == false)
    {
      GetComponent<PhotonView>().RPC ("SendButtonUp", RpcTarget.All, button);
    }
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
    if (photonView.IsMine == true || PhotonNetwork.IsConnected == false)
    {
      announcementTimer += Time.deltaTime;
      
      if (announcementTimer > announcementLimit)
      {
        GetComponent<PhotonView>().RPC ("ControllerStarted", RpcTarget.All, controllerID.text);
        announcementTimer = 0.0f;
      }
    }
  }
  
  // One of the system/controller IDs have changed. Reconnect.
  public void changeConnection (string value)
  {
    photonManager.updateConnectionDetails (systemID.text, controllerID.text);
    photonManager.reconnect ();
  }
  
  private void reportStatus ()
  {
    if (PhotonNetwork.IsConnected)
    {
      statusText.text = "Connected to region: " + PhotonNetwork.CloudRegion;
    }
    else
    {
      statusText.text = "Disconnected";
    }
  }
  
  private void Start()
  {
    if (photonView.IsMine == true || PhotonNetwork.IsConnected == false)
    {
      // Controls must only be enabled for the active controller - otherwise the imposter for another controller
      // will take over this device.
      defaultControls.gameObject.SetActive (true);
      Input.gyro.enabled = true;
      announceController ();
      
      if (useAR)
      {
        ARTrackable = GameObject.Find (ARTrackableName);
      }
      
      reportStatus ();
    }
  }
  
  private Quaternion getOrientation ()
  {
    if (useAR && (ARTrackable != null))
    {
      Quaternion q = ARTrackable.transform.rotation * Quaternion.AngleAxis (-90, Vector3.right);
      return new Quaternion (q.x, q.y, q.z, q.w);
    }
    else
    {
      Quaternion q = Input.gyro.attitude;
      return new Quaternion (-q.x, -q.z, -q.y, q.w);
    }
  }
  
  private Vector3 getPosition ()
  {
    if (useAR && (ARTrackable != null))
    {
      Vector3 p = ARTrackable.transform.position;
      return new Vector3 (p.x, p.y, p.z);
    }
    else
    {
      return Vector3.zero;
    }
  }
  
  void Update()   
  {
    announceController ();
    if (useAR && (ARTrackable != null))
    {
      statusText.text = ARTrackable.transform.position.ToString ("F5") + " " + ARTrackable.transform.rotation.ToString ("F5");
      Quaternion orientation = Quaternion.Inverse (restOrientation) * getOrientation ();
      Vector3 position = getPosition () - restPosition;
      
      GetComponent<PhotonView>().RPC("SendControlInfo", RpcTarget.All, 
                                    orientation.x, orientation.y, orientation.z, orientation.w,
                                    position.x, position.y, position.z);
    }
    else
    {
      if (SystemInfo.supportsGyroscope)
      {
        if (photonView.IsMine == true || PhotonNetwork.IsConnected == false)
        {
          
          // Convert android to unity coordinates.
          Quaternion orientation = Quaternion.Inverse (restOrientation) * getOrientation ();
          Vector3 position = getPosition () - restPosition;
          
          GetComponent<PhotonView>().RPC("SendControlInfo", RpcTarget.All, 
                                        orientation.x, orientation.y, orientation.z, orientation.w,
                                        position.x, position.y, position.z);
        }
      }
    }
  }  
}
