using System.IO;
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
    private string offlineEntryScene;

    [SerializeField]
    [Scene]
    private string onlineEntryScene;

    private NetworkManager _networkManager;

    private void Awake()
    {
        _networkManager = InstanceFinder.NetworkManager;

        _networkManager.ServerManager.OnServerConnectionState += OnServerConnectionState;
        _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;
        _networkManager.SceneManager.OnLoadEnd += OnSceneLoadEnd;
    }

    private void OnDestroy()
    {
        _networkManager.ServerManager.OnServerConnectionState -= OnServerConnectionState;
        _networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
        _networkManager.SceneManager.OnLoadEnd -= OnSceneLoadEnd;
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
            if (!_networkManager.ServerManager.OneServerStarted())
            {
                return;
            }

            //If here can load scene.
            var sld = new SceneLoadData(GetSceneName(onlineEntryScene));
            sld.ReplaceScenes = ReplaceOption.None;
            _networkManager.SceneManager.LoadGlobalScenes(sld);
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
            if (!_networkManager.IsServer)
            {
                LoadOfflineScene();
            }
        }
    }

    private void OnSceneLoadEnd(SceneLoadEndEventArgs obj)
    {
        var onlineLoaded = false;
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
        {
            UnloadOfflineScene();
        }
    }

    private void LoadOfflineScene()
    {
        //Already in offline scene.
        if (UnitySceneManager.GetActiveScene().name == GetSceneName(offlineEntryScene))
        {
            return;
        }

        //Only use scene manager if networking scenes.
        UnitySceneManager.LoadScene(offlineEntryScene);
    }

    private void UnloadOfflineScene()
    {
        Scene scene = UnitySceneManager.GetSceneByName(GetSceneName(offlineEntryScene));
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