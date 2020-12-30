using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using OpenMod.Rcon.Api;
using OpenMod.Rcon.Api.Models;
using OpenMod.Rcon.Common;
using OpenMod.Rcon.Common.Tcp;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMod.Rcon
{

    public class OpenModRconHost : RconHostBase
    {
        public OpenModRconHost(ILifetimeScope scope, ILogger<IRconHost> logger, IConfiguration configuration) : base(scope, logger, configuration)
        {

        }

        protected override void RegisterConnection(ContainerBuilder builder)
        {
            builder.RegisterType<OpenModRconConnection>()
                .WithAttributeFiltering()
                .As<IRconConnection>()
                .InstancePerLifetimeScope();
        }
    }
}
