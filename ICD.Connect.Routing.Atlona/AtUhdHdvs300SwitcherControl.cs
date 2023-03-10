using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Utils;

namespace ICD.Connect.Routing.Atlona
{
	public sealed class AtUhdHdvs300SwitcherControl : AbstractRouteSwitcherControl<AtUhdHdvs300Device>
	{
		private const string REGEX_OUTPUT_ON = @"^x1\$ (on|off)";
		private const string REGEX_SOURCE_DETECT = @"^^InputStatus (0|1)(0|1)(0|1)(0|1)(0|1)";
		private const string REGEX_SOURCE_DETECT_SINGLE = @"^InputStatus(\d) (0|1)";
		private const string REGEX_ROUTING = @"^x(\d)AVx1";

		private static readonly Dictionary<int, string> s_InputIds = new Dictionary<int, string>
		{
			{1, "HDMI IN 1"},
			{2, "HDMI IN 2"},
			{3, "VGA IN"},
			{4, "DP IN 4"},
			{5, "HDMI IN 5"}
		};

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

		private bool m_OutputOn;

		/// <summary>
		/// Returns true if the output is currently turned on.
		/// </summary>
		public bool OutputOn
		{
			get { return m_OutputOn; }
			private set
			{
				if (value == m_OutputOn)
					return;

				m_OutputOn = value;

				OnActiveTransmissionStateChanged.Raise(this,
				                                       new TransmissionStateEventArgs(1,
				                                                                      eConnectionType.Audio | eConnectionType.Video,
				                                                                      m_OutputOn));
			}
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public AtUhdHdvs300SwitcherControl(AtUhdHdvs300Device parent, int id)
			: base(parent, id)
		{
			m_Cache = new SwitcherCache();

			Subscribe(m_Cache);
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnSourceDetectionStateChange = null;
			OnActiveTransmissionStateChanged = null;
			OnActiveInputsChanged = null;
			OnRouteChange = null;

			base.DisposeFinal(disposing);
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
			return input >= 1 && input <= 5;
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			return Enumerable.Range(1, 5).Select(i => new ConnectorInfo(i, eConnectionType.Audio | eConnectionType.Video));
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
			return output == 1;
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			yield return new ConnectorInfo(1, eConnectionType.Audio | eConnectionType.Video);
		}

		/// <summary>
		/// Gets the outputs for the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs(int input, eConnectionType type)
		{
			return OutputOn ? m_Cache.GetOutputsForInput(input, type) : Enumerable.Empty<ConnectorInfo>();
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
			return OutputOn ? m_Cache.GetInputConnectorInfoForOutput(output, type) : null;
		}

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
				VideoInputSyncFeedbackSupported = supportsVideo
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
				OutputId = "HDMI OUT",
				OutputIdFeedbackSupport = true,
				VideoOutputSource = supportsVideo ? GetActiveSourceIdName(output, eConnectionType.Video) : null,
				VideoOutputSourceFeedbackSupport = supportsVideo,
				AudioOutputSource = supportsAudio ? GetActiveSourceIdName(output, eConnectionType.Audio) : null,
				AudioOutputSourceFeedbackSupport = supportsAudio
			};
		}

		private string GetInputId(ConnectorInfo info)
		{
			return ContainsInput(info.Address) ? s_InputIds[info.Address] : null;
		}

		private bool GetVideoInputSyncState(ConnectorInfo info)
		{
			return m_Cache.GetSourceDetectedState(info.Address, eConnectionType.Video);
		}

		/// <summary>Performs the given route operation.</summary>
		/// <param name="info"></param>
		/// <returns></returns>
		public override bool Route(RouteOperation info)
		{
			eConnectionType type = info.ConnectionType;
			int input = info.LocalInput;
			int output = info.LocalOutput;

			if (input < 1 || input > 5)
				throw new ArgumentOutOfRangeException("info", "Input must be between 1 and 5");

			if (output != 1)
				throw new ArgumentOutOfRangeException("info", "Output must be 1");

			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
				                .Select(t => this.Route(input, output, t))
				                .ToArray()
				                .Unanimous(false);
			}

			switch (type)
			{
				case eConnectionType.Audio:
				case eConnectionType.Video:
					Route(input);
					TurnOnOutput(true);
					return true;

				default:
					throw new NotSupportedException(string.Format("{0} routing not supported", type));
			}
		}

		/// <summary>
		/// Stops routing to the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns>True if successfully cleared.</returns>
		public override bool ClearOutput(int output, eConnectionType type)
		{
			if (output != 1)
				throw new ArgumentOutOfRangeException("output");

			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
				                .Select(t => ClearOutput(output, t))
				                .ToArray()
				                .Unanimous(false);
			}

			switch (type)
			{
				case eConnectionType.Audio:
				case eConnectionType.Video:
					TurnOnOutput(false);
					return true;

				default:
					throw new NotSupportedException(string.Format("{0} routing not supported", type));
			}
		}

		/// <summary>
		/// Routes the given input to the output.
		/// </summary>
		/// <param name="input"></param>
		public void Route(int input)
		{
			if (input < 1 || input > 5)
				throw new ArgumentOutOfRangeException("input");

			Parent.SendCommand(string.Format("x{0}AVx1", input));
		}

		/// <summary>
		/// Turns the output on/off.
		/// </summary>
		/// <param name="on"></param>
		public void TurnOnOutput(bool on)
		{
			Parent.SendCommand(string.Format("x1$ {0}", on ? "on" : "off"));
		}

		#endregion

		#region Parent Callbacks

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected override void Subscribe(AtUhdHdvs300Device parent)
		{
			base.Subscribe(parent);

			parent.OnResponseReceived += ParentOnOnResponseReceived;
			parent.OnInitializedChanged += ParentOnOnInitializedChanged;
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected override void Unsubscribe(AtUhdHdvs300Device parent)
		{
			base.Unsubscribe(parent);

			parent.OnResponseReceived -= ParentOnOnResponseReceived;
			parent.OnInitializedChanged -= ParentOnOnInitializedChanged;
		}

		private void ParentOnOnInitializedChanged(object sender, BoolEventArgs boolEventArgs)
		{
			Parent.SendCommand("Statusx1"); // Routing
			Parent.SendCommand("InputStatus"); // Source detection
			Parent.SendCommand("x1$ sta"); // Output on state
		}

		private void ParentOnOnResponseReceived(object sender, StringEventArgs eventArgs)
		{
			// Output on state
			Match outputOnMatch = new Regex(REGEX_OUTPUT_ON).Match(eventArgs.Data);
			if (outputOnMatch.Success)
			{
				OutputOn = outputOnMatch.Groups[1].Value == "on";
				return;
			}

			// All source detection
			Match sourceDetectMatch = new Regex(REGEX_SOURCE_DETECT).Match(eventArgs.Data);
			if (sourceDetectMatch.Success)
			{
				for (int index = 1; index <= 5; index++)
				{
					int address = index;
					bool detected = sourceDetectMatch.Groups[index].Value == "1";

					m_Cache.SetSourceDetectedState(address, eConnectionType.Audio | eConnectionType.Video, detected);
				}

				return;
			}

			// Individual source detection
			Match sourceDetectSingleMatch = new Regex(REGEX_SOURCE_DETECT_SINGLE).Match(eventArgs.Data);
			if (sourceDetectSingleMatch.Success)
			{
				int address = int.Parse(sourceDetectSingleMatch.Groups[1].Value);
				bool detected = sourceDetectSingleMatch.Groups[1].Value == "1";

				m_Cache.SetSourceDetectedState(address, eConnectionType.Audio | eConnectionType.Video, detected);

				return;
			}

			// Routing
			Match routingMatch = new Regex(REGEX_ROUTING).Match(eventArgs.Data);
			if (routingMatch.Success)
			{
				int input = int.Parse(routingMatch.Groups[1].Value);

				m_Cache.SetInputForOutput(1, input, eConnectionType.Audio | eConnectionType.Video);

				return;
			}
		}

		#endregion

		#region Cache Callbacks

		private void Subscribe(SwitcherCache cache)
		{
			cache.OnActiveInputsChanged += CacheOnOnActiveInputsChanged;
			cache.OnRouteChange += CacheOnOnRouteChange;
			cache.OnSourceDetectionStateChange += CacheOnOnSourceDetectionStateChange;
		}

		private void CacheOnOnSourceDetectionStateChange(object sender, SourceDetectionStateChangeEventArgs eventArgs)
		{
			OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(eventArgs));
		}

		private void CacheOnOnRouteChange(object sender, RouteChangeEventArgs eventArgs)
		{
			OnRouteChange.Raise(this, new RouteChangeEventArgs(eventArgs));
		}

		private void CacheOnOnActiveInputsChanged(object sender, ActiveInputStateChangeEventArgs eventArgs)
		{
			OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(eventArgs));
		}

		#endregion

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("Output On", OutputOn);
			addRow("Routed Input", m_Cache.GetInputForOutput(1, eConnectionType.Video));
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new GenericConsoleCommand<int>("RouteInput", "Route <INPUT>", i => Route(i));
			yield return new GenericConsoleCommand<bool>("TurnOnOutput", "TurnOnOutput <true/false>", i => TurnOnOutput(i));
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
