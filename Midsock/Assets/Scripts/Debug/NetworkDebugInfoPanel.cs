using FishNet;
using FishNet.Managing.Timing;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class NetworkDebugInfoPanel : MonoBehaviour
{
    private Rect _debugArea;
    private bool _showing = true;
    private GUIStyle _textStyle = new();

    private void Awake()
    {
        SetupAppearance();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _showing = !_showing;
        }
    }

    private void OnGUI()
    {
        if (!_showing)
        {
            return;
        }

        GUILayout.BeginArea(_debugArea);

        DrawPing();
        DrawConnectionState();
        DrawScenes();

        GUILayout.EndArea();
    }

    private void SetupAppearance()
    {
        _textStyle.fontSize = 18;
        _textStyle.fontStyle = FontStyle.Bold;
        _textStyle.normal.textColor = Color.green;
        _debugArea = new Rect(10, 10, 200, 1000);
    }

    private void DrawScenes()
    {
        // list out all open scenee
        var style = new GUIStyle(_textStyle);
        style.fontStyle = FontStyle.Italic;
        GUILayout.Label("Open Scenes:", style);
        for (var i = 0; i < UnitySceneManager.sceneCount; i++)
        {
            style = new GUIStyle(_textStyle);
            style.fontStyle = FontStyle.Normal;
            style.fontSize = 14;
            GUILayout.Label(UnitySceneManager.GetSceneAt(i).name, style);
        }

        GameObject[] gameObjects = FindObjectsOfType<GameObject>();
        var style2 = new GUIStyle(_textStyle);
        style2.fontSize = 10;
        style2.margin = new RectOffset(0, 0, 0, 0);
        foreach (GameObject gameObject in gameObjects)
        {
            if (gameObject.transform.parent == null)
            {
                GUILayout.Label(gameObject.name, style2);
            }
        }
    }

    private void DrawPing()
    {
        long ping;
        long pingWithTickRate;
        TimeManager tm = InstanceFinder.TimeManager;
        if (tm == null)
        {
            ping = 0;
            pingWithTickRate = 0;
        }
        else
        {
            ping = tm.RoundTripTime;
            var deduction = (long)(tm.TickDelta * 2000d);

            pingWithTickRate = (long)Mathf.Max(1, ping - deduction);
        }

        GUILayout.Label($"Ping: {ping}ms", _textStyle);
        GUILayout.Label($"Ping (Tickrate): {pingWithTickRate}ms", _textStyle);
    }

    private void DrawConnectionState()
    {
        bool IsServer = InstanceFinder.NetworkManager.IsServer;
        bool IsClient = InstanceFinder.NetworkManager.IsClient;
        bool IsHost = InstanceFinder.NetworkManager.IsHost;

        string connectionState = IsHost ? "Host" : IsClient ? "Client" : IsServer ? "Server" : "Offline";

        GUILayout.Label($"Connection: {connectionState}", _textStyle);
    }
}