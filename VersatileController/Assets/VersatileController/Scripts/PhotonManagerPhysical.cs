using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#if FUSION2
using Fusion;
using Fusion.Sockets;
#endif 

using TMPro;
using System.IO;
using System.Text;

using UnityEngine;

public class PhotonManagerPhysical : MonoBehaviour, INetworkRunnerCallbacks
{
  [Tooltip ("The system ID for all your controllers. Set this to be distinct if you don't want other people's controllers being used in your experience")]
  private string systemID = "General";
  [Tooltip ("The controller ID for this specific controllers. Use this to distinguish between different controllers in the same application (e.g. LeftHand and RighHand)")]
  private string controllerID = "DefaultController";
  [Tooltip ("Handedness - is the controller intended for left or right handed use.")]
  private bool isLeftHanded = true;
  [Tooltip ("Skin - the name of the skin applied to this controller.")]
  public string skinName = "Controller Emulation";
  
  private NetworkRunner networkRunner;
  private PlayerRef networkPlayer;
  private bool setControlData = false;

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
#if FUSION2
    // connect ();
#endif    
  }

  // Restart the connection.
  public void reconnect ()
  {
    disconnect ();
    connect ();
  }
  
  private IEnumerator connectCoroutine ()
  {
#if FUSION2
    if (networkRunner == null)
    {
      networkRunner = gameObject.AddComponent <NetworkRunner> ();
    }
        
    Task t = networkRunner.StartGame (new StartGameArgs () { GameMode = GameMode.Client, SessionName = "ApplicationLobby" });    
    yield return new WaitUntil (() => t.IsCompleted);
#endif    
  }

  private bool connectionInProgress = false;
  private void connect ()
  {
    unpersist ();
    
    if ((networkRunner == null) && (!connectionInProgress))
    {
      connectionInProgress = true;
      StartCoroutine (connectCoroutine ());
    }
  }
  
  private IEnumerator disconnectCoroutine ()
  {
#if FUSION2
    networkRunner.Disconnect (networkPlayer);
    Destroy (networkRunner);
    yield return null;
    networkRunner = null;
#endif    
  }
  
  private void disconnect ()
  {
    if ((networkRunner != null) && (!connectionInProgress))
    {
      StartCoroutine (disconnectCoroutine ());
    }
  }
  
  public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
  {
#if FUSION2
    // base.OnJoinedRoom();
    Debug.Log("Joined room with " + runner.ActivePlayers.Count () + " particpants"); 
    connectionInProgress = false;
    networkPlayer = player;
    setControlData = false;
#endif    
  }
  
  private float reconnectInterval = 2.0f;
  private float reconnectTimer = 0.0f;
  
  public void Update ()
  {
    if (!setControlData)
    {
      if ((networkRunner != null) && (networkRunner.LocalPlayer != null) && networkRunner.GetPlayerObject (networkRunner.LocalPlayer) != null)
      {
        GameObject avatar = networkRunner.GetPlayerObject (networkRunner.LocalPlayer).gameObject;
        if (avatar.GetComponentInChildren <VersatileControllerPhysical> (true) != null)
        {
          avatar.GetComponentInChildren <VersatileControllerPhysical> (true).setPhotonManager (this, systemID, controllerID, isLeftHanded, skinName, networkRunner, networkRunner.LocalPlayer);
          
          setControlData = true;
        }
      }
    }
    
    if (networkRunner?.IsRunning == true)
    {
      reconnectTimer = reconnectInterval;
    }
    else
    {
      reconnectTimer -= Time.deltaTime;
      if (reconnectTimer < 0.0f)
      {
        reconnectTimer = reconnectInterval;
        
        disconnect ();

        connect ();
      }
    }
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

  public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) 
  { 
    Debug.Log ("Server shutdown: retrying connection");
    networkRunner.enabled = false;
    connectionInProgress = false;
  }

  public void OnDisconnectedFromServer (NetworkRunner runner, NetDisconnectReason reason) 
  { 
    Debug.Log ("Server disconnected: retrying connection");
    connectionInProgress = false;
    disconnect ();
  }
  
  public void OnConnectedToServer(NetworkRunner runner) { }
  public void OnObjectExitAOI (NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
  public void OnObjectEnterAOI (NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
  public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
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
