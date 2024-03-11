using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour
{
    public int _maxHealth = 10;

    [SyncVar]
    public int _health;

    [SerializeField]
    private Slider _healthSlider;

    [SerializeField]
    private Slider _healthSliderUI;

    private void Update()
    {
        _healthSlider.value = _health / (float)_maxHealth;
        _healthSliderUI.value = _health / (float)_maxHealth;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        _health = _maxHealth;
        if (!IsOwner)
        {
            _healthSliderUI.gameObject.SetActive(false);
        }
    }

    [ServerRpc]
    private void TakeDamageServerRpc(PlayerHealth target, int damage)
    {
        _health -= damage;
    }

    public void ReceiveDamageServer(int damage)
    {
        if (!IsServer)
        {
            return;
        }

        _health -= damage;
    }
}