using FishNet.Object;
using GameKit.Utilities.Types;
using UnityEngine;

public class SceneSwitcher : MonoBehaviour
{
    [SerializeField]
    [Scene]
    private string _targetScene;

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponentInParent<NetworkObject>();

        Debug.Log(other.gameObject);
        if (player != null && player.IsOwner)
        {
            SessionServiceFinder.SessionStateManager.MoveConnection(player.Owner, _targetScene);
        }
    }
}