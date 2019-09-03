using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace THS_Condica_Backend.Models
{
    public class ChatMessage
    {
        public string SenderName { get; set; }
        public string Text { get; set; }
        public DateTimeOffset SentAt { get; set; }

    }
}
