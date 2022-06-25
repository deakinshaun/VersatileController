using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PhotonManagerPhysical : MonoBehaviourPunCallbacks
{
  [Tooltip ("The avatar representation on the physical side of the controller")]
  public GameObject avatarPrefab;
  
  [Tooltip ("The system ID for all your controllers. Set this to be distinct if you don't want other people's controllers being used in your experience")]
  public string systemID = "General";
  
  void Start()
  {
    // Recreate the pool, so avatars with the same name can exist
    // on different platforms.
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
    Debug.Log("Joined room with " +
    PhotonNetwork.CurrentRoom.PlayerCount + " particpants");
    
    GameObject avatar = PhotonNetwork.Instantiate(avatarPrefab.name, new Vector3(), Quaternion.identity, 0);
  }
}
