using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour
{
    public int maxHealth = 10;

    [SyncVar]
    public int health;
    
    [SerializeField]
    private Slider healthSlider;

    [SerializeField]
    private Slider healthSliderUI;
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        health = maxHealth;
        if (!base.IsOwner)
        {
            healthSliderUI.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        healthSlider.value = health / (float) maxHealth;
        healthSliderUI.value = health / (float) maxHealth;
    }
    
    [ServerRpc]
    private void TakeDamageServerRpc(PlayerHealth target, int damage)
    {
        health -= damage;
    }

    public void ReceiveDamageServer(int damage)
    {
        if(!base.IsServer)
        {
            return;
        }
        health -= damage;
    }
}
