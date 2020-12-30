using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using OpenMod.API.Commands;
using OpenMod.Rcon.Api;
using OpenMod.Rcon.Api.Packets;
using OpenMod.Rcon.Common;
using OpenMod.Rcon.Common.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenMod.Rcon.Rocket
{
    public class RocketRconConnection : RconConnectionBase
    {
        public RocketRconConnection([KeyFilter(nameof(RocketRconHost))] IRconHost host,
            IPacketSerializer packetSerializer, 
            IAsyncTcpClient tcpClient, 
            ICommandExecutor commandExecutor,
            ILogger<IRconConnection> logger) : base(host, packetSerializer, tcpClient, commandExecutor, logger)
        {

        }

        public override async Task Start()
        {
            await base.Start();
        }

        protected override Task ProcessPacket(RconPacket packet)
        {
            var splitted = packet.Body.Split(' ');
            string command = splitted.Length == 1 ? packet.Body : splitted.First();


            switch (command)
            {
                case "login":
                    return ProcessAuthorizationPacket(packet);
                default:
                    return ProcessExecuteCommand(packet); //Works without overriding.
            }
        }

        protected override async Task ProcessAuthorizationPacket(RconPacket packet)
        {
            var splitted = packet.Body.Split(' ');
            if(splitted.Length < 2)
            {
                await SendResponse(packet.Id, "Invalid Syntax! Usage: login <password>", default);
                return;
            }

            var password = string.Join(" ", splitted.Skip(1)); //Skip first command

            if(!PasswordCheck(password))
            {
                await SendResponse(packet.Id, "Invalid password!", default);
                return;
            }

            await SendResponse(packet.Id, "Succesfully logged in!", default);

            this.AuthLevel = AuthLevel.Authorized;
        }
    }
}
