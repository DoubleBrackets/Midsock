using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Transporting.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
#endif

/// <summary>
///     Handles setting up Unity Relay service and hosting/connecting
/// </summary>
public class RelayConnectionHandler : MonoBehaviour
{
    private enum RelayState
    {
        Disconnected,
        Host,
        Client
    }

    [SerializeField]
    private FishyUnityTransport _fishyUnityTransport;

    public static RelayConnectionHandler Instance { get; private set; }

    public string JoinCode { get; private set; }

    private Allocation _currentAllocation;
    private JoinAllocation _currentJoinAllocation;

    private RelayState _relayState = RelayState.Disconnected;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    ///     Get a list of regions from the relay service
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public async UniTask<List<Region>> GetRegionList(CancellationToken token)
    {
        try
        {
            await TryInitializeUnityServices(token);
            token.ThrowIfCancellationRequested();
            List<Region> result = await RelayService.Instance.ListRegionsAsync();
            token.ThrowIfCancellationRequested();
            return result;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return null;
        }
    }

    /// <summary>
    ///     Attempt to start hosting a game through the relay service
    /// </summary>
    /// <param name="regionId">regionId, taken from region list</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async UniTask<string> BeginHostingAsync(string regionId, CancellationToken token)
    {
        Debug.Log($"Trying to host on region {regionId}");
        try
        {
            _currentAllocation = await RelayService.Instance.CreateAllocationAsync(4, regionId);
            token.ThrowIfCancellationRequested();

            string joinCode =
                await RelayService.Instance.GetJoinCodeAsync(_currentAllocation.AllocationId);
            token.ThrowIfCancellationRequested();

            // Codes are case insensitive, leave as upper since it's easier to read
            JoinCode = joinCode;
#if UNITY_EDITOR
            Clipboard.Copy(JoinCode);
#endif
            SetupTransport(_currentAllocation);

            Debug.Log($"Created Relay with code {JoinCode} in region {_currentAllocation.Region}");

            _relayState = RelayState.Host;

            // Host is both server and client
            InstanceFinder.ServerManager.StartConnection();
            InstanceFinder.ClientManager.StartConnection();

            return JoinCode;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            _fishyUnityTransport.Shutdown();
            throw;
        }
    }

    /// <summary>
    ///     Attempt to join a host using a join code and specified region.
    ///     Note that join codes are cross region, and determined by the host
    /// </summary>
    /// <param name="joinCode"></param>
    /// <param name="token"></param>
    public async UniTaskVoid JoinGameAsync(string joinCode, CancellationToken token)
    {
        try
        {
            await TryInitializeUnityServices(token);

            _currentJoinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            token.ThrowIfCancellationRequested();

            SetupTransport(_currentJoinAllocation);

            Debug.Log($"Joined Relay with code {joinCode} in region {_currentJoinAllocation.Region}");

            JoinCode = joinCode;

            _relayState = RelayState.Client;

            InstanceFinder.ClientManager.StartConnection();
        }
        catch (Exception e)
        {
            Debug.Log(e);
            _fishyUnityTransport.Shutdown();
            throw;
        }
    }

    private void SetupTransport(Allocation allocation)
    {
        ConfigureTransportType(out string connectionType);
        _fishyUnityTransport.SetRelayServerData(new RelayServerData(allocation, connectionType));
    }

    private void SetupTransport(JoinAllocation joinAllocation)
    {
        ConfigureTransportType(out string connectionType);
        _fishyUnityTransport.SetRelayServerData(new RelayServerData(joinAllocation, connectionType));
    }

    private void ConfigureTransportType(out string connectionType)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log("WebGL; using wss");
        _fishyUnityTransport.UseWebSockets = true;
        connectionType = "wss";
#else
        Debug.Log("Not webgl; using dtls");
        connectionType = "dtls";
#endif
    }

    /// <summary>
    ///     Try to initialize and sign in to Unity Services.
    ///     This should be called by any method that requires Unity Services to be initialized, so retries are handled
    ///     implicitly
    /// </summary>
    /// <param name="token"></param>
    private async UniTask TryInitializeUnityServices(CancellationToken token)
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
}