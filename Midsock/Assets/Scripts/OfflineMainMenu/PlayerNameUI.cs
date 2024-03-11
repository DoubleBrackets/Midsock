using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerNameUI : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField _playerNameInput;

    private void Start()
    {
        _playerNameInput.text = LocalPlayerDataService.Instance.LocalPlayerDataContainer.LocalPlayerData.PlayerName;
        _playerNameInput.onEndEdit.AddListener(OnPlayerNameInputEndEdit);
    }

    private void OnPlayerNameInputEndEdit(string newName)
    {
        LocalPlayerDataService.Instance.PlayerName = newName;
    }
}