using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Attributes;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Proxies;

namespace ICD.Connect.Routing.Controls
{
	/// <summary>
	/// A source control has output connectors.
	/// </summary>
	[ApiClass(typeof(ProxyRouteSourceControl), typeof(IRouteControl))]
	public interface IRouteSourceControl : IRouteControl
	{
		/// <summary>
		/// Raised when the device starts/stops actively transmitting on an output.
		/// </summary>
		event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		/// <summary>
		/// Returns true if the device is actively transmitting on the given output.
		/// This is NOT the same as sending video, since some devices may send an
		/// idle signal by default.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		bool GetActiveTransmissionState(int output, eConnectionType type);

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		IEnumerable<ConnectorInfo> GetOutputs();
	}

	public static class RouteSourceControlExtensions
	{
		/// <summary>
		/// Gets the endpoint info for the given output.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="output"></param>
		/// <returns></returns>
		public static EndpointInfo GetOutputEndpointInfo(this IRouteSourceControl extends, int output)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return new EndpointInfo(extends.Parent.Id, extends.Id, output);
		}

		/// <summary>
		/// Returns the outputs that have the given type flags.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static IEnumerable<ConnectorInfo> GetOutputs(this IRouteSourceControl extends, eConnectionType type)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.GetOutputs().Where(o => o.ConnectionType.HasFlags(type));
		}
	}
}
