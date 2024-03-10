using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{

    public override void OnStartClient()
    {
        base.OnStartClient();

        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        PlayerDataService.Instance.SpawnCharacterClient(LocalConnection);
    }
}
