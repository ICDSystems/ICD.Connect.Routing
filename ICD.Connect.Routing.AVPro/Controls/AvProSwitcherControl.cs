using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.AVPro.Devices.Switchers;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Utils;

namespace ICD.Connect.Routing.AVPro.Controls
{
	public sealed class AvProSwitcherControl : AbstractRouteSwitcherControl<IAvProSwitcherDevice>, IAvProSwitcherControl
	{
		/// <summary>
		/// Raised when an input source status changes.
		/// </summary>
		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;

		/// <summary>
		/// Raised when the device starts/stops actively using an input, e.g. unroutes an input.
		/// </summary>
		public override event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;

		/// <summary>
		/// Raised when the device starts/stops actively transmitting on an output.
		/// </summary>
		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		/// <summary>
		/// Called when a route changes.
		/// </summary>
		public override event EventHandler<RouteChangeEventArgs> OnRouteChange;

		private readonly SwitcherCache m_Cache;
		private readonly Dictionary<int, bool> m_OutputTransmission;
		private readonly SafeCriticalSection m_OutputTransmissionSection;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public AvProSwitcherControl(IAvProSwitcherDevice parent, int id)
			: base(parent, id)
		{
			m_Cache = new SwitcherCache();
			m_OutputTransmission = new Dictionary<int, bool>();
			m_OutputTransmissionSection = new SafeCriticalSection();

			Subscribe(m_Cache);
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnSourceDetectionStateChange = null;
			OnActiveInputsChanged = null;
			OnActiveTransmissionStateChanged = null;
			OnRouteChange = null;

			base.DisposeFinal(disposing);

			Unsubscribe(m_Cache);
		}

		#region Methods

		/// <summary>
		/// Returns true if a signal is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool GetSignalDetectedState(int input, eConnectionType type)
		{
			if (!ContainsInput(input))
				throw new ArgumentOutOfRangeException("input");

			return m_Cache.GetSourceDetectedState(input, type);
		}

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
			if (!ContainsOutput(output))
				throw new ArgumentOutOfRangeException("output");

			return m_OutputTransmissionSection.Execute(() => m_OutputTransmission.GetDefault(output));
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
			return input >= 1 && input <= Parent.NumberOfInputs;
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			return Enumerable.Range(1, Parent.NumberOfInputs)
			                 .Select(input => GetInput(input));
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
			return output >= 1 && output <= Parent.NumberOfOutputs;
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			return Enumerable.Range(1, Parent.NumberOfOutputs)
							 .Select(output => GetOutput(output));
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

		protected override InputPort CreateInputPort(ConnectorInfo input)
		{
			bool supportsVideo = input.ConnectionType.HasFlag(eConnectionType.Video);
			return new InputPort
			{
				Address = input.Address,
				ConnectionType = input.ConnectionType,
				InputId = input.Address.ToString(),
				InputIdFeedbackSupported = true,
				VideoInputSync =
					supportsVideo &&
					GetSignalDetectedState(input.Address, eConnectionType.Video),
				VideoInputSyncFeedbackSupported =
					supportsVideo
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
				OutputId = output.Address.ToString(),
				OutputIdFeedbackSupport = true,
				VideoOutputSource =
					supportsVideo
						? GetActiveSourceIdName(output, eConnectionType.Video)
						: null,
				VideoOutputSourceFeedbackSupport = supportsVideo,
				AudioOutputSource =
					supportsAudio
						? GetActiveSourceIdName(output, eConnectionType.Audio)
						: null,
				AudioOutputSourceFeedbackSupport = supportsAudio
			};
		}

		/// <summary>
		/// Performs the given RouteOperation on this switcher.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		public override bool Route(RouteOperation info)
		{
			int localInput = info.LocalInput;
			int localOutput = info.LocalOutput;

			if (localInput < 1 || localInput > Parent.NumberOfInputs)
				throw new ArgumentOutOfRangeException("info", string.Format("Input must be between 1 and {0}", Parent.NumberOfInputs));

			if (localOutput < 1 || localOutput > Parent.NumberOfOutputs)
				throw new ArgumentOutOfRangeException("info", string.Format("Output must be between 1 and {0}", Parent.NumberOfOutputs));

			Parent.SendCommand("SET OUT{0} VS IN{1}", localOutput, localInput);
			Parent.SendCommand("SET OUT{0} STREAM ON", localOutput);

			return true;
		}

		/// <summary>
		/// Removes the given connection type from the output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool ClearOutput(int output, eConnectionType type)
		{
			if (!ContainsOutput(output))
				throw new ArgumentOutOfRangeException("output");

			Parent.SendCommand("SET OUT{0} STREAM OFF", output);
			return true;
		}

		#endregion

		#region Parent Callbacks

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected override void Subscribe(IAvProSwitcherDevice parent)
		{
			base.Subscribe(parent);

			parent.OnInitializedChanged += ParentOnInitializedChanged;
			parent.OnResponseReceived += ParentOnResponseReceived;
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected override void Unsubscribe(IAvProSwitcherDevice parent)
		{
			base.Unsubscribe(parent);

			parent.OnInitializedChanged -= ParentOnInitializedChanged;
			parent.OnResponseReceived -= ParentOnResponseReceived;
		}

		/// <summary>
		/// Raised when the parent device becomes initialized or deinitialized.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void ParentOnInitializedChanged(object sender, BoolEventArgs args)
		{
			if (args.Data)
				Initialize();
		}

		/// <summary>
		/// Raised when the parent device receives a response from the serial port.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void ParentOnResponseReceived(object sender, StringEventArgs args)
		{
			ParseResponse(args.Data);
		}

		private void ParseResponse(string data)
		{
			// Routing
			Match match = Regex.Match(data, @"OUT(?'output'\d+) VS IN(?'input'\d+)");
			if (match.Success)
			{
				int output = int.Parse(match.Groups["output"].Value);
				int input = int.Parse(match.Groups["input"].Value);

				m_Cache.SetInputForOutput(output, input, eConnectionType.Audio | eConnectionType.Video);
				return;
			}

			// Output enabled
			match = Regex.Match(data, @"OUT(?'output'\d+) STREAM (?'enabled'ON|OFF)");
			if (match.Success)
			{
				int output = int.Parse(match.Groups["output"].Value);
				bool enabled = match.Groups["enabled"].Value == "ON";

				m_OutputTransmissionSection.Enter();

				try
				{
					if (enabled == m_OutputTransmission.GetDefault(output))
						return;

					m_OutputTransmission[output] = enabled;
				}
				finally
				{
					m_OutputTransmissionSection.Leave();
				}

				OnActiveTransmissionStateChanged.Raise(this,
				                                       new TransmissionStateEventArgs(output,
				                                                                      eConnectionType.Audio |
				                                                                      eConnectionType.Video, enabled));
				return;
			}

			// Source detected
			match = Regex.Match(data, @"IN(?'input'\d+) SIG STA (?'detected'0|1)");
			if (match.Success)
			{
				int input = int.Parse(match.Groups["input"].Value);
				bool detected = match.Groups["detected"].Value == "1";

				m_Cache.SetSourceDetectedState(input, eConnectionType.Audio | eConnectionType.Video, detected);
				return;
			}
		}

		private void Initialize()
		{
			Parent.SendCommand("GET OUT0 VS"); // List routing
			Parent.SendCommand("GET OUT0 STREAM"); // List output enabled
			Parent.SendCommand("GET IN0 SIG STA"); // List source detected
		}

		#endregion

		#region Cache Callbacks

		/// <summary>
		/// Subscribe to the cache events.
		/// </summary>
		/// <param name="cache"></param>
		private void Subscribe(SwitcherCache cache)
		{
			cache.OnSourceDetectionStateChange += CacheOnSourceDetectionStateChange;
			cache.OnRouteChange += CacheOnRouteChange;
			cache.OnActiveInputsChanged += CacheOnActiveInputsChanged;
		}

		/// <summary>
		/// Unsubscribe from the cache events.
		/// </summary>
		/// <param name="cache"></param>
		private void Unsubscribe(SwitcherCache cache)
		{
			cache.OnSourceDetectionStateChange -= CacheOnSourceDetectionStateChange;
			cache.OnRouteChange -= CacheOnRouteChange;
			cache.OnActiveInputsChanged -= CacheOnActiveInputsChanged;
		}

		private void CacheOnRouteChange(object sender, RouteChangeEventArgs args)
		{
			OnRouteChange.Raise(this, new RouteChangeEventArgs(args));
		}

		private void CacheOnSourceDetectionStateChange(object sender, SourceDetectionStateChangeEventArgs args)
		{
			OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(args));
		}

		private void CacheOnActiveInputsChanged(object sender, ActiveInputStateChangeEventArgs args)
		{
			OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(args));
		}

		#endregion
	}
}