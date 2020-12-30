using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using OpenMod.Rcon.Api;
using OpenMod.Rcon.Api.Models;
using OpenMod.Rcon.Api.Packets;
using OpenMod.Rcon.Common;
using OpenMod.Rcon.Common.Tcp;
using OpenMod.Rcon.Rocket.Packets;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMod.Rcon.Rocket
{
    public class RocketRconHost : RconHostBase
    {
        public RocketRconHost(ILifetimeScope scope, ILogger<IRconHost> logger, IConfiguration configuration) : base(scope, logger, configuration)
        {

        }

        protected override void RegisterConnection(ContainerBuilder builder)
        {
            builder.RegisterType<RocketRconConnection>()
                .As<IRconConnection>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();
        }

        protected override void RegisterPacketSerializer(ContainerBuilder builder)
        {
            builder.RegisterType<RocketRconPacketSerializer>()
                .As<IPacketSerializer>()
                .InstancePerLifetimeScope();
        }
    }
}
