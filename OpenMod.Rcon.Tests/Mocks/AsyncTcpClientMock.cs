using OpenMod.Rcon.Common.Tcp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMod.Rcon.Common.Tests.Mocks
{
    public class AsyncTcpClientMock : IAsyncTcpClient //hehe, got a feeling im doing something wrong
    {
		private Stream stream = new MemoryStream();

		public bool IsConnected => true;

		public Stream ReceivedStream { get; set; } = new MemoryStream();

		public Func<IAsyncTcpClient, Task> Closed { get; set; }

		public Func<IAsyncTcpClient, ArraySegment<byte>, Task> Received { get; set; }
		public Func<IAsyncTcpClient, Task> Disconnected { get; set; }

		public Task Start()
		{
			return Task.CompletedTask;
		}

		public Task Stop(CancellationToken cancellationToken = default) => DisposeAsync(cancellationToken).AsTask();


		public async Task ReceivedMock(Stream stream)
        {
			var buffer = new byte[stream.Length];
			var length = await stream.ReadAsync(buffer, 0, buffer.Length);

			await ReceivedStream.WriteAsync(buffer, 0, length);

		}

		public async Task Send(byte[] data, CancellationToken cancellationToken = default)
		{

			await stream.WriteAsync(data, 0, data.Length, cancellationToken);
		}


		public ValueTask DisposeAsync(CancellationToken cancellationToken = default)
		{
			return new ValueTask();
		}

		public ValueTask DisposeAsync() => DisposeAsync(default);

	}
}
