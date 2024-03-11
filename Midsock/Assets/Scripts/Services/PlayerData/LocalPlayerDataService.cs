using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the local player's data
/// </summary>
public class LocalPlayerDataService : MonoBehaviour
{
    public static LocalPlayerDataService Instance { get; private set; }

    [field: SerializeField]
    public LocalPlayerDataContainer LocalPlayerDataContainer { get; private set; }

    public string PlayerName
    {
        get => LocalPlayerDataContainer.LocalPlayerData.PlayerName;
        set
        {
            LocalPlayerDataContainer.LocalPlayerData.SetPlayerName(value);
            SaveData();
        }
    }

    private const string dataPath = "/playerData.json";

    private void Awake()
    {
        Instance = this;
        LoadDataFromSave();
    }

    private void OnDestroy()
    {
        SaveData();
    }

    public PlayerData LoadDataFromSave()
    {
        try
        {
            string path = Application.persistentDataPath + dataPath;
            var data = JsonUtility.FromJson<PlayerData>(System.IO.File.ReadAllText(path));
            LocalPlayerDataContainer.LocalPlayerData = data;
            return data;
        }
        catch (Exception e)
        {
            // Failed, just use an empty player data
            Console.WriteLine(e);
            var data = new PlayerData();
            LocalPlayerDataContainer.LocalPlayerData = data;
            return data;
        }
    }

    public void SaveData()
    {
        Debug.Log($"Updating name to {LocalPlayerDataContainer.LocalPlayerData.PlayerName}");
        string path = Application.persistentDataPath + dataPath;
        System.IO.File.WriteAllText(path, JsonUtility.ToJson(LocalPlayerDataContainer.LocalPlayerData));
    }
}