using FishNet.Object;
using UnityEngine;

public class PlayerColor : NetworkBehaviour
{
    [SerializeField]
    private GameObject player;

    [SerializeField]
    private Color color;

    private bool _isOwner;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && _isOwner)
        {
            ChangeColorServerRpc(player, color);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        _isOwner = IsOwner;
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