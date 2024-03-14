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
        _connectionData.OnDisconnect += DisplayDisconnectReason;
    }

    private void OnDestroy()
    {
        _connectionData.OnDisconnect -= DisplayDisconnectReason;
    }

    private void DisplayDisconnectReason(ConnectionDataSO.DisconnectReason disconnectReason)
    {
        if (_popupUI.IsShowing)
        {
            return;
        }

        _popupUI.Show();
        var reason = "Disconnected";

        if (disconnectReason == ConnectionDataSO.DisconnectReason.ClientRequestedDisconnect)
        {
            reason = "You left the game";
        }
        else if (disconnectReason == ConnectionDataSO.DisconnectReason.ConnectionTimeout)
        {
            reason = "Connection timed out";
        }
        else if (disconnectReason == ConnectionDataSO.DisconnectReason.VersionCheckFailed)
        {
            reason = "Version check failed";
        }
        else
        {
            reason = "Host closed the connection";
        }

        _popupUI.SetText(reason);
    }
}