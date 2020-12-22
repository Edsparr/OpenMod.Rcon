using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMod.Rcon.Common.Tcp
{
    public class AsyncTcpClient : IAsyncTcpClient
    {
		public AsyncTcpClient(TcpClient tcpClient)
		{
			this.TcpClient = tcpClient ?? throw new ArgumentNullException(nameof(tcpClient));
		}

		private NetworkStream stream;
		private Task listenerTask;

		public TcpClient TcpClient { get; set; }

		public bool IsConnected => TcpClient.Client.Connected;

		public Func<IAsyncTcpClient, Task> Closed { get; set; }

		public Func<IAsyncTcpClient, ArraySegment<byte>, Task> Received { get; set; }

        public Task Start()
		{
			if (stream != null)
				throw new Exception("Already started!");

			if (TcpClient == null)
				throw new ObjectDisposedException(nameof(TcpClient));

			stream = TcpClient.GetStream();

			listenerTask = Task.Run(async () =>
			{
				byte[] buffer = new byte[TcpClient.ReceiveBufferSize];

				while (true)
				{
					int readLength;
					try
					{
						readLength = await stream.ReadAsync(buffer, 0, TcpClient.ReceiveBufferSize);
					}
					catch (IOException ex) when ((ex.InnerException as SocketException)?.ErrorCode == (int)SocketError.OperationAborted ||
						(ex.InnerException as SocketException)?.ErrorCode == 125)
					{

							//Connection closed locally.
							readLength = -1;
					}
					catch (IOException ex) when ((ex.InnerException as SocketException)?.ErrorCode == (int)SocketError.ConnectionAborted)
					{
							//Connection aborted.
							readLength = -1;
					}
					catch (IOException ex) when ((ex.InnerException as SocketException)?.ErrorCode == (int)SocketError.ConnectionReset)
					{
							//Connection reset remotely.
							readLength = -2;
					}
                    catch (Exception)
                    {
						readLength = 0;
					}



					Console.WriteLine(readLength);

					if (readLength <= 0)
					{
						await Closed?.Invoke(this);
						TcpClient.Close();
						return;
					}


					await Received?.Invoke(this, new ArraySegment<byte>(buffer, 0, readLength));
				}
			});

			return Task.CompletedTask;
		}

		public Task Stop(CancellationToken cancellationToken = default) => DisposeAsync(cancellationToken).AsTask();
		

		public async Task Send(byte[] data, CancellationToken cancellationToken = default)
		{
			if (TcpClient == null)
				throw new ObjectDisposedException(nameof(TcpClient));

			

			await stream.WriteAsync(data, 0, data.Length, cancellationToken);
		}


		public async ValueTask DisposeAsync(CancellationToken cancellationToken = default)
        {
			TcpClient.Dispose();
			stream = null;
			await Task.WhenAny(listenerTask, Task.Delay(-1, cancellationToken));

		}

		public ValueTask DisposeAsync() => DisposeAsync(default);
	}
}
