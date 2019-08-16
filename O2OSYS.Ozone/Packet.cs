using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2OSYS.Ozone
{
	public class Packet
	{
		public static short MAX_BUFFER_SIZE = 1024;
		public static short PACKET_TYPE_SIZE = 2;
		public static short PACKET_BODY_SIZE = 2;
		public static short HEADER_SIZE = (short)(PACKET_TYPE_SIZE + PACKET_BODY_SIZE);
		public byte[] Buffer { get; set; }
		public short Position { get; set; }
		public short PacketType { get; set; }
		public short BodySize { get; set; }

		public Packet(short packetType = 0)
		{
			Buffer = new byte[MAX_BUFFER_SIZE];
			Position = HEADER_SIZE;
			PacketType = packetType;
			BodySize = 0;

			byte[] tempBuffer = BitConverter.GetBytes(PacketType);
			tempBuffer.CopyTo(Buffer, 0);
		}

		public Packet(Packet packet)
		{
			Buffer = packet.Buffer;
			Position = HEADER_SIZE;
			PacketType = packet.PacketType;
			BodySize = packet.BodySize;
		}

		public Packet Clone()
		{
			Packet packet = new Packet(PacketType);
			Buffer.CopyTo(packet.Buffer, 0);
			packet.Position = HEADER_SIZE;
			packet.BodySize = BodySize;
			return packet;
		}

		public void Write(short data)
		{
			byte[] tempBuffer = BitConverter.GetBytes(data);
			WriteBody(tempBuffer);
		}

		public void Write(int data)
		{
			byte[] tempBuffer = BitConverter.GetBytes(data);
			WriteBody(tempBuffer);
		}

		public void Write(long data)
		{
			byte[] tempBuffer = BitConverter.GetBytes(data);
			WriteBody(tempBuffer);
		}

		public void Write(string data)
		{
			byte[] tempBuffer = Encoding.UTF8.GetBytes(data);
			Write((short)tempBuffer.Length);
			WriteBody(tempBuffer);
		}

		public short ReadInt16()
		{
			short data = BitConverter.ToInt16(Buffer, Position);
			Position += sizeof(short);
			return data;
		}

		public int ReadInt32()
		{
			int data = BitConverter.ToInt32(Buffer, Position);
			Position += sizeof(int);
			return data;
		}

		public long ReadInt64()
		{
			long data = BitConverter.ToInt64(Buffer, Position);
			Position += sizeof(long);
			return data;
		}

		public string ReadString()
		{
			short length = ReadInt16();
			string data = Encoding.UTF8.GetString(Buffer, Position, length);
			Position += length;
			return data;
		}

		private void WriteBody(byte[] srcBuffer)
		{
			srcBuffer.CopyTo(Buffer, Position);
			Position += (short)srcBuffer.Length;
			BodySize += (short)srcBuffer.Length;
			WriteBodySize();
		}

		private void WriteBodySize()
		{
			byte[] tempBuffer = BitConverter.GetBytes(BodySize);
			tempBuffer.CopyTo(Buffer, PACKET_TYPE_SIZE);
		}
	}
}
