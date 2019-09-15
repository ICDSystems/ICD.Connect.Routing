using System.Collections.Generic;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing.Endpoints.Destinations
{
	public abstract class AbstractDestination<TSettings> : AbstractSourceDestinationCommon<TSettings>, IDestination
		where TSettings : IDestinationSettings, new()
	{
		/// <summary>
		/// Gets the destinations represented by this instance.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IDestination> IDestinationBase.GetDestinations()
		{
			yield return this;
		}

		IEnumerable<IDestination> IDestinationBase.GetDestinations(eConnectionType type)
		{
			if (ConnectionType.HasFlags(type))
				yield return this;
		}
	}
}
