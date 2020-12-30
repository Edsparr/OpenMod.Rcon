using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using OpenMod.API.Commands;
using OpenMod.Rcon.Api;
using OpenMod.Rcon.Api.Packets;
using OpenMod.Rcon.Common;
using OpenMod.Rcon.Common.Tcp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenMod.Rcon
{
    public class OpenModRconConnection : RconConnectionBase
    {
        public OpenModRconConnection([KeyFilter(nameof(OpenModRconHost))] IRconHost host,
            IPacketSerializer packetSerializer, 
            IAsyncTcpClient tcpClient, 
            ICommandExecutor commandExecutor,
            ILogger<IRconConnection> logger) : base(host, packetSerializer, tcpClient, commandExecutor, logger)
        {

        }

        protected override Task ProcessAuthorizationPacket(RconPacket packet)
        {

            return base.ProcessAuthorizationPacket(packet);
        }
    }
}
