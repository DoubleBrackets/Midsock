using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Server side only service for managing player's character
/// </summary>
public class PlayerCharacterService : NetworkBehaviour
{
    [SerializeField]
    private NetworkObject _characterPrefab;

    private Dictionary<NetworkConnection, NetworkObject> _playerCharacters = new();

    public override void OnStartServer()
    {
        SessionServiceFinder.SetPlayerCharacterService(this);
    }

    public override void OnStartClient()
    {
        if (!IsServer)
        {
            Destroy(gameObject);
        }
    }

    public void SpawnCharacter(NetworkConnection connection, Scene scene, Vector3 position)
    {
        if (_playerCharacters.ContainsKey(connection))
        {
            return;
        }

        string playerName = SessionServiceFinder.PlayerDataNetworkService.GetPlayerData(connection)._playerName;

        Debug.Log($"Spawning char for {connection.ClientId} in {scene.name} {scene.handle}...");
        NetworkObject spawned = Instantiate(_characterPrefab, position, Quaternion.identity);

        InstanceFinder.ServerManager.Spawn(spawned, connection, scene);

        _playerCharacters.Add(connection, spawned);
    }

    public NetworkObject GetCharacterNob(NetworkConnection connection)
    {
        if (_playerCharacters.TryGetValue(connection, out NetworkObject character))
        {
            return character;
        }

        return null;
    }

    public void DespawnCharacter(NetworkConnection connection)
    {
        if (_playerCharacters.TryGetValue(connection, out NetworkObject character))
        {
            _playerCharacters.Remove(connection);
            InstanceFinder.ServerManager.Despawn(character);
        }
    }
}