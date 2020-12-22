using OpenMod.Rcon.Api.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenMod.Rcon.Common.Packets
{
    public class ValveRconPacketSerializer : IPacketSerializer
    {
        private static IDictionary<string, int> packetTypes = new Dictionary<string, int>()
        {
            [RconPacket.ServerDataResponsePacket] = 0,
            [RconPacket.ServerDataExecuteCommandPacket] = 2, 
            [RconPacket.ServerDataAuthResponsePacket] = 2,
            [RconPacket.ServerDataAuthPacket] = 3
        };

        public async Task<RconPacket> Deserialize(Stream stream)
        {
            var packet = new RconPacket();

            var sizeBuffer = new byte[sizeof(int)];
            await stream.ReadAsync(sizeBuffer, 0, sizeof(int));

            var size = BitConverter.ToInt32(sizeBuffer, 0);

            var packetBuffer = new byte[size];
            await stream.ReadAsync(packetBuffer, 0, packetBuffer.Length);

            packet.Id = BitConverter.ToInt32(packetBuffer, 0);
            if (!TryGetNativePacketTypeIdentifier(BitConverter.ToInt32(packetBuffer, sizeof(int)), out var packetType))
                throw new ArgumentException($"Packet contained unknown packet type!");

            packet.Type = packetType;

            packet.Body = Encoding.ASCII.GetString(packetBuffer, sizeof(int) * 2, packetBuffer.Length - sizeof(int) * 2 - sizeof(byte) * 2);

            return packet;
        }

        public byte[] Serialize(RconPacket packet)
        {

            if (!TryGetPacketTypeIdentifier(packet.Type, out var typeId))
                throw new ArgumentException($"Type: {packet.Type} not recognized!");

            var body = Encoding.UTF8.GetBytes(packet.Body + "\0"); // add null string terminator
            var buffer = new byte[sizeof(int) * 3 + body.Length + 1]; // 12 bytes for Length, Id and Type

            BitConverter.GetBytes(buffer.Length - sizeof(int)).CopyTo(buffer, 0); //Size 
            BitConverter.GetBytes(packet.Id).CopyTo(buffer, sizeof(int));
            BitConverter.GetBytes(typeId).CopyTo(buffer, sizeof(int) * 2);
            body.CopyTo(buffer, sizeof(int) * 3);

            return buffer;
        }

        private bool TryGetNativePacketTypeIdentifier(int num, out string type)
        {
            type = default;
            if (!packetTypes.Any(c => c.Value == num))
                return false;

            type = packetTypes.FirstOrDefault(c => c.Value == num).Key;

            return true;
        }

        private bool TryGetPacketTypeIdentifier(string type, out int result) => packetTypes.TryGetValue(type, out result);
    }
}
