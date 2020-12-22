using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMod.Rcon.Common.Tcp
{
	/// <summary>
	/// Listens asynchronously for connections from TCP network clients.
	/// </summary>
	public class AsyncTcpListener : IAsyncTcpListener
	{
		private TcpListener listener;
		private volatile bool isStopped;

		private Task listenerTask = default;

		public IPAddress Address { get; set; } = IPAddress.IPv6Any;
		public int Port { get; set; }

		public Func<IAsyncTcpClient, Task> ClientConnected { get; set; }

		public Task Start(CancellationToken cancellationToken = default)
		{
			if (listener != null)
				throw new InvalidOperationException("The listener is already running.");

			if (Port <= 0 || Port > ushort.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(Port));

			isStopped = false;
			
			listener = new TcpListener(Address, Port); 

			cancellationToken.Register(listener.Stop); //Stop is fire and forget.

			var task = Task.Run(async () =>
			{
				Console.WriteLine("Does this print??? Started");
				listener.Start();
				Console.WriteLine("Stuck");

				var clients = new List<IAsyncTcpClient>();

				try
				{
					while (true)
					{
						TcpClient tcpClient;
						try
						{
							tcpClient = await listener.AcceptTcpClientAsync();

							var client = await ClientConnect(tcpClient);

							clients.Add(client);
						}
						catch (ObjectDisposedException) when (isStopped)
						{
							// Listener was stopped
							break;
						}
						catch(Exception ex)
                        {
							Console.WriteLine(ex);
                        }
					}
				}
				finally
				{
					this.isStopped = true;
					await Task.WhenAll(clients.Select(c => c.Stop()));
					clients.Clear();

					listener = null;
				}
			});

			this.listenerTask = task;

			return Task.CompletedTask;
		}

		public async Task Stop(CancellationToken cancellationToken = default)
		{
			if (listener == null || (listenerTask?.IsCompleted ?? true))
				throw new InvalidOperationException("The listener has not started yet.");
			if (isStopped)
				throw new InvalidOperationException("The listener is already stopped!");

			listener.Stop();

			await Task.WhenAny(listenerTask, Task.Delay(-1, cancellationToken));
		}

		protected virtual async Task<IAsyncTcpClient> ClientConnect(TcpClient tcpClient)
		{
			var client = new AsyncTcpClient(tcpClient);

			await ClientConnected?.Invoke(client);
			return client;
		}

	}

}
