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

    private struct MovingCharacter
    {
        public Vector3 destinationPos;
        public NetworkObject characterNob;
    }

    [SerializeField]
    [Scene]
    private string _lobbyScene;

    [SerializeField]
    private ConnectionDataSO _connectionData;

    private Dictionary<NetworkConnection, Scene> _connectionScenes = new();

    private Dictionary<NetworkConnection, MovingCharacter> _inTransitCharacters = new();

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

    private void OnDestroy()
    {
        if (IsServer)
        {
            ServerManager.StopConnection(false);
        }
    }

    private void OnClientPresenceChangeEnd(ClientPresenceChangeEventArgs args)
    {
        if (!args.Added)
        {
            return;
        }

        // make sure a client is finished loading into a scene before allowing them to switch scenes again
        // Also move the character to the destination position
        if (_inTransitCharacters.ContainsKey(args.Connection))
        {
            /*_inTransitCharacters[args.Connection].characterNob.transform.position =
                _inTransitCharacters[args.Connection].destinationPos;*/
            _inTransitCharacters.Remove(args.Connection);
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

    public void MoveConnection(NetworkConnection playerOwner, string targetScene, Vector3 destPos)
    {
        MoveConnectionRPC(playerOwner, targetScene, destPos);
    }

    [ServerRpc(RequireOwnership = false)]
    private void MoveConnectionRPC(NetworkConnection playerOwner, string targetScene, Vector3 destPos)
    {
        if (_inTransitCharacters.ContainsKey(playerOwner))
        {
            Debug.Log($"{playerOwner.ClientId} is already in the process of moving");
            return;
        }

        // find character nob
        NetworkObject targetNob = SessionServiceFinder.PlayerCharacterService.GetCharacterNob(playerOwner);

        Scene leavingScene = targetNob.gameObject.scene;

        LoadConnectionIntoExistingScene(playerOwner, targetScene, new[] { targetNob });
        UnloadConnectionFromScene(playerOwner, leavingScene.name, targetScene);

        ServerManager.Broadcast(new PlayerController.TeleportBroadcast
        {
            target = playerOwner,
            position = destPos
        });

        _inTransitCharacters.Add(playerOwner, new MovingCharacter
        {
            characterNob = targetNob,
            destinationPos = destPos
        });
    }
}