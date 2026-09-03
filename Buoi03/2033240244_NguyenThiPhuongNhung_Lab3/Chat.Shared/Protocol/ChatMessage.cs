using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Shared.Protocol
{
    public sealed class ChatMessage
    {
        public MessageType Type { get; init; }
        public string Sender { get; init; } = string.Empty;
        public string Content { get; init; } = string.Empty;
        public bool Success { get; init; }
        public List<string>? Users { get; init; }
        public DateTimeOffset SentAt { get; init; } = DateTimeOffset.Now;
    }
}
