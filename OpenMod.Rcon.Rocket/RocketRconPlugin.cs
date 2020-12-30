using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OpenMod.Core.Plugins;
using OpenMod.API.Plugins;
using OpenMod.Rcon.Api;
using Autofac.Features.AttributeFilters;
using Autofac;

[assembly: PluginMetadata("OpenMod.Rcon.Rocket", DisplayName = "Legacy Rocketmod Rcon")]
namespace OpenMod.Rcon.Rocket
{
    public class RocketRconPlugin : OpenModUniversalPlugin
    {
        private readonly IConfiguration m_Configuration;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly ILogger<RocketRconPlugin> m_Logger;
        
        private readonly IRconHost rconHost;
        
        public RocketRconPlugin(
            IConfiguration configuration, 
            IStringLocalizer stringLocalizer,
            ILogger<RocketRconPlugin> logger,
            ILifetimeScope lifetime,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_Configuration = configuration;
            m_StringLocalizer = stringLocalizer;
            this.rconHost = lifetime.ResolveKeyed<IRconHost>(nameof(RocketRconHost));
            m_Logger = logger;
            
        }

        protected override async Task OnLoadAsync()
        {
            m_Logger.LogInformation(m_StringLocalizer["plugin_events:plugin_start"]);

            await rconHost.Start();
        }

        protected override async Task OnUnloadAsync()
        {
            m_Logger.LogInformation(m_StringLocalizer["plugin_events:plugin_stop"]);

            await rconHost.Stop();
        }
    }
}
