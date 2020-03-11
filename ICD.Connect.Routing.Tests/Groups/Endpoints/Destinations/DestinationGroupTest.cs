using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Endpoints.Destinations;
using ICD.Connect.Routing.Groups.Endpoints.Destinations;
using NUnit.Framework;

namespace ICD.Connect.Routing.Tests.Groups.Endpoints.Destinations
{
	[TestFixture]
	public sealed class DestinationGroupTest : AbstractSourceDestinationGroupCommonTest<DestinationGroup, DestinationGroupSettings, IDestination>
	{
		protected override IDestination GetOriginator(int id, eConnectionType connectionType)
		{
			return new Destination()
			{
				Id = id,
				ConnectionType = connectionType
			};
		}

		protected override DestinationGroup InstantiateGroup(eConnectionType connectionTypeMask)
		{
			return new DestinationGroup()
			{
				ConnectionTypeMask = connectionTypeMask
			};
		}
	}
}
