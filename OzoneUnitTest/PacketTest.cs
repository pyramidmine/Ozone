using O2OSYS.Ozone;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace O2OSYS.Ozone.Tests
{
	[TestClass()]
	public class PacketTest
	{
		[TestMethod()]
		public void PacketCreationTest()
		{
			Packet packet = new Packet();
			Assert.AreEqual(0, packet.PacketType);
		}
	}
}