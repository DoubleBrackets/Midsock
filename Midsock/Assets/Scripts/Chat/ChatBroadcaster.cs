using FishNet;
using FishNet.Broadcast;
using FishNet.Connection;
using TMPro;
using UnityEngine;

public class ChatBroadcaster : MonoBehaviour
{
    public struct ChatMessage : IBroadcast
    {
        public string Name;
        public string Message;
    }

    [SerializeField]
    private Transform chatContainer;

    [SerializeField]
    private GameObject chatEntryPrefab;

    [SerializeField]
    private TMP_InputField chatInput, userName;

    private void OnEnable()
    {
        chatInput.onSubmit.AddListener(OnSubmitMessage);

        InstanceFinder.ClientManager.RegisterBroadcast<ChatMessage>(OnServerChatMessage);
        InstanceFinder.ServerManager.RegisterBroadcast<ChatMessage>(OnClientChatMessage);
    }

    private void OnDisable()
    {
        chatInput.onSubmit.RemoveListener(OnSubmitMessage);

        InstanceFinder.ClientManager.UnregisterBroadcast<ChatMessage>(OnServerChatMessage);
        InstanceFinder.ServerManager.UnregisterBroadcast<ChatMessage>(OnClientChatMessage);
    }

    private void OnServerChatMessage(ChatMessage message)
    {
        Debug.Log("Client: Received chat message from Server, Displaying");
        GameObject newMessage = Instantiate(chatEntryPrefab, chatContainer);
        newMessage.GetComponent<ChatEntry>().SetChatText(message.Name, message.Message);
    }

    private void OnClientChatMessage(NetworkConnection connection, ChatMessage message)
    {
        Debug.Log("Server: Received chat message from Client, rebroadcasting");
        InstanceFinder.ServerManager.Broadcast(message);
    }

    private void OnSubmitMessage(string message)
    {
        if (!InstanceFinder.IsClient)
        {
            Debug.Log("Not a client");
            return;
        }

        string name = userName.text;
        if (message == string.Empty || name == string.Empty)
        {
            return;
        }

        var chatMessage = new ChatMessage
        {
            Name = name,
            Message = message
        };

        InstanceFinder.ClientManager.Broadcast(chatMessage);
    }
}