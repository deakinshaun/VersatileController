using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

#if PHOTON_UNITY_NETWORKING
using Fusion;
using Fusion.Sockets;
#endif

public class VersatileControllerPhysical : MonoBehaviour
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
#if PHOTON_UNITY_NETWORKING
  public TMP_InputField systemID;
  public TMP_InputField controllerID;
#endif  
  [Tooltip ("Status display")]
#if PHOTON_UNITY_NETWORKING
  public TextMeshProUGUI statusText;
  public Toggle leftHandToggle;
  public Toggle rightHandToggle;
  public TMP_Dropdown skinSelection;
#endif
  
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

  private NetworkRunner networkRunner;
  private PlayerRef networkPlayer;

  private ControllerMode controllerMode;
  
  // Called to initialize controller interface, with details of the system and 
  // controller IDs used.
  public void setPhotonManager (PhotonManagerPhysical pm, string sid, string cid, bool left, string skin, NetworkRunner ns, PlayerRef np)
  {
    networkRunner = ns;
    networkPlayer = np;
    
    setStatus ();
    
    addSkins ();
    directlySetting = true;
    photonManager = pm;
#if PHOTON_UNITY_NETWORKING    
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
#endif
    
    panelVisibility ();

    directlySetting = false;    
  }
  
  public void setControllerMode (ControllerMode cm)
  {
    controllerMode = cm;
  }
  
  // Set current pose as the "zero" state.
  public void recenter ()
  {
    restOrientation = getOrientation ();
    restPosition = getPosition ();
  }
  
  public void sendButtonDown (string button)
  {
#if PHOTON_UNITY_NETWORKING    
    if ((networkRunner?.LocalPlayer == networkPlayer) || (networkRunner?.IsConnectedToServer == false))
    {
      controllerMode.RPC_SendButtonDown (button, systemID.text, controllerID.text);
    }
#endif    
  }
  public void sendButtonUp (string button)
  {
#if PHOTON_UNITY_NETWORKING
    if ((networkRunner?.LocalPlayer == networkPlayer) || (networkRunner?.IsConnectedToServer == false))
    {
      controllerMode.RPC_SendButtonUp (button, systemID.text, controllerID.text);
    }
#endif    
  }
  public void sendSliderChanged (string slider, float value)
  {
#if PHOTON_UNITY_NETWORKING
    if ((networkRunner?.LocalPlayer == networkPlayer) || (networkRunner?.IsConnectedToServer == false))
    {
      controllerMode.RPC_SendSliderChanged (slider, value, systemID.text, controllerID.text);
    }
#endif    
  }
    
  private float announcementTimer = 0.0f;
  private float announcementLimit = 2.0f; // The time delay between new announcements of this controller.
  private void announceController ()
  {
#if PHOTON_UNITY_NETWORKING
    if ((networkRunner?.LocalPlayer == networkPlayer) || (networkRunner?.IsConnectedToServer == false))
    {
      announcementTimer += Time.deltaTime;
      
      if (announcementTimer > announcementLimit)
      {
        controllerMode.RPC_ControllerStarted (controllerID.text, leftHandToggle.isOn, skinSelection.options[skinSelection.value].text);
        announcementTimer = 0.0f;
      }
    }
#endif    
  }
  
  // One of the system/controller IDs have changed. Reconnect.
  public void changeConnection (string value)
  {
#if PHOTON_UNITY_NETWORKING
    if (!directlySetting)
    {
      if (photonManager != null)
      {
        photonManager.updateConnectionDetails (systemID.text, controllerID.text, leftHandToggle.isOn, skinSelection.options[skinSelection.value].text);
        photonManager.reconnect ();
      }
    }
#endif    
  }
  
  private void reportStatus ()
  {
#if PHOTON_UNITY_NETWORKING
    if (networkRunner?.IsConnectedToServer == true)
    {
      statusText.text = "Connected to region: " + networkRunner.SessionInfo.Region;
    }
    else
    {
      statusText.text = "Disconnected";
    }
#endif    
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

#if PHOTON_UNITY_NETWORKING
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
#endif    
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
#if PHOTON_UNITY_NETWORKING
      skinSelection.ClearOptions ();
      skinSelection.AddOptions (options);
#endif
      
      setSkins = true;
    }
  }
  
  private void setStatus ()
  {
#if PHOTON_UNITY_NETWORKING
    if ((networkRunner?.LocalPlayer == networkPlayer) || (networkRunner?.IsConnectedToServer == false))
    {
      // Controls must only be enabled for the active controller - otherwise the imposter for another controller
      // will take over this device.
      defaultControls.gameObject.SetActive (true);
      InputSystem.EnableDevice (InputSystem.GetDevice<UnityEngine.InputSystem.AttitudeSensor>());
      announceController ();
      
      if (useAR)
      {
        ARTrackable = GameObject.Find (ARTrackableName);
      }
      
      addSkins ();
      
      reportStatus ();
      gameObject.SetActive (true);
    }
    else
    {
      gameObject.SetActive (false);
    }
#endif    
  }
  
  void Start ()
  {
    setStatus ();
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
      Quaternion q = InputSystem.GetDevice<UnityEngine.InputSystem.AttitudeSensor>().attitude.ReadValue ();
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

#if PHOTON_UNITY_NETWORKING      
      controllerMode.RPC_SendControlInfo (orientation.x, orientation.y, orientation.z, orientation.w,
                           position.x, position.y, position.z);
#endif      
    }
    else
    {
      if (SystemInfo.supportsGyroscope)
      {
#if PHOTON_UNITY_NETWORKING
    if ((networkRunner?.LocalPlayer == networkPlayer) || (networkRunner?.IsConnectedToServer == false))
        {
          
          // Convert android to unity coordinates.
          Quaternion orientation = Quaternion.Inverse (restOrientation) * getOrientation ();
          Vector3 position = getPosition () - restPosition;
          
          controllerMode.RPC_SendControlInfo (orientation.x, orientation.y, orientation.z, orientation.w,
                               position.x, position.y, position.z);
        }
#endif          
      }
    }
  }

}
