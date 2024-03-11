using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Broadcast;
using FishNet.Connection;
using UnityEngine;

public class CubePositionBroadcast : MonoBehaviour
{
    public List<Transform> cubePositions = new List<Transform>();
    public int transformIndex;

    private void OnEnable()
    {
        InstanceFinder.ClientManager.RegisterBroadcast<PositionIndex>(OnPositionBroadcast);
        InstanceFinder.ServerManager.RegisterBroadcast<PositionIndex>(OnClientPositionBroadcast);
    }

    private void OnDisable()
    {
        if (InstanceFinder.NetworkManager != null && InstanceFinder.NetworkManager.Initialized)
        {
            InstanceFinder.ClientManager.UnregisterBroadcast<PositionIndex>(OnPositionBroadcast);
            InstanceFinder.ServerManager.UnregisterBroadcast<PositionIndex>(OnClientPositionBroadcast);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            int nextIndex = (transformIndex + 1) % cubePositions.Count;
            /*if (InstanceFinder.IsServer)
            {
                Debug.Log("Server broadcast out");
                InstanceFinder.ServerManager.Broadcast(new PositionIndex()
                {
                    tIndex = nextIndex
                });
            }*/
            if (InstanceFinder.IsClient)
            {
                Debug.Log("Client broadcast out");
                InstanceFinder.ClientManager.Broadcast(new PositionIndex()
                {
                    tIndex = nextIndex
                });
            }
        }

        transform.position = cubePositions[transformIndex].position;
    }

    private void OnPositionBroadcast(PositionIndex index)
    {
        if (InstanceFinder.IsServer)
        {
            return;
        }

        Debug.Log("Client: Received position broadcast from Server");
        transformIndex = index.tIndex;
    }

    private void OnClientPositionBroadcast(NetworkConnection networkConnection, PositionIndex index)
    {
        Debug.Log("Server: Received position broadcast from Client");
        transformIndex = index.tIndex;
        InstanceFinder.ServerManager.Broadcast(index);
    }

    public struct PositionIndex : IBroadcast
    {
        public int tIndex;
    }
}