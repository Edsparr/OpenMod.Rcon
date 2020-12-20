using Autofac;
using Microsoft.Extensions.Configuration;
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
using Unclassified.Net;

namespace OpenMod.Rcon.Common
{
    public abstract class RconHostBase : IRconHost
    {
        private readonly ILifetimeScope scope;
        private readonly IConfiguration configuration;
        
        public RconHostBase(ILifetimeScope scope, IConfiguration configuration)
        {
            this.scope = scope;
            this.configuration = configuration;

        }

        protected AsyncTcpListener listener;
        private List<(ILifetimeScope, IRconConnection)> connections = new List<(ILifetimeScope, IRconConnection)>();

        public virtual RconHostInfo HostInfo => configuration.GetSection("hostInfo").Get<RconHostInfo>();
        public IReadOnlyCollection<IRconConnection> Connections => connections.Select(c => c.Item2).ToList(); //For casting.


        public async Task Start()
        {
            listener = new AsyncTcpListener()
            {
                ClientConnectedCallback = ClientConnected,
                IPAddress = IPAddress.IPv6Any,
                Port = HostInfo.Port
            };
            await listener.RunAsync();
        }
        public Task Stop()
        {
            listener.Stop(true);
            listener = null;

            return Task.CompletedTask;
        }

        protected virtual async Task ClientConnected(TcpClient arg)
        {
            var connectionScope = scope.BeginLifetimeScope(builder => 
            {
                RegisterConnectionTcpClient(builder);
                RegisterPacketSerializer(builder);
                RegisterConnection(builder);
            });

            var connection = connectionScope.Resolve<IRconConnection>();

            connection.Disconnected = ClientDisconnected;

            connections.Add((connectionScope, connection));

            await connection.Start();
        }


        protected virtual void RegisterConnectionTcpClient(ContainerBuilder builder)
        {
            builder.RegisterType<AsyncTcpClientWrapper>()
                .As<IAsyncTcpClient>()
                .InstancePerLifetimeScope();
        }

        protected virtual void RegisterPacketSerializer(ContainerBuilder builder)
        {
            builder.RegisterType<ValveRconPacketSerializer>()
                .As<IPacketSerializer>()
                .InstancePerDependency();
        }

        protected abstract void RegisterConnection(ContainerBuilder builder);

        private async Task ClientDisconnected(IRconConnection connection)
        {
            var item = connections.SingleOrDefault(c => c.Item2 == connection);
            await item.Item1.DisposeAsync();
            connections.Remove(item);
        }
    }
}
