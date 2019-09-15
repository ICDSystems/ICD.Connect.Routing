using System.Collections.Generic;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Settings.Originators;

namespace ICD.Connect.Routing.Endpoints
{
	/// <summary>
	/// Common interface for sources, destinations and their groups.
	/// </summary>
	public interface ISourceDestinationBaseCommon : IOriginator
	{
		/// <summary>
		/// Specifies which media types to use for this source/destination.
		/// </summary>
		eConnectionType ConnectionType { get; }

		/// <summary>
		/// Gets the devices for this instance.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IDeviceBase> GetDevices();

		/// <summary>
		/// Gets the endpoints for this instance.
		/// </summary>
		/// <returns></returns>
		IEnumerable<EndpointInfo> GetEndpoints();
	}
}
