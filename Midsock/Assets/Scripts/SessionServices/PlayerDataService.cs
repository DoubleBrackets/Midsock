using System;
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
    [Serializable]
    public struct PlayerData
    {
        public string playerName;
        public NetworkConnection Connection;
    }

    public GameObject characterPrefab;

    public GameObject playerClientPrefab;
    public static PlayerDataService Instance { get; private set; }

    [SyncObject]
    private readonly SyncDictionary<NetworkConnection, PlayerData> _playerDataMap = new();

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
        _playerDataMap.Add(connection, new PlayerData
        {
            playerName = "Player " + connection.ClientId,
            Connection = connection
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
        PlayerData playerData = _playerDataMap[connection];
        Debug.Log($"Spawning character for {playerData.playerName}");
        GameObject spawned = Instantiate(characterPrefab, Vector3.up * 5f, Quaternion.identity);
        ServerManager.Spawn(spawned, playerData.Connection);
        InstanceFinder.ServerManager.Broadcast(new SessionStateManager.SpawnCharactersBroadcast
        {
            DisplayName = playerData.playerName
        });
    }

    public PlayerData GetLocalClientPlayerData()
    {
        if (!IsClient)
        {
            return default;
        }

        if (_playerDataMap.TryGetValue(LocalConnection, out PlayerData playerData))
        {
            return playerData;
        }

        return default;
    }

    public IEnumerable<PlayerData> GetAllPlayerData()
    {
        return _playerDataMap.Values;
    }

    public void SetLocalClientPlayerData(PlayerData data)
    {
        if (!IsClient)
        {
            return;
        }

        ServerSetPlayerData(LocalConnection, data);
    }

    [ServerRpc]
    private void ServerSetPlayerData(NetworkConnection conn, PlayerData playerData)
    {
        Debug.Log($"Connection {conn.ClientId} is setting their player data");
        if (_playerDataMap.TryAdd(conn, playerData))
        {
            Debug.Log($"Player {playerData.playerName} has joined the game");
        }
        else
        {
            _playerDataMap[conn] = playerData;
            Debug.Log($"Player {playerData.playerName} has updated their data");
        }
    }
}