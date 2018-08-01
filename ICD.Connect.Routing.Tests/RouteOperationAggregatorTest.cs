using System.Collections.Generic;
using System.Linq;
using ICD.Connect.Routing.Connections;
using NUnit.Framework;

namespace ICD.Connect.Routing.Tests
{
	[TestFixture]
	public sealed class RouteOperationAggregatorTest
	{
		[Test]
		public void AddRouteOperationTest()
		{
			RouteOperationAggregator aggregator = new RouteOperationAggregator();
			Assert.AreEqual(0, aggregator.Count());

			// Add first item
			RouteOperation a = new RouteOperation
			{
				ConnectionType = eConnectionType.Video,
				LocalDevice = 1,
				LocalInput = 10,
				LocalOutput = 20
			};
			aggregator.Add(a);
			Assert.AreEqual(1, aggregator.Count());

			// Add a duplicate
			RouteOperation b = new RouteOperation(a);
			aggregator.Add(b);
			Assert.AreEqual(1, aggregator.Count());

			// Update the existing item
			RouteOperation c = new RouteOperation(a) {ConnectionType = eConnectionType.Audio};
			aggregator.Add(c);
			Assert.AreEqual(1, aggregator.Count());

			// Add a new item
			RouteOperation d = new RouteOperation(a) {LocalOutput = 30};
			aggregator.Add(d);
			Assert.AreEqual(2, aggregator.Count());

			// Verify results
			List<RouteOperation> results = aggregator.ToList();

			Assert.AreEqual(2, results.Count);

			RouteOperation aggregate1 = results[0];
			RouteOperation aggregate2 = results[1];

			Assert.AreEqual(a.LocalDevice, aggregate1.LocalDevice);
			Assert.AreEqual(a.LocalInput, aggregate1.LocalInput);
			Assert.AreEqual(a.LocalOutput, aggregate1.LocalOutput);
			Assert.AreEqual(eConnectionType.Video | eConnectionType.Audio, aggregate1.ConnectionType);

			Assert.AreEqual(d.LocalDevice, aggregate2.LocalDevice);
			Assert.AreEqual(d.LocalInput, aggregate2.LocalInput);
			Assert.AreEqual(d.LocalOutput, aggregate2.LocalOutput);
			Assert.AreEqual(d.ConnectionType, aggregate2.ConnectionType);
		}
	}
}
