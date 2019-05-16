using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Utils;

namespace ICD.Connect.Routing.SPlus
{
	public sealed class SPlusSwitcherControl : AbstractRouteSwitcherControl<SPlusSwitcher>
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

		#region S+ Delegates

		public delegate bool SPlusGetSignalDetectedState(int input, eConnectionType type);

		public delegate IEnumerable<ConnectorInfo> SPlusGetInputs();

		public delegate IEnumerable<ConnectorInfo> SPlusGetOutputs();

		public delegate bool SPlusRoute(RouteOperation info);

		public delegate bool SPlusClearOutput(int output, eConnectionType type);

		#endregion

		#region S+ Events

		[PublicAPI("S+")]
		public SPlusGetSignalDetectedState GetSignalDetectedStateCallback { get; set; }

		[PublicAPI("S+")]
		public SPlusGetInputs GetInputsCallback { get; set; }

		[PublicAPI("S+")]
		public SPlusGetOutputs GetOutputsCallback { get; set; }

		[PublicAPI("S+")]
		public SPlusRoute RouteCallback { get; set; }

		[PublicAPI("S+")]
		public SPlusClearOutput ClearOutputCallback { get; set; }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public SPlusSwitcherControl(SPlusSwitcher parent, int id)
			: base(parent, id)
		{
			m_Cache = new SwitcherCache();
			Subscribe(m_Cache);
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnRouteChange = null;
			OnActiveTransmissionStateChanged = null;
			OnSourceDetectionStateChange = null;
			OnActiveInputsChanged = null;

			GetSignalDetectedStateCallback = null;
			GetInputsCallback = null;
			GetOutputsCallback = null;
			RouteCallback = null;
			ClearOutputCallback = null;

			base.DisposeFinal(disposing);

			Unsubscribe(m_Cache);
		}

		#region Methods

		/// <summary>
		/// Called to inform the system of a source detection change.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		[PublicAPI("S+")]
		public void UpdateSourceDetection(int input, eConnectionType type)
		{
			bool state = GetSignalDetectedStateCallback != null && GetSignalDetectedStateCallback(input, type);
			m_Cache.SetSourceDetectedState(input, type, state);
		}

		/// <summary>
		/// Called to inform the system of a switcher routing change.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="input"></param>
		/// <param name="type"></param>
		[PublicAPI("S+")]
		public void SetInputForOutput(int output, int input, eConnectionType type)
		{
			m_Cache.SetInputForOutput(output, input, type);
		}

		#endregion

		#region IRouteSwitcherControl

		protected override void InitializeInputPorts()
		{
			foreach (ConnectorInfo input in GetInputs())
			{
				bool supportsVideo = input.ConnectionType.HasFlag(eConnectionType.Video);
				inputPorts.Add(input, new InputPort
				{
					ConnectionType = input.ConnectionType,
					InputId = GetInputId(input),
					InputIdFeedbackSupported = true,
					VideoInputSync = supportsVideo && GetVideoInputSyncState(input),
					VideoInputSyncFeedbackSupported = supportsVideo,
				});
			}
		}

		protected override void InitializeOutputPorts()
		{
			foreach (ConnectorInfo output in GetOutputs())
			{
				bool supportsVideo = output.ConnectionType.HasFlag(eConnectionType.Video);
				bool supportsAudio = output.ConnectionType.HasFlag(eConnectionType.Audio);
				outputPorts.Add(output, new OutputPort
				{
					ConnectionType = output.ConnectionType,
					OutputId = GetOutputId(output),
					OutputIdFeedbackSupport = true,
					VideoOutputSource = supportsVideo ? GetActiveSourceIdName(output, eConnectionType.Video) : null,
					VideoOutputSourceFeedbackSupport = supportsVideo,
					AudioOutputSource = supportsAudio ? GetActiveSourceIdName(output, eConnectionType.Audio) : null,
					AudioOutputSourceFeedbackSupport = supportsAudio
				});
			}
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
			return string.Format("s+ Video Input {0}", info.Address);
		}

		private bool GetVideoInputSyncState(ConnectorInfo info)
		{
			return GetSignalDetectedState(info.Address, eConnectionType.Video);
		}

		private string GetOutputId(ConnectorInfo info)
		{
			return string.Format("S+ Video Output {0}", info.Address);
		}

		private string GetActiveSourceIdName(ConnectorInfo info, eConnectionType type)
		{
			if (!EnumUtils.HasSingleFlag(type))
				throw new InvalidOperationException("Cannot get active source for multiple type flags");

			var activeInput = m_Cache.GetInputConnectorInfoForOutput(info.Address, type);
			return activeInput != null
					   ? string.Format("{0} {1}", inputPorts[activeInput.Value].InputId ?? string.Empty,
									   inputPorts[activeInput.Value].InputName ?? string.Empty)
					   : null;
		}

		/// <summary>
		/// Gets the input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override ConnectorInfo GetInput(int input)
		{
			return GetInputs().First(c => c.Address == input);
		}

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override bool ContainsInput(int input)
		{
			return GetInputs().Any(c => c.Address == input);
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			return GetInputsCallback == null
				       ? Enumerable.Empty<ConnectorInfo>()
				       : GetInputsCallback();
		}

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override ConnectorInfo GetOutput(int address)
		{
			return GetOutputs().First(c => c.Address == address);
		}

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override bool ContainsOutput(int output)
		{
			return GetOutputs().Any(c => c.Address == output);
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			return GetOutputsCallback == null
				       ? Enumerable.Empty<ConnectorInfo>()
				       : GetOutputsCallback();
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
		/// Returns true if a signal is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool GetSignalDetectedState(int input, eConnectionType type)
		{
			return m_Cache.GetSourceDetectedState(input, type);
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
			KeyValuePair<ConnectorInfo, OutputPort> outputPort = outputPorts.FirstOrDefault(kvp => kvp.Key.Address == args.Output);
			if(outputPort.Value == null)
				return;
			if (args.Type.HasFlag(eConnectionType.Video))
				outputPort.Value.VideoOutputSource = GetActiveSourceIdName(outputPort.Key, eConnectionType.Video);
			if (args.Type.HasFlag(eConnectionType.Audio))
				outputPort.Value.AudioOutputSource = GetActiveSourceIdName(outputPort.Key, eConnectionType.Audio);
		}

		private void CacheOnActiveTransmissionStateChanged(object sender, TransmissionStateEventArgs args)
		{
			OnActiveTransmissionStateChanged.Raise(this, new TransmissionStateEventArgs(args));
		}

		private void CacheOnSourceDetectionStateChange(object sender, SourceDetectionStateChangeEventArgs args)
		{
			OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(args));
			KeyValuePair<ConnectorInfo, InputPort> inputPort = inputPorts.FirstOrDefault(kvp => kvp.Key.Address == args.Input);
			if (inputPort.Value != null && args.Type.HasFlag(eConnectionType.Video))
				inputPort.Value.VideoInputSync = GetVideoInputSyncState(inputPort.Key);
		}

		private void CacheOnActiveInputsChanged(object sender, ActiveInputStateChangeEventArgs args)
		{
			OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(args));
		}

		#endregion
	}
}
