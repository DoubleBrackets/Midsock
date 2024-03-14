using UnityEngine;

public class OnDisconnectPopupUI : MonoBehaviour
{
    [SerializeField]
    private PopupUI _popupUI;

    [SerializeField]
    private ConnectionDataSO _connectionData;

    private void Start()
    {
        _popupUI.Hide();
        if (_connectionData.LastDisconnectReason != 0)
        {
            _popupUI.Show();
            var reason = "Disconnected";

            if ((_connectionData.LastDisconnectReason & ConnectionDataSO.DisconnectReason.ClientRequestedDisconnect) !=
                0)
            {
                reason = "You left the game";
            }
            else if ((_connectionData.LastDisconnectReason & ConnectionDataSO.DisconnectReason.ConnectionTimeout) != 0)
            {
                reason = "Connection timed out";
            }
            else
            {
                reason = "Host closed the connection";
            }

            _popupUI.SetText(reason);
        }
    }
}