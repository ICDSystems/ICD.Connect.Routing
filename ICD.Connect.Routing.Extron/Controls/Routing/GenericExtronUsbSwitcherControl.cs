using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Extron.Devices.Switchers;
using ICD.Connect.Routing.Utils;

namespace ICD.Connect.Routing.Extron.Controls.Routing
{
	public sealed class GenericExtronUsbSwitcherControl : AbstractRouteSwitcherControl<IExtronSwitcherDevice>, IExtronSwitcherControl
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
		private readonly int m_NumberOfInputs;
		private readonly int m_NumberOfOutputs;

		#region Properties

		/// <summary>
		/// Gets the number of inputs.
		/// </summary>
		public int NumberOfInputs
		{
			get { return m_NumberOfInputs; }
		}

		/// <summary>
		/// Gets the number of outputs.
		/// </summary>
		public int NumberOfOutputs
		{
			get { return m_NumberOfOutputs; }
		}

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		/// <param name="numInputs"></param>
		/// <param name="numOutputs"></param>
		public GenericExtronUsbSwitcherControl(IExtronSwitcherDevice parent, int id, int numInputs, int numOutputs)
			: base(parent, id)
		{
			m_Cache = new SwitcherCache();
			m_NumberOfInputs = numInputs;
			m_NumberOfOutputs = numOutputs;

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
			return m_Cache.GetSourceDetectedState(input, type);
		}

		/// <summary>
		/// Gets the input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override ConnectorInfo GetInput(int input)
		{
			return GetInputs().First(i => i.Address == input);
		}

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override bool ContainsInput(int input)
		{
			return GetInputs().Any(i => i.Address == input);
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			return Enumerable.Range(1, NumberOfInputs)
			                 .Select(input => new ConnectorInfo(input, eConnectionType.Usb));
		}

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override ConnectorInfo GetOutput(int address)
		{
			return GetOutputs().First(o => o.Address == address);
		}

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override bool ContainsOutput(int output)
		{
			return GetOutputs().Any(o => o.Address == output);
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			return Enumerable.Range(1, NumberOfOutputs)
			                 .Select(output => new ConnectorInfo(output, eConnectionType.Usb));
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
					GetVideoInputSyncState(input),
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

		private bool GetVideoInputSyncState(ConnectorInfo info)
		{
			return m_Cache.GetSourceDetectedState(info.Address, eConnectionType.Video);
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

			if (localInput < 1 || localInput > NumberOfInputs)
				throw new ArgumentOutOfRangeException("info", string.Format("Input must be between 1 and {0}", NumberOfInputs));

			if (localOutput < 1 || localOutput > NumberOfOutputs)
				throw new ArgumentOutOfRangeException("info", string.Format("Output must be between 1 and {0}", NumberOfOutputs));

			Route(localInput);
			return true;
		}

		/// <summary>
		/// Routes the input to all outputs.
		/// </summary>
		/// <param name="input"></param>
		public void Route(int input)
		{
			Parent.SendCommand("{0}!", input);
		}

		/// <summary>
		/// Removes the given connection type from the output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool ClearOutput(int output, eConnectionType type)
		{
			if (m_Cache.GetInputConnectorInfoForOutput(output, eConnectionType.Usb) == null)
				return false;

			Route(0);
			return true;
		}

		#endregion

		#region Parent Callbacks

		protected override void Subscribe(IExtronSwitcherDevice parent)
		{
			base.Subscribe(parent);

			parent.OnInitializedChanged += ParentOnOnInitializedChanged;
			parent.OnResponseReceived += ParentOnOnResponseReceived;
		}

		protected override void Unsubscribe(IExtronSwitcherDevice parent)
		{
			base.Unsubscribe(parent);

			parent.OnInitializedChanged -= ParentOnOnInitializedChanged;
			parent.OnResponseReceived -= ParentOnOnResponseReceived;
		}

		private void ParentOnOnInitializedChanged(object sender, BoolEventArgs args)
		{
			InitializeCache();
		}

		private void ParentOnOnResponseReceived(object sender, StringEventArgs args)
		{
			ParseResponse(args.Data);
		}

		private void ParseResponse(string data)
		{
			const string channelPattern = @"Chn(?'channel'\d+)";

			Match match = Regex.Match(data, channelPattern);
			if (!match.Success)
				return;

			int input = int.Parse(match.Groups["channel"].Value);

			foreach (ConnectorInfo outputConnector in GetOutputs())
			{
				m_Cache.SetInputForOutput(outputConnector.Address,
				                          input == 0 ? (int?)null : input, eConnectionType.Usb);
			}
		}

		private void InitializeCache()
		{
			Parent.SendCommand("I");
		}

		#endregion

		#region Cache Callbacks

		private void Subscribe(SwitcherCache cache)
		{
			cache.OnSourceDetectionStateChange += CacheOnSourceDetectionStateChange;
			cache.OnRouteChange += CacheOnRouteChange;
			cache.OnActiveInputsChanged += CacheOnActiveInputsChanged;
			cache.OnActiveTransmissionStateChanged += CacheOnActiveTransmissionStateChanged;
		}

		private void Unsubscribe(SwitcherCache cache)
		{
			cache.OnSourceDetectionStateChange -= CacheOnSourceDetectionStateChange;
			cache.OnRouteChange -= CacheOnRouteChange;
			cache.OnActiveInputsChanged -= CacheOnActiveInputsChanged;
			cache.OnActiveTransmissionStateChanged -= CacheOnActiveTransmissionStateChanged;
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

		private void CacheOnActiveTransmissionStateChanged(object sender, TransmissionStateEventArgs args)
		{
			OnActiveTransmissionStateChanged.Raise(this, new TransmissionStateEventArgs(args));
		}

		#endregion
	}
}
