using System;
using System.Net;
using System.Net.Sockets;

namespace O2OSYS.Ozone
{
	public class Server
    {
		public int MaxConnection { get; set; } = 1000;
		public int BufferSize { get; set; } = 1000;

		Listener listener;
		SocketAsyncEventArgsPool receiveArgsPool;
		SocketAsyncEventArgsPool sendArgsPool;
		BufferManager bufferManager;

		public void Initialize()
		{
			this.receiveArgsPool = new SocketAsyncEventArgsPool(MaxConnection);
			this.sendArgsPool = new SocketAsyncEventArgsPool(MaxConnection);
			this.bufferManager = new BufferManager(MaxConnection * BufferSize * 2, BufferSize);
			this.bufferManager.InitializeBuffer();

			for (int i = 0; i < MaxConnection; i++)
			{
				UserToken userToken = new UserToken();

				// Pool of receive
				{
					SocketAsyncEventArgs args = new SocketAsyncEventArgs();
					args.Completed += new System.EventHandler<SocketAsyncEventArgs>(ReceiveCompleted);
					args.UserToken = userToken;
					this.bufferManager.SetBuffer(args);
					this.receiveArgsPool.Push(args);
				}

				// Pool of send
				{
					SocketAsyncEventArgs args = new SocketAsyncEventArgs();
					args.Completed += new System.EventHandler<SocketAsyncEventArgs>(SendCompleted);
					args.UserToken = userToken;
					this.bufferManager.SetBuffer(args);
					this.sendArgsPool.Push(args);
				}
			}
		}

		public void Listen(IPEndPoint endPoint, int backlog)
		{
			Listener listener = new Listener();
			listener.AcceptCallback += AcceptCompleted;
			listener.Start(endPoint, backlog);
		}

		private void CloseConnection(UserToken userToken)
		{
			userToken.OnRemove();
			this.receiveArgsPool?.Push(userToken.ReceiveArgs);
			this.sendArgsPool?.Push(userToken.SendArgs);
		}

		private void StartReceive(Socket socket, SocketAsyncEventArgs receiveArgs, SocketAsyncEventArgs sendArgs)
		{
			UserToken userToken = receiveArgs.UserToken as UserToken;
			userToken.ReceiveArgs = receiveArgs;
			userToken.SendArgs = sendArgs;
			userToken.Socket = socket;

			bool pending = socket.ReceiveAsync(receiveArgs);
			if (!pending)
			{
				ProcessReceive(receiveArgs);
			}
		}

		private void ProcessReceive(SocketAsyncEventArgs args)
		{
			UserToken userToken = args.UserToken as UserToken;

			if ((0 < args.BytesTransferred) && (args.SocketError == SocketError.Success))
			{
				userToken.OnReceive(args.Buffer, args.Offset, args.BytesTransferred);

				bool pending = userToken.Socket.ReceiveAsync(args);
				if (!pending)
				{
					ProcessReceive(args);
				}
			}
			else
			{
				CloseConnection(userToken);
			}
		}

		private void AcceptCompleted(Socket clientSocket, object userToken)
		{
			SocketAsyncEventArgs receiveArgs = this.receiveArgsPool.Pop();
			SocketAsyncEventArgs sendArgs = this.sendArgsPool.Pop();

			StartReceive(clientSocket, receiveArgs, sendArgs);
		}

		private void ReceiveCompleted(object sender, SocketAsyncEventArgs args)
		{
			if (args.LastOperation == SocketAsyncOperation.Receive)
			{
				ProcessReceive(args);
				return;
			}
			else
			{
				throw new ArgumentException("The last operation completed on the socket was not a receive.");
			}
		}

		private void SendCompleted(object sender, SocketAsyncEventArgs args)
		{
			UserToken userToken = args.UserToken as UserToken;
			userToken.OnSend(args);
		}
	}
}
