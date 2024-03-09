using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Transporting.Tugboat;
using UnityEngine;
using UnityEngine.Serialization;

public class ConnectionStarter : MonoBehaviour
{
    public enum ConnectionType
    {
        Host,
        Client
    }

    [SerializeField]
    private bool connectOnAwake;

    [FormerlySerializedAs("type")]
    [SerializeField]
    private ConnectionType awakeConnectType;
    
    [SerializeField]
    private Tugboat tugboat;
    
    [SerializeField]
    private NetworkManager networkManager;

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
        if (state.ConnectionState == LocalConnectionState.Stopping)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
    }
    #endif

    private void Start()
    {
        if (connectOnAwake)
        {
            StartConnection(awakeConnectType);
        }
    }

    public void StartConnection(ConnectionType type)
    {
#if UNITY_EDITOR
        if (type == ConnectionType.Host)
        {
            tugboat.SetClientAddress("localhost");
            if (ParrelSync.ClonesManager.IsClone())
            {
                // Just client
                networkManager.ClientManager.StartConnection();
            }
            else
            {
                // Local host; both server and client
                networkManager.ServerManager.StartConnection();
                networkManager.ClientManager.StartConnection();
            }
        }
        else
        {
            networkManager.ClientManager.StartConnection();
        }
#endif
        
        // Assumes that we're testing from the editor, and that the current running build is a remote host
#if !UNITY_EDITOR
        
        if (type == ConnectionType.Host)
        {
            // Local host; both server and client
            networkManager.ServerManager.StartConnection();
            networkManager.ClientManager.StartConnection();
        }
        else
        {
            networkManager.ClientManager.StartConnection();
        }
        
#endif
    }
}
