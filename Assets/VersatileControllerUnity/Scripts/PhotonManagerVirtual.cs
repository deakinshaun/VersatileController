using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Photon.Pun;
using Photon.Realtime;
using TMPro;

// This version of the photon manager pattern doesn't create avatars. Only the avatars
// of physical controllers need to exist.
public class PhotonManagerVirtual : MonoBehaviourPunCallbacks
{
  [Tooltip ("The avatar representation on the virtual side of the controller")]
  public GameObject avatarPrefab;

  [Tooltip ("The system ID for all your controllers. Set this to be distinct if you don't want other people's controllers being used in your experience")]
  public string systemID = "General";
  
  void Start()
  {
    DefaultPool pool = PhotonNetwork.PrefabPool as DefaultPool;
    if (pool != null)
    {
      pool.ResourceCache.Add(avatarPrefab.name, avatarPrefab);
    }
    Debug.Log("Starting - connected status = " + PhotonNetwork.IsConnected);
    PhotonNetwork.ConnectUsingSettings();
  }
  
  public override void OnConnectedToMaster ()
  {
    Debug.Log("Connected to Master.");
    RoomOptions roomopt = new RoomOptions();
    PhotonNetwork.JoinOrCreateRoom(systemID, roomopt, new TypedLobby("ApplicationLobby", LobbyType.Default));
  }
  
  public override void OnJoinedRoom()
  {
    base.OnJoinedRoom();
    Debug.Log("Joined room with " + PhotonNetwork.CurrentRoom.PlayerCount + " particpants");    
  }
  
  
}
