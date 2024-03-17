using UnityEditor;
using UnityEngine;

public class PassageExit : MonoBehaviour
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

        Gizmos.color = Color.green;

        Vector3 position = transform.position;
        Gizmos.DrawWireSphere(position, 0.5f);

        Handles.Label(position + Vector3.up * 2f, $"Exit from {_scenePassage.name}");

        // Idk where else to put this
        _scenePassage.SetExitDebugTrace(this);

        _scenePassage.SetDestinationPosition(transform.position);
    }

#endif
}