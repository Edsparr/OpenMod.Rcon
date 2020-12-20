using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OpenMod.Rcon.Common.Tcp
{
    public interface IAsyncTcpClient : IDisposable
    {
        Func<byte[], Task> Received { get; set; }
        Func<IAsyncTcpClient, Task> Disconnected { get; set; }

        Task Start();
        void Stop();

        Task Send(Stream stream);
    }
}
