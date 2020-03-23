using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.Controls.Streaming;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Utils;
using ICD.Connect.Settings.Cores;

namespace ICD.Connect.Routing.Devices.Streaming
{
	public sealed class StreamSwitcherDeviceRoutingControl : AbstractRouteSwitcherControl<StreamSwitcherDevice>
	{
		#region Events

		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;
		public override event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;
		public override event EventHandler<RouteChangeEventArgs> OnRouteChange;
		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		#endregion

		private readonly SafeCriticalSection m_ConnectorsSection;
		private readonly BiDictionary<int, IStreamRouteSourceControl> m_InputEndpoints;
		private readonly BiDictionary<int, IStreamRouteDestinationControl> m_OutputEndpoints;
		private readonly BiDictionary<IRouteControl, Connection> m_ControlConnections;

		private readonly SwitcherCache m_SwitcherCache;

		private ICore m_CachedCore;

		/// <summary>
		/// Gets the core.
		/// </summary>
		public ICore Core { get { return m_CachedCore = m_CachedCore ?? ServiceProvider.GetService<ICore>(); } }


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public StreamSwitcherDeviceRoutingControl(StreamSwitcherDevice parent, int id) 
			: base(parent, id)
		{
			m_ConnectorsSection = new SafeCriticalSection();

			m_InputEndpoints = new BiDictionary<int, IStreamRouteSourceControl>();
			m_OutputEndpoints = new BiDictionary<int, IStreamRouteDestinationControl>();
			m_ControlConnections = new BiDictionary<IRouteControl, Connection>();

			m_SwitcherCache = new SwitcherCache();
			Subscribe(m_SwitcherCache);
		}

		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			Unsubscribe(m_SwitcherCache);
		}

		#region Methods

		public override bool Route(RouteOperation info)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			eConnectionType type = info.ConnectionType;
			int input = info.LocalInput;
			int output = info.LocalOutput;

			if (!ContainsInput(input))
				throw new InvalidOperationException("No input at address");

			if (!ContainsOutput(output))
				throw new InvalidOperationException("No output at address");

			if (type == eConnectionType.None)
				throw new ArgumentException();

			if (EnumUtils.ExcludeFlags(type, eConnectionType.Audio | eConnectionType.Video) != eConnectionType.None)
				throw new ArgumentException();

			// Set the stream input on the destination at the output address to match the source on the input address
			Uri inputStream = GetStreamForInput(input);
			return SetStreamForOutput(output, inputStream);
		}

		public override bool ClearOutput(int output, eConnectionType type)
		{
			if (!ContainsOutput(output))
				throw new InvalidOperationException("No output at address");

			if (type == eConnectionType.None)
				throw new ArgumentException();

			if (EnumUtils.ExcludeFlags(type, eConnectionType.Audio | eConnectionType.Video) != eConnectionType.None)
				throw new ArgumentException();

			// Set the stream input on the destination at the output address to null
			return SetStreamForOutput(output, null);
		}

		/// <summary>
		/// Returns true if a signal is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool GetSignalDetectedState(int input, eConnectionType type)
		{
			return m_SwitcherCache.GetSourceDetectedState(input, type);
		}

		/// <summary>
		/// Gets the input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override ConnectorInfo GetInput(int input)
		{
			if (!ContainsInput(input))
				throw new ArgumentOutOfRangeException("input");

			return new ConnectorInfo(input, eConnectionType.Audio | eConnectionType.Video);
		}

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override bool ContainsInput(int input)
		{
			return m_ConnectorsSection.Execute(() => m_InputEndpoints.ContainsKey(input));
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			m_ConnectorsSection.Enter();

			try
			{
				return m_InputEndpoints.Keys
				                       .Select(e => new ConnectorInfo(e, eConnectionType.Audio | eConnectionType.Video));
			}
			finally
			{
				m_ConnectorsSection.Leave();
			}
		}

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override ConnectorInfo GetOutput(int address)
		{
			if (!ContainsOutput(address))
				throw new ArgumentOutOfRangeException("address");

			return new ConnectorInfo(address, eConnectionType.Audio | eConnectionType.Video);
		}

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override bool ContainsOutput(int output)
		{
			return m_ConnectorsSection.Execute(() => m_OutputEndpoints.ContainsKey(output));
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			m_ConnectorsSection.Enter();

			try
			{
				return m_OutputEndpoints.Keys
				                        .Select(e =>
					                                new ConnectorInfo(e, eConnectionType.Audio | eConnectionType.Video));
			}
			finally
			{
				m_ConnectorsSection.Leave();
			}
		}

		/// <summary>
		/// Gets the outputs for the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs(int input, eConnectionType type)
		{
			return m_SwitcherCache.GetOutputsForInput(input, type);
		}

		/// <summary>
		/// Gets the input routed to the given output matching the given type.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">Type has multiple flags.</exception>
		public override ConnectorInfo? GetInput(int output, eConnectionType type)
		{
			return m_SwitcherCache.GetInputConnectorInfoForOutput(output, type);
		}

		#endregion

		#region Endpoint Caching

		/// <summary>
		/// Clears the cached endpoints.
		/// </summary>
		public void ClearEndpointCache()
		{
			m_ConnectorsSection.Enter();

			try
			{
				UnsubscribeSourceControls();
				UnsubscribeDestinationControls();

				m_InputEndpoints.Clear();
				m_OutputEndpoints.Clear();
				m_ControlConnections.Clear();

				// Finally clear routing states
				m_SwitcherCache.Clear();
			}
			finally
			{
				m_ConnectorsSection.Leave();
			}
		}

		/// <summary>
		/// Builds the endpoint cache.
		/// </summary>
		public void BuildEndpointCache()
		{
			m_ConnectorsSection.Enter();

			try
			{
				ClearEndpointCache();

				BuildInputEndpointCache();
				BuildOutputEndpointCache();

				SubscribeSourceControls();
				SubscribeDestinationControls();

				UpdateRouting();
			}
			finally
			{
				m_ConnectorsSection.Leave();
			}
		}

		/// <summary>
		/// Loops over the input connections to this switcher and builds a table of endpoint info.
		/// </summary>
		private void BuildInputEndpointCache()
		{
			m_ConnectorsSection.Enter();

			try
			{
				foreach (Connection inputConnection in GetInputConnections())
				{
					EndpointInfo sourceEndpoint = inputConnection.Source;
					IStreamRouteSourceControl control = GetSourceControl(inputConnection);

					if (control == null)
					{
						Log(eSeverity.Error, "Unable to support connection from {0}", sourceEndpoint);
						continue;
					}

					m_InputEndpoints.Add(inputConnection.Destination.Address, control);
					m_ControlConnections.Add(control, inputConnection);
				}
			}
			finally
			{
				m_ConnectorsSection.Leave();
			}

		}

		/// <summary>
		/// Loops over the input connections to this switcher and builds a table of endpoint info.
		/// </summary>
		private void BuildOutputEndpointCache()
		{
			m_ConnectorsSection.Enter();

			try
			{
				foreach (Connection outputConnection in GetOutputConnections())
				{
					EndpointInfo destinationEndpoint = outputConnection.Destination;
					IStreamRouteDestinationControl control = GetDestinationControl(outputConnection);

					if (control == null)
					{
						Log(eSeverity.Error, "Unable to support connection to {0}", destinationEndpoint);
						continue;
					}

					m_OutputEndpoints.Add(outputConnection.Source.Address, control);
					m_ControlConnections.Add(control, outputConnection);
				}
			}
			finally
			{
				m_ConnectorsSection.Leave();
			}
		}

		/// <summary>
		/// Hack - Getting connections direct from the core instead of the routing graph
		/// because we are still loading settings.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<Connection> GetInputConnections()
		{
			return Core.Originators.GetChildren<Connection>().Where(c => c.Destination.Device == Parent.Id);
		}

		/// <summary>
		/// Hack - Getting connections direct from the core instead of the routing graph
		/// because we are still loading settings.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<Connection> GetOutputConnections()
		{
			return Core.Originators.GetChildren<Connection>().Where(c => c.Source.Device == Parent.Id);
		}

		/// <summary>
		/// Hack - Getting controls direct from the core instead of the routing graph
		/// because we are still loading settings.
		/// </summary>
		/// <returns></returns>
		private IStreamRouteSourceControl GetSourceControl(Connection inputConnection)
		{
			return Core.Originators
					   .GetChild<IDeviceBase>(inputConnection.Source.Device)
					   .Controls.GetControl<IStreamRouteSourceControl>(inputConnection.Source.Control);
		}

		/// <summary>
		/// Hack - Getting controls direct from the core instead of the routing graph
		/// because we are still loading settings.
		/// </summary>
		/// <returns></returns>
		private IStreamRouteDestinationControl GetDestinationControl(Connection outputConnection)
		{
			return Core.Originators
					   .GetChild<IDeviceBase>(outputConnection.Destination.Device)
					   .Controls.GetControl<IStreamRouteDestinationControl>(outputConnection.Destination.Control);
		}

		/// <summary>
		/// Updates the routing states for the switchers.
		/// </summary>
		private void UpdateRouting()
		{
			foreach (ConnectorInfo info in GetOutputs())
			{
				Uri outputStream = GetStreamForOutput(info.Address);

				// Find the matching input stream (the output stream on the destinations, this can be null)
				int? input =
					outputStream == null
					? null
					: GetInputs().Where(i => GetStreamForInput(i.Address) == outputStream)
					             .Select(i => (int?)i.Address)
					             .FirstOrDefault();

				// Set input for output on the switcher cache (Audio | Video)
				m_SwitcherCache.SetInputForOutput(info.Address, input, eConnectionType.Audio | eConnectionType.Video);
			}
		}

		/// <summary>
		/// Gets the URI of the stream associated with the output.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		[CanBeNull]
		private Uri GetStreamForOutput(int output)
		{
			IStreamRouteDestinationControl control;
			if (!m_OutputEndpoints.TryGetValue(output, out control))
				return null;

			Connection connection = m_ControlConnections.GetValue(control);

			// Get the input stream on the destination
			return control.GetStreamForInput(connection.Destination.Address);
		}

		/// <summary>
		/// Gets the URI of the stream associated with the input.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		[CanBeNull]
		private Uri GetStreamForInput(int input)
		{
			IStreamRouteSourceControl control;
			if (!m_InputEndpoints.TryGetValue(input, out control))
				return null;

			Connection connection = m_ControlConnections.GetValue(control);

			// Get the output stream on the source
			return control.GetStreamForOutput(connection.Source.Address);
		}

		/// <summary>
		/// Sets the stream associated for the output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="stream"></param>
		/// <returns></returns>
		private bool SetStreamForOutput(int output, Uri stream)
		{
			// Lookup the destination control, set the stream on the relevant input
			IStreamRouteDestinationControl control = m_OutputEndpoints.GetValue(output);
			var input = m_ControlConnections.GetValue(control).Destination.Address;
			return control.SetStreamForInput(input, stream);
		}

		#endregion

		#region Source Control Callbacks

		/// <summary>
		/// Subscribe to the connected control events.
		/// </summary>
		private void SubscribeSourceControls()
		{
			foreach (IStreamRouteSourceControl control in GetCachedSourceControls())
				Subscribe(control);
		}

		/// <summary>
		/// Unsubscribe from the connected control events.
		/// </summary>
		private void UnsubscribeSourceControls()
		{
			foreach (IStreamRouteSourceControl control in GetCachedSourceControls())
				Unsubscribe(control);
		}

		/// <summary>
		/// Gets all of the input endpoints that have been cached.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IStreamRouteSourceControl> GetCachedSourceControls()
		{
			m_ConnectorsSection.Enter();

			try
			{
				return m_InputEndpoints.Values.ToArray();
			}
			finally
			{
				m_ConnectorsSection.Leave();
			}
		}

		/// <summary>
		/// Subscribe to the control events.
		/// </summary>
		/// <param name="control"></param>
		private void Subscribe(IStreamRouteSourceControl control)
		{
			control.OnOutputStreamUriChanged += ControlOnOutputStreamUriChanged;
		}

		/// <summary>
		/// Unsubscribe from the control events.
		/// </summary>
		/// <param name="control"></param>
		private void Unsubscribe(IStreamRouteSourceControl control)
		{
			control.OnOutputStreamUriChanged -= ControlOnOutputStreamUriChanged;
		}

		private void ControlOnOutputStreamUriChanged(object sender, StreamUriEventArgs e)
		{
			Connection inputConnection = m_ControlConnections.GetValue((IStreamRouteSourceControl)sender);
			int input = inputConnection.Destination.Address;

			foreach (ConnectorInfo info in GetOutputs(input, eConnectionType.Audio | eConnectionType.Video))
				SetStreamForOutput(info.Address, e.StreamUri);
		}

		#endregion

		#region Destination Control Callbacks

		/// <summary>
		/// Subscribe to the connected control events.
		/// </summary>
		private void SubscribeDestinationControls()
		{
			foreach (IStreamRouteDestinationControl control in GetCachedDestinationControls())
				Subscribe(control);
		}

		/// <summary>
		/// Unsubscribe from the connected control events.
		/// </summary>
		private void UnsubscribeDestinationControls()
		{
			foreach (IStreamRouteDestinationControl control in GetCachedDestinationControls())
				Unsubscribe(control);
		}

		/// <summary>
		/// Gets all of the output endpoints that have been cached.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IStreamRouteDestinationControl> GetCachedDestinationControls()
		{
			m_ConnectorsSection.Enter();

			try
			{
				return m_OutputEndpoints.Values.ToArray();
			}
			finally
			{
				m_ConnectorsSection.Leave();
			}
		}

		/// <summary>
		/// Subscribe to the control events.
		/// </summary>
		/// <param name="control"></param>
		private void Subscribe(IStreamRouteDestinationControl control)
		{
			control.OnInputStreamUriChanged += ControlOnInputStreamUriChanged;
		}

		/// <summary>
		/// Unsubscribe from the control events.
		/// </summary>
		/// <param name="control"></param>
		private void Unsubscribe(IStreamRouteDestinationControl control)
		{
			control.OnInputStreamUriChanged -= ControlOnInputStreamUriChanged;
		}

		private void ControlOnInputStreamUriChanged(object sender, StreamUriEventArgs e)
		{
			UpdateRouting();
		}

		#endregion

		#region Switcher Cache Callbacks

		private void Subscribe(SwitcherCache switcherCache)
		{
			switcherCache.OnActiveInputsChanged += SwitcherCacheOnActiveInputsChanged;
			switcherCache.OnActiveTransmissionStateChanged += SwitcherCacheOnActiveTransmissionStateChanged;
			switcherCache.OnRouteChange += SwitcherCacheOnRouteChange;
			switcherCache.OnSourceDetectionStateChange += SwitcherCacheOnSourceDetectionStateChange;
		}

		private void Unsubscribe(SwitcherCache switcherCache)
		{
			switcherCache.OnActiveInputsChanged -= SwitcherCacheOnActiveInputsChanged;
			switcherCache.OnActiveTransmissionStateChanged -= SwitcherCacheOnActiveTransmissionStateChanged;
			switcherCache.OnRouteChange -= SwitcherCacheOnRouteChange;
			switcherCache.OnSourceDetectionStateChange -= SwitcherCacheOnSourceDetectionStateChange;
		}

		private void SwitcherCacheOnActiveInputsChanged(object sender, ActiveInputStateChangeEventArgs e)
		{
			OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(e.Input, e.Type, e.Active));
		}

		private void SwitcherCacheOnActiveTransmissionStateChanged(object sender, TransmissionStateEventArgs e)
		{
			OnActiveTransmissionStateChanged.Raise(this, new TransmissionStateEventArgs(e.Output, e.Type, e.State));
		}

		private void SwitcherCacheOnRouteChange(object sender, RouteChangeEventArgs e)
		{
			OnRouteChange.Raise(this, new RouteChangeEventArgs(e.OldInput, e.NewInput, e.Output, e.Type));
		}

		private void SwitcherCacheOnSourceDetectionStateChange(object sender, SourceDetectionStateChangeEventArgs e)
		{
			OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(e.Input, e.Type, e.State));
		}

		#endregion
	}
}
