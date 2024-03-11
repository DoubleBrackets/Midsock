using FishNet.Broadcast;
using FishNet.Object;
using TMPro;
using UnityEngine;

public class PlayerWorldUI : NetworkBehaviour
{
    private struct NameBroadcast : IBroadcast
    {
        public string Name;
    }

    [SerializeField]
    private TMP_Text _playerNameText;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner)
        {
            ServerSetPlayerNameServerRpc(LocalPlayerDataService.Instance.PlayerName);
        }
    }

    [ServerRpc]
    private void ServerSetPlayerNameServerRpc(string playerName)
    {
        // updating on the server makes sure new clients get the updated name
        _playerNameText.text = playerName;
        // broadcast to all clients
        ClientSetPlayerNameObserversRpc(playerName);
    }

    [ObserversRpc(BufferLast = true)]
    private void ClientSetPlayerNameObserversRpc(string playerName)
    {
        _playerNameText.text = playerName;
    }
}