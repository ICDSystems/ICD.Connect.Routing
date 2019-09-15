using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Settings.Groups;

namespace ICD.Connect.Routing.Groups.Endpoints
{
	public interface ISourceDestinationGroupCommon : IGroup, ISourceDestinationBaseCommon
	{
		/// <summary>
		/// Masks the connection types inherited from the group items.
		/// </summary>
		eConnectionType ConnectionTypeMask { get; set; }
	}
}
