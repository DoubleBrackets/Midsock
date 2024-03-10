using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

/// <summary>
/// Server controlled player data manager
/// </summary>
public class PlayerDataService : NetworkBehaviour
{
    public static PlayerDataService Instance { get; private set; }
    
    public GameObject characterPrefab;
    
    public GameObject playerClientPrefab;
    
    [System.Serializable]
    public struct PlayerData
    {
        public string playerName;
        public NetworkConnection connection;
    }

    [SyncObject]
    private readonly SyncDictionary<NetworkConnection, PlayerData> playerDataMap = new();

    private void Awake()
    {
        Instance = this;
    }
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (IsClient)
        {
            AddPlayerClient(LocalConnection);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void AddPlayerClient(NetworkConnection connection)
    {
        playerDataMap.Add(connection, new PlayerData
        {
            playerName = "Player " + connection.ClientId,
            connection = connection
        });
        
        if (IsServer)
        {
            SpawnPlayerClient(connection);
        }
    }

    private void SpawnPlayerClient(NetworkConnection connection)
    {
        GameObject spawned = Instantiate(playerClientPrefab);
        ServerManager.Spawn(spawned, connection, gameObject.scene);
    }

    public void SpawnCharacterClient(NetworkConnection connection)
    {
        Debug.Log("Spawning Player...");
        SpawnCharacter(connection);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnCharacter(NetworkConnection connection)
    {
        var playerData = playerDataMap[connection];
        Debug.Log($"Spawning character for {playerData.playerName}");
        GameObject spawned = Instantiate(characterPrefab, Vector3.up * 5f, Quaternion.identity);
        ServerManager.Spawn(spawned, playerData.connection);
        InstanceFinder.ServerManager.Broadcast(new SessionStateManager.SpawnCharactersBroadcast()
        {
            displayName = playerData.playerName
        });
    }

    public PlayerData GetLocalClientPlayerData()
    {
        if (!base.IsClient)
        {
            return default;
        }
        
        if (playerDataMap.TryGetValue(LocalConnection, out var playerData))
        {
            return playerData;
        }

        return default;
    }
    
    public IEnumerable<PlayerData> GetAllPlayerData()
    {
        return playerDataMap.Values;
    }

    public void SetLocalClientPlayerData(PlayerData data)
    {
        if (!base.IsClient)
        {
            return;
        }
        
        ServerSetPlayerData(LocalConnection, data);
    }
    
    [ServerRpc]
    private void ServerSetPlayerData(NetworkConnection conn, PlayerData playerData)
    {
        Debug.Log($"Connection {conn.ClientId} is setting their player data");
        if (playerDataMap.TryAdd(conn, playerData))
        {
            Debug.Log($"Player {playerData.playerName} has joined the game");
        }
        else
        {
            playerDataMap[conn] = playerData;
            Debug.Log($"Player {playerData.playerName} has updated their data");
        }
    }
    
}
