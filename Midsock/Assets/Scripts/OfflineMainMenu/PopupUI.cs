using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupUI : MonoBehaviour
{
    [SerializeField]
    private Button _button;

    [SerializeField]
    private TMP_Text _text;

    [SerializeField]
    private GameObject _popup;

    private void Start()
    {
        _button.onClick.AddListener(OnButtonClicked);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnButtonClicked);
    }

    public void SetText(string text)
    {
        _text.text = text;
    }

    [Button("Show")]
    public void Show()
    {
        gameObject.SetActive(true);
    }

    [Button("Hide")]
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnButtonClicked()
    {
        Hide();
    }
}