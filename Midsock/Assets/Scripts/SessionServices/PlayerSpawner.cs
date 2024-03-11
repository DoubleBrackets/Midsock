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
        PlayerDataService.Instance.SpawnCharacterClient(LocalConnection);
    }
}