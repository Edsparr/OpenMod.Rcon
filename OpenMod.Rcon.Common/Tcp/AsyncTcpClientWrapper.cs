using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unclassified.Net;

namespace OpenMod.Rcon.Common.Tcp
{
    public class AsyncTcpClientWrapper : IAsyncTcpClient
    {
        public AsyncTcpClientWrapper()
        {
            Client = new AsyncTcpClient()
            {
                AutoReconnect = false
            };
            Client.ReceivedCallback = OnReceived;
            Client.ClosedCallback = (client, reconnected) => Disconnected?.Invoke(this);

        }

        private (CancellationTokenSource, Task) closedTaskCallback;

        public AsyncTcpClient Client { get; }

        public Func<byte[], Task> Received { get; set; }
        public Func<IAsyncTcpClient, Task> Disconnected { get; set; }

        public async Task Start()
        {
            var task = Task.Run(async () =>
            {
                await Task.WhenAny(Client.ClosedTask, Task.Delay(-1, closedTaskCallback.Item1.Token));
                if (!Client.ClosedTask.IsCompleted) //Incase it got cancelled instead of completed because the cancel means we don't listen for ClientDisconnect anymore.
                    return;
                await Disconnected?.Invoke(this);

            });

            await Client.RunAsync();

        }


        private async Task OnReceived(AsyncTcpClient arg1, int count)
        {
            var data = await arg1.ByteBuffer.DequeueAsync(count);

            await Received?.Invoke(data);
        }

        public void Stop()
        {
            this.closedTaskCallback.Item1?.Cancel();
            Client.Dispose();
        }

        public void Dispose()
        {
            Stop();
        }

        public async Task Send(Stream stream)
        {
            var count = stream.Length;
            var buffer = new byte[count];
            
            await stream.ReadAsync(buffer, 0, buffer.Length);

            await Client.Send(new ArraySegment<byte>(buffer));
        }
    }
}
