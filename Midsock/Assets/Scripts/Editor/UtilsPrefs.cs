using GameKit.Utilities.Types;
using UnityEngine;

public class UtilsPrefs : ScriptableObject
{
    [field: SerializeField]
    [field: Scene]
    public string StartupScenePath { get; private set; }
}