using System;
using ICD.Common.Utils;
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
		/// Returns the string representation for the routing operation.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			ReprBuilder builder = new ReprBuilder(this);

			builder.AppendProperty("Id", Id);
			builder.AppendProperty("Source", Source);
			builder.AppendProperty("Destination", Destination);
			builder.AppendProperty("LocalInput", LocalInput);
			builder.AppendProperty("LocalOutput", LocalOutput);
			builder.AppendProperty("ConnectionType", ConnectionType);
			builder.AppendProperty("RoomId", RoomId);
			builder.AppendProperty("RouteRequestFrom", RouteRequestFrom);

			return builder.ToString();
		}

		/// <summary>
		/// Returns a shorthand string representation for a local routing operation.
		/// </summary>
		/// <returns></returns>
		public string ToStringLocal()
		{
			ReprBuilder builder = new ReprBuilder(this);

			builder.AppendProperty("Source", Source);
			builder.AppendProperty("Destination", Destination);
			builder.AppendProperty("ConnectionType", ConnectionType);
			builder.AppendProperty("RoomId", RoomId);

			return builder.ToString();
		}
	}
}
