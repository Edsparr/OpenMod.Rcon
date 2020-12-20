using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenMod.API.Commands;
using OpenMod.Rcon.Api;
using OpenMod.Rcon.Api.Actors;
using OpenMod.Rcon.Api.Packets;
using OpenMod.Rcon.Common.Packets;
using OpenMod.Rcon.Common.Tests.Mocks;
using OpenMod.Rcon.Tests.Mocks;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unclassified.Net;

namespace OpenMod.Rcon.Common.Tests
{
    [TestClass]
    public class OpenModRconConnectionTests //Would appreciate some feedback because I never ever write tests, pretty aware all this is absolute code horror
    {
        private OpenModRconHostMock host;
        private AsyncTcpClientMock tcpClient = new AsyncTcpClientMock();
        private ValveRconPacketSerializer packetSerializer;
        private ICommandExecutor commandExecutor;

        private OpenModRconConnection connection;


        [TestInitialize]
        public void Initialize()
        {
            tcpClient = new AsyncTcpClientMock();
            host = new OpenModRconHostMock();
            packetSerializer = new ValveRconPacketSerializer();;
            commandExecutor = new CommandExecutorMock();

            var loggerMock = Mock.Of<ILogger<IRconConnection>>();

            connection = new OpenModRconConnection(host, packetSerializer, tcpClient, commandExecutor, loggerMock);

            connection.Start()?.GetAwaiter().GetResult();
        }

        [TestMethod]
        public async Task Authorize_ShouldThrowException_IfAuthFailes()
        {
            const int packetId = 1;

            await Authorize(packetId, "password");

            Assert.IsTrue(this.tcpClient.BytesSent.Count == 2, "{count} packets was found instead of 2!", tcpClient.BytesSent.Count);
            using var authPacketStream =  new MemoryStream(tcpClient.BytesSent.ElementAt(1));

            var authPacket = await packetSerializer.Deserialize(authPacketStream);

            Assert.AreEqual(authPacket.Id, packetId);
        }

        [TestMethod]
        public async Task Authorize_ShouldThrowException_IfAuthDoNotFail()
        {
            const int packetId = 1;

            await Authorize(packetId, "some password");

            Assert.IsTrue(this.tcpClient.BytesSent.Count == 2, "{count} packets was found instead of 2!", tcpClient.BytesSent.Count);
            using var authPacketStream = new MemoryStream(tcpClient.BytesSent.ElementAt(1));

            var authPacket = await packetSerializer.Deserialize(authPacketStream);
            using var responsePacketStream = new MemoryStream(tcpClient.BytesSent.ElementAt(0));

            var responsePacket = await packetSerializer.Deserialize(responsePacketStream);

            Assert.AreEqual(authPacket.Id, -1);

            Assert.AreEqual(responsePacket.Id, packetId);

        }

        [TestMethod]
        public async Task ExecuteCommand_ShouldThrowException_IfFailed()
        {
            int packetId = 1;

            await Authorize(packetId, host.HostInfo.Password);
            packetId++;

            await tcpClient.ReceivedMock(await packetSerializer.Serialize(new RconPacket()
            {
                Body = "quit",
                Id = packetId,
                Type = RconPacket.ServerDataExecuteCommandPacket
            }));

            Assert.IsTrue(this.tcpClient.BytesSent.Count == 3, "{count} packets was found instead of 3!", tcpClient.BytesSent.Count);

            using var messagePacketStream = new MemoryStream(tcpClient.BytesSent.ElementAt(2));

            var messagePacket = await packetSerializer.Deserialize(messagePacketStream);

            Assert.AreEqual(messagePacket.Id, packetId);
            Assert.AreEqual(messagePacket.Type, RconPacket.ServerDataResponsePacket);
            Assert.AreEqual(messagePacket.Body, CommandExecutorMock.DemoResponse);

        }

        private async Task Authorize(int packetId, string password)
        {
            using var authPacket = await packetSerializer.Serialize(new RconPacket()
            {
                Type = RconPacket.ServerDataAuthPacket,
                Body = password,
                Id = packetId
            });

            await tcpClient.ReceivedMock(authPacket);

        }



    }
}
