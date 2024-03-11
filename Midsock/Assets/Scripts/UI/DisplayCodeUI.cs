using FishNet.Object;
using TMPro;
using UnityEngine;

public class DisplayCodeUI : NetworkBehaviour
{
    [SerializeField]
    private TMP_Text _codeText;

    public override void OnStartClient()
    {
        base.OnStartClient();
        _codeText.text = "Code: " + RelayConnectionHandler.Instance.JoinCode;
    }
}