using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TMPro;
using Unity.Services.Relay.Models;
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

    [SerializeField]
    private TMP_Dropdown regionDropdown;

    private const string AutoRegion = "Auto";

    private const int GetRegionRetryTime = 10;

    private bool foundRegions;

    private void Start()
    {
        hostButton.onClick.AddListener(OnHostButtonClicked);
        joinButton.onClick.AddListener(OnJoinButtonClicked);

        foundRegions = false;

        SetupRegionDropdownAsync(gameObject.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTaskVoid SetupRegionDropdownAsync(CancellationToken token)
    {
        try
        {
            List<Region> regions = await RelayConnectionHandler.Instance.GetRegionList(token);

            while (regions == null)
            {
                token.ThrowIfCancellationRequested();

                regions = await RelayConnectionHandler.Instance.GetRegionList(token);

                token.ThrowIfCancellationRequested();

                string status = $"Unable to get region list. Trying again in {GetRegionRetryTime} seconds";
                statusText.text = status;
                Debug.Log(status);
                await Task.Delay(TimeSpan.FromSeconds(GetRegionRetryTime), token);
            }

            statusText.text = $"Found regions.";

            foundRegions = true;

            var options = regions.Select(r => new TMP_Dropdown.OptionData(r.Id)).ToList();

            // QOS autodetect region isn't available in WebGL
#if !UNITY_WEBGL || UNITY_EDITOR
            options.Insert(0, new TMP_Dropdown.OptionData(AutoRegion));
#endif

            regionDropdown.ClearOptions();
            regionDropdown.AddOptions(options);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }


    private void OnDestroy()
    {
        hostButton.onClick.RemoveListener(OnHostButtonClicked);
        joinButton.onClick.RemoveListener(OnJoinButtonClicked);
    }

    private void OnHostButtonClicked()
    {
        var regionId = regionDropdown.options[regionDropdown.value].text;

        if (regionId == AutoRegion)
        {
            regionId = String.Empty;
        }

        RelayConnectionHandler.Instance.BeginHostingAsync(
            regionId,
            gameObject.GetCancellationTokenOnDestroy()).Forget();
    }

    private void OnJoinButtonClicked()
    {
        if (!foundRegions)
        {
            DisplayInvalidJoinCode("No selected region...");
            return;
        }

        regionDropdown.gameObject.SetActive(false);

        string joinCode = joinCodeInput.text;
        if (string.IsNullOrEmpty(joinCode))
        {
            DisplayInvalidJoinCode("Join code cannot be empty");
            return;
        }

        RelayConnectionHandler.Instance.JoinGameAsync(joinCode, gameObject.GetCancellationTokenOnDestroy()).Forget();
    }

    private void DisplayInvalidJoinCode(string message)
    {
        statusText.text = $"Invalid Join Code: {message}";
    }
}