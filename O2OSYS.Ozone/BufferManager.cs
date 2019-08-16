using System.Collections.Generic;
using System.Net.Sockets;

namespace O2OSYS.Ozone
{
	class BufferManager
	{
		int totalBytes;
		int bufferSize;
		byte[] buffer;
		Stack<int> freeIndexPool;
		int currentIndex;

		public BufferManager(int totalBytes, int bufferSize)
		{
			this.totalBytes = totalBytes;
			this.bufferSize = bufferSize;
			this.freeIndexPool = new Stack<int>();
			this.currentIndex = 0;
		}

		public void InitializeBuffer()
		{
			this.buffer = new byte[this.totalBytes];
		}

		public bool SetBuffer(SocketAsyncEventArgs args)
		{
			if (0 < this.freeIndexPool.Count)
			{
				args.SetBuffer(this.buffer, this.freeIndexPool.Pop(), this.bufferSize);
			}
			else
			{
				if ((this.totalBytes - this.bufferSize) < this.currentIndex)
				{
					return false;
				}
				args.SetBuffer(this.buffer, this.currentIndex, this.bufferSize);
				this.currentIndex += this.bufferSize;
			}
			return true;
		}

		public void FreeBuffer(SocketAsyncEventArgs args)
		{
			this.freeIndexPool.Push(args.Offset);
			args.SetBuffer(null, 0, 0);
		}
	}
}
