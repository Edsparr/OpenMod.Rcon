using OpenMod.Rcon.Api.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenMod.Rcon.Rocket.Packets
{
    public class RocketRconPacketSerializer : IPacketSerializer
    {
        public async Task<RconPacket> Deserialize(Stream stream)
        {
            var buffer = new byte[stream.Length];
            await stream.ReadAsync(buffer, 0, buffer.Length);

            return new RconPacket()
            {
                Body = Encoding.UTF8.GetString(buffer, 0, buffer.Length - 2),
                Id = default,
                Type = default
            };
        }

        public byte[] Serialize(RconPacket packet)
        {
            var text = Encoding.UTF8.GetBytes(packet.Body + "\r\n");

            return text;
        }
    }
}
