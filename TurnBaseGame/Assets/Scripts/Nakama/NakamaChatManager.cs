using Nakama;
using Nakama.TinyJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NakamaChatManager : Singelton<NakamaChatManager>
{

    public string IdChannel;
    public ISocket socket;
   
    public async void JoinChat(string UserId, bool Persistence, bool Hidden)
    {
        var userId = UserId;
        var persistence = Persistence;
        var hidden = Hidden;
        var channel = await socket.JoinChatAsync(userId, ChannelType.DirectMessage, persistence, hidden);
        IdChannel = channel.Id;
    }

    public  String ReceiveMessages()
    {
        var mess = "";
        socket.ReceivedChannelMessage += message =>
        {
            mess= message.Content;
        };
        return mess;
    }

    public async void SendMessages( string message)
    {
        var channelId = IdChannel;
        var content =message.ToJson();
        var sendAck = await socket.WriteChatMessageAsync(channelId, content);

    }
    public async void LeaveChat()
    {
       
        await socket.LeaveChatAsync(IdChannel);

    }

}
