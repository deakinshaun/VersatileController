using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class FlexibleController : MonoBehaviourPun
{
    public TextMeshProUGUI debug;
    
    [Header ("Default Controller Widgets")]
    [Tooltip ("Canvas for this set of widgets, so they can be switched on and off as a group")]
    public Canvas defaultControls;
    public TextMeshProUGUI systemID;
    public TextMeshProUGUI controllerID;
    
//     // Returns true if the provided strings match the settings for this controller.
//     private bool validateSource (string sID, string cID)
//     {
//       return true;
//     }
    
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
    public void SendButtonDown (string button, string systemID, string controllerID, PhotonMessageInfo info) {}
    [PunRPC]
    public void SendButtonUp (string button, PhotonMessageInfo info) {}
    [PunRPC]
    void SendControlInfo (float x, float y, float z, float w, PhotonMessageInfo info) {}
    
    // 
    
    private void Start()
    {
        Input.gyro.enabled = true;
    }
    void Update()   
    {
      if (SystemInfo.supportsGyroscope)
      {
        Debug.Log ("Gyro active, " + photonView.IsMine + " " + PhotonNetwork.IsConnected);
        if (photonView.IsMine == true || PhotonNetwork.IsConnected == false)
        {

          // Convert android to unity coordinates.
          Quaternion orientation = Quaternion.Euler (90, 0, 90) * Input.gyro.attitude * Quaternion.Euler (180, 180, 0);
      //    debug.text = orientation.ToString ("F5");

          GetComponent<PhotonView>().RPC("SendControlInfo", RpcTarget.All, 
              orientation.x, orientation.y, orientation.z, orientation.w);
        }
      }
    }

//     [PunRPC]
//     void SendControlInfo (float x, float y, float z, float w, PhotonMessageInfo info)
//     {
//         Quaternion o = new Quaternion(x, y, z, w);
//         Debug.Log("Got controller info: " + o.ToString ("F5") + " " + info.Sender.ToString ());
//         transform.rotation = o;
//     }
}
