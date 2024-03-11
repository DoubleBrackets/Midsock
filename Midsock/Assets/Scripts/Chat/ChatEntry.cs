using TMPro;
using UnityEngine;

public class ChatEntry : MonoBehaviour
{
    [SerializeField]
    private TMP_Text chatText;

    [SerializeField]
    private TMP_Text chatName;

    public void SetChatText(string name, string text)
    {
        chatName.text = name;
        chatText.text = text;
    }
}