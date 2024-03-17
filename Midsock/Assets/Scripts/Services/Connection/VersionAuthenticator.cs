using System;
using FishNet.Authenticating;
using FishNet.Broadcast;
using FishNet.Connection;
using FishNet.Example.Authenticating;
using FishNet.Managing;
using FishNet.Transporting;
using UnityEngine;

public class VersionAuthenticator : HostAuthenticator
{
    // Start is called before the first frame update

    public struct VersionBroadcast : IBroadcast
    {
        public string Version;
    }

    public struct VersionResponseBroadcast : IBroadcast
    {
        public bool Passed;
    }

    #region Public.

    /// <summary>
    /// Called when authenticator has concluded a result for a connection. Boolean is true if authentication passed, false if failed.
    /// Server listens for this event automatically.
    /// </summary>
    public override event Action<NetworkConnection, bool> OnAuthenticationResult;

    #endregion

    public override void InitializeOnce(NetworkManager networkManager)
    {
        base.InitializeOnce(networkManager);

        //Listen for connection state change as client.
        NetworkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
        //Listen for broadcast from client. Be sure to set requireAuthentication to false.
        NetworkManager.ServerManager.RegisterBroadcast<VersionBroadcast>(OnPasswordBroadcast, false);
        //Listen to response from server.
        NetworkManager.ClientManager.RegisterBroadcast<VersionResponseBroadcast>(OnResponseBroadcast);
    }

    /// <summary>
    /// Called when a connection state changes for the local client.
    /// </summary>
    private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs args)
    {
        /* If anything but the started state then exit early.
         * Only try to authenticate on started state. The server
         * doesn't have to send an authentication request before client
         * can authenticate, that is entirely optional and up to you. In this
         * example the client tries to authenticate soon as they connect. */
        if (args.ConnectionState != LocalConnectionState.Started)
        {
            return;
        }

        //Authentication was sent as host, no need to authenticate normally.
        if (AuthenticateAsHost())
        {
            return;
        }

        var pb = new VersionBroadcast
        {
            Version = _version
        };

        NetworkManager.ClientManager.Broadcast(pb);
    }

    /// <summary>
    /// Received on server when a client sends the password broadcast message.
    /// </summary>
    /// <param name="conn">Connection sending broadcast.</param>
    /// <param name="pb"></param>
    private void OnPasswordBroadcast(NetworkConnection conn, VersionBroadcast pb)
    {
        /* If client is already authenticated this could be an attack. Connections
         * are removed when a client disconnects so there is no reason they should
         * already be considered authenticated. */
        if (conn.Authenticated)
        {
            conn.Disconnect(true);
            return;
        }

        bool correctPassword = pb.Version == _version;
        SendAuthenticationResponse(conn, correctPassword);
        /* Invoke result. This is handled internally to complete the connection or kick client.
         * It's important to call this after sending the broadcast so that the broadcast
         * makes it out to the client before the kick. */
        OnAuthenticationResult?.Invoke(conn, correctPassword);
    }

    /// <summary>
    /// Received on client after server sends an authentication response.
    /// </summary>
    /// <param name="rb"></param>
    private void OnResponseBroadcast(VersionResponseBroadcast rb)
    {
        string result = rb.Passed ? "Authentication complete." : "Authentication failed.";
        if (!rb.Passed)
        {
            _connectionData.InvokeOnDisconnect(ConnectionDataSO.DisconnectReason.VersionCheckFailed);
        }

        NetworkManager.Log(result);
    }

    /// <summary>
    /// Sends an authentication result to a connection.
    /// </summary>
    private void SendAuthenticationResponse(NetworkConnection conn, bool authenticated)
    {
        /* Tell client if they authenticated or not. This is
         * entirely optional but does demonstrate that you can send
         * broadcasts to client on pass or fail. */
        var rb = new ResponseBroadcast
        {
            Passed = authenticated
        };
        NetworkManager.ServerManager.Broadcast(conn, rb, false);
    }

    /// <summary>
    /// Called after handling a host authentication result.
    /// </summary>
    /// <param name="conn">Connection authenticating.</param>
    /// <param name="authenticated">True if authentication passed.</param>
    protected override void OnHostAuthenticationResult(NetworkConnection conn, bool authenticated)
    {
        SendAuthenticationResponse(conn, authenticated);
        OnAuthenticationResult?.Invoke(conn, authenticated);
    }

    #region Serialized.

    /// <summary> 
    /// Password to authenticate.
    /// </summary>
    [Tooltip("Version to authenticate.")]
    [SerializeField]
    private string _version = "HelloWorld";

    [SerializeField]
    private ConnectionDataSO _connectionData;

    #endregion
}