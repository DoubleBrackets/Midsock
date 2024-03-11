using FishNet.Object;

public class PlayerSpawner : NetworkBehaviour
{
    public override void OnStartClient()
    {
        base.OnStartClient();

        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        PlayerDataNetworkService.Instance.SpawnCharacterClient(LocalConnection);
    }
}