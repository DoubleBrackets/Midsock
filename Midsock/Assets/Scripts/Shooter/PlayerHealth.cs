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

    private void Update()
    {
        healthSlider.value = health / (float)maxHealth;
        healthSliderUI.value = health / (float)maxHealth;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        health = maxHealth;
        if (!IsOwner)
        {
            healthSliderUI.gameObject.SetActive(false);
        }
    }

    [ServerRpc]
    private void TakeDamageServerRpc(PlayerHealth target, int damage)
    {
        health -= damage;
    }

    public void ReceiveDamageServer(int damage)
    {
        if (!IsServer)
        {
            return;
        }

        health -= damage;
    }
}