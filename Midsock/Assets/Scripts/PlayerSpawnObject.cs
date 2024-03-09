using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class PlayerSpawnObject : NetworkBehaviour
{
    public GameObject objectToSpawn;

    [HideInInspector]
    public GameObject spawnedObject;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
        {
            enabled = false;
        }
    }

    private void Update()
    {
        if(spawnedObject == null && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log(objectToSpawn.name);
            SpawnObjectServerRpc(objectToSpawn, transform, this);
        }
        
        if(spawnedObject != null && Input.GetKeyDown(KeyCode.E))
        {
            DespawnObjectServerRpc(this);
        }
    }
    
    [ServerRpc]
    private void SpawnObjectServerRpc(GameObject obj, Transform player, PlayerSpawnObject self)
    {
        Debug.Log(obj.name);
        Debug.Log(player.gameObject.name);
        GameObject spawned = Instantiate(obj, player.position + player.forward * 2, Quaternion.identity);
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
