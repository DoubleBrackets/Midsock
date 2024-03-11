using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LocalPlayerDataContainer", menuName = "LocalPlayerDataContainer")]
public class LocalPlayerDataContainer : AnnotatedScriptableObject
{
    [field: SerializeField]
    public PlayerData LocalPlayerData { get; set; }
}

[System.Serializable]
public class PlayerData
{
    [field: SerializeField]
    public string PlayerName { get; private set; }

    public void SetPlayerName(string playerName)
    {
        PlayerName = playerName;
    }
}