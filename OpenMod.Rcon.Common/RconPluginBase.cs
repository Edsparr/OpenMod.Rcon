using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OpenMod.Core.Plugins;
using OpenMod.API.Plugins;
using OpenMod.Rcon.Api;
using System.Net;

namespace OpenMod.Rcon
{
    public class RconPluginBase : OpenModUniversalPlugin, IRconPlugin
    {
        private readonly IRconHost host;

        public RconPluginBase(IRconHost host, 
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this.host = host;
        }

        protected override async Task OnLoadAsync()
        {

            await host.Start();
        }

        protected override async Task OnUnloadAsync()
        {
            await host.Stop();
        }
    }
}
