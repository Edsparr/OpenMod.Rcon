using OpenMod.Rcon.Common.Tcp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OpenMod.Rcon.Common.Tests.Mocks
{
    public class AsyncTcpClientMock : IAsyncTcpClient //hehe, got a feeling im doing something wrong
    {
        public Func<byte[], Task> Received { get; set; }
        public Func<IAsyncTcpClient, Task> Disconnected { get; set; }

        public ICollection<byte[]> BytesSent { get; } = new List<byte[]>();

        public void Dispose()
        {
            Stop();
        }

        public async Task Send(Stream stream)
        {
            var buffer = new byte[stream.Length];

            await stream.ReadAsync(buffer, 0, buffer.Length);

            BytesSent.Add(buffer);
        }

        public async Task ReceivedMock(Stream stream)
        {
            var buffer = new byte[stream.Length];

            await stream.ReadAsync(buffer, 0, buffer.Length);

            await Received?.Invoke(buffer);
        }

        public Task Start()
        {
            return Task.CompletedTask;
        }

        public void Stop()
        {

        }
    }
}
