using System;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "ConnectionState")]
public class ConnectionDataSO : AnnotatedScriptableObject, IValueResettable
{
    [Flags]
    public enum DisconnectReason
    {
        Disconnected = 1 << 0,
        ClientRequestedDisconnect = 1 << 1,
        ConnectionTimeout = 1 << 2
    }

    [field: ShowInInspector]
    public DisconnectReason LastDisconnectReason { get; set; }

    public void ResetValues()
    {
        LastDisconnectReason = 0;
    }
}