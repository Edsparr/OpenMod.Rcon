using Microsoft.Extensions.Logging;
using OpenMod.API.Commands;
using OpenMod.Core.Helpers;
using OpenMod.Rcon.Api;
using OpenMod.Rcon.Api.Actors;
using OpenMod.Rcon.Api.Packets;
using OpenMod.Rcon.Common.Tcp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unclassified.Net;

namespace OpenMod.Rcon.Common
{
    public abstract class RconConnectionBase : IRconConnection
    {
        private readonly IRconHost host;
        private readonly IPacketSerializer packetSerializer;
        private readonly IAsyncTcpClient tcpClient;
        private readonly ICommandExecutor commandExecutor;
        private readonly ILogger<IRconConnection> logger;
        
        public RconConnectionBase(IRconHost host, IPacketSerializer packetSerializer, IAsyncTcpClient tcpClient, ICommandExecutor commandExecutor, ILogger<IRconConnection> logger) : base()
        {
            this.host = host;
            this.packetSerializer = packetSerializer;
            this.tcpClient = tcpClient;
            this.commandExecutor = commandExecutor;
            this.logger = logger;
        }


        public Func<IRconConnection, Task> Disconnected { get; set; }

        public virtual AuthLevel AuthLevel { get; set; } = AuthLevel.Unauthorized;

        public async Task SendPacket(Api.Packets.RconPacket packet)
        {
            using (var packetStream = await packetSerializer.Serialize(packet))
            {
                await tcpClient.Send(packetStream);
            }
        }

        public async Task SendResponse(int originalPacketId, RconPacket packet)
        {
            packet.Id = originalPacketId;
            await SendPacket(packet);
        }

        public async Task SendResponse(int originalPacketId, string message, Color color)
        {
            await SendResponse(originalPacketId, new RconPacket()
            {
                Type = RconPacket.ServerDataResponsePacket,
                Body = message
            });
        }

        public async Task Start()
        {

            tcpClient.Received = Received;
            tcpClient.Disconnected = (client) => Disconnected?.Invoke(this);

            await tcpClient.Start();
        }

        
        
        protected virtual async Task Received(byte[] buffer)
        {
            using(var stream = new MemoryStream(buffer)) //Meh, I tried atleast.
            {
                try
                {
                    var packet = await packetSerializer.Deserialize(stream);
                    await ProcessPacket(packet);
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, "Error while procesing packet. Byte count: {count}", buffer.Length);
                }
            }
        }

        protected virtual async Task ProcessPacket(RconPacket packet)
        {
            switch (packet.Type)
            {
                case RconPacket.ServerDataAuthPacket:
                    await ProcessAuthorizationPacket(packet);
                    break;
                case RconPacket.ServerDataExecuteCommandPacket:
                    await ProcessExecuteCommand(packet);
                    break;
            }
        }

        protected virtual async Task ProcessAuthorizationPacket(RconPacket packet)
        {
            
            bool successfull = host.HostInfo.Password == packet.Body;
            int id = -1;
            if (successfull)
            {
                AuthLevel = AuthLevel.Authorized;
                id = packet.Id;
            }

            await SendPacket(new RconPacket()
            {
                Id = packet.Id,
                Body = default,
                Type = RconPacket.ServerDataResponsePacket
            });

            await SendPacket(new RconPacket()
            {
                Type = RconPacket.ServerDataAuthResponsePacket,
                Body = default,
                Id = id,
            });
        }

        protected virtual async Task ProcessExecuteCommand(RconPacket packet)
        {
            if(AuthLevel == AuthLevel.Unauthorized)
            {
                await SendPacket(new RconPacket()
                {
                    Id = packet.Id,
                    Body = "Not authorized!",
                    Type = RconPacket.ServerDataResponsePacket
                });
                return;
            }

            var commandActor = new RconCommandActor(this, packet.Id);
            var args = ArgumentsParser.ParseArguments(packet.Body);

            if (args.Length == 0)
                return;

            await commandExecutor.ExecuteAsync(commandActor, args, default);

        }



    }
}
