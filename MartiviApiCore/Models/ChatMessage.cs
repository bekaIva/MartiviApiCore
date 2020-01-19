using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MartiviApiCore.Models
{
    public enum MessageSide
    {
        Support,
        Client
    }
    public class ChatMessage
    {
        public int ChatMessageId { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; }
        public MessageSide Side { get; set; }
    }
}
