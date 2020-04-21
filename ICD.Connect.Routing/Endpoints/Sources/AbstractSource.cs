using System.Collections.Generic;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing.Endpoints.Sources
{
	public abstract class AbstractSource<TSettings> : AbstractSourceDestinationCommon<TSettings>, ISource
		where TSettings : ISourceSettings, new()
	{
		#region Properties

		/// <summary>
		/// Gets the category for this originator type (e.g. Device, Port, etc)
		/// </summary>
		public override string Category { get { return "Source"; } }

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the sources represented by this instance.
		/// </summary>
		/// <returns></returns>
		IEnumerable<ISource> ISourceBase.GetSources()
		{
			yield return this;
		}

		IEnumerable<ISource> ISourceBase.GetSources(eConnectionType type)
		{
			if (ConnectionType.HasFlags(type))
				yield return this;
		}

		#endregion
	}
}
