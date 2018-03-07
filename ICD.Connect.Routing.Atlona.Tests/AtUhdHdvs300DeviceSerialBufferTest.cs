using System;
using System.Collections.Generic;
using ICD.Common.Utils.EventArguments;
using NUnit.Framework;

namespace ICD.Connect.Routing.Atlona.Tests
{
	[TestFixture]
	public sealed class AtUhdHdvs300DeviceSerialBufferTest
	{
		[Test]
		public void CompletedStringEventTest()
		{
			List<StringEventArgs> completedSerials = new List<StringEventArgs>();
			List<EventArgs> login = new List<EventArgs>();
			List<EventArgs> password = new List<EventArgs>();
			List<EventArgs> empty = new List<EventArgs>();

			//List<StringEventArgs> receivedEvents = new List<StringEventArgs>();
			AtUhdHdvs300DeviceSerialBuffer buffer = new AtUhdHdvs300DeviceSerialBuffer();

			buffer.OnCompletedSerial += (sender, e) => completedSerials.Add(e);
			buffer.OnLoginPrompt += (sender, e) => login.Add(e);
			buffer.OnPasswordPrompt += (sender, e) => password.Add(e);
			buffer.OnEmptyPrompt += (sender, e) => empty.Add(e);

			buffer.Enqueue("\r\nLogin:");
			Assert.AreEqual(1, login.Count);

			buffer.Enqueue("\r\nPassword:");
			Assert.AreEqual(1, password.Count);

			buffer.Enqueue("\r\n");
			Assert.AreEqual(1, empty.Count);

			buffer.Enqueue("x4AVx1\r\n");
			buffer.Enqueue("x1$ on\r\n");

			Assert.AreEqual(1, login.Count);
			Assert.AreEqual(1, password.Count);
			Assert.AreEqual(1, empty.Count);

			Assert.AreEqual(2, completedSerials.Count);
			Assert.AreEqual("x4AVx1", completedSerials[0].Data);
			Assert.AreEqual("x1$ on", completedSerials[1].Data);
		}
	}
}
