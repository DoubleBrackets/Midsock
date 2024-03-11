using FishNet.Object;
using UnityEngine;

public class PlayerSpawnObject : NetworkBehaviour
{
    public GameObject objectToSpawn;

    [HideInInspector]
    public GameObject spawnedObject;

    private void Update()
    {
        if (spawnedObject == null && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log(objectToSpawn.name);
            SpawnObjectServerRpc(objectToSpawn, transform, this);
        }

        if (spawnedObject != null && Input.GetKeyDown(KeyCode.E))
        {
            DespawnObjectServerRpc(this);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
        {
            enabled = false;
        }
    }

    [ServerRpc]
    private void SpawnObjectServerRpc(GameObject obj, Transform player, PlayerSpawnObject self)
    {
        Debug.Log(obj.name);
        Debug.Log(player.gameObject.name);
        var spawned = Instantiate(obj, player.position + player.forward * 2, Quaternion.identity);
        ServerManager.Spawn(spawned);
        SpawnObject(spawned, player, self);
    }

    [ObserversRpc]
    private void SpawnObject(GameObject obj, Transform player, PlayerSpawnObject self)
    {
        self.spawnedObject = obj;
    }

    [ServerRpc]
    private void DespawnObjectServerRpc(PlayerSpawnObject self)
    {
        ServerManager.Despawn(spawnedObject);
    }
}