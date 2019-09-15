using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Endpoints.Destinations;

namespace ICD.Connect.Routing.Groups.Endpoints.Destinations
{
	public abstract class AbstractDestinationGroup<TOriginator, TSettings> :
		AbstractSourceDestinationGroupCommon<TOriginator, TSettings>, IDestinationGroup
		where TOriginator : class, IDestination
		where TSettings : IDestinationGroupSettings, new()
	{
		/// <summary>
		/// Gets the destinations represented by this instance.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IDestination> IDestinationBase.GetDestinations()
		{
			return GetItems().Cast<IDestination>();
		}

		/// <summary>
		/// Gets destinations supporting the given connection type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public IEnumerable<IDestination> GetDestinations(eConnectionType type)
		{
			return GetItems().Cast<IDestination>()
			                 .Where(d => d.ConnectionType.HasFlags(type));
		}
	}
}
