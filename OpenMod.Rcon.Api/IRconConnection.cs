using OpenMod.Rcon.Api.Packets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpenMod.Rcon.Api
{
    public interface IRconConnection
    {
        AuthLevel AuthLevel { get; set; }

        Func<IRconConnection, Task> Disconnected { get; set; }

        Task Start();

        Task SendPacket(RconPacket packet);
        Task SendResponse(int originalPacketId, RconPacket packet);
        Task SendResponse(int originalPacketId, string message, Color color);
    }

    public enum AuthLevel
    {
        Unauthorized,
        Authorized
    }
}
