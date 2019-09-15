using System;
using System.Collections.Generic;
using ICD.Common.Utils.EventArguments;

namespace ICD.Connect.Routing.Endpoints
{
	/// <summary>
	/// Common interface for Sources and Destinations.
	/// </summary>
	public interface ISourceDestinationCommon : ISourceDestinationBaseCommon
	{
		/// <summary>
		/// Raised when the disable state changes.
		/// </summary>
		event EventHandler<BoolEventArgs> OnDisableStateChanged;

		#region Properties

		/// <summary>
		/// Specifies the device this source/destination is pointing to.
		/// </summary>
		int Device { get; set; }

		/// <summary>
		/// Specifies the control this source/destination is pointing to.
		/// </summary>
		int Control { get; set; }

		/// <summary>
		/// Specifies if this instance was discovered via remote broadcast.
		/// </summary>
		bool Remote { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Gets the addresses used by this source/destination.
		/// </summary>
		/// <returns></returns>
		IEnumerable<int> GetAddresses();

		/// <summary>
		/// Gets all of the addresses as endpoint info.
		/// </summary>
		/// <returns></returns>
		IEnumerable<EndpointInfo> GetEndpoints();

		/// <summary>
		/// Sets the addresses used by this source/destination.
		/// </summary>
		/// <param name="addresses"></param>
		void SetAddresses(IEnumerable<int> addresses);

		/// <summary>
		/// Returns true if the source/destination contains the given endpoint info.
		/// </summary>
		/// <param name="endpoint"></param>
		/// <returns></returns>
		bool Contains(EndpointInfo endpoint);

		/// <summary>
		/// Filters the endpoints by the endpoints contained in this source/destination.
		/// </summary>
		/// <param name="endpoints"></param>
		/// <returns></returns>
		IEnumerable<EndpointInfo> FilterEndpoints(IEnumerable<EndpointInfo> endpoints);

		#endregion
	}
}
