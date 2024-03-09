using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerColor : NetworkBehaviour
{
    [SerializeField]
    private GameObject player;
    
    [SerializeField]
    private Color color;

    private bool isOwner;
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        isOwner = base.IsOwner;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && isOwner)
        {
            ChangeColorServerRpc(player, color);
        }
    }
    
    [ServerRpc]
    private void ChangeColorServerRpc(GameObject ren, Color c)
    {
        ChangeColor(ren, c);
    }
    
    [ObserversRpc]
    private void ChangeColor(GameObject ren, Color c)
    {
        ren.GetComponentInChildren<Renderer>().material.color = c;
    }
}
