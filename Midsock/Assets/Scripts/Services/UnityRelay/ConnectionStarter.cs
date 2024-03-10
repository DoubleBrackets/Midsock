using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Transporting.UTP;
using TMPro;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay.Models;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Sets up UGS relay service and starts fishnet connection
/// </summary>
public class ConnectionStarter : MonoBehaviour
{
    public static ConnectionStarter Instance { get; private set; }
    
    [SerializeField]
    private FishyUnityTransport fishyUnityTransport;

    public string JoinCode { get; private set; }
    
    private Allocation allocation;
    private JoinAllocation joinAllocation;

    private enum RelayState
    {
        Disconnected,
        Host,
        Client
    }
    
    private RelayState relayState = RelayState.Disconnected;
    
    private void Awake()
    {
        Instance = this;
        InitializeServices();
    }

    private async UniTaskVoid InitializeServices()
    {
        //Initialize the Unity Services engine
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            //If not already logged, log the user in
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }
    
    public async UniTask<string> BeginHostingAsync()
    {
        try
        {
            allocation = await Unity.Services.Relay.RelayService.Instance.CreateAllocationAsync(4);
            JoinCode = await Unity.Services.Relay.RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        
            SetupTransport(allocation);
        
            Debug.Log($"Created Relay with code {JoinCode}");
        
            relayState = RelayState.Host;

            InstanceFinder.ServerManager.StartConnection();
            InstanceFinder.ClientManager.StartConnection();

            return JoinCode;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            fishyUnityTransport.Shutdown();
            throw;
        }
    }
    
    public async UniTaskVoid JoinGameAsync(string joinCode)
    {
        try
        {
            joinAllocation = await Unity.Services.Relay.RelayService.Instance.JoinAllocationAsync(joinCode);
        
            SetupTransport(joinAllocation);
        
            Debug.Log($"Joined Relay with code {joinCode}");

            relayState = RelayState.Client;
        
            InstanceFinder.ClientManager.StartConnection();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            fishyUnityTransport.Shutdown();
            throw;
        }
        
    }

    private void SetupTransport(Allocation allocation)
    {
        string connectionType = ConfigureTransportType();
        fishyUnityTransport.SetRelayServerData(new RelayServerData(allocation, connectionType:connectionType));
    }
    
    private void SetupTransport(JoinAllocation allocation)
    {
        string connectionType = ConfigureTransportType();
        fishyUnityTransport.SetRelayServerData(new RelayServerData(allocation, connectionType:connectionType));
    }

    private string ConfigureTransportType()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log("WebGL; using wss");
        fishyUnityTransport.UseWebSockets = true;
        return "wss";
#else
        Debug.Log("Not webgl; using dtls");
        return "dtls";
#endif
    }
}
