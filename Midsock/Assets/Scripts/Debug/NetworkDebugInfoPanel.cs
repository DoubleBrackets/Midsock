using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Managing.Timing;
using FishNet.Object;
using UnityEngine;

public class NetworkDebugInfoPanel : NetworkBehaviour
{
    private bool showing = true;
    private GUIStyle textStyle = new GUIStyle();

    private Rect debugArea;

    private void Awake()
    {
        SetupAppearance();
    }

    private void SetupAppearance()
    {
        textStyle.fontSize = 20;
        textStyle.fontStyle = FontStyle.Bold;
        textStyle.normal.textColor = Color.green;
        debugArea = new Rect(10, 10, 200, 200);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            showing = !showing;
        }
    }

    private void OnGUI()
    {
        if (!showing)
        {
            return;
        }
        
        GUILayout.BeginArea(debugArea);
        
        DrawPing();
        DrawState();
        
        GUILayout.EndArea();
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
            long deduction = (long)(tm.TickDelta * 2000d);

            pingWithTickRate = (long)Mathf.Max(1, ping - deduction);
        }

        GUILayout.Label($"Ping: {ping}ms", textStyle);
        GUILayout.Label($"Ping (Tickrate): {pingWithTickRate}ms", textStyle);
    }
    
    private void DrawState()
    {
        GUILayout.Label($"IsServer: {base.IsServer}", textStyle);
        GUILayout.Label($"IsClient: {base.IsClient}", textStyle);
    }
    
}
