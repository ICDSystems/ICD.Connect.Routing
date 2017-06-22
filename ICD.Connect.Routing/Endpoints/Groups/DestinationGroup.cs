using System.Collections.Generic;
using Newtonsoft.Json;

namespace ICD.Connect.Routing.Endpoints.Groups
{
	public sealed class DestinationGroup : AbstractDestinationGroup<DestinationGroupSettings>
	{
		#region Constructors

		public DestinationGroup()
		{
		}

		[JsonConstructor]
		public DestinationGroup(int id, string name, IEnumerable<int> destinations, bool remote, int order, bool disable)
		{
			Id = id;
			Name = name;
			Destinations = destinations;
			Remote = remote;
			Order = order;
			Disable = disable;
		}

		#endregion
	}
}
