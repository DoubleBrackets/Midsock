using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using GameKit.Utilities.Types;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

/// <summary>
/// Handles loading the online/offline entry scenes when connection states change.
/// </summary>
public class ConnectionEntrySceneSwitcher : MonoBehaviour
{
    [SerializeField]
    [Scene]
    private string _offlineEntryScene;

    [SerializeField]
    [Scene]
    private string _coreSceneProtected;

    [SerializeField]
    [Scene]
    private string _onlineEntryScene;

    private NetworkManager _networkManager;

    private void Awake()
    {
        _networkManager = InstanceFinder.NetworkManager;

        _networkManager.ServerManager.OnServerConnectionState += OnServerConnectionState;
        _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;
        _networkManager.SceneManager.OnLoadEnd += OnSceneLoadEnd;
        _networkManager.ClientManager.OnAuthenticated += OnAuthenticated;
    }

    private void OnDestroy()
    {
        _networkManager.ServerManager.OnServerConnectionState -= OnServerConnectionState;
        _networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
        _networkManager.SceneManager.OnLoadEnd -= OnSceneLoadEnd;
        _networkManager.ClientManager.OnAuthenticated -= OnAuthenticated;
    }

    private void OnAuthenticated()
    {
    }

    private void OnServerConnectionState(ServerConnectionStateArgs args)
    {
        // We want to load the "base" connection scene when the server starts
        // Global scene so all clients receive it.
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            if (!_networkManager.ServerManager.OneServerStarted())
            {
                return;
            }

            var sld = new SceneLoadData(GetSceneName(_onlineEntryScene));

            // There shouldn't be any online scenes loaded, just the offline scene.
            sld.ReplaceScenes = ReplaceOption.None;
            _networkManager.SceneManager.LoadGlobalScenes(sld);
        }
        // When server stops load offline scene.
        else if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            Debug.Log("Server stopped: Loading offline scenes");
            LoadOfflineScene(gameObject.GetCancellationTokenOnDestroy());
        }
    }

    private void OnClientConnectionState(ClientConnectionStateArgs obj)
    {
        if (obj.ConnectionState == LocalConnectionState.Stopped)
        {
            // Only load offline scene if not also server. (in the case of a host)
            if (!_networkManager.IsServer)
            {
                LoadOfflineScene(gameObject.GetCancellationTokenOnDestroy());
            }
        }
    }

    private void OnSceneLoadEnd(SceneLoadEndEventArgs args)
    {
        // When online scene is loaded, unload offline scene.
        var onlineLoaded = false;
        foreach (Scene s in args.LoadedScenes)
        {
            if (s.name == GetSceneName(_onlineEntryScene))
            {
                onlineLoaded = true;
                break;
            }
        }

        //If online scene was loaded then unload offline.
        if (onlineLoaded)
        {
            UnloadOfflineScene();
        }
    }

    private async void LoadOfflineScene(CancellationToken token)
    {
        //Already in offline scene.
        if (UnitySceneManager.GetActiveScene().name == GetSceneName(_offlineEntryScene))
        {
            return;
        }

        try
        {
            // Unload all scenes except the core scene.
            List<UniTask> unloadTasks = new();
            foreach (Scene s in UnitySceneManager.GetAllScenes())
            {
                if (s.name != GetSceneName(_coreSceneProtected))
                {
                    unloadTasks.Add(UnitySceneManager.UnloadSceneAsync(s).ToUniTask());
                }
            }

            token.ThrowIfCancellationRequested();

            await UniTask.WhenAll(unloadTasks);
            token.ThrowIfCancellationRequested();

            //Only use scene manager if networking scenes.
            await UnitySceneManager.LoadSceneAsync(_offlineEntryScene, LoadSceneMode.Additive);
            token.ThrowIfCancellationRequested();

            UnitySceneManager.SetActiveScene(UnitySceneManager.GetSceneByName(GetSceneName(_offlineEntryScene)));
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    private void UnloadOfflineScene()
    {
        Scene scene = UnitySceneManager.GetSceneByName(GetSceneName(_offlineEntryScene));
        if (string.IsNullOrEmpty(scene.name))
        {
            return;
        }

        UnitySceneManager.UnloadSceneAsync(scene);
    }

    private string GetSceneName(string fullPath)
    {
        return Path.GetFileNameWithoutExtension(fullPath);
    }
}