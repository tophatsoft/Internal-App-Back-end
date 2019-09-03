using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using THS_Condica_Backend.Models;

namespace THS_Condica_Backend.Chat
{
    public class InMemoryChatRoomService : IChatRoomService
    {
        private readonly List<ChatMessage> _messageHistory = new List<ChatMessage>();
        public Task AddMessage(ChatMessage message)
        {
            _messageHistory.Add(message);

            return Task.CompletedTask;
        }

        public Task<IEnumerable<ChatMessage>> GetMessageHistory()
        {
            var sortedMessages = _messageHistory.OrderBy(x => x.SentAt).AsEnumerable();
            return Task.FromResult(sortedMessages);
        }
    }
}
