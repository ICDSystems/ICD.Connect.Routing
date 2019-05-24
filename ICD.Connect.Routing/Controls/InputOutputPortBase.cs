using System;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Telemetry;

namespace ICD.Connect.Routing.Controls
{
	public abstract class InputOutputPortBase : ITelemetryProvider
	{
		public event EventHandler OnRequestTelemetryRebuild;

		/// <summary>
		/// Gets the port connection type.
		/// </summary>
		public eConnectionType ConnectionType { get; set; }

		/// <summary>
		/// Gets the address for the port.
		/// </summary>
		public int Address { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		protected InputOutputPortBase()
			: this(default(ConnectorInfo))
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="connector"></param>
		protected InputOutputPortBase(ConnectorInfo connector)
		{
			ConnectionType = connector.ConnectionType;
			Address = connector.Address;
		}
	}
}