using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Timing;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class NetworkDebugInfoPanel : MonoBehaviour
{
    private Rect _debugAreaRect;
    private bool _showing = true;
    private GUIStyle _smallTextStyle = new();
    private GUIStyle _textStyle = new();

    private int currentPanel;

    private void Awake()
    {
        SetupAppearance();
    }

    private void Update()
    {
        for (var i = (int)KeyCode.Alpha1; i <= (int)KeyCode.Alpha9; i++)
        {
            if (Input.GetKeyDown((KeyCode)i))
            {
                currentPanel = i - (int)KeyCode.Alpha1;
            }
        }
    }

    private void OnGUI()
    {
        if (!_showing)
        {
            return;
        }

        GUILayout.BeginArea(_debugAreaRect);

        switch (currentPanel)
        {
            case 0:
                DrawPing();
                DrawConnectionState();
                break;
            case 1:
                DrawScenes();
                break;
            case 2:
                DrawClientPresentScenes();
                break;
            case 3:
                break;
        }

        GUILayout.EndArea();
    }

    private void SetupAppearance()
    {
        _textStyle.fontSize = 18;
        _textStyle.fontStyle = FontStyle.Bold;
        _textStyle.normal.textColor = Color.green;
        _debugAreaRect = new Rect(10, 10, 1000, 1000);

        _smallTextStyle = new GUIStyle(_textStyle);
        _smallTextStyle.fontSize = 12;
        _smallTextStyle.normal.textColor = Color.black;
        _smallTextStyle.margin = new RectOffset(0, 0, 0, 0);
    }

    private void DrawClientPresentScenes()
    {
        // Show every connected scene and the clients in that scene
        List<Scene> scenes = InstanceFinder.SceneManager.SceneConnections.Keys.ToList();
        foreach (Scene scene in scenes)
        {
            GUILayout.Label(scene.name, _textStyle);
            foreach (NetworkConnection pair in InstanceFinder.SceneManager.SceneConnections[scene])
            {
                GUILayout.Label(pair.ClientId.ToString(), _smallTextStyle);
            }
        }
    }

    private void DrawScenes()
    {
        // list out all open scenee
        GUILayout.Label("Open Scenes:", _textStyle);

        Rect sceneObjectListRect = _debugAreaRect;
        for (var i = 0; i < UnitySceneManager.sceneCount; i++)
        {
            GUILayout.BeginArea(sceneObjectListRect);

            Scene scene = UnitySceneManager.GetSceneAt(i);
            GUILayout.Label(scene.name, _textStyle);
            // List out objects in the scene
            GameObject[] gameObjects = scene.GetRootGameObjects();

            foreach (GameObject go in gameObjects)
            {
                GUILayout.Label(go.name, _smallTextStyle);
            }

            GUILayout.EndArea();
            sceneObjectListRect.x += 200;
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