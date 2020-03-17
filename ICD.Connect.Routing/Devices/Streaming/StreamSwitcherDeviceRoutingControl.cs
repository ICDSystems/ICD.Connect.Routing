using System;
using System.Collections.Generic;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls.Streaming;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Devices.Streaming
{
	public sealed class StreamSwitcherDeviceRoutingControl : AbstractStreamRouteSwitcherControl<StreamSwitcherDevice>
	{
		#region Events

		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;
		public override event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;
		public override event EventHandler<RouteChangeEventArgs> OnRouteChange;
		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;
		public override event EventHandler<StreamUriEventArgs> OnStreamUriChanged;

		#endregion

		private StreamSwitcherDevice m_Device;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public StreamSwitcherDeviceRoutingControl(StreamSwitcherDevice parent, int id) 
			: base(parent, id)
		{
			m_Device = parent;
		}

		#region Methods

		public override bool GetSignalDetectedState(int input, eConnectionType type)
		{
			throw new NotImplementedException();
		}

		public override ConnectorInfo GetInput(int input)
		{
			throw new NotImplementedException();
		}

		public override bool ContainsInput(int input)
		{
			throw new NotImplementedException();
		}

		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			throw new NotImplementedException();
		}

		public override ConnectorInfo GetOutput(int address)
		{
			throw new NotImplementedException();
		}

		public override bool ContainsOutput(int output)
		{
			throw new NotImplementedException();
		}

		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			throw new NotImplementedException();
		}

		public override IEnumerable<ConnectorInfo> GetOutputs(int input, eConnectionType type)
		{
			throw new NotImplementedException();
		}

		public override ConnectorInfo? GetInput(int output, eConnectionType type)
		{
			throw new NotImplementedException();
		}

		public override Uri GetStreamForOutput(int output)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
