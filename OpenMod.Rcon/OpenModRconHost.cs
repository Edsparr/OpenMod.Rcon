using Microsoft.Extensions.Configuration;
using OpenMod.Rcon.Api;
using OpenMod.Rcon.Api.Models;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMod.Rcon
{
    public class OpenModRconHost : IRconHost
    {
        public RconHostInfo HostInfo { get; }
        public IReadOnlyCollection<IRconConnection> Connections { get; }

        public Task Start()
        {
            throw new NotImplementedException();
        }

        public Task Stop()
        {
            throw new NotImplementedException();
        }
    }
}
