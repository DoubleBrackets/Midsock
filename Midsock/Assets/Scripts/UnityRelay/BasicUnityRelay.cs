using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FishNet.Transporting.UTP;
using TMPro;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay.Models;
using UnityEngine;

/// <summary>
/// Sets up UGS relay service
/// </summary>
public class BasicUnityRelay : MonoBehaviour
{
    
    [SerializeField]
    private FishyUnityTransport fishyUnityTransport;
    
    [SerializeField]
    private ConnectionStarter connectionStarter;

    [SerializeField]
    private TMP_InputField joinCodeField;

    private string joinCode;
    
    private Allocation allocation;
    private JoinAllocation joinAllocation;
    
    private void Awake()
    {
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

    public void CreateLobby()
    {
        AllocateLobby();
    }
    
    public async UniTaskVoid AllocateLobby()
    {
        allocation = await Unity.Services.Relay.RelayService.Instance.CreateAllocationAsync(7);
        joinCode = await Unity.Services.Relay.RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        
        SetupTransport(allocation);
        
        joinCodeField.text = joinCode;
        
        Debug.Log($"Created Relay with code {joinCode}");
        
        connectionStarter.StartConnection(ConnectionStarter.ConnectionType.Host);
    }

    public void Join()
    {
        JoinLobby();
    }
    
    public async UniTaskVoid JoinLobby()
    {
        joinCode = joinCodeField.text;
        joinAllocation = await Unity.Services.Relay.RelayService.Instance.JoinAllocationAsync(joinCode);
        
        SetupTransport(joinAllocation);
        
        Debug.Log($"Joined Relay with code {joinCode}");
        
        connectionStarter.StartConnection(ConnectionStarter.ConnectionType.Client);

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
