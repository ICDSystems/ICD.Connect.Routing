using System;
using System.Collections.Generic;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Services;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.Endpoints
{
	/// <summary>
	/// Base interface for Sources and Destinations.
	/// </summary>
	public interface ISourceDestinationBase : IOriginator
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
		/// Specifies which media types to use for this source/destination.
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

	public static class SourceDestinationBaseExtensions
	{
		/// <summary>
		/// Gets the name of the source. If no name specified, returns the name of the device
		/// with the specified id.
		/// </summary>
		/// <param name="extends"></param>
		/// <returns></returns>
		public static string GetNameOrDeviceName(this ISourceDestinationBase extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.GetNameOrDeviceName(false);
		}

		/// <summary>
		/// Gets the name of the source. If no name specified, returns the name of the device
		/// with the specified id.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="combine">If true uses the combine names for the originators.</param>
		/// <returns></returns>
		public static string GetNameOrDeviceName(this ISourceDestinationBase extends, bool combine)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			string name = extends.GetName(combine);
			if (!string.IsNullOrEmpty(name))
				return name;

			IOriginator device = ServiceProvider.GetService<ICore>().Originators.GetChild(extends.Device);
			return device.GetName(combine);
		}
	}
}
