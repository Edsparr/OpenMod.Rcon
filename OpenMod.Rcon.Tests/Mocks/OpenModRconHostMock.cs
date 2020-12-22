using OpenMod.Rcon.Api;
using OpenMod.Rcon.Api.Models;
using OpenMod.Rcon.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMod.Rcon.Tests.Mocks
{
    public class OpenModRconHostMock : IRconHost
    {
        public RconHostInfo HostInfo { get; } = new RconHostInfo()
        {
            Password = "password",
            Port = 27016
        };

        public IReadOnlyCollection<IRconConnection> Connections { get; } = new List<IRconConnection>();

        public Task Start(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task Stop(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
