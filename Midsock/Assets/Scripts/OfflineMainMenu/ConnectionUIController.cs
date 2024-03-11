using System;
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
    private const string AutoRegion = "Auto";

    private const int GetRegionRetryTime = 10;

    [SerializeField]
    private TMP_InputField _joinCodeInput;

    [SerializeField]
    private Button _hostButton;

    [SerializeField]
    private Button _joinButton;

    [SerializeField]
    private TMP_Text _statusText;

    [SerializeField]
    private TMP_Dropdown _regionDropdown;

    private bool _foundRegions;

    private void Start()
    {
        _hostButton.onClick.AddListener(OnHostButtonClicked);
        _joinButton.onClick.AddListener(OnJoinButtonClicked);

        _foundRegions = false;

        SetupRegionDropdownAsync(gameObject.GetCancellationTokenOnDestroy()).Forget();
    }

    private void OnDestroy()
    {
        _hostButton.onClick.RemoveListener(OnHostButtonClicked);
        _joinButton.onClick.RemoveListener(OnJoinButtonClicked);
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

                var status = $"Unable to get region list. Trying again in {GetRegionRetryTime} seconds";
                _statusText.text = status;
                Debug.Log(status);
                await Task.Delay(TimeSpan.FromSeconds(GetRegionRetryTime), token);
            }

            _statusText.text = "Found regions.";

            _foundRegions = true;

            List<TMP_Dropdown.OptionData> options = regions.Select(r => new TMP_Dropdown.OptionData(r.Id)).ToList();

            // QOS autodetect region isn't available in WebGL
#if !UNITY_WEBGL || UNITY_EDITOR
            options.Insert(0, new TMP_Dropdown.OptionData(AutoRegion));
#endif

            _regionDropdown.ClearOptions();
            _regionDropdown.AddOptions(options);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    private void OnHostButtonClicked()
    {
        string regionId = _regionDropdown.options[_regionDropdown.value].text;

        if (regionId == AutoRegion)
        {
            regionId = string.Empty;
        }

        RelayConnectionHandler.Instance.BeginHostingAsync(
            regionId,
            gameObject.GetCancellationTokenOnDestroy()).Forget();
    }

    private void OnJoinButtonClicked()
    {
        if (!_foundRegions)
        {
            DisplayInvalidJoinCode("No selected region...");
            return;
        }

        string joinCode = _joinCodeInput.text;
        if (string.IsNullOrEmpty(joinCode))
        {
            DisplayInvalidJoinCode("Join code cannot be empty");
            return;
        }

        RelayConnectionHandler.Instance.JoinGameAsync(joinCode, gameObject.GetCancellationTokenOnDestroy()).Forget();
    }

    private void DisplayInvalidJoinCode(string message)
    {
        _statusText.text = $"Invalid Join Code: {message}";
    }
}