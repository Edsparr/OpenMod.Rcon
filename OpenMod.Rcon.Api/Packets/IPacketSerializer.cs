using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OpenMod.Rcon.Api.Packets
{
    public interface IPacketSerializer
    {
        Task<Stream> Serialize(RconPacket packet);
        Task<RconPacket> Deserialize(Stream stream);
    }
}
