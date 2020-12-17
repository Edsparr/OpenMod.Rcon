using OpenMod.Rcon.Api.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMod.Rcon.Api
{
    public interface IRconHost
    {
        RconHostInfo HostInfo { get; }
        IReadOnlyCollection<IRconConnection> Connections { get; }

        Task Start(); //Cancellation token isn't supported by the lib
        Task Stop();
    }
}
