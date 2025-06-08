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
    private VersatileControllerVirtual vcv;
    private NetworkRunner networkRunner;
    
    private GameObject controller;
    
    void Start ()
    {
        networkRunner = GetComponent <NetworkObject> ().Runner;
        controller = null;
        if (networkRunner?.IsServer == true)
        {
            controller = Instantiate (serverPrefab);
            vcv = controller.GetComponent <VersatileControllerVirtual> ();
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
            vcv?.ControllerStarted (name, isLeftHanded, skinName);
        }        
    }
    
    #if FUSION2
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    #endif  
    public void RPC_SendButtonDown (string button, string systemID, string controllerID) 
    {
        if (networkRunner?.IsServer == true)
        {
            vcv?.SendButtonDown (button, systemID, controllerID);
        }
    }
    
    #if FUSION2
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    #endif  
    public void RPC_SendButtonUp (string button, string systemID, string controllerID) 
    {
        if (networkRunner?.IsServer == true)
        {
            vcv?.SendButtonUp (button, systemID, controllerID);
        }        
    }
    
    #if FUSION2
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    #endif  
    public void RPC_SendSliderChanged (string slider, float value, string systemID, string controllerID) 
    {
        if (networkRunner?.IsServer == true)
        {
            vcv?.SendSliderChanged (slider, value, systemID, controllerID);
        }        
    }
    
    #if FUSION2
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    #endif  
    public void RPC_SendControlInfo (float x, float y, float z, float w, float px, float py, float pz)
    {
        if (networkRunner?.IsServer == true)
        {
            vcv?.SendControlInfo (x, y, z, w, px, py, pz);
        }
    }
    
}
