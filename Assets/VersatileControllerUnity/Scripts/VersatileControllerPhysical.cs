using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class VersatileControllerPhysical : MonoBehaviourPun
{
  public enum Handedness
  {
    LeftHanded,
    RightHanded,
    BothHands
  }
  
  [System.Serializable]
  public class Skins
  {
    public string name;
    public Handedness whichHand;
    public GameObject [] panels;
  }
  
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
  public Toggle leftHandToggle;
  public Toggle rightHandToggle;
  public TMP_Dropdown skinSelection;
  
  [SerializeField]
  public Skins [] skins;
  
  private Quaternion restOrientation = Quaternion.identity;
  private Vector3 restPosition = Vector3.zero;
  
  private PhotonManagerPhysical photonManager;

  private GameObject ARTrackable;
  
  // Used to indicate when this script is directly setting a field in a UI element. Stops
  // event handlers from responding.
  private bool directlySetting = false;
  private bool setSkins = false;
  
  // Called to initialize controller interface, with details of the system and 
  // controller IDs used.
  public void setPhotonManager (PhotonManagerPhysical pm, string sid, string cid, bool left, string skin)
  {
    addSkins ();
    directlySetting = true;
    photonManager = pm;
    systemID.text = sid;
    controllerID.text = cid;
    leftHandToggle.isOn = left;
    rightHandToggle.isOn = !left;

    int option = skinSelection.options.FindIndex(option => option.text == skin);
    if (option < 0)
    {
      option = 0;
    }
    skinSelection.SetValueWithoutNotify (option);
    skinSelection.RefreshShownValue ();
    
    panelVisibility ();

    directlySetting = false;    
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
      GetComponent<PhotonView>().RPC ("SendButtonUp", RpcTarget.All, button, systemID.text, controllerID.text);
    }
  }
  public void sendSliderChanged (string slider, float value)
  {
    if (photonView.IsMine == true || PhotonNetwork.IsConnected == false)
    {
      GetComponent<PhotonView>().RPC ("SendSliderChanged", RpcTarget.All, slider, value, systemID.text, controllerID.text);
    }
  }
  
  // Just stubs, since physical controllers don't need to process these.
  [PunRPC]
  public void ControllerStarted (string name, bool isLeftHanded, string skinName) {}
  [PunRPC]
  public void SendButtonDown (string button, string systemID, string controllerID, PhotonMessageInfo info) {}
  [PunRPC]
  public void SendButtonUp (string button, string systemID, string controllerID, PhotonMessageInfo info) {}
  [PunRPC]
  public void SendSliderChanged (string slider, float value, string systemID, string controllerID, PhotonMessageInfo info) {}
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
        GetComponent<PhotonView>().RPC ("ControllerStarted", RpcTarget.All, controllerID.text, leftHandToggle.isOn, skinSelection.options[skinSelection.value].text);
        announcementTimer = 0.0f;
      }
    }
  }
  
  // One of the system/controller IDs have changed. Reconnect.
  public void changeConnection (string value)
  {
    if (!directlySetting)
    {
      if (photonManager != null)
      {
        photonManager.updateConnectionDetails (systemID.text, controllerID.text, leftHandToggle.isOn, skinSelection.options[skinSelection.value].text);
        photonManager.reconnect ();
      }
    }
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
  
  // Set the visibility of the various panels, based on the current skin.
  // The prefab should have all panels disabled by default. This will try
  // to switch off any named panels, but any unused ones will be left alone.
  private void panelVisibility ()
  {
    // Switch everything off.
    foreach (Skins s in skins)
    {
      Debug.Log ("Disable: " + s.name);
      foreach (GameObject g in s.panels)
      {
        g.SetActive (false);
      }
    }

    // Switch the active skin on.
    foreach (Skins s in skins)
    {
      if ((s.name == skinSelection.options[skinSelection.value].text) && 
          ((s.whichHand == Handedness.BothHands) || ((s.whichHand == Handedness.LeftHanded) == leftHandToggle.isOn)))
      {
        Debug.Log ("Enable: " + s.name);
        foreach (GameObject g in s.panels)
        {
          g.SetActive (true);
        }
      }
    }    
  }
  
  private void addSkins ()
  {
    if (!setSkins)
    {
      List <string> options = new List <string> ();
      foreach (Skins s in skins)
      {
        if (!(options.Contains (s.name)))
        {
          options.Add (s.name);
        }
      }
      skinSelection.ClearOptions ();
      skinSelection.AddOptions (options);
        
      setSkins = true;
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
      
      addSkins ();
      
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
