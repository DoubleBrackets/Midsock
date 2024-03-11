using FishNet;
using FishNet.Object;
using UnityEngine;

public class NetworkDebugInfoPanel : NetworkBehaviour
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
        DrawState();

        GUILayout.EndArea();
    }

    private void SetupAppearance()
    {
        _textStyle.fontSize = 20;
        _textStyle.fontStyle = FontStyle.Bold;
        _textStyle.normal.textColor = Color.green;
        _debugArea = new Rect(10, 10, 200, 200);
    }

    private void DrawPing()
    {
        long ping;
        long pingWithTickRate;
        var tm = InstanceFinder.TimeManager;
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

    private void DrawState()
    {
        GUILayout.Label($"IsServer: {IsServer}", _textStyle);
        GUILayout.Label($"IsClient: {IsClient}", _textStyle);
    }
}