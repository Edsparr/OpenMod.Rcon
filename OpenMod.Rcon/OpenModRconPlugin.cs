﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OpenMod.Core.Plugins;
using OpenMod.API.Plugins;
using OpenMod.Rcon.Api;

[assembly: PluginMetadata("OpenMod.Rcon", DisplayName = "OpenMod Rcon")]
namespace OpenMod.Rcon
{
    public class OpenModRconPlugin : OpenModUniversalPlugin
    {
        private readonly IConfiguration m_Configuration;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly ILogger<OpenModRconPlugin> m_Logger;
        
        private readonly IOpenModRconHost rconHost;
        
        public OpenModRconPlugin(
            IConfiguration configuration, 
            IStringLocalizer stringLocalizer,
            ILogger<OpenModRconPlugin> logger,
            IOpenModRconHost rconHost,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_Configuration = configuration;
            m_StringLocalizer = stringLocalizer;
            this.rconHost = rconHost;
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
