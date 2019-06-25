using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services;
using ICD.Connect.API.Commands;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.RoutingGraphs;
using ICD.Connect.Routing.Utils;

namespace ICD.Connect.Routing.Mock.Switcher
{
	public sealed class MockRouteSwitcherControl : AbstractRouteSwitcherControl<IDeviceBase>
	{
		/// <summary>
		/// Called when a route changes.
		/// </summary>
		public override event EventHandler<RouteChangeEventArgs> OnRouteChange;

		/// <summary>
		/// Raised when the device starts/stops actively transmitting on an output.
		/// </summary>
		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		/// <summary>
		/// Raised when an input source status changes.
		/// </summary>
		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;

		/// <summary>
		/// Raised when the device starts/stops actively using an input, e.g. unroutes an input.
		/// </summary>
		public override event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;

		private readonly SwitcherCache m_Cache;

		private IRoutingGraph m_CachedRoutingGraph;

		/// <summary>
		/// Gets the routing graph.
		/// </summary>
		public IRoutingGraph RoutingGraph
		{
			get { return m_CachedRoutingGraph = m_CachedRoutingGraph ?? ServiceProvider.GetService<IRoutingGraph>(); }
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public MockRouteSwitcherControl(IDeviceBase parent, int id)
			: base(parent, id)
		{
			m_Cache = new SwitcherCache();
			Subscribe(m_Cache);
			AudioBreakawayEnabled = true;
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnRouteChange = null;
			OnActiveInputsChanged = null;
			OnSourceDetectionStateChange = null;
			OnActiveInputsChanged = null;

			base.DisposeFinal(disposing);

			Unsubscribe(m_Cache);
		}

		#region Methods

		protected override InputPort CreateInputPort(ConnectorInfo input)
		{
			bool supportsVideo = input.ConnectionType.HasFlag(eConnectionType.Video);
			return new InputPort
			{
				Address = input.Address,
				ConnectionType = input.ConnectionType,
				InputId = GetInputId(input),
				InputIdFeedbackSupported = true,
				VideoInputSync = supportsVideo && GetVideoInputSyncState(input),
				VideoInputSyncFeedbackSupported = supportsVideo,
			};
		}

		protected override OutputPort CreateOutputPort(ConnectorInfo output)
		{
			bool supportsVideo = output.ConnectionType.HasFlag(eConnectionType.Video);
			bool supportsAudio = output.ConnectionType.HasFlag(eConnectionType.Audio);
			return new OutputPort
			{
				Address = output.Address,
				ConnectionType = output.ConnectionType,
				OutputId = GetOutputId(output),
				OutputIdFeedbackSupport = true,
				VideoOutputSource = supportsVideo ? GetActiveSourceIdName(output, eConnectionType.Video) : null,
				VideoOutputSourceFeedbackSupport = supportsVideo,
				AudioOutputSource = supportsAudio ? GetActiveSourceIdName(output, eConnectionType.Audio) : null,
				AudioOutputSourceFeedbackSupport = supportsAudio
			};
		}

		/// <summary>
		/// Routes the input to the given output.
		/// </summary>
		/// <param name="info"></param>
		public override bool Route(RouteOperation info)
		{
			return m_Cache.SetInputForOutput(info.LocalOutput, info.LocalInput, info.ConnectionType);
		}

		/// <summary>
		/// Stops routing to the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		public override bool ClearOutput(int output, eConnectionType type)
		{
			return m_Cache.SetInputForOutput(output, null, type);
		}

		private string GetInputId(ConnectorInfo info)
		{
			return string.Format("Mock Video Input {0}", info.Address);
		}

		private bool GetVideoInputSyncState(ConnectorInfo info)
		{
			return GetSignalDetectedState(info.Address, eConnectionType.Video);
		}

		private string GetOutputId(ConnectorInfo info)
		{
			return string.Format("Mock Video Output {0}", info.Address);
		}

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override ConnectorInfo GetOutput(int address)
		{
			Connection connection = RoutingGraph.Connections.GetOutputConnection(new EndpointInfo(Parent.Id, Id, address));
			if (connection == null)
				throw new ArgumentOutOfRangeException("address");

			return new ConnectorInfo(connection.Source.Address, connection.ConnectionType);
		}

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override bool ContainsOutput(int output)
		{
			return RoutingGraph.Connections.GetOutputConnection(new EndpointInfo(Parent.Id, Id, output)) != null;
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			return RoutingGraph.Connections
			                   .GetOutputConnections(Parent.Id, Id)
			                   .Select(c => new ConnectorInfo(c.Source.Address, c.ConnectionType));
		}

		/// <summary>
		/// Gets the outputs for the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs(int input, eConnectionType type)
		{
			return m_Cache.GetOutputsForInput(input, type);
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
			return m_Cache.GetInputConnectorInfoForOutput(output, type);
		}

		/// <summary>
		/// Gets the input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override ConnectorInfo GetInput(int input)
		{
			Connection connection = RoutingGraph.Connections.GetInputConnection(new EndpointInfo(Parent.Id, Id, input));
			if (connection == null)
				throw new ArgumentOutOfRangeException("input");

			return new ConnectorInfo(connection.Destination.Address, connection.ConnectionType);
		}

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override bool ContainsInput(int input)
		{
			return RoutingGraph.Connections.GetInputConnection(new EndpointInfo(Parent.Id, Id, input)) != null;
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			return RoutingGraph.Connections
							   .GetInputConnections(Parent.Id, Id)
							   .Select(c => new ConnectorInfo(c.Destination.Address, c.ConnectionType));
		}

		/// <summary>
		/// Returns true if video is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool GetSignalDetectedState(int input, eConnectionType type)
		{
			return m_Cache.GetSourceDetectedState(input, type);
		}

		/// <summary>
		/// Sets the video detected state at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <param name="state"></param>
		[PublicAPI]
		public void SetSignalDetectedState(int input, eConnectionType type, bool state)
		{
			m_Cache.SetSourceDetectedState(input, type, state);
		}

		#endregion

		#region Cache Callbacks

		/// <summary>
		/// Subscribe to the cache events.
		/// </summary>
		/// <param name="cache"></param>
		private void Subscribe(SwitcherCache cache)
		{
			cache.OnRouteChange += CacheOnRouteChange;
			cache.OnActiveInputsChanged += CacheOnActiveInputsChanged;
			cache.OnSourceDetectionStateChange += CacheOnSourceDetectionStateChange;
			cache.OnActiveTransmissionStateChanged += CacheOnActiveTransmissionStateChanged;
		}

		/// <summary>
		/// Unsubscribe from the cache events.
		/// </summary>
		/// <param name="cache"></param>
		private void Unsubscribe(SwitcherCache cache)
		{
			cache.OnRouteChange -= CacheOnRouteChange;
			cache.OnActiveInputsChanged -= CacheOnActiveInputsChanged;
			cache.OnSourceDetectionStateChange -= CacheOnSourceDetectionStateChange;
			cache.OnActiveTransmissionStateChanged -= CacheOnActiveTransmissionStateChanged;
		}

		private void CacheOnRouteChange(object sender, RouteChangeEventArgs args)
		{
			OnRouteChange.Raise(this, new RouteChangeEventArgs(args));
			OutputPort outputPort = GetOutputPort(args.Output);
			ConnectorInfo info = GetOutput(args.Output);
			if (args.Type.HasFlag(eConnectionType.Video))
				outputPort.VideoOutputSource = GetActiveSourceIdName(info, eConnectionType.Video);
			if (args.Type.HasFlag(eConnectionType.Audio))
				outputPort.AudioOutputSource = GetActiveSourceIdName(info, eConnectionType.Audio);
		}

		private void CacheOnActiveTransmissionStateChanged(object sender, TransmissionStateEventArgs args)
		{
			OnActiveTransmissionStateChanged.Raise(this, new TransmissionStateEventArgs(args));
		}

		private void CacheOnSourceDetectionStateChange(object sender, SourceDetectionStateChangeEventArgs args)
		{
			OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(args));

			InputPort inputPort = GetInputPort(args.Input);
			ConnectorInfo info = GetInput(args.Input);
			inputPort.VideoInputSync = args.State;
		}

		private void CacheOnActiveInputsChanged(object sender, ActiveInputStateChangeEventArgs args)
		{
			OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(args));
		}

		#endregion

		#region Console

		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new GenericConsoleCommand<int, eConnectionType, bool>(
				"SetSignalDetectedState",
				"<input> <connectionType> <true/false>",
				(a, b, c) => SetSignalDetectedState(a, b, c));
			yield return new GenericConsoleCommand<int, int, eConnectionType>(
				"Route",
				"Routes the input address to the output address with the given connection type",
				(a, b, c) => this.Route(a, b, c));
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion
	}
}
