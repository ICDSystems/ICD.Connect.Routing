using System.Collections.Generic;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Endpoints
{
	public interface ISourceDestinationCommonSettings : ISettings
	{
		/// <summary>
		/// Gets the endpoint device.
		/// </summary>
		int Device { get; set; }

		/// <summary>
		/// Gets the endpoint device control.
		/// </summary>
		int Control { get; set; }

		/// <summary>
		/// Specifies which media types to use for the source.
		/// </summary>
		eConnectionType ConnectionType { get; set; }

		/// <summary>
		/// Gets the addresses used by this source/destination.
		/// </summary>
		/// <returns></returns>
		IEnumerable<int> GetAddresses();

		/// <summary>
		/// Sets the addresses used by this source/destination.
		/// </summary>
		/// <param name="addresses"></param>
		void SetAddresses(IEnumerable<int> addresses);
	}
}
