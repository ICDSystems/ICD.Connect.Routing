using System;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Endpoints;
using NUnit.Framework;

namespace ICD.Connect.Routing.Tests
{
	[TestFixture]
	public sealed class RouteOperationTest
	{
		[Test]
		public void IdTest()
		{
			RouteOperation op = new RouteOperation();

			Assert.IsTrue(op.Id != default(Guid));
		}

		[TestCase(1, 2, 3)]
		public void SourceTest(int device, int control, int address)
		{
			EndpointInfo expected = new EndpointInfo(device, control, address);
			RouteOperation op = new RouteOperation {Source = expected};

			Assert.AreEqual(expected, op.Source);
		}

		[TestCase(1, 2, 3)]
		public void DestinationTest(int device, int control, int address)
		{
			EndpointInfo expected = new EndpointInfo(device, control, address);
			RouteOperation op = new RouteOperation {Destination = expected};

			Assert.AreEqual(expected, op.Destination);
		}

		[TestCase(10)]
		public void LocalInputTest(int expected)
		{
			RouteOperation op = new RouteOperation {LocalInput = expected};

			Assert.AreEqual(expected, op.LocalInput);
		}

		[TestCase(10)]
		public void LocalOutputTest(int expected)
		{
			RouteOperation op = new RouteOperation { LocalOutput = expected };

			Assert.AreEqual(expected, op.LocalOutput);
		}

		[TestCase(10)]
		public void LocalDeviceTest(int expected)
		{
			RouteOperation op = new RouteOperation { LocalDevice = expected };

			Assert.AreEqual(expected, op.LocalDevice);
		}

		[TestCase(10)]
		public void LocalControlTest(int expected)
		{
			RouteOperation op = new RouteOperation { LocalControl = expected };

			Assert.AreEqual(expected, op.LocalControl);
		}

		[TestCase(eConnectionType.Video)]
		public void ConnectionType(eConnectionType expected)
		{
			RouteOperation op = new RouteOperation { ConnectionType = expected };

			Assert.AreEqual(expected, op.ConnectionType);
		}

		[TestCase(10)]
		public void RoomIdTest(int expected)
		{
			RouteOperation op = new RouteOperation { RoomId = expected };

			Assert.AreEqual(expected, op.RoomId);
		}

		[TestCase("test", (ushort)7263)]
		public void RouteRequestFromTest(string hostname, ushort port)
		{
			HostInfo expected = new HostInfo(hostname, port);
			RouteOperation op = new RouteOperation { RouteRequestFrom = expected };

			Assert.AreEqual(expected, op.RouteRequestFrom);
		}

		[Test]
		public void CopyTest()
		{
			Assert.Inconclusive();
		}
	}
}
