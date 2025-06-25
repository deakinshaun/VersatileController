using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

#if FUSION2
using Fusion;
#endif

//////////////////////////////////////////////////////////////////
// For registering the controller with the unity input system.
#if UNITY_XR_INSTALLED
using UnityEngine.Scripting;

using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem.Utilities;

using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

using System.Runtime.InteropServices;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

#if !FUSION2    
public class NetworkBehaviour : MonoBehaviour
{
}
#endif

//////////////////////////////////////////////////////////////////

// This is the application side of the versatile controller. Use the public functions provided to subscribe
// to events from the controller (or if appropriate) to poll the current state of controls.
public class VersatileControllerVirtual : NetworkBehaviour
{
  [System.Serializable]
  public class Skins
  {
    public string name;
    public VersatileControllerHandedness whichHand;
    public GameObject [] parts;
  }
  
  #if FUSION2  
  public TextMeshProUGUI debug;
  #endif
  
  [Tooltip ("Disable this if you want to manually set the position and rotation, using the control input. Otherwise the object this component is attached to will be driven directly by this component")]
  public bool setPose = true;
  
  public Skins [] skins;
  
  public bool leftHanded = false;
  
  // Input system state and controller.
  #if UNITY_XR_INSTALLED  
  XRController controllerDevice = null;
  VersatileControllerState controllerState;
  #endif  
  
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
  
  // Register to receive a callback whenever a new controller connects. The callback
  // will be provided with the gameobject representing the controller. This gameobject
  // will have a VersatileControllerVirtual component. This method is static, so you
  // don't need any existing controller before you can register to learn about new
  // controllers.
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
  
  // Set the skin and handedness for a given controller representation. The 
  // appropriate skin needs to be part of the virtual controller prefab.
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
        ((s.whichHand == VersatileControllerHandedness.BothHands) || 
        ((s.whichHand == VersatileControllerHandedness.LeftHanded) == isLeftHanded)))
      {
        foreach (GameObject g in s.parts)
        {
          g.SetActive (true);
        }
      }
    }
  }
  
  // This function is called (remotely) by the physical controller whenever the controller
  // application starts.
  public void ControllerStarted (string name, bool isLeftHanded, string skinName)
  {
    initialize ();
    classInitialize ();
    
    leftHanded = isLeftHanded;
    setSkin (skinName, isLeftHanded);
    this.gameObject.name = name;
    // Can also be invoked by a keep alive message.
    if (!knownControllers.Contains (this.gameObject))
    {
      // A new controller has been started. Unlikely to get duplicates, but checking anyway.
      knownControllers.Add (this.gameObject);
      controllerObjects[this.gameObject] = name;
      newControllers.Invoke (this.gameObject);
      
      // Register to provide data via the input system.
      #if UNITY_XR_INSTALLED
      InputDeviceCharacteristics christic;
      string handname;
      if (isLeftHanded)
      {
        christic = XRInputTrackingAggregator.Characteristics.leftController;
        handname = "LeftHand";
      }
      else
      {
        christic = XRInputTrackingAggregator.Characteristics.rightController;
        handname = "RightHand";
      }
      Debug.Log ("Starting controller: " + handname);
      var desc = new InputDeviceDescription
      {
        product = name,
        capabilities = new XRDeviceDescriptor
        {
          deviceName = $"{name} - {handname}",
          characteristics = christic,
        }.ToJson (),
      };
      try 
      {
        controllerDevice = InputSystem.AddDevice (desc) as XRController;
        InputSystem.SetDeviceUsage (controllerDevice, handname);
        controllerState.Reset ();
      }
      catch (ArgumentException)
      {
        Debug.Log ("Failed to register an input system controller device named: " + name);
        controllerDevice = null;
      }
      #endif      
    }
    
    nameUpdates.Invoke (name, isLeftHanded, skinName);
  }
  
  // Event tracking for button presses.
  
  private bool classInitialized = false;
  
  // This function ensures that all data structures are initialized. Each internal
  // function calls this, so that initialization state is guaranteed, regardless of 
  // the Unity initialization sequence.
  private void classInitialize ()
  {
    if (!classInitialized)
    {
      buttonDownEvents = new Dictionary <string, UnityEvent <string, VersatileControllerVirtual>> ();
      buttonUpEvents = new Dictionary <string, UnityEvent <string, VersatileControllerVirtual>> ();
      touchEvents = new Dictionary <string, UnityEvent <string, Vector2, VersatileControllerVirtual>> ();
      sliderEvents = new Dictionary <string, UnityEvent <string, float, VersatileControllerVirtual>> ();
      
      allButtonDownEvents = new UnityEvent <string, VersatileControllerVirtual> ();
      allButtonUpEvents = new UnityEvent <string, VersatileControllerVirtual> ();
      allTouchEvents = new UnityEvent <string, Vector2, VersatileControllerVirtual> ();
      allSliderEvents = new UnityEvent <string, float, VersatileControllerVirtual> ();
      
      buttonState = new Dictionary <string, bool> ();
      touchState = new Dictionary <string, Vector2> ();
      sliderState = new Dictionary <string, float> ();
      
      poseEvents = new UnityEvent<GameObject, Quaternion, Vector3> ();
      nameUpdates = new UnityEvent<string, bool, string> ();
      classInitialized = true;
    }
  }
  
  // Dictionaries to map button/slider names to various callbacks and state data.
  private Dictionary <string, UnityEvent <string, VersatileControllerVirtual>> buttonDownEvents;
  private UnityEvent <string, VersatileControllerVirtual> allButtonDownEvents;
  private Dictionary <string, UnityEvent <string, VersatileControllerVirtual>> buttonUpEvents;
  private UnityEvent <string, VersatileControllerVirtual> allButtonUpEvents;
  private Dictionary <string, bool> buttonState;
  
  private Dictionary <string, UnityEvent <string, Vector2, VersatileControllerVirtual>> touchEvents;
  private UnityEvent <string, Vector2, VersatileControllerVirtual> allTouchEvents;
  private Dictionary <string, Vector2> touchState;
  
  private Dictionary <string, UnityEvent <string, float, VersatileControllerVirtual>> sliderEvents;
  private UnityEvent <string, float, VersatileControllerVirtual> allSliderEvents;
  private Dictionary <string, float> sliderState;
  
  private UnityEvent<GameObject, Quaternion, Vector3> poseEvents;
  private UnityEvent<string, bool, string> nameUpdates;
  
  // Register to receive a callback whenever the name of the controller is updated.
  public void subscribeNameUpdates (UnityAction <string, bool, string> call)
  {
    classInitialize ();
    nameUpdates.AddListener (call);
  }
  
  // Use this to receive call backs whenever the named button is pressed. 
  // The callback provides the name of the button, so that the callback
  // can be used to subscribe to multiple buttons.
  // If button is null, then subscribe to all button down events.
  public void subscribeButtonDown (string button, UnityAction <string, VersatileControllerVirtual> call)
  {
    classInitialize ();
    if ((button != null) && (!buttonDownEvents.ContainsKey (button)))
    {
      buttonDownEvents[button] = new UnityEvent <string, VersatileControllerVirtual> ();
      buttonState[button] = false;
    }
    
    if (button == null)
    {
      allButtonDownEvents.AddListener (call);
    }
    else
    {
      buttonDownEvents[button].AddListener (call);
    }
  }
  
  // Use this to receive call backs whenever the named button is released.
  public void subscribeButtonUp (string button, UnityAction <string, VersatileControllerVirtual> call)
  {
    classInitialize ();
    if ((button != null) && (!buttonUpEvents.ContainsKey (button)))
    {
      buttonUpEvents[button] = new UnityEvent <string, VersatileControllerVirtual> ();
      buttonState[button] = false;
    }
    
    if (button == null)
    {
      allButtonUpEvents.AddListener (call);
    }
    else
    {
      buttonUpEvents[button].AddListener (call);
    }
  }
  
  // Subscribe to controller events.
  public void subscribeSlider (string slider, UnityAction <string, float, VersatileControllerVirtual> call)
  {
    classInitialize ();
    if ((slider != null) && (!sliderEvents.ContainsKey (slider)))
    {
      sliderEvents[slider] = new UnityEvent <string, float, VersatileControllerVirtual> ();
      sliderState[slider  ] = 0.0f;
    }
    
    if (slider == null)
    {
      allSliderEvents.AddListener (call);
    }
    else
    {
      sliderEvents[slider].AddListener (call);
    }
  }
  
  // Called from the physical controller to indicate a button has been pressed.
  public void SendButtonDown (string button, string systemID, string controllerID)
  {
    classInitialize ();
    if (buttonDownEvents.ContainsKey (button))
    {
      buttonState[button] = true;
      buttonDownEvents[button].Invoke (button, this);
    }
    allButtonDownEvents.Invoke (button, this);
    
    // Manage input system, on specific controls.
    #if UNITY_XR_INSTALLED
    if (controllerDevice != null)
    {
      if (button == "Trigger")
      {
        controllerState.trigger = 1.0f;
        controllerState.WithButton (VersatileControllerState.ControllerButton.TriggerButton, true);
        InputState.Change(controllerDevice, controllerState);
      }
      if (button == "Grip")
      {
        controllerState.grip = 1.0f;
        controllerState.WithButton (VersatileControllerState.ControllerButton.GripButton, true);
        InputState.Change(controllerDevice, controllerState);
      }
      if ((button == "A") || (button == "X"))
      {
        controllerState.WithButton (VersatileControllerState.ControllerButton.PrimaryButton, true);
        InputState.Change(controllerDevice, controllerState);
      }
      if ((button == "B") || (button == "Y"))
      {
        controllerState.WithButton (VersatileControllerState.ControllerButton.SecondaryButton, true);
        InputState.Change(controllerDevice, controllerState);
      }
      if (button == "Menu")
      {
        controllerState.WithButton (VersatileControllerState.ControllerButton.MenuButton, true);
        InputState.Change(controllerDevice, controllerState);
      }
      if (button == "Primary2DAxisTouch")
      {
        controllerState.WithButton (VersatileControllerState.ControllerButton.Primary2DAxisTouch, true);
        InputState.Change(controllerDevice, controllerState);
      }
      if (button == "Secondary2DAxisTouch")
      {
        controllerState.WithButton (VersatileControllerState.ControllerButton.Secondary2DAxisTouch, true);
        InputState.Change(controllerDevice, controllerState);
      }
    }
    #endif    
  }
  
  // Called from the physical controller to indicate a button has been released.
  public void SendButtonUp (string button, string systemID, string controllerID)
  {
    classInitialize ();
    if (buttonUpEvents.ContainsKey (button))
    {
      buttonState[button] = false;
      buttonUpEvents[button].Invoke (button, this);
    }
    allButtonUpEvents.Invoke (button, this);
    
    // Manage input system, on specific controls.
    #if UNITY_XR_INSTALLED
    if (controllerDevice != null)
    {
      if (button == "Trigger")
      {
        controllerState.trigger = 0.0f;
        controllerState.WithButton (VersatileControllerState.ControllerButton.TriggerButton, false);
        InputState.Change(controllerDevice, controllerState);
      }
      if (button == "Grip")
      {
        controllerState.grip = 0.0f;
        controllerState.WithButton (VersatileControllerState.ControllerButton.GripButton, false);
        InputState.Change(controllerDevice, controllerState);
      }
      if ((button == "A") || (button == "X"))
      {
        controllerState.WithButton (VersatileControllerState.ControllerButton.PrimaryButton, false);
        InputState.Change(controllerDevice, controllerState);
      }
      if ((button == "B") || (button == "Y"))
      {
        controllerState.WithButton (VersatileControllerState.ControllerButton.SecondaryButton, false);
        InputState.Change(controllerDevice, controllerState);
      }
      if (button == "Menu")
      {
        controllerState.WithButton (VersatileControllerState.ControllerButton.MenuButton, false);
        InputState.Change(controllerDevice, controllerState);
      }
      if (button == "Primary2DAxisTouch")
      {
        controllerState.WithButton (VersatileControllerState.ControllerButton.Primary2DAxisTouch, false);
        controllerState.primary2DAxis = Vector2.zero;
        InputState.Change(controllerDevice, controllerState);
      }
      if (button == "Secondary2DAxisTouch")
      {
        controllerState.WithButton (VersatileControllerState.ControllerButton.Secondary2DAxisTouch, false);
        controllerState.secondary2DAxis = Vector2.zero;
        InputState.Change(controllerDevice, controllerState);
      }
    }
    #endif    
  }
  
  // Called from the physical controller to indicate a 2D axis value has changed.
  public void Send2DAxisTouch (string touch, Vector2 value, string systemID, string controllerID)
  {
    classInitialize ();
    if (touchEvents.ContainsKey (touch))
    {
      touchState[touch] = value;
      touchEvents[touch].Invoke (touch, value, this);
    }
    allTouchEvents.Invoke (touch, value, this);
    
    #if UNITY_XR_INSTALLED
    if (controllerDevice != null)
    {
      if (touch == "Primary2DAxis")
      {
        controllerState.primary2DAxis = value;
        InputState.Change(controllerDevice, controllerState);
      }
      if (touch == "Secondary2DAxis")
      {
        controllerState.secondary2DAxis = value;
        InputState.Change(controllerDevice, controllerState);
      }
    }
    #endif    
  }
  
  // Called from the physical controller to indicate a slider value has changed.
  public void SendSliderChanged (string slider, float value, string systemID, string controllerID)
  {
    classInitialize ();
    if (sliderEvents.ContainsKey (slider))
    {
      sliderState[slider] = value;
      sliderEvents[slider].Invoke (slider, value, this);
    }
    allSliderEvents.Invoke (slider, value, this);
  }
  
  // State checking.
  
  // Returns the state of the given button. Returns false if the button has
  // never provided a state update, or doesn't exist.
  public bool getButtonState (string button)
  {
    if (buttonState.ContainsKey (button))
    {
      return buttonState[button];
    }
    return false;
  }
  
  // Returns the last known value of the given slider. Returns 0 if the slider
  // doesn't exist or has never provided any value updates.
  public float getSliderState (string slider)
  {
    if (sliderState.ContainsKey (slider))
    {
      return sliderState[slider];
    }
    return 0.0f;
  }
  
  // Event tracking for pose updates
  
  // Subscribe to updates whenever the physical controller pose changes (i.e. it is moved).
  public void subscribePose (UnityAction <GameObject, Quaternion, Vector3> call)
  {
    classInitialize ();
    poseEvents.AddListener (call);
  }
  
  // Called from the physical controller to communicate pose updates.
  public void SendControlInfo (float x, float y, float z, float w, float px, float py, float pz)
  {
    classInitialize ();
    
    Quaternion o = new Quaternion(x, y, z, w);
    Vector3 p = new Vector3 (px, py, pz);
    
    poseEvents.Invoke (this.gameObject, o, p);
    
    if (setPose)
    {
      transform.localRotation = o;
      transform.localPosition = p;
      
      // Update input system.
      #if UNITY_XR_INSTALLED
      if (controllerDevice != null)
      {
        controllerState.deviceRotation = o;
        controllerState.devicePosition = p;
        controllerState.isTracked = true;
        controllerState.trackingState = (int) (InputTrackingState.Position | InputTrackingState.Rotation);
        InputState.Change(controllerDevice, controllerState);
      }
      #endif      
    }
  }
  
  void OnDestroy()
  {
    #if UNITY_XR_INSTALLED
    controllerState.isTracked = false;
    controllerState.trackingState = default;
    Debug.Log ("Disabling versatile controller");
    if (controllerDevice != null)
    {
      InputState.Change (controllerDevice, controllerState);
    }
    #endif    
  }
  
  #if FUSION2
  [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
  #endif  
  public void RPC_ControllerStarted (string name, bool isLeftHanded, string skinName) 
  {
    // Debug.Log ("Controller started: " + name + " - " + skinName);
    bool shouldShow = true;
#if FUSION2    
    shouldShow = Runner.gameObject.GetComponent <PhotonManagerVirtual> ().showControllerRepresentations;
#endif    
    if (!shouldShow)
    {
      skinName = "None";
    }
    ControllerStarted (name, isLeftHanded, skinName);
  }
  
  #if FUSION2
  [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
  #endif  
  public void RPC_SendButtonDown (string button, string systemID, string controllerID) 
  {
    SendButtonDown (button, systemID, controllerID);
  }
  
  #if FUSION2
  [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
  #endif  
  public void RPC_SendButtonUp (string button, string systemID, string controllerID) 
  {
    SendButtonUp (button, systemID, controllerID);
  }
  
  #if FUSION2
  [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
  #endif  
  public void RPC_Send2DAxisTouch (string touch, Vector2 value, string systemID, string controllerID) 
  {
    Send2DAxisTouch (touch, value, systemID, controllerID);
  }
  
  #if FUSION2
  [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
  #endif  
  public void RPC_SendSliderChanged (string slider, float value, string systemID, string controllerID) 
  {
    SendSliderChanged (slider, value, systemID, controllerID);
  }
  
  #if FUSION2
  [Rpc(RpcSources.InputAuthority, RpcTargets.All, Channel = RpcChannel.Unreliable)]
  #endif  
  public void RPC_SendControlInfo (float x, float y, float z, float w, float px, float py, float pz)
  {
    SendControlInfo (x, y, z, w, px, py, pz);
  }
  
  
}

#if UNITY_XR_INSTALLED
// Set up the versatile controller, as a registered controller for the input system.
#if UNITY_EDITOR
[InitializeOnLoad]
#endif
[Preserve]
public static class VersatileControllerLayoutLoader
{
  [Preserve]
  static VersatileControllerLayoutLoader()
  {
    RegisterInputLayouts();
  }
  
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad), Preserve]
  public static void Initialize()
  {
    // Will execute the static constructor as a side effect.
  }
  
  static void RegisterInputLayouts()
  {
    Debug.Log ("Registering layouts for versatile controller");
    InputSystem.RegisterLayout<VersatileController>(
      matches: new InputDeviceMatcher()
      .WithProduct(nameof (VersatileController)));

    InputSystem.RegisterLayout<VersatileControllerHead>(
      matches: new InputDeviceMatcher()
      .WithProduct(nameof (VersatileControllerHead)));

    InputSystem.RegisterLayout<VersatileControllerFeet>(
      matches: new InputDeviceMatcher()
      .WithProduct(nameof (VersatileControllerFeet)));
  }
}

[StructLayout(LayoutKind.Explicit, Size = 63)]
public struct VersatileControllerState : IInputStateTypeInfo
{
  public enum ControllerButton { PrimaryButton, PrimaryTouch, SecondaryButton, SecondaryTouch, GripButton, TriggerButton, MenuButton, Primary2DAxisClick, Primary2DAxisTouch, Secondary2DAxisClick, Secondary2DAxisTouch, UserPresence, }
  
  public static FourCC formatId => new FourCC('X', 'R', 'S', 'C');
  public FourCC format => formatId;
  
  // Based on XRSimulatedControllerState from XR Interaction Toolkit samples.
  [InputControl(usage = "Primary2DAxis", aliases = new[] { "thumbstick", "joystick" }, offset = 0)]
  [FieldOffset(0)]
  public Vector2 primary2DAxis;
  [InputControl(usage = "Trigger", layout = "Axis", offset = 8)]
  [FieldOffset(8)]
  public float trigger;
  [InputControl(usage = "Grip", layout = "Axis", offset = 12)]
  [FieldOffset(12)]
  public float grip;
  [InputControl(usage = "Secondary2DAxis", offset = 16)]
  [FieldOffset(16)]
  public Vector2 secondary2DAxis;
  
  [InputControl(name = nameof(ControllerButton.PrimaryButton), usage = "PrimaryButton", layout = "Button", bit = (uint)ControllerButton.PrimaryButton, offset = 24)]
  [InputControl(name = nameof(ControllerButton.PrimaryTouch), usage = "PrimaryTouch", layout = "Button", bit = (uint)ControllerButton.PrimaryTouch, offset = 24)]
  [InputControl(name = nameof(ControllerButton.SecondaryButton), usage = "SecondaryButton", layout = "Button", bit = (uint)ControllerButton.SecondaryButton, offset = 24)]
  [InputControl(name = nameof(ControllerButton.SecondaryTouch), usage = "SecondaryTouch", layout = "Button", bit = (uint)ControllerButton.SecondaryTouch, offset = 24)]
  [InputControl(name = nameof(ControllerButton.GripButton), usage = "GripButton", layout = "Button", bit = (uint)ControllerButton.GripButton, offset = 24, alias = "gripPressed")]
  [InputControl(name = nameof(ControllerButton.TriggerButton), usage = "TriggerButton", layout = "Button", bit = (uint)ControllerButton.TriggerButton, offset = 24, alias = "triggerPressed")]
  [InputControl(name = nameof(ControllerButton.MenuButton), usage = "MenuButton", layout = "Button", bit = (uint)ControllerButton.MenuButton, offset = 24)]
  [InputControl(name = nameof(ControllerButton.Primary2DAxisClick), usage = "Primary2DAxisClick", layout = "Button", bit = (uint)ControllerButton.Primary2DAxisClick, offset = 24)]
  [InputControl(name = nameof(ControllerButton.Primary2DAxisTouch), usage = "Primary2DAxisTouch", layout = "Button", bit = (uint)ControllerButton.Primary2DAxisTouch, offset = 24)]
  [InputControl(name = nameof(ControllerButton.Secondary2DAxisClick), usage = "Secondary2DAxisClick", layout = "Button", bit = (uint)ControllerButton.Secondary2DAxisClick, offset = 24)]
  [InputControl(name = nameof(ControllerButton.Secondary2DAxisTouch), usage = "Secondary2DAxisTouch", layout = "Button", bit = (uint)ControllerButton.Secondary2DAxisTouch, offset = 24)]
  [InputControl(name = nameof(ControllerButton.UserPresence), usage = "UserPresence", layout = "Button", bit = (uint)ControllerButton.UserPresence, offset = 24)]
  [FieldOffset(24)]
  public ushort buttons;
  
  [InputControl(usage = "BatteryLevel", layout = "Axis", offset = 26)]
  [FieldOffset(26)]
  public float batteryLevel;
  [InputControl(usage = "TrackingState", layout = "Integer", offset = 30)]
  [FieldOffset(30)]
  public int trackingState;
  [InputControl(usage = "IsTracked", layout = "Button", offset = 34)]
  [FieldOffset(34)]
  public bool isTracked;
  
  [InputControl(usage = "DevicePosition", offset = 35)]
  [FieldOffset(35)]
  public Vector3 devicePosition;
  
  [InputControl(usage = "DeviceRotation", offset = 47)]
  [FieldOffset(47)]
  public Quaternion deviceRotation;
  
  public void WithButton (ControllerButton button, bool state = true)
  {
    var bit = 1 << (int)button;
    if (state)
      buttons |= (ushort)bit;
    else
      buttons &= (ushort)~bit;
  }
  
  public void Reset()
  {
    primary2DAxis = default;
    trigger = default;
    grip = default;
    secondary2DAxis = default;
    buttons = default;
    batteryLevel = default;
    trackingState = default;
    isTracked = default;
    devicePosition = default;
    deviceRotation = Quaternion.identity;
  }
}

[InputControlLayout(stateType = typeof(VersatileControllerState), commonUsages = new[] { "LeftHand", "RightHand" }, isGenericTypeOfDevice = false, displayName = "Versatile Controller", updateBeforeRender = true)]
[Preserve]
public class VersatileController : XRController
{
  protected override void FinishSetup()
  {
    base.FinishSetup();   
  }
}
[InputControlLayout(stateType = typeof(VersatileControllerState), commonUsages = new[] { "LeftHand", "RightHand" }, isGenericTypeOfDevice = false, displayName = "Versatile Controller Head", updateBeforeRender = true)]
[Preserve]
public class VersatileControllerHead : XRController
{
  protected override void FinishSetup()
  {
    base.FinishSetup();   
  }
}
[InputControlLayout(stateType = typeof(VersatileControllerState), commonUsages = new[] { "LeftHand", "RightHand" }, isGenericTypeOfDevice = false, displayName = "Versatile Controller Feet", updateBeforeRender = true)]
[Preserve]
public class VersatileControllerFeet : XRController
{
  protected override void FinishSetup()
  {
    base.FinishSetup();   
  }
}
#endif
