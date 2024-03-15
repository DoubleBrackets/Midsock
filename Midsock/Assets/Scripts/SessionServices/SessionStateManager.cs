using FishNet;
using FishNet.Broadcast;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using GameKit.Utilities.Types;
using UnityEngine;

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

    private void Start()
    {
        InstanceFinder.ClientManager.OnClientTimeOut += OnClientTimeOut;
        InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;

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
        if (!obj.QueueData.AsServer || obj.LoadedScenes.Length == 0)
        {
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (IsHost)
        {
        }
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

        SceneManager.LoadGlobalScenes(sd);
    }
}