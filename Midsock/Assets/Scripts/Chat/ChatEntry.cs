using TMPro;
using UnityEngine;

public class ChatEntry : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _chatText;

    [SerializeField]
    private TMP_Text _chatName;

    public void SetChatText(string name, string text)
    {
        _chatName.text = name;
        _chatText.text = text;
    }
}