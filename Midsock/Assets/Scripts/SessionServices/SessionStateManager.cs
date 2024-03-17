using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using GameKit.Utilities.Types;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the game state
/// </summary>
public class SessionStateManager : NetworkBehaviour
{
    private enum SessionState
    {
        Lobby,
        MatchStarted
    }

    [SerializeField]
    [Scene]
    private string _lobbyScene;

    [SerializeField]
    private ConnectionDataSO _connectionData;

    private Dictionary<NetworkConnection, Scene> _connectionScenes = new();

    private Dictionary<NetworkConnection, Scene> _removeQueue = new();

    private SessionState _sessionState;

    private bool didDisconnectManually;

    private void Awake()
    {
        SessionServiceFinder.SetSessionStateManager(this);
    }

    private void Start()
    {
        InstanceFinder.ClientManager.OnClientTimeOut += OnClientTimeOut;
        InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;

        SceneManager.OnClientPresenceChangeEnd += OnClientPresenceChangeEnd;
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
                // This needs to be called before disconnecting so the disconnect popup doesn't show generic disconnect
                _connectionData.InvokeOnDisconnect(ConnectionDataSO.DisconnectReason.ClientRequestedDisconnect);
                NetworkManager.ClientManager.StopConnection();
            }
        }
    }

    private void OnClientPresenceChangeEnd(ClientPresenceChangeEventArgs args)
    {
        if (!args.Added)
        {
            return;
        }

        // make sure a client is finished loading into a scene before allowing them to switch scenes again
        if (_removeQueue.ContainsKey(args.Connection))
        {
            _removeQueue.Remove(args.Connection);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        LoadIntoLobby(LocalConnection);
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
        if (IsServer)
        {
            // LoadLobby();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void LoadIntoLobby(NetworkConnection senderConnection)
    {
        Debug.Log($"Loading Lobby Scene for {senderConnection.ClientId}");

        LoadConnectionIntoExistingScene(senderConnection, _lobbyScene);
    }

    private void LoadConnectionIntoExistingScene(NetworkConnection connection, string sceneName,
        NetworkObject[] toMove = null)
    {
        var sceneLookupData = new SceneLookupData(sceneName);
        var sceneLoadData = new SceneLoadData(sceneLookupData);

        sceneLoadData.PreferredActiveScene = sceneLookupData;

        if (toMove != null)
        {
            sceneLoadData.MovedNetworkObjects = toMove;
            foreach (NetworkObject obj in sceneLoadData.MovedNetworkObjects)
            {
                Debug.Log($"Moving {obj} into {sceneName}");
            }
        }


        SceneManager.LoadConnectionScenes(connection, sceneLoadData);
    }

    private void UnloadConnectionFromScene(NetworkConnection connection, string sceneName, string activeScene)
    {
        var sceneLookupData = new SceneLookupData(sceneName);
        var activeSceneLookupData = new SceneLookupData(activeScene);
        var sceneLoadData = new SceneUnloadData(sceneLookupData);

        sceneLoadData.PreferredActiveScene = activeSceneLookupData;

        SceneManager.UnloadConnectionScenes(connection, sceneLoadData);
    }

    public void MoveConnection(NetworkConnection playerOwner, string targetScene)
    {
        MoveConnectionRPC(playerOwner, targetScene);
    }

    [ServerRpc(RequireOwnership = false)]
    private void MoveConnectionRPC(NetworkConnection playerOwner, string targetScene)
    {
        if (_removeQueue.ContainsKey(playerOwner))
        {
            Debug.Log($"{playerOwner.ClientId} is already in the process of moving");
            return;
        }

        // find character nob
        NetworkObject targetNob = SessionServiceFinder.PlayerCharacterService.GetCharacterNob(playerOwner);

        Scene leavingScene = targetNob.gameObject.scene;

        LoadConnectionIntoExistingScene(playerOwner, targetScene, new[] { targetNob });
        UnloadConnectionFromScene(playerOwner, leavingScene.name, targetScene);

        _removeQueue.Add(playerOwner, leavingScene);
    }
}