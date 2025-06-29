using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

#if FUSION2
using Fusion;
using Fusion.Sockets;

using TMPro;

// This version of the photon manager pattern doesn't create avatars. Only the avatars
// of physical controllers need to exist.
public class PhotonManagerVirtual : MonoBehaviour, INetworkRunnerCallbacks
{
  [Tooltip ("The avatar representation on the virtual side of the controller")]
  public GameObject avatarPrefab;
  
  [Tooltip ("The system ID for all your controllers. Set this to be distinct if you don't want other people's controllers being used in your experience")]
  public string systemID = "General";
  
  [Tooltip ("Switch this off, if your application has its own controller representations, and you just want the sensor input from the controllers")]
  public bool showControllerRepresentations = true;
  
  [Tooltip ("Apply this when running on desktop. Forces controllers forward so they can be more easily seen.")]
  public bool controllerOffset = true;
  
  private NetworkRunner networkRunner;
  
  async void Start()
  {
    #if FUSION2    
    networkRunner = gameObject.AddComponent <NetworkRunner> ();
    
    await networkRunner.StartGame (new StartGameArgs () { GameMode = GameMode.Shared, SessionName = systemID });
    #endif    
  }
  
  public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
  {
    #if FUSION2    
    // base.OnJoinedRoom();
    Debug.Log("Joined room " + runner.SessionInfo.Name + " with " + runner.ActivePlayers.Count () + " particpants, as player: " + player + " and player object: " + runner.GetPlayerObject (player)); 
    
    #endif    
  }
  
  public void OnConnectedToServer(NetworkRunner runner) { }
  public void OnObjectExitAOI (NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
  public void OnObjectEnterAOI (NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
  public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) 
  { 
    Debug.Log ("Player left: " + player);
    // if (networkRunner.IsServer)
    // {
    //   networkRunner.Despawn (networkRunner.GetPlayerObject (player));
    // }
  }
  public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
  public void OnDisconnectedFromServer (NetworkRunner runner, NetDisconnectReason reason) { }
  public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
  public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
  public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
  public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
  public void OnReliableDataReceived (NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment< byte > data) { }
  public void OnReliableDataProgress (NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
  public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
  public void OnInput(NetworkRunner runner, NetworkInput input) { }
  public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
  public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
  public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
  public void OnSceneLoadDone(NetworkRunner runner) { }
  public void OnSceneLoadStart(NetworkRunner runner) { }
  
}
#endif
