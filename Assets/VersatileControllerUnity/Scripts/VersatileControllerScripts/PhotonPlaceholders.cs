using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This file provides replacements for photon functions (stubs only)
// to keep the project sane and allow the Photon installer to run.
#if !PHOTON_UNITY_NETWORKING

public class MonoBehaviourPun : MonoBehaviour {}
public class MonoBehaviourPunCallbacks : MonoBehaviour
{
  public virtual void OnConnectedToMaster () {}
  public virtual void OnJoinedRoom() {}
  
}

public class PhotonMessageInfo
{
  
}

#endif
