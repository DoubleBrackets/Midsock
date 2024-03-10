using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using UnityEngine;

public class InSessionPlayerClient : NetworkBehaviour
{
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            if(base.IsHost)
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
        
        InstanceFinder.ClientManager.RegisterBroadcast<SessionStateManager.SpawnCharactersBroadcast>(HandleCharacterSpawnedBroadcast);
    }

    
    private void HandleCharacterSpawnedBroadcast(SessionStateManager.SpawnCharactersBroadcast data)
    {
        // What.
    }
}
