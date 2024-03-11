using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
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
    
    [SerializeField, Scene]
    private string LobbyScene;
    
    private enum SessionState
    {
        Lobby,
        MatchStarted
    }
    
    private SessionState sessionState;
    
    public override void OnStartServer()
    {
        base.OnStartServer();
        LoadLobby();
    }

    private void LoadLobby()
    {
        Debug.Log("Loading Lobby Scene...");
        sessionState = SessionState.Lobby;
        
        SceneLookupData lobbyScene = new SceneLookupData(LobbyScene);
        SceneLoadData sd = new SceneLoadData(lobbyScene);
        sd.PreferredActiveScene = lobbyScene;
        
        SceneManager.LoadGlobalScenes(sd);
    }


    public struct SpawnCharactersBroadcast : IBroadcast
    {
        public string displayName;
    }
}
