using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Endpoints.Sources;
using ICD.Connect.Routing.Groups.Endpoints.Sources;
using NUnit.Framework;

namespace ICD.Connect.Routing.Tests.Groups.Endpoints.Sources
{
	[TestFixture]
	public sealed class SourceGroupTest : AbstractSourceDestinationGroupCommonTest<SourceGroup, SourceGroupSettings,ISource>
	{
		protected override SourceGroup InstantiateGroup(eConnectionType connectionTypeMask)
		{
			return new SourceGroup()
			{
				ConnectionTypeMask = connectionTypeMask
			};
		}

		/// <summary>
		/// Method to get an originator of TOriginator with the given id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		protected override ISource GetOriginator(int id, eConnectionType connectionType)
		{
			return new Source()
			{
				Id=id,
				ConnectionType =  connectionType
			};
		}
	}
}
