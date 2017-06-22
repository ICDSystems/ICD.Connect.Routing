using ICD.Connect.Routing.Connections;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Endpoints
{
	/// <summary>
	/// Base interface for Sources and Destinations.
	/// </summary>
	public interface ISourceDestinationBase : IOriginator
	{
		/// <summary>
		/// Device id
		/// </summary>
		EndpointInfo Endpoint { get; set; }

		/// <summary>
		/// Specifies which media types to use for this source.
		/// </summary>
		eConnectionType ConnectionType { get; set; }

		/// <summary>
		/// Specifies if this instance was discovered via remote broadcast.
		/// </summary>
		bool Remote { get; set; }

		/// <summary>
		/// Specifies custom ordering of the instance to the end user.
		/// </summary>
		int Order { get; set; }

		/// <summary>
		/// Shorthand for disabling an instance in the system.
		/// </summary>
		bool Disable { get; set; }
	}

	
}
