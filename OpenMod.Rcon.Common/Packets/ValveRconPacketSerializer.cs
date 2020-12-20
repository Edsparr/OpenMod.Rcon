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

            var packetBuffer = new byte[BitConverter.ToInt32(sizeBuffer, 0)];
            await stream.ReadAsync(packetBuffer, 0, packetBuffer.Length);

            packet.Id = BitConverter.ToInt32(packetBuffer, 0);
            if (!TryGetNativePacketTypeIdentifier(BitConverter.ToInt32(packetBuffer, sizeof(int)), out var packetType))
                throw new ArgumentException($"Packet contained unknown packet type!");

            packet.Type = packetType;

            packet.Body = Encoding.ASCII.GetString(packetBuffer, sizeof(int) * 2, packetBuffer.Length - sizeof(int) * 2 - sizeof(byte));
            //Total size - the first 2 ints and minus the last empty ascii string

            return packet;
        }

        public Task<Stream> Serialize(RconPacket packet)
        {

            if (!TryGetPacketTypeIdentifier(packet.Type, out var typeId))
                throw new ArgumentException($"Type: {packet.Type} not recognized!");

            var bodyBytes = Encoding.ASCII.GetBytes(packet.Body ?? string.Empty);
            var result = new byte[sizeof(int) + sizeof(int) + sizeof(int) + bodyBytes.Length + 1];

            if (result.Length > 4096)
                throw new ArgumentException("Packet is bigger than 4096!");

            BitConverter.GetBytes(result.Length - sizeof(int)).CopyTo(result, 0); //The packet size int is not included in packet size.
            BitConverter.GetBytes(packet.Id).CopyTo(result, sizeof(int));
            BitConverter.GetBytes(typeId).CopyTo(result, sizeof(int) * 2);

            bodyBytes.CopyTo(result, sizeof(int) * 3); //Will never be out of bounds,
            //The last byte is already put as 0s so no need to put 0 in there.

            return Task.FromResult((Stream)new MemoryStream(result));
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
