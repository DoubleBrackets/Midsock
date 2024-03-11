using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnotatedScriptableObject : ScriptableObject
{
    [TextArea(3, 20)]
    public string Description;
}