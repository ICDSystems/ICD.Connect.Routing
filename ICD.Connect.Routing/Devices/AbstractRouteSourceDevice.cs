using System;
using System.Collections.Generic;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Devices
{
	public abstract class AbstractRouteSourceDevice<TSettings> : AbstractDevice<TSettings>, IRouteSourceDevice
		where TSettings : IDeviceSettings, new()
	{
		public event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		protected AbstractRouteSourceDevice()
		{
			Controls.Add(new RouteSourceControl(this, 0));
		}

		#region Methods

		public abstract IEnumerable<ConnectorInfo> GetOutputs();
		public abstract bool GetActiveTransmissionState(int output, eConnectionType type);

		protected void RaiseOnActiveTransmissionStateChanged(int output, eConnectionType type, bool state)
		{
			OnActiveTransmissionStateChanged.Raise(this, new TransmissionStateEventArgs(output, type, state));
		}

		#endregion

	}
}