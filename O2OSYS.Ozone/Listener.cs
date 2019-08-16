using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace O2OSYS.Ozone
{
	class Listener
	{
		SocketAsyncEventArgs acceptEventArgs;
		Socket socket;
		AutoResetEvent flowControlEvent;

		public delegate void AcceptHandler(Socket clientSocket, object token);
		public AcceptHandler AcceptCallback;

		public Listener()
		{
			AcceptCallback = null;
		}

		public void Start(IPEndPoint endPoint, int backlog)
		{
			try
			{
				this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				this.socket.Bind(endPoint);
				this.socket.Listen(backlog);

				this.acceptEventArgs = new SocketAsyncEventArgs();
				this.acceptEventArgs.Completed += new System.EventHandler<SocketAsyncEventArgs>(AcceptCompleted);

				Thread acceptThread = new Thread(AcceptAsync);
				acceptThread.Start();
			}
			catch (Exception ex)
			{
				// TODO: Exception handling
			}
		}

		private void AcceptAsync()
		{
			this.flowControlEvent = new AutoResetEvent(false);

			while (true)
			{
				this.acceptEventArgs.AcceptSocket = null;

				bool pending = true;
				try
				{
					pending = this.socket.AcceptAsync(this.acceptEventArgs);
				}
				catch (Exception ex)
				{
					// TODO: Exception handling

					continue;
				}

				if (!pending)
				{
					AcceptCompleted(null, this.acceptEventArgs);
				}

				this.flowControlEvent.WaitOne();
			}
		}

		private void AcceptCompleted(object sender, SocketAsyncEventArgs args)
		{
			if (args.SocketError == SocketError.Success)
			{
				Socket clientSocket = args.AcceptSocket;
				this.flowControlEvent.Set();
				this.AcceptCallback?.Invoke(clientSocket, args.UserToken);
			}
			else
			{
				// TODO: Treat socket error
			}
			this.flowControlEvent.Set();
		}
	}
}
