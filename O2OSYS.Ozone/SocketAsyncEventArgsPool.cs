using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace O2OSYS.Ozone
{
	class SocketAsyncEventArgsPool
	{
		Stack<SocketAsyncEventArgs> pool;

		public int Count { get { return this.pool.Count; } }

		public SocketAsyncEventArgsPool(int capacity)
		{
			this.pool = new Stack<SocketAsyncEventArgs>(capacity);
		}

		public void Push(SocketAsyncEventArgs item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null");
			}
			lock (this.pool)
			{
				this.pool.Push(item);
			}
		}

		public SocketAsyncEventArgs Pop()
		{
			lock (this.pool)
			{
				return this.pool.Pop();
			}
		}
	}
}
