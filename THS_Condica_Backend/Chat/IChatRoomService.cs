using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using THS_Condica_Backend.Models;

namespace THS_Condica_Backend.Chat
{
    public interface IChatRoomService
    {
        Task AddMessage(ChatMessage message);
        Task<IEnumerable<ChatMessage>> GetMessageHistory();

    }
}
