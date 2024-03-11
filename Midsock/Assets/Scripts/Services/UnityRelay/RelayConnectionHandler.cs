using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
public class RelayConnectionHandler : MonoBehaviour
{
    public static RelayConnectionHandler Instance { get; private set; }

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
    }

    /// <summary>
    /// Try to initialize services. Call this every time we need to use a service, just in case
    /// </summary>
    /// <param name="token"></param>
    private async UniTask TryInitializeServices(CancellationToken token)
    {
        try
        {
            if (UnityServices.State != ServicesInitializationState.Uninitialized)
            {
                return;
            }

            await UnityServices.InitializeAsync();

            token.ThrowIfCancellationRequested();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async UniTask<string> BeginHostingAsync(string regionId, CancellationToken token)
    {
        try
        {
            allocation = await Unity.Services.Relay.RelayService.Instance.CreateAllocationAsync(4, regionId);
            token.ThrowIfCancellationRequested();

            JoinCode = await Unity.Services.Relay.RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            token.ThrowIfCancellationRequested();

            SetupTransport(allocation);

            Debug.Log($"Created Relay with code {JoinCode} in region {allocation.Region}");

            relayState = RelayState.Host;

            InstanceFinder.ServerManager.StartConnection();
            InstanceFinder.ClientManager.StartConnection();

            return JoinCode;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            fishyUnityTransport.Shutdown();
            throw;
        }
    }

    public async UniTaskVoid JoinGameAsync(string joinCode, CancellationToken token)
    {
        try
        {
            await TryInitializeServices(token);

            joinAllocation = await Unity.Services.Relay.RelayService.Instance.JoinAllocationAsync(joinCode);
            token.ThrowIfCancellationRequested();

            SetupTransport(joinAllocation);

            Debug.Log($"Joined Relay with code {joinCode} in region {allocation.Region}");

            relayState = RelayState.Client;

            InstanceFinder.ClientManager.StartConnection();
        }
        catch (Exception e)
        {
            Debug.Log(e);
            fishyUnityTransport.Shutdown();
            throw;
        }
    }

    private void SetupTransport(Allocation allocation)
    {
        string connectionType = ConfigureTransportType();
        fishyUnityTransport.SetRelayServerData(new RelayServerData(allocation, connectionType: connectionType));
    }

    private void SetupTransport(JoinAllocation allocation)
    {
        string connectionType = ConfigureTransportType();
        fishyUnityTransport.SetRelayServerData(new RelayServerData(allocation, connectionType: connectionType));
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

    private void OnDestroy()
    {
        fishyUnityTransport.Shutdown();
    }

    public async UniTask<List<Region>> GetRegionList(CancellationToken token)
    {
        try
        {
            await TryInitializeServices(token);
            token.ThrowIfCancellationRequested();
            var result = await Unity.Services.Relay.RelayService.Instance.ListRegionsAsync();
            token.ThrowIfCancellationRequested();
            return result;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return null;
        }
    }
}