using System;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

/// <summary>
/// Server controlled player data manager
/// </summary>
public class PlayerDataNetworkService : NetworkBehaviour
{
    [Serializable]
    public struct PlayerData
    {
        public string _playerName;

        public NetworkConnection Connection;
    }

    [SerializeField]
    private NetworkObject _playerClientPrefab;

    [SerializeField]
    private Transform _playerClientParent;

    [SyncObject]
    private readonly SyncDictionary<NetworkConnection, PlayerData> _playerDataMap = new();

    public override void OnStartServer()
    {
        SessionServiceFinder.SetPlayerDataNetworkService(this);
    }

    public override void OnStartClient()
    {
        SessionServiceFinder.SetPlayerDataNetworkService(this);
        base.OnStartClient();
        if (IsClient)
        {
            Debug.Log("PlayerDataNetworkService Spawning Client RPC");
            AddPlayerClient(LocalConnection);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddPlayerClient(NetworkConnection connection)
    {
        Debug.Log("Adding Player Client");
        _playerDataMap.Add(connection, new PlayerData
        {
            _playerName = "Player " + connection.ClientId,
            Connection = connection
        });

        if (IsServer)
        {
            Debug.Log("Spawning Player Client");
            SpawnPlayerClient(connection);
        }
    }

    private void SpawnPlayerClient(NetworkConnection connection)
    {
        NetworkObject spawned = Instantiate(_playerClientPrefab);
        ServerManager.Spawn(spawned, connection, gameObject.scene);
        spawned.transform.parent = _playerClientParent;
    }

    public PlayerData GetPlayerData(NetworkConnection connection)
    {
        if (_playerDataMap.TryGetValue(connection, out PlayerData playerData))
        {
            return playerData;
        }

        return default;
    }

    public IEnumerable<PlayerData> GetAllPlayerData()
    {
        return _playerDataMap.Values;
    }

    [ServerRpc]
    private void ServerSetPlayerData(NetworkConnection conn, PlayerData playerData)
    {
        Debug.Log($"Connection {conn.ClientId} is setting their player data");
        if (_playerDataMap.TryAdd(conn, playerData))
        {
            Debug.Log($"Player {playerData._playerName} has joined the game");
        }
        else
        {
            _playerDataMap[conn] = playerData;
            Debug.Log($"Player {playerData._playerName} has updated their data");
        }
    }
}