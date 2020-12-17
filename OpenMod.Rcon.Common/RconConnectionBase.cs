using Microsoft.Extensions.Logging;
using OpenMod.API.Commands;
using OpenMod.Core.Helpers;
using OpenMod.Rcon.Api;
using OpenMod.Rcon.Api.Actors;
using OpenMod.Rcon.Api.Packets;
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
        private readonly IPacketSerializer packetSerializer;
        private readonly AsyncTcpClient tcpClient;
        private readonly ICommandExecutor commandExecutor;
        private readonly ILogger<IRconConnection> logger;
        
        public RconConnectionBase(IPacketSerializer packetSerializer, AsyncTcpClient tcpClient, ICommandExecutor commandExecutor, ILogger<IRconConnection> logger) : base()
        {
            this.packetSerializer = packetSerializer;
            this.tcpClient = tcpClient;
            this.commandExecutor = commandExecutor;
            this.logger = logger;
        }

        private (CancellationTokenSource, Task) closedTaskCallback;

        public Func<IRconConnection, Task> Disconnected { get; set; }

        public IRconHost Host { get; }
        public AuthLevel AuthLevel { get; private set; } = AuthLevel.Unauthorized;

        public async Task SendPacket(Api.Packets.RconPacket packet)
        {
            using (var packetStream = await packetSerializer.Serialize(packet))
            {
                var bytes = new byte[packetStream.Length];

                await packetStream.ReadAsync(bytes, 0, (int)packetStream.Length);
                await tcpClient.Send(new ArraySegment<byte>(bytes));
            }
        }

        public async Task SendResponse(int originalPacketId, RconPacket packet)
        {
            packet.ID = originalPacketId;
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

            tcpClient.ReceivedCallback = ReceivedAsync;
            tcpClient.ClosedCallback = (client, reconnected) => Disconnected?.Invoke(this);
            var task = Task.Run(async () =>
            {
                await Task.WhenAny(tcpClient.ClosedTask, Task.Delay(-1, closedTaskCallback.Item1.Token));
                if (!tcpClient.ClosedTask.IsCompleted) //Incase it got cancelled instead of completed because the cancel means we don't listen for ClientDisconnect anymore.
                    return;
                await Disconnected?.Invoke(this);

            });

            this.closedTaskCallback = (new CancellationTokenSource(), task);

            await tcpClient.RunAsync();
        }

        
        
        protected virtual async Task ReceivedAsync(AsyncTcpClient tcpClient, int count)
        {
            byte[] bytes = tcpClient.ByteBuffer.Dequeue(count);
            using(var stream = new MemoryStream(bytes)) //Meh, I tried atleast.
            {
                try
                {
                    var packet = await packetSerializer.Deserialize(stream);
                    await ProcessPacket(packet);
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, "Error while procesing packet. Byte count: {count}", count);
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
            
            bool successfull = Host.HostInfo.Password == packet.Body;
            int id = -1;
            if (successfull)
            {
                AuthLevel = AuthLevel.Authorized;
                id = packet.ID;
            }
            await SendPacket(new RconPacket()
            {
                Type = RconPacket.ServerDataAuthResponsePacket,
                Body = default,
                ID = id,
            });
        }

        protected virtual async Task ProcessExecuteCommand(RconPacket packet)
        {
            var commandActor = new RconCommandActor(this, packet.ID);
            var args = ArgumentsParser.ParseArguments(packet.Body);

            if (args.Length == 0)
                return;

            await commandExecutor.ExecuteAsync(commandActor, args, default);

        }

        public virtual void Dispose()
        {
            this.closedTaskCallback.Item1?.Cancel();
            tcpClient.Dispose();
        }


    }
}
