using System.Linq;
using FishNet;
using FishNet.Broadcast;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using GameKit.Utilities.Types;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Server side manager for the session. Does not do client side logic.
/// </summary>
public class SessionStateManager : NetworkBehaviour
{
    private enum SessionState
    {
        Lobby,
        MatchStarted
    }

    public struct SpawnCharactersBroadcast : IBroadcast
    {
        public string DisplayName;
    }

    [SerializeField]
    [Scene]
    private string _lobbyScene;

    [SerializeField]
    private ConnectionDataSO _connectionData;

    private SessionState _sessionState;

    private Scene lobbyHandle;

    private void Start()
    {
        InstanceFinder.ClientManager.OnClientTimeOut += OnClientTimeOut;
        InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
        InstanceFinder.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;

        SceneManager.OnLoadEnd += OnSceneLoadEnd;
    }

    private void Update()
    {
        // disconnect if Q is pressed
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (IsServer)
            {
                NetworkManager.ServerManager.StopConnection(true);
            }
            else if (IsClient)
            {
                NetworkManager.ClientManager.StopConnection();
                _connectionData.InvokeOnDisconnect(ConnectionDataSO.DisconnectReason.ClientRequestedDisconnect);
            }
        }
    }

    private void OnDestroy()
    {
        if (IsServer)
        {
            NetworkManager.ServerManager.StopConnection(true);
        }
        else if (IsClient)
        {
            NetworkManager.ClientManager.StopConnection();
        }
    }

    private void OnSceneLoadEnd(SceneLoadEndEventArgs obj)
    {
        //Only Register on Server
        if (!obj.QueueData.AsServer)
        {
            return;
        }

        Debug.Log("Scene Loaded : " + obj.LoadedScenes[0].name);
        if (obj.LoadedScenes.Select(a => a.name).Contains(_lobbyScene.PathToSceneName()))
        {
            lobbyHandle = obj.LoadedScenes.First(a => a.name == _lobbyScene.PathToSceneName());
            Debug.Log($"Lobby scene registered {lobbyHandle}");
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (IsHost)
        {
            LoadConnectionIntoLobby(LocalConnection);
        }
    }

    private void OnRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs state)
    {
        // load client into lobby scene
        if (state.ConnectionState == RemoteConnectionState.Started)
        {
            LoadConnectionIntoLobby(connection);
        }
    }

    private void LoadConnectionIntoLobby(NetworkConnection connection)
    {
        Debug.Log($"Loading connection into lobby scene: {connection.ClientId}");
        var scene = new SceneLookupData(_lobbyScene);

        if (lobbyHandle.IsValid())
        {
            Debug.Log(lobbyHandle.name);
            scene = new SceneLookupData(lobbyHandle.handle);
        }

        var sd = new SceneLoadData(scene);
        sd.PreferredActiveScene = scene;

        SceneManager.LoadConnectionScenes(connection, sd);
    }

    private void OnClientConnectionState(ClientConnectionStateArgs obj)
    {
        if (obj.ConnectionState == LocalConnectionState.Stopping)
        {
            _connectionData.InvokeOnDisconnect(ConnectionDataSO.DisconnectReason.Disconnected);
        }
    }

    private void OnClientTimeOut()
    {
        _connectionData.InvokeOnDisconnect(ConnectionDataSO.DisconnectReason.ConnectionTimeout);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        LoadLobby();
    }

    private void LoadLobby()
    {
        Debug.Log("Loading Lobby Scene...");
        _sessionState = SessionState.Lobby;

        var lobbyScene = new SceneLookupData(_lobbyScene);
        var sd = new SceneLoadData(lobbyScene);
        sd.PreferredActiveScene = lobbyScene;

        // SceneManager.LoadGlobalScenes(sd);
    }
}