using OpenMod.API.Commands;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace OpenMod.Rcon.Api.Actors
{
    public class RconCommandActor : ICommandActor
    {
        public RconCommandActor(IRconConnection connection, int packetId)
        {
            this.Connection = connection;
            this.PacketId = packetId;
        }

        public IRconConnection Connection { get; }
        public int PacketId { get; }

        public string Id => "Rcon"; //Since Rcon only uses password an identifier does not really exist..
        public string Type => "Console";
        public string DisplayName => "Rcon";

        public Task PrintMessageAsync(string message) => PrintMessageAsync(message, default);

        public Task PrintMessageAsync(string message, Color color) => Connection.SendResponse(PacketId, message, color);

    }
}
