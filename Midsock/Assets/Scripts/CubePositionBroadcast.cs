using System.Collections.Generic;
using FishNet;
using FishNet.Broadcast;
using FishNet.Connection;
using UnityEngine;

public class CubePositionBroadcast : MonoBehaviour
{
    public struct PositionIndex : IBroadcast
    {
        public int Index;
    }

    public List<Transform> _cubePositions = new();

    public int _transformIndex;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            int nextIndex = (_transformIndex + 1) % _cubePositions.Count;
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
                InstanceFinder.ClientManager.Broadcast(new PositionIndex
                {
                    Index = nextIndex
                });
            }
        }

        transform.position = _cubePositions[_transformIndex].position;
    }

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

    private void OnPositionBroadcast(PositionIndex index)
    {
        if (InstanceFinder.IsServer)
        {
            return;
        }

        Debug.Log("Client: Received position broadcast from Server");
        _transformIndex = index.Index;
    }

    private void OnClientPositionBroadcast(NetworkConnection networkConnection, PositionIndex index)
    {
        Debug.Log("Server: Received position broadcast from Client");
        _transformIndex = index.Index;
        InstanceFinder.ServerManager.Broadcast(index);
    }
}