using System;
using System.Collections.Generic;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Devices;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Controls
{
	public sealed class RouteSourceControl : AbstractRouteSourceControl<IRouteSourceDevice>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public RouteSourceControl(IRouteSourceDevice parent, int id)
			: base(parent, id)
		{
			parent.OnActiveTransmissionStateChanged += ParentOnActiveTransmissionStateChanged;
		}

		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		/// <summary>
		/// Returns true if the device is actively transmitting on the given output.
		/// This is NOT the same as sending video, since some devices may send an
		/// idle signal by default.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool GetActiveTransmissionState(int output, eConnectionType type)
		{
			return Parent.GetActiveTransmissionState(output, type);
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			return Parent.GetOutputs();
		}

		private void ParentOnActiveTransmissionStateChanged(object sender, TransmissionStateEventArgs transmissionStateEventArgs)
		{
			OnActiveTransmissionStateChanged.Raise(this, new TransmissionStateEventArgs(transmissionStateEventArgs));
		}
	}
}