using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Endpoints.Sources;

namespace ICD.Connect.Routing.Groups.Endpoints.Sources
{
	public abstract class AbstractSourceGroup<TOriginator, TSettings> :
		AbstractSourceDestinationGroupCommon<TOriginator, TSettings>, ISourceGroup
		where TOriginator : class, ISource
		where TSettings : ISourceGroupSettings, new()
	{
		/// <summary>
		/// Gets the category for this originator type (e.g. Device, Port, etc)
		/// </summary>
		public override string Category { get { return "SourceGroup"; } }

		/// <summary>
		/// Gets the sources represented by this instance.
		/// </summary>
		/// <returns></returns>
		IEnumerable<ISource> ISourceBase.GetSources()
		{
			return GetItems().Cast<ISource>();
		}

		/// <summary>
		/// Gets sources supporting the given connection type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public IEnumerable<ISource> GetSources(eConnectionType type)
		{
			return GetItems().Cast<ISource>()
			                 .Where(d => d.ConnectionType.HasFlags(type));
		}
	}
}
