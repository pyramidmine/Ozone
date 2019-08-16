using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace O2OSYS.Ozone
{
	class UserToken
	{
		public SocketAsyncEventArgs ReceiveArgs { get; set; }
		public SocketAsyncEventArgs SendArgs { get; set; }
		public Socket Socket { get; set; }

		private PacketResolver packetResolver;
		private Queue<Packet> packetQueue;

		public UserToken()
		{
			this.packetResolver = new PacketResolver();
			this.packetQueue = new Queue<Packet>();
		}

		public void OnPacketCompleted(byte[] buffer)
		{
			// TODO: Treat packet completed event
		}

		public void OnReceive(byte[] buffer, int offset, int transffered)
		{
			this.packetResolver.OnReceive(buffer, offset, transffered, OnPacketCompleted);
		}

		public void OnRemove()
		{

		}

		public void OnSend(SocketAsyncEventArgs args)
		{

		}

		public void Send(Packet packet)
		{
			lock (this.packetQueue)
			{
				if (this.packetQueue.Count <= 0)
				{
					this.packetQueue.Enqueue(packet);
					StartSend();
					return;
				}
				else
				{
					this.packetQueue.Enqueue(packet);
				}
			}
		}

		private void StartSend()
		{
			lock (this.packetQueue)
			{
				Packet packet = this.packetQueue.Peek();
				this.SendArgs.SetBuffer(this.SendArgs.Offset, packet.Position);
				Array.Copy(packet.Buffer, 0, this.SendArgs.Buffer, this.SendArgs.Offset, packet.Position);

				bool pending = this.Socket.SendAsync(this.SendArgs);
				if (!pending)
				{
					ProcessSend(this.SendArgs);
				}
			}
		}

		private void ProcessSend(SocketAsyncEventArgs args)
		{
			if ((0 < args.BytesTransferred) && (args.SocketError == SocketError.Success))
			{
				lock (this.packetQueue)
				{
					int bytesTransffered = this.packetQueue.Peek().Position;
					if (args.BytesTransferred != bytesTransffered)
					{
						// TODO: Continue sending remained packet data
					}

					this.packetQueue.Dequeue();

					if (0 < this.packetQueue.Count)
					{
						StartSend();
					}
				}
			}
			else
			{
				// TODO: Error handling
			}
		}
	}
}
