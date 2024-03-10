using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls hosting/joining UI
/// </summary>
public class ConnectionUIController : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField joinCodeInput;
    
    [SerializeField]
    private Button hostButton;
    
    [SerializeField]
    private Button joinButton;

    [SerializeField]
    private TMP_Text statusText;
    
    private void Start()
    {
        hostButton.onClick.AddListener(OnHostButtonClicked);
        joinButton.onClick.AddListener(OnJoinButtonClicked);
    }
    
    
    private void OnDestroy()
    {
        hostButton.onClick.RemoveListener(OnHostButtonClicked);
        joinButton.onClick.RemoveListener(OnJoinButtonClicked);
    }
    
    private void OnHostButtonClicked()
    {
        ConnectionStarter.Instance.BeginHostingAsync().Forget();
    }

    private void OnJoinButtonClicked()
    {
        string joinCode = joinCodeInput.text;
        if (string.IsNullOrEmpty(joinCode))
        {
            DisplayInvalidJoinCode("Join code cannot be empty");
            return;
        }

        ConnectionStarter.Instance.JoinGameAsync(joinCode).Forget();
    }
    
    private void DisplayInvalidJoinCode(string message)
    {
        statusText.text = "$Invalid Join Code: {message}";
    }
}
