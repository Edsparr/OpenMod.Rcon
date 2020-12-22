using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using OpenMod.Rcon.Api;
using OpenMod.Rcon.Api.Models;
using OpenMod.Rcon.Common;
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
        public OpenModRconHost(ILifetimeScope scope, IConfiguration configuration) : base(scope, configuration)
        {

        }

        protected override void RegisterConnection(ContainerBuilder builder)
        {
            builder.RegisterType<OpenModRconConnection>()
                .As<IRconConnection>()
                .InstancePerLifetimeScope();
        }
    }
}
