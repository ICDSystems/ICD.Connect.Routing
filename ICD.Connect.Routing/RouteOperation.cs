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
		/// When configuring a single switcher, describes the id of the switcher device.
		/// </summary>
		public int LocalDevice { get; set; }

		/// <summary>
		/// When configuring a single switcher, describes the id of the switcher control.
		/// </summary>
		public int LocalControl { get; set; }

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
		public HostSessionInfo RouteRequestFrom { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public RouteOperation()
		{
			Id = Guid.NewGuid();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="op"></param>
		public RouteOperation(RouteOperation op)
			: this()
		{
			Source = op.Source;
			Destination = op.Destination;
			LocalInput = op.LocalInput;
			LocalOutput = op.LocalOutput;
			LocalDevice = op.LocalDevice;
			LocalControl = op.LocalControl;
			ConnectionType = op.ConnectionType;
			RoomId = op.RoomId;
			RouteRequestFrom = op.RouteRequestFrom;
		}

		/// <summary>
		/// Returns the string representation for the routing operation.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			ReprBuilder builder = new ReprBuilder(this);

			builder.AppendProperty("Id", Id);

			if (Source != default(EndpointInfo))
				builder.AppendProperty("Source", Source);

			if (Destination != default(EndpointInfo))
				builder.AppendProperty("Destination", Destination);

			if (LocalInput != 0)
				builder.AppendProperty("LocalInput", LocalInput);

			if (LocalOutput != 0)
				builder.AppendProperty("LocalOutput", LocalOutput);

			if (LocalDevice != 0)
				builder.AppendProperty("LocalDevice", LocalDevice);

			if (LocalControl != 0)
				builder.AppendProperty("LocalControl", LocalControl);

			// If connection type is "None" then we probably want to know
			builder.AppendProperty("ConnectionType", ConnectionType);

			if (RoomId != 0)
				builder.AppendProperty("RoomId", RoomId);

			if (RouteRequestFrom != default(HostSessionInfo))
				builder.AppendProperty("RouteRequestFrom", RouteRequestFrom);

			return builder.ToString();
		}
	}
}
