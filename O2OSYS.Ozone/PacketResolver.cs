using System;

namespace O2OSYS.Ozone
{
	class PacketResolver
	{
		public delegate void PacketCompletedCallback(byte[] buffer);
		public static int PACKET_HEADER_SIZE = 4;
		public static int PACKET_BUFFER_SIZE = 1024;

		int packetSize;
		byte[] buffer = new byte[PACKET_BUFFER_SIZE];
		int bufferPosition;
		int targetPosition;
		int remainBytes;

		public PacketResolver()
		{
			this.packetSize = 0;
			this.bufferPosition = 0;
			this.targetPosition = 0;
			this.remainBytes = 0;
		}

		public void OnReceive(byte[] buffer, int offset, int transffered, PacketCompletedCallback packetCompletedCallback)
		{
			this.remainBytes = transffered;
			int position = offset;

			while (0 < this.remainBytes)
			{
				bool completed = false;
				if (this.bufferPosition < PACKET_HEADER_SIZE)
				{
					this.targetPosition = PACKET_HEADER_SIZE;
					completed = ReadUntil(buffer, offset, transffered, ref position);
					if (!completed)
					{
						return;
					}
					this.packetSize = BitConverter.ToInt32(this.buffer, 0);
					this.targetPosition = PACKET_HEADER_SIZE + this.packetSize;
				}

				completed = ReadUntil(buffer, offset, transffered, ref position);

				if (completed)
				{
					packetCompletedCallback(this.buffer);
					ClearBuffer();
				}
			}

			bool ReadUntil(byte[] srcBuffer, int srcOffset, int srcTransffered, ref int srcPosition)
			{
				if ((srcOffset + srcTransffered) <= this.bufferPosition)
				{
					return false;
				}

				int copySize = this.targetPosition - this.bufferPosition;
				if (this.remainBytes < copySize)
				{
					copySize = this.remainBytes;
				}

				Array.Copy(srcBuffer, srcPosition, this.buffer, this.bufferPosition, copySize);
				srcPosition += copySize;
				this.bufferPosition += copySize;
				this.remainBytes -= copySize;

				if (this.bufferPosition < this.targetPosition)
				{
					return false;
				}
				return true;
			}
		}

		private void ClearBuffer()
		{
			Array.Clear(this.buffer, 0, this.buffer.Length);
			this.bufferPosition = 0;
			this.packetSize = 0;
		}
	}
}
