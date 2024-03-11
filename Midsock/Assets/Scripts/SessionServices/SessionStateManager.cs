using FishNet.Broadcast;
using FishNet.Managing.Scened;
using FishNet.Object;
using GameKit.Utilities.Types;
using UnityEngine;

/// <summary>
/// Server side manager for the session. Does not do client side logic.
/// </summary>
public class SessionStateManager : NetworkBehaviour
{
    private enum SessionState
    {
        Lobby,
        MatchStarted
    }

    public struct SpawnCharactersBroadcast : IBroadcast
    {
        public string DisplayName;
    }

    [SerializeField]
    [Scene]
    private string _lobbyScene;

    private SessionState _sessionState;

    public override void OnStartServer()
    {
        base.OnStartServer();
        LoadLobby();
    }

    private void LoadLobby()
    {
        Debug.Log("Loading Lobby Scene...");
        _sessionState = SessionState.Lobby;

        var lobbyScene = new SceneLookupData(_lobbyScene);
        var sd = new SceneLoadData(lobbyScene);
        sd.PreferredActiveScene = lobbyScene;

        SceneManager.LoadGlobalScenes(sd);
    }
}