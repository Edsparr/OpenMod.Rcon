using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Ioc;
using OpenMod.Rcon.Api;
using OpenMod.Rcon.Api.Packets;
using OpenMod.Rcon.Rocket.Packets;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenMod.Rcon.Rocket.Configurators
{
    public class RocketRconContainerConfigurator : IContainerConfigurator
    {
        public void ConfigureContainer(IOpenModServiceConfigurationContext openModStartupContext, ContainerBuilder containerBuilder)
        {

            containerBuilder.Register<IRconHost>(context => ActivatorUtilities.CreateInstance<RocketRconHost>(context.Resolve<IServiceProvider>()))
                .Keyed<IRconHost>(nameof(RocketRconHost)); //Can't make it work without it.

        }
    }
}
