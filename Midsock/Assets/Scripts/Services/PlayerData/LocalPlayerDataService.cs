using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Handles the local player's data
/// </summary>
public class LocalPlayerDataService : MonoBehaviour
{
    private const string dataPath = "/playerData.json";

    [field: SerializeField]
    public LocalPlayerDataContainer LocalPlayerDataContainer { get; private set; }

    public static LocalPlayerDataService Instance { get; private set; }

    public string PlayerName
    {
        get => LocalPlayerDataContainer.LocalPlayerData.PlayerName;
        set
        {
            LocalPlayerDataContainer.LocalPlayerData.PlayerName = value;
            SaveData();
        }
    }

    public string RegionPreference
    {
        get => LocalPlayerDataContainer.LocalPlayerData.RegionPreference;
        set
        {
            LocalPlayerDataContainer.LocalPlayerData.RegionPreference = value;
            SaveData();
        }
    }

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
            var data = JsonUtility.FromJson<PlayerData>(File.ReadAllText(path));
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
        string path = Application.persistentDataPath + dataPath;
        File.WriteAllText(path, JsonUtility.ToJson(LocalPlayerDataContainer.LocalPlayerData));
    }
}