using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenMod.Rcon.Api;
using OpenMod.Rcon.Api.Models;
using OpenMod.Rcon.Api.Packets;
using OpenMod.Rcon.Common.Packets;
using OpenMod.Rcon.Common.Tcp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMod.Rcon.Common
{
    public abstract class RconHostBase : IRconHost
    {
        private readonly ILifetimeScope scope;
        private readonly ILogger<IRconHost> logger;
        private readonly IConfiguration configuration;
        
        public RconHostBase(ILifetimeScope scope,
            ILogger<IRconHost> logger,
            IConfiguration configuration)
        {
            this.scope = scope;
            this.logger = logger;
            this.configuration = configuration;

        }

        protected AsyncTcpListener listener;
        private List<(ILifetimeScope, IRconConnection)> connections = new List<(ILifetimeScope, IRconConnection)>();

        public virtual RconHostInfo HostInfo => configuration.GetSection("hostInfo").Get<RconHostInfo>();
        public IReadOnlyCollection<IRconConnection> Connections => connections.Select(c => c.Item2).ToList(); //For casting.


        public async Task Start(CancellationToken cancellationToken = default)
        {

            if (HostInfo.Password.Equals("ChangeThisToEnableRcon", StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException($"Current password is the default password! Change this to enable Rcon and prevent others from connecting to Rcon.");

            listener = new AsyncTcpListener()
            {
                ClientConnected = ClientConnected,
                Address = IPAddress.IPv6Any,
                Port = HostInfo.Port
            };

            await listener.Start(cancellationToken);
        }
        public async Task Stop(CancellationToken cancellationToken = default)
        {
            await listener.Stop(cancellationToken);
            

            foreach (var item in connections)
                await item.Item1.DisposeAsync(); //LifetimeScope.Dispose() -> IRconConnection.Dispose() & all dependencies.

            connections.Clear();
        }

        protected virtual IAsyncTcpClient Build(TcpClient tcpClient) => new AsyncTcpClient(tcpClient);

        protected virtual async Task<IAsyncTcpClient> ClientConnected(TcpClient arg)
        {
            var client = Build(arg);

            var connectionScope = scope.BeginLifetimeScope(builder => 
            {
                builder.RegisterInstance(client)
                .As<IAsyncTcpClient>();

                RegisterPacketSerializer(builder);
                RegisterConnection(builder);
            });

            var connection = connectionScope.Resolve<IRconConnection>();

            connection.Disconnected = ClientDisconnected;

            connections.Add((connectionScope, connection));

            await connection.Start();

            logger.LogDebug("Rcon client connected.");

            return client;
        }


        private async Task ClientDisconnected(IRconConnection connection)
        {
            logger.LogDebug($"Rcon user disconnected.");
            var item = connections.SingleOrDefault(c => c.Item2 == connection);
            await item.Item1.DisposeAsync();
            connections.Remove(item);
        }

        protected virtual void RegisterPacketSerializer(ContainerBuilder builder)
        {
            builder.RegisterType<ValveRconPacketSerializer>()
                .As<IPacketSerializer>()
                .InstancePerDependency();
        }

        protected abstract void RegisterConnection(ContainerBuilder builder);
    }
}
