using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;

#if FUSION2
using Fusion;
using Fusion.Sockets;
#endif

public class VersatileControllerPhysical : NetworkBehaviour
{
  [System.Serializable]
  public class Skins
  {
    public string name;
    public VersatileControllerHandedness whichHand;
    public GameObject [] panels;
  }
  
  [Tooltip ("Use ARCore on supported devices for orientation and position tracking")]
  public bool useAR = false;
  public string ARTrackableName;
  
  [Header ("Default Controller Widgets")]
  [Tooltip ("Canvas for this set of widgets, so they can be switched on and off as a group")]
  public Canvas defaultControls;
  #if FUSION2
  public TMP_InputField systemID;
  public TMP_InputField controllerID;
  #endif  
  [Tooltip ("Status display")]
  #if FUSION2
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
    #if FUSION2    
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
  
  // Set current pose as the "zero" state.
  public void recenter ()
  {
    restOrientation = getOrientation ();
    restPosition = getPosition ();
  }
  
  public void sendButtonDown (string button)
  {
    #if FUSION2    
    if ((networkRunner?.LocalPlayer == networkPlayer) || (networkRunner?.IsConnectedToServer == false))
    {
      RPC_SendButtonDown (button, systemID.text, controllerID.text);
    }
    #endif    
  }
  public void sendButtonUp (string button)
  {
    #if FUSION2
    if ((networkRunner?.LocalPlayer == networkPlayer) || (networkRunner?.IsConnectedToServer == false))
    {
      RPC_SendButtonUp (button, systemID.text, controllerID.text);
    }
    #endif    
  }
  public void send2DAxisTouch (BaseEventData data)
  {
    #if FUSION2
    if ((networkRunner?.LocalPlayer == networkPlayer) || (networkRunner?.IsConnectedToServer == false))
    {
      RectTransform rt = ((PointerEventData) data).pointerDrag.transform.parent.GetComponent <RectTransform> ();
      Rect bounds = rt.rect;
      Vector2 localPoint;
      RectTransformUtility.ScreenPointToLocalPointInRectangle(
        rt,
        ((PointerEventData) data).position,
                                                              ((PointerEventData) data).pressEventCamera,
                                                              out localPoint
      );
      string touch = ((PointerEventData) data).pointerDrag.name;
      Vector2 value = new Vector2 (Mathf.InverseLerp(bounds.xMin, bounds.xMax, localPoint.x) * 2.0f - 1.0f,
                                   Mathf.InverseLerp(bounds.yMin, bounds.yMax, localPoint.y) * 2.0f - 1.0f);
      Debug.Log ("Touch " + ((PointerEventData) data).pointerDrag.name + " " + ((PointerEventData) data).position + " " + bounds + " " + value);
      RPC_Send2DAxisTouch (touch, value, systemID.text, controllerID.text);
    }
    #endif    
  }
  public void sendSliderChanged (string slider, float value)
  {
    #if FUSION2
    if ((networkRunner?.LocalPlayer == networkPlayer) || (networkRunner?.IsConnectedToServer == false))
    {
      RPC_SendSliderChanged (slider, value, systemID.text, controllerID.text);
    }
    #endif    
  }
  
  private float announcementTimer = 0.0f;
  private float announcementLimit = 2.0f; // The time delay between new announcements of this controller.
  private void announceController ()
  {
    #if FUSION2
    if ((networkRunner?.LocalPlayer == networkPlayer) || (networkRunner?.IsConnectedToServer == false))
    {
      announcementTimer += Time.deltaTime;
      
      if (announcementTimer > announcementLimit)
      {
        Debug.Log ("Send: started " + controllerID.text + " " + skinSelection.options[skinSelection.value].text);
        RPC_ControllerStarted (controllerID.text, leftHandToggle.isOn, skinSelection.options[skinSelection.value].text);
        announcementTimer = 0.0f;
      }
    }
    #endif    
  }
  
  // One of the system/controller IDs have changed. Reconnect.
  public void changeConnection (string value)
  {
    #if FUSION2
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
    #if FUSION2
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
    
    #if FUSION2
    // Switch the active skin on.
    foreach (Skins s in skins)
    {
      if ((s.name == skinSelection.options[skinSelection.value].text) && 
        ((s.whichHand == VersatileControllerHandedness.BothHands) || ((s.whichHand == VersatileControllerHandedness.LeftHanded) == leftHandToggle.isOn)))
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
      #if FUSION2
      skinSelection.ClearOptions ();
      skinSelection.AddOptions (options);
      #endif
      
      setSkins = true;
    }
  }
  
  private void setStatus ()
  {
    #if FUSION2
    if ((networkRunner?.LocalPlayer == networkPlayer) || (networkRunner?.IsConnectedToServer == false))
    {
      // Controls must only be enabled for the active controller - otherwise the imposter for another controller
      // will take over this device.
      defaultControls.gameObject.SetActive (true);
      if (SystemInfo.supportsGyroscope)
      {
        InputSystem.EnableDevice (InputSystem.GetDevice<UnityEngine.InputSystem.AttitudeSensor>());
      }
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
      Vector3 position = Quaternion.Inverse (restOrientation) *  (getPosition () - restPosition);
      
      #if FUSION2      
      RPC_SendControlInfo (orientation.x, orientation.y, orientation.z, orientation.w,
                                          position.x, position.y, position.z);
      #endif      
    }
    else
    {
      if (SystemInfo.supportsGyroscope)
      {
        #if FUSION2
        if ((networkRunner?.LocalPlayer == networkPlayer) || (networkRunner?.IsConnectedToServer == false))
        {
          
          // Convert android to unity coordinates.
          Quaternion orientation = Quaternion.Inverse (restOrientation) * getOrientation ();
          Vector3 position = getPosition () - restPosition;
          
          RPC_SendControlInfo (orientation.x, orientation.y, orientation.z, orientation.w,
                                              position.x, position.y, position.z);
        }
        #endif          
      }
    }
  }  
  
  #if FUSION2
  [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
  #endif  
  public void RPC_ControllerStarted (string name, bool isLeftHanded, string skinName) 
  {
  }
  
  #if FUSION2
  [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
  #endif  
  public void RPC_SendButtonDown (string button, string systemID, string controllerID) 
  {
  }
  
  #if FUSION2
  [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
  #endif  
  public void RPC_SendButtonUp (string button, string systemID, string controllerID) 
  {
  }
  
  #if FUSION2
  [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
  #endif  
  public void RPC_Send2DAxisTouch (string touch, Vector2 value, string systemID, string controllerID) 
  {
  }
  
  #if FUSION2
  [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
  #endif  
  public void RPC_SendSliderChanged (string slider, float value, string systemID, string controllerID) 
  {
  }
  
  #if FUSION2
  [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
  #endif  
  public void RPC_SendControlInfo (float x, float y, float z, float w, float px, float py, float pz)
  {
  }
    
}
