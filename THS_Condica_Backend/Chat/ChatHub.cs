using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using THS_Condica_Backend.Models;

namespace THS_Condica_Backend.Chat
{
    public class ChatHub: Hub
    {
        static List<ChatMessage> CurrentMessage = new List<ChatMessage>();

        public Task SendToAll(string name, string text)
        {

            var message = new ChatMessage
            {
                SenderName = name,
                Text = text,
                SentAt = DateTimeOffset.UtcNow
            };
            AddMessageinCache(message);

            //await _chatRoomService.AddMessage(message);
            return Clients.All.SendAsync("sendToAll", CurrentMessage);
        }
        private void AddMessageinCache(ChatMessage _MessageDetail)
        {
            CurrentMessage.Add(_MessageDetail);
            if (CurrentMessage.Count > 100)
                CurrentMessage.RemoveAt(0);
        }

        public Task ReceiveMessage()
        {
            return Clients.Caller.SendAsync("receiveMessage", CurrentMessage);
        }

    }
}
