using System;
using System.Collections.Generic;
using System.Text;

namespace OpenMod.Rcon.Api.Packets
{
    public class RconPacket
    {
        public const string ServerDataAuthPacket = "SERVERDATA_AUTH";
        public const string ServerDataAuthResponsePacket = "SERVERDATA_AUTH_RESPONSE";
        public const string ServerDataExecuteCommandPacket = "SERVERDATA_EXECCOMMAND";
        public const string ServerDataResponsePacket = "SERVERDATA_RESPONSE_VALUE";

        public int Id { get; set; }
        public string Type { get; set; }
        public string Body { get; set; }
    }
}
