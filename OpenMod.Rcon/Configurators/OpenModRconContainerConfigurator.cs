using Autofac;
using Microsoft.Extensions.Configuration;
using OpenMod.API.Ioc;
using OpenMod.Rcon.Api;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenMod.Rcon.Configurators
{
    public class OpenModRconContainerConfigurator : IContainerConfigurator
    {
        public void ConfigureContainer(IOpenModServiceConfigurationContext openModStartupContext, ContainerBuilder containerBuilder)
        {

            containerBuilder.Register<IRconHost>(context =>
            {
                return new OpenModRconHost(context.Resolve<ILifetimeScope>(), context.Resolve<IConfiguration>());
            }); //Can't make it work without it.
        }
    }
}
