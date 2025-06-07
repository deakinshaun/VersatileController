using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Scripting;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem.Utilities;

using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
//using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

using System.Runtime.InteropServices;

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
    }
}



[StructLayout(LayoutKind.Explicit, Size = 63)]
public struct VersatileControllerState : IInputStateTypeInfo
{
    public static FourCC formatId => new FourCC('X', 'R', 'S', 'C');
    public FourCC format => formatId;

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

public class ControlTranslation : MonoBehaviour
{
    XRController m_LeftControllerDevice;
    XRController m_LeftControllerDevice2;
    VersatileControllerState m_LeftControllerState;
    VersatileControllerState m_LeftControllerState2;
    void Start()
    {
                Debug.Log ("Setting AA");

        var descLeftHand = new InputDeviceDescription
                {
                    product = "XRSimulatedController",
                    capabilities = new XRDeviceDescriptor
                    {
                        // deviceName = $"{nameof(XRSimulatedController)} - {InputSystem.CommonUsages.LeftHand}",
                        deviceName = "fred",
                        characteristics = XRInputTrackingAggregator.Characteristics.leftController,
                    }.ToJson(),
                };
        m_LeftControllerDevice = InputSystem.AddDevice(descLeftHand) as XRController;
        Debug.Log ("Setting AB");

        var descLeftHand2 = new InputDeviceDescription
                {
                    product = nameof(VersatileController),
                    capabilities = new XRDeviceDescriptor
                    {
                        deviceName = $"{nameof(VersatileController)} - {UnityEngine.InputSystem.CommonUsages.RightHand}",
                        characteristics = XRInputTrackingAggregator.Characteristics.rightController,
                    }.ToJson(),
                };
        m_LeftControllerDevice2 = InputSystem.AddDevice(descLeftHand2) as XRController;
        InputSystem.SetDeviceUsage(m_LeftControllerDevice2, UnityEngine.InputSystem.CommonUsages.RightHand);
        
        Debug.Log ("Setting AC");
        
        // m_LeftControllerState.Reset();
        m_LeftControllerState.isTracked = true;
        m_LeftControllerState.trackingState = (int) (InputTrackingState.Position | InputTrackingState.Rotation);
        m_LeftControllerState.deviceRotation = Quaternion.identity;

        Debug.Log ("Setting AD");
        
        m_LeftControllerState2.isTracked = true;
        m_LeftControllerState2.trackingState = (int) (InputTrackingState.Position | InputTrackingState.Rotation);
        m_LeftControllerState2.deviceRotation = Quaternion.identity;
        Debug.Log ("Setting AE");
        
        
    }

    void Update()
    {
        Debug.Log ("Setting A");
        m_LeftControllerState.devicePosition += new Vector3 (0.0f, 0.0001f, 0.0f);
        m_LeftControllerState.deviceRotation *= Quaternion.AngleAxis (0.3f, new Vector3 (0.7f, 0.2f, -0.5f));
        Debug.Log ("Setting B");
        //m_LeftControllerState.deviceRotation = Quaternion.AngleAxis (60.0f, Vector3.up);
        InputState.Change(m_LeftControllerDevice, m_LeftControllerState);
        Debug.Log ("Setting C");
        
        m_LeftControllerState2.deviceRotation *= Quaternion.AngleAxis (0.5f, new Vector3 (0.7f, 0.2f, -0.5f));
        InputState.Change(m_LeftControllerDevice2, m_LeftControllerState2);
        Debug.Log ("Setting : " + m_LeftControllerState2.deviceRotation);
    }
}
