using System.Collections;
using System.Collections.Generic;
using GameKit.Utilities.Types;
using UnityEditor.Animations;
using UnityEngine;

public class UtilsPrefs : ScriptableObject
{
    [field: SerializeField, Scene]
    public string StartupScenePath { get; private set; }
}