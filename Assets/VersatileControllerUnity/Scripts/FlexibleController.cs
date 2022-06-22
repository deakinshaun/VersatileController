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
        orientation = Input.gyro.attitude;
    //    debug.text = orientation.ToString ("F5");

        GetComponent<PhotonView>().RPC("SendControlInfo", RpcTarget.All, 
            orientation.x, orientation.y, orientation.z, orientation.w);
    }

    [PunRPC]
    void SendControlInfo (float x, float y, float z, float w, PhotonMessageInfo info)
    {
        Quaternion o = new Quaternion(x, y, z, w);
        bool other = info.Sender.ToString ().StartsWith ("#01");
        Debug.Log("Got controller info: " + o.ToString ("F5") + " " + info.Sender.ToString () + 
            " " + other + " " + string.Compare(info.Sender.ToString(), "#01"));
        if (other)
        {
            transform.rotation = o;
        }
    }
}
