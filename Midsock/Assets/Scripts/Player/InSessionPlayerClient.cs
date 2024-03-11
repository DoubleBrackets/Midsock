using FishNet;
using FishNet.Object;

public class InSessionPlayerClient : NetworkBehaviour
{
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (IsOwner)
        {
            if (IsHost)
            {
                gameObject.name = "Player Client (Host)";
            }
            else
            {
                gameObject.name = "Player Client (Local)";
            }
        }
        else
        {
            gameObject.name = "Player Client (Remote)";
        }

        InstanceFinder.ClientManager.RegisterBroadcast<SessionStateManager.SpawnCharactersBroadcast>(
            HandleCharacterSpawnedBroadcast);
    }

    private void HandleCharacterSpawnedBroadcast(SessionStateManager.SpawnCharactersBroadcast data)
    {
        // What.
    }
}