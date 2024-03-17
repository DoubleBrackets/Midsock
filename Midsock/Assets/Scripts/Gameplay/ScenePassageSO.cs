using GameKit.Utilities.Types;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Scene Passageway")]
public class ScenePassageSO : AnnotatedScriptableObject
{
    [field: SerializeField]
    [field: Scene]
    public string DestinationScene { get; private set; }

    [field: SerializeField]
    [field: ReadOnly]
    public Vector3 DestinationPosition { get; private set; }

    /// <summary>
    /// Debugging use only
    /// </summary>
    [field: SerializeField]
    [field: ReadOnly]
    [field: TextArea]
    public string Entrance { get; private set; }

    /// <summary>
    /// Debugging use only
    /// </summary>
    [field: SerializeField]
    [field: ReadOnly]
    [field: TextArea]
    public string Exit { get; private set; }

    public void SetEntranceDebugTrace(PassageEntrance entrance)
    {
        GameObject gameObject = entrance.gameObject;
        string previousEntrance = Entrance;
        Entrance = $"{gameObject.name}\n{gameObject.scene.path}";

#if UNITY_EDITOR

        if (previousEntrance != Entrance)
        {
            EditorUtility.SetDirty(this);
        }

#endif
    }

    public void SetExitDebugTrace(PassageExit exit)
    {
        GameObject gameObject = exit.gameObject;
        string previousExit = Exit;
        Exit = $"{gameObject.name}\n{gameObject.scene.path}";

#if UNITY_EDITOR

        if (previousExit != Exit)
        {
            EditorUtility.SetDirty(this);
        }

#endif
    }

    public void SetDestinationPosition(Vector3 position)
    {
        Vector3 previousDestinationPosition = DestinationPosition;
        DestinationPosition = position;

#if UNITY_EDITOR

        if (previousDestinationPosition != DestinationPosition)
        {
            EditorUtility.SetDirty(this);
        }

#endif
    }
}