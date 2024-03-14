using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ConnectionState")]
public class ConnectionDataSO : AnnotatedScriptableObject
{
    public enum DisconnectReason
    {
        Disconnected,
        ClientRequestedDisconnect,
        ConnectionTimeout,
        VersionCheckFailed
    }

    public event Action<DisconnectReason> OnDisconnect;

    public void InvokeOnDisconnect(DisconnectReason reason)
    {
        OnDisconnect?.Invoke(reason);
    }
}