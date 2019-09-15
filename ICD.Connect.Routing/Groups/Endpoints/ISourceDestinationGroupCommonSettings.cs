using ICD.Connect.Routing.Connections;
using ICD.Connect.Settings.Groups;

namespace ICD.Connect.Routing.Groups.Endpoints
{
	public interface ISourceDestinationGroupCommonSettings : IGroupSettings
	{
		/// <summary>
		/// Masks the connection types inherited from the group items.
		/// </summary>
		eConnectionType ConnectionTypeMask { get; set; }
	}
}