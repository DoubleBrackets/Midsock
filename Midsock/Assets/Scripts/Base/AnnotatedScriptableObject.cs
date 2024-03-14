using UnityEngine;

public abstract class AnnotatedScriptableObject : ScriptableObject
{
    [TextArea(3, 20)]
    public string Description;
}