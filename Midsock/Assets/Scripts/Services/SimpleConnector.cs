using FishNet;
using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Transporting.Tugboat;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
using ParrelSync;
#endif

public class SimpleConnector : MonoBehaviour
{
    public enum ConnectionType
    {
        Host,
        Client,
        Server
    }

    [SerializeField]
    private bool _connectOnStart;

    [SerializeField]
    private ConnectionType _awakeConnectType;

    [SerializeField]
    private Tugboat _tugboat;

    [SerializeField]
    private NetworkManager _networkManager;

    private void Start()
    {
        if (_connectOnStart)
        {
            StartConnection(_awakeConnectType);
        }
    }

    private void OnEnable()
    {
#if UNITY_EDITOR
        InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionState;
#endif
    }

#if UNITY_EDITOR
    private void OnClientConnectionState(ClientConnectionStateArgs state)
    {
        // Stop playing on all editor instances when the server stops (just a nice utility)
        if (state.ConnectionState == LocalConnectionState.Stopping)
        {
            EditorApplication.isPlaying = false;
        }
    }
#endif

    public void StartConnection(ConnectionType type)
    {
#if UNITY_EDITOR
        if (type == ConnectionType.Host)
        {
            _tugboat.SetClientAddress("localhost");
            if (ClonesManager.IsClone())
            {
                // Clone; Join as a client
                _networkManager.ClientManager.StartConnection();
            }
            else
            {
                // Local host; both server and client
                _networkManager.ServerManager.StartConnection();
                _networkManager.ClientManager.StartConnection();
            }
        }
        else if (type == ConnectionType.Server)
        {
            _networkManager.ServerManager.StartConnection();
        }
        else if (type == ConnectionType.Client)
        {
            _networkManager.ClientManager.StartConnection();
        }

#endif

#if UNITY_SERVER
        _networkManager.ServerManager.StartConnection();
        return;
#endif

#if !UNITY_EDITOR
        if (type == ConnectionType.Host)
        {
            // Local host; both server and client
            _networkManager.ServerManager.StartConnection();
            _networkManager.ClientManager.StartConnection();
        }
        else if(type == ConnectionType.Server)
        {
            _networkManager.ServerManager.StartConnection();
        }
        else if(type == ConnectionType.Client)
        {
            _networkManager.ClientManager.StartConnection();
        }
        
        return;
#endif
    }
}