using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using GameKit.Utilities.Types;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

/// <summary>
/// Handles loading the online/offline entry scenes when connection states change.
/// </summary>
public class ConnectionEntrySceneSwitcher : MonoBehaviour
{
    [SerializeField, Scene]
    private string offlineEntryScene;
    
    [SerializeField, Scene]
    private string onlineEntryScene;
    
    private NetworkManager networkManager;
    
    void Awake()
    {
        networkManager = InstanceFinder.NetworkManager;
        
        networkManager.ServerManager.OnServerConnectionState += OnServerConnectionState;
        networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;
        networkManager.SceneManager.OnLoadEnd += OnSceneLoadEnd;
    }

    private void OnDestroy()
    {
        networkManager.ServerManager.OnServerConnectionState -= OnServerConnectionState;
        networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
        networkManager.SceneManager.OnLoadEnd -= OnSceneLoadEnd;
    }
    
    private void OnServerConnectionState(ServerConnectionStateArgs args)
    {
        /* When server starts load online scene as global.
         * Since this is a global scene clients will automatically
         * join it when connecting. */
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            /* If not exactly one server is started then
             * that means either none are started, which isnt true because
             * we just got a started callback, or two+ are started.
             * When a server has already started there's no reason to load
             * scenes again. */
            if (!networkManager.ServerManager.OneServerStarted())
                return;

            //If here can load scene.
            SceneLoadData sld = new SceneLoadData(GetSceneName(onlineEntryScene));
            sld.ReplaceScenes = ReplaceOption.None;
            networkManager.SceneManager.LoadGlobalScenes(sld);
        }
        //When server stops load offline scene.
        else if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            LoadOfflineScene();
        }
    }
    
    private void OnClientConnectionState(ClientConnectionStateArgs obj)
    {
        if (obj.ConnectionState == LocalConnectionState.Stopped)
        {
            //Only load offline scene if not also server. (in the case of a host)
            if (!networkManager.IsServer)
                LoadOfflineScene();
        }
    }

    private void OnSceneLoadEnd(SceneLoadEndEventArgs obj)
    {
        bool onlineLoaded = false;
        foreach (Scene s in obj.LoadedScenes)
        {
            if (s.name == GetSceneName(onlineEntryScene))
            {
                onlineLoaded = true;
                break;
            }
        }

        //If online scene was loaded then unload offline.
        if (onlineLoaded)
            UnloadOfflineScene();
    }
    
    private void LoadOfflineScene()
    {
        //Already in offline scene.
        if (UnitySceneManager.GetActiveScene().name == GetSceneName(offlineEntryScene))
            return;
        
        //Only use scene manager if networking scenes.
        UnitySceneManager.LoadScene(offlineEntryScene);
    }
    
    private void UnloadOfflineScene()
    {
        Scene scene = UnitySceneManager.GetSceneByName(GetSceneName(offlineEntryScene));
        if (string.IsNullOrEmpty(scene.name))
            return;

        UnitySceneManager.UnloadSceneAsync(scene);
    }
    
    private string GetSceneName(string fullPath)
    {
        return Path.GetFileNameWithoutExtension(fullPath);
    }
}
