using System;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Endpoints;

namespace ICD.Connect.Routing
{
	/// <summary>
	/// RouteOperation provides a number of optional fields for describing 
	/// </summary>
	public sealed class RouteOperation
	{
		/// <summary>
		/// Unique identifier for the operation
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// The source to be routed to the destination.
		/// </summary>
		public EndpointInfo Source { get; set; }

		/// <summary>
		/// The destination that is being routed to.
		/// </summary>
		public EndpointInfo Destination { get; set; }

		/// <summary>
		/// When configuring a single switcher, describes the input address on the switcher.
		/// </summary>
		public int LocalInput { get; set; }

		/// <summary>
		/// When configuring a single switcher, describes the output address on the switcher.
		/// </summary>
		public int LocalOutput { get; set; }

		/// <summary>
		/// The media types that are being routed.
		/// </summary>
		public eConnectionType ConnectionType { get; set; }

		/// <summary>
		/// The room responsible for the routing operation.
		/// </summary>
		public int RoomId { get; set; }

		/// <summary>
		/// Used with remote routing 
		/// </summary>
		public HostInfo RouteRequestFrom { get; set; }

		/// <summary>
		/// Returns a shorthand string representation for a local routing operation.
		/// </summary>
		/// <returns></returns>
		public string ToStringLocal()
		{
			return string.Format("{0}(Source={1}, Destination={2}, Type={3}, Room={4})", GetType().Name, Source, Destination,
			                     ConnectionType, RoomId);
		}
	}
}
