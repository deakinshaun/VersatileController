using UnityEngine;

#if FUSION2
using Fusion;
#endif

public class ControllerMode : NetworkBehaviour
{
    public GameObject serverPrefab;
    public GameObject clientPrefab;

#if VersatileControllerPhysicalClass    
    private VersatileControllerPhysical vcp;
#endif    
#if VersatileControllerVirtualClass
    private VersatileControllerVirtual vcv;
#endif
    private NetworkRunner networkRunner;
    
    private GameObject controller;
    
    void Start ()
    {
        networkRunner = GetComponent <NetworkObject> ().Runner;
        controller = null;
        if (networkRunner?.IsServer == true)
        {
            controller = Instantiate (serverPrefab);
#if VersatileControllerVirtualClass
            vcv = controller.GetComponent <VersatileControllerVirtual> ();
#endif            
        }
        else
        {
            controller = Instantiate (clientPrefab);
#if VersatileControllerPhysicalClass    
            vcp = controller.GetComponent <VersatileControllerPhysical> ();
            vcp?.setControllerMode (this);
#endif            
        }
        controller.transform.SetParent (transform);
    }
    
    void OnDestroy()
    {
        Destroy (controller);
    }
    
    #if FUSION2
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    #endif  
    public void RPC_ControllerStarted (string name, bool isLeftHanded, string skinName) 
    {
        if (networkRunner?.IsServer == true)
        {
#if VersatileControllerVirtualClass
            vcv?.ControllerStarted (name, isLeftHanded, skinName);
#endif            
        }        
    }
    
    #if FUSION2
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    #endif  
    public void RPC_SendButtonDown (string button, string systemID, string controllerID) 
    {
        if (networkRunner?.IsServer == true)
        {
#if VersatileControllerVirtualClass
            vcv?.SendButtonDown (button, systemID, controllerID);
#endif            
        }
    }
    
    #if FUSION2
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    #endif  
    public void RPC_SendButtonUp (string button, string systemID, string controllerID) 
    {
        if (networkRunner?.IsServer == true)
        {
#if VersatileControllerVirtualClass
            vcv?.SendButtonUp (button, systemID, controllerID);
#endif            
        }        
    }
    
    #if FUSION2
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    #endif  
    public void RPC_SendSliderChanged (string slider, float value, string systemID, string controllerID) 
    {
        if (networkRunner?.IsServer == true)
        {
#if VersatileControllerVirtualClass
            vcv?.SendSliderChanged (slider, value, systemID, controllerID);
#endif            
        }        
    }
    
    #if FUSION2
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    #endif  
    public void RPC_SendControlInfo (float x, float y, float z, float w, float px, float py, float pz)
    {
        if (networkRunner?.IsServer == true)
        {
#if VersatileControllerVirtualClass
            vcv?.SendControlInfo (x, y, z, w, px, py, pz);
#endif            
        }
    }
    
}
