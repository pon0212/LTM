using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Shared.Protocol
{
    public enum MessageType
    {
        Login,
        LoginResult,
        Chat,
        System,
        UserList,
        Disconnect,
        Error
    }
}
