using System;
using ICD.Common.EventArguments;
using ICD.Common.Services;
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

			return string.IsNullOrEmpty(extends.Name)
					   ? ServiceProvider.GetService<ICore>().Originators.GetChild(extends.Endpoint.Device).Name
					   : extends.Name;
		}
	}
}
