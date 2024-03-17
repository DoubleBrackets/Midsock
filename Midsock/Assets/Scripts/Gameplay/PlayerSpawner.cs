using FishNet.Connection;
using FishNet.Object;
using UnityEditor;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Vector3 position = transform.position;
        Handles.Label(position, "PlayerSpawner");
        Gizmos.DrawWireSphere(position, 1f);
    }
#endif

    public override void OnStartClient()
    {
        base.OnStartClient();
        SpawnCharacter();
    }

    private void SpawnCharacter()
    {
        Debug.Log("Making call to spawn character");
        SpawnCharacterServer(LocalConnection, transform.position);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnCharacterServer(NetworkConnection owner, Vector3 position)
    {
        PlayerCharacterService playerCharacterService = SessionServiceFinder.PlayerCharacterService;

        // Character already spawned
        if (playerCharacterService.GetCharacterNob(owner) != null)
        {
            return;
        }

        // we need to pass in the scene from the server side!
        SessionServiceFinder.PlayerCharacterService.SpawnCharacter(
            owner,
            gameObject.scene,
            position);
    }
}