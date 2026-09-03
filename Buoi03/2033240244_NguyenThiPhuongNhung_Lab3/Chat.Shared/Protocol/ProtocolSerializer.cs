using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Chat.Shared.Protocol
{
    public static class ProtocolSerializer
    {
        private static readonly JsonSerializerOptions Options 
            = new(JsonSerializerDefaults.Web);

        public static string Serializer(ChatMessage message) => 
            JsonSerializer.Serialize(message, Options);

        public static ChatMessage Deserializer(string json) =>
            JsonSerializer.Deserialize<ChatMessage>(json, Options)
            ?? throw new InvalidDataException("Ban tin khong hop le");
    }
}
