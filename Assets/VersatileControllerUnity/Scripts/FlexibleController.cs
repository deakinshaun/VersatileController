using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class FlexibleController : MonoBehaviour
{
    public TextMeshProUGUI debug;
    private Quaternion orientation;
    private void Start()
    {
        Input.gyro.enabled = true;
    }
    void Update()   
    {
      if (SystemInfo.supportsGyroscope)
      {
        Debug.Log ("Gyro active");

        // Convert android to unity coordinates.
        orientation = Quaternion.Euler (90, 0, 90) * Input.gyro.attitude * Quaternion.Euler (180, 180, 0);
    //    debug.text = orientation.ToString ("F5");

        GetComponent<PhotonView>().RPC("SendControlInfo", RpcTarget.All, 
            orientation.x, orientation.y, orientation.z, orientation.w);
      }
    }

    [PunRPC]
    void SendControlInfo (float x, float y, float z, float w, PhotonMessageInfo info)
    {
        Quaternion o = new Quaternion(x, y, z, w);
        Debug.Log("Got controller info: " + o.ToString ("F5") + " " + info.Sender.ToString ());
        transform.rotation = o;
    }
}
