using System.Collections;
using System.Collections.Generic;

using Photon.Pun;
using Photon.Realtime;
using TMPro;

using System;
using System.IO;
using System.Text;

using UnityEngine;

public class PhotonManagerPhysical : MonoBehaviourPunCallbacks
{
  [Tooltip ("The avatar representation on the physical side of the controller")]
  public GameObject avatarPrefab;
  
  [Tooltip ("The system ID for all your controllers. Set this to be distinct if you don't want other people's controllers being used in your experience")]
  private string systemID = "General";
  [Tooltip ("The controller ID for this specific controllers. Use this to distinguish between different controllers in the same application (e.g. LeftHand and RighHand)")]
  private string controllerID = "DefaultController";
  [Tooltip ("Handedness - is the controller intended for left or right handed use.")]
  private bool isLeftHanded = true;
  [Tooltip ("Skin - the name of the skin applied to this controller.")]
  public string skinName = "Controller Emulation";
  
  // Define the system and controller IDs. These are stored persistently, so
  // are reused when the controller next reconnects.
  public void updateConnectionDetails (string sid, string cid, bool left, string skin)
  {
    systemID = sid;
    controllerID = cid;
    isLeftHanded = left;
    skinName = skin;
    persist ();
  }
  
  // Store persistent settings.
  private void persist ()
  {
    string persistFilename = Application.persistentDataPath + "/" + "persist.txt";
    string [] data = { systemID, controllerID, isLeftHanded.ToString (), skinName };
    Debug.Log ("Persisting " + data[3]);
    File.WriteAllLines(persistFilename, data, Encoding.UTF8);
  }
  
  // Retrieve persistent settings.
  private void unpersist ()
  {
    try 
    {
      string persistFilename = Application.persistentDataPath + "/" + "persist.txt";
      string [] lines = System.IO.File.ReadAllLines (persistFilename);
      if (lines.Length >= 4)
      {
        systemID = lines[0];
        controllerID = lines[1];
        isLeftHanded = lines[2].Equals ("True");
        skinName = lines[3];
      }
      Debug.Log ("Got lines : " + lines + " " + lines[0] + " " + lines[1] + " " + lines[2] + " " + lines[3]);
    }
    catch (Exception)
    {
      // Failed to unpersist. Ignore.
    }
  }
  
  void Start()
  {
    // Recreate the pool, so avatars with the same name can exist
    // on different platforms.
    DefaultPool pool = PhotonNetwork.PrefabPool as DefaultPool;
    if (pool != null)
    {
      pool.ResourceCache.Add(avatarPrefab.name, avatarPrefab);
    }
    connect ();
  }

  // Restart the connection.
  public void reconnect ()
  {
    disconnect ();
    connect ();
  }
  
  private void connect ()
  {
    Debug.Log("Starting - connected status = " + PhotonNetwork.IsConnected);
    unpersist ();
    PhotonNetwork.ConnectUsingSettings();
  }
  
  private void disconnect ()
  {
    PhotonNetwork.Disconnect ();
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
    avatar.GetComponent <VersatileControllerPhysical> ().setPhotonManager (this, systemID, controllerID, isLeftHanded, skinName);
  }
  
  void OnApplicationPause(bool pauseStatus)
  {
    Debug.Log ("Pause status: " + pauseStatus);
    if (pauseStatus)
    { 
      disconnect ();
    }
    else
    {
      connect ();
    }
  }
}
