using FishNet.Object;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    public override void OnStartClient()
    {
        Debug.Log("PlayerSpawner OnStartClient");
        base.OnStartClient();

        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        Debug.Log("PlayerSpawner Spawning Player");
        PlayerDataNetworkService.Instance.SpawnCharacterClient(LocalConnection);
    }
}