using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            containerBuilder.Register<OpenModRconHost>(context => ActivatorUtilities.CreateInstance<OpenModRconHost>(context.Resolve<IServiceProvider>()))
                .Keyed<IRconHost>(nameof(OpenModRconHost)); //Can't make it work without it.
        }
    }
}
