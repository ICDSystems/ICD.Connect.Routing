using System.Collections.Generic;
using System.Linq;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Settings.Groups;

namespace ICD.Connect.Routing.Groups.Endpoints
{
	public abstract class AbstractSourceDestinationBaseGroup<TOriginator, TSettings> :
		AbstractGroup<TOriginator, TSettings>, ISourceDestinationBaseGroup<TOriginator>
		where TOriginator : class, ISourceDestinationBase
		where TSettings : ISourceDestinationBaseGroupSettings, new()
	{
		/// <summary>
		/// Gets the items in the group.
		/// </summary>
		IEnumerable<ISourceDestinationBase> IGroup<ISourceDestinationBase>.Items
		{
			get { return Items.Cast<ISourceDestinationBase>(); }
		}
	}
}
