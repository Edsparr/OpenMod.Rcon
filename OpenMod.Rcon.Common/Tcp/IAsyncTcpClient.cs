using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMod.Rcon.Common.Tcp
{
    public interface IAsyncTcpClient : IAsyncDisposable
    {
        Func<IAsyncTcpClient, Task> Closed { get; set; }
        Func<IAsyncTcpClient, ArraySegment<byte>, Task> Received { get; set; }

        Task Start();
        Task Stop(CancellationToken cancellationToken = default);

        Task Send(byte[] data, CancellationToken cancellationToken = default);
    }
}
