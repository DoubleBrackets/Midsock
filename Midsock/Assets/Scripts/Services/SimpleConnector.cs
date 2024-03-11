using FishNet;
using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Transporting.Tugboat;
using ParrelSync;
using UnityEditor;
using UnityEngine;

public class SimpleConnector : MonoBehaviour
{
    public enum ConnectionType
    {
        Host,
        Client,
        Server
    }

    [SerializeField]
    private bool connectOnStart;

    [SerializeField]
    private ConnectionType awakeConnectType;

    [SerializeField]
    private Tugboat tugboat;

    [SerializeField]
    private NetworkManager networkManager;

    private void Start()
    {
        if (connectOnStart)
        {
            StartConnection(awakeConnectType);
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
            tugboat.SetClientAddress("localhost");
            if (ClonesManager.IsClone())
            {
                // Clone; Join as a client
                networkManager.ClientManager.StartConnection();
            }
            else
            {
                // Local host; both server and client
                networkManager.ServerManager.StartConnection();
                networkManager.ClientManager.StartConnection();
            }
        }
        else if (type == ConnectionType.Server)
        {
            networkManager.ServerManager.StartConnection();
        }
        else if (type == ConnectionType.Client)
        {
            networkManager.ClientManager.StartConnection();
        }

        return;
#endif

#if UNITY_SERVER
        networkManager.ServerManager.StartConnection();
        return;
#endif

#if !UNITY_EDITOR
        if (type == ConnectionType.Host)
        {
            // Local host; both server and client
            networkManager.ServerManager.StartConnection();
            networkManager.ClientManager.StartConnection();
        }
        else if(type == ConnectionType.Server)
        {
            networkManager.ServerManager.StartConnection();
        }
        else if(type == ConnectionType.Client)
        {
            networkManager.ClientManager.StartConnection();
        }
        
        return;
#endif
    }
}