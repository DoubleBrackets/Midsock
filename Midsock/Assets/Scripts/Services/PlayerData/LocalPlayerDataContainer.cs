using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LocalPlayerDataContainer", menuName = "LocalPlayerDataContainer")]
public class LocalPlayerDataContainer : AnnotatedScriptableObject
{
    [field: SerializeField]
    public PlayerData LocalPlayerData { get; set; }
}

[Serializable]
public class PlayerData
{
    [field: SerializeField]
    public string PlayerName { get; set; }

    [field: SerializeField]
    public string RegionPreference { get; set; }
}