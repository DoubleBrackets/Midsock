using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Broadcast;
using FishNet.Connection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ChatBroadcaster : MonoBehaviour
{
    [SerializeField]
    private Transform chatContainer;
    
    [SerializeField]
    private GameObject chatEntryPrefab;

    [SerializeField]
    private TMP_InputField chatInput, userName;

    public struct ChatMessage : IBroadcast
    {
        public string name;
        public string message;
    }

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
        newMessage.GetComponent<ChatEntry>().SetChatText(message.name, message.message);
    }
    
    private void OnClientChatMessage(NetworkConnection connection, ChatMessage message)
    {
        Debug.Log("Server: Received chat message from Client, rebroadcasting");
        InstanceFinder.ServerManager.Broadcast<ChatMessage>(message);
    }

    private void OnSubmitMessage(string message)
    {
        if (!InstanceFinder.IsClient)
        {
            Debug.Log("Not a client");
            return;
        }
        
        string name = userName.text;
        if (message == String.Empty || name == String.Empty)
        {
            return;
        }
        
        ChatMessage chatMessage = new ChatMessage()
        {
            name = name,
            message = message
        };
        
        InstanceFinder.ClientManager.Broadcast<ChatMessage>(chatMessage);
    }
}
