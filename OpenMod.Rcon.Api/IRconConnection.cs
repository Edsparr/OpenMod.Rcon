using OpenMod.Rcon.Api.Packets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpenMod.Rcon.Api
{
    public interface IRconConnection : IDisposable
    {
        Func<IRconConnection, Task> Disconnected { get; set; }
        IRconHost Host { get; }
        AuthLevel AuthLevel { get; }

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
