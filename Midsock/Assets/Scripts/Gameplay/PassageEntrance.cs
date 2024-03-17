using FishNet.Object;
using UnityEditor;
using UnityEngine;

public class PassageEntrance : MonoBehaviour
{
    [SerializeField]
    private ScenePassageSO _scenePassage;

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (_scenePassage == null)
        {
            return;
        }

        Gizmos.color = Color.red;

        Vector3 position = transform.position;
        Gizmos.DrawWireSphere(position, 0.5f);
        _scenePassage.SetEntranceDebugTrace(this);

        Handles.Label(position + Vector3.up * 2f, $"Entrance to {_scenePassage.name}");
    }

#endif

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponentInParent<NetworkObject>();

        Debug.Log(other.gameObject);
        if (player != null && player.IsOwner)
        {
            Debug.Log($"{player.Owner} is entering {_scenePassage.DestinationScene}");
            SessionServiceFinder.SessionStateManager.MoveConnection(player.Owner, _scenePassage.DestinationScene,
                _scenePassage.DestinationPosition);
        }
    }
}