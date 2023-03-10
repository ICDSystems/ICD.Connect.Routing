#if !NETSTANDARD
using System;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharpPro.DM;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Utils;
using ICD.Connect.Misc.CrestronPro.Extensions;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMdNxM4kzE
{
// ReSharper disable once InconsistentNaming
	public sealed class HdMdNxM4kzESwitcherControl : AbstractRouteSwitcherControl<IHdMdNxM4kzEAdapter>
	{
#region Events

		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;
		public override event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;
		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;
		public override event EventHandler<RouteChangeEventArgs> OnRouteChange;

#endregion

#region Members

		// Track the state in our cache because Crestron is unreliable.
		private readonly SwitcherCache m_Cache;

		[CanBeNull]
		private Crestron.SimplSharpPro.DM.HdMdNxM4kzE m_Switcher;

		private int? m_Inputs;
		private int? m_Outputs;

		public int Inputs
		{
			get
			{
				if (!m_Inputs.HasValue)
				{
					if (m_Switcher == null)
						return 0;

					m_Inputs = m_Switcher.HdmiInputs.Count;
				}

				return m_Inputs.Value;
			}
		}

		public int Outputs
		{
			get
			{
				if (!m_Outputs.HasValue)
				{
					if (m_Switcher == null)
						return 0;

					m_Outputs = m_Switcher.HdmiOutputs.Count;
				}

				return m_Outputs.Value;
			}
		}

#endregion

#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		public HdMdNxM4kzESwitcherControl(IHdMdNxM4kzEAdapter parent) 
			: base(parent, 0)
		{
			m_Cache = new SwitcherCache();
			Subscribe(m_Cache);

			SetSwitcher(parent.Switcher);
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnActiveTransmissionStateChanged = null;
			OnActiveInputsChanged = null;
			OnSourceDetectionStateChange = null;
			OnRouteChange = null;

			base.DisposeFinal(disposing);

			Unsubscribe(m_Cache);
			SetSwitcher(null);
		}

#endregion

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
				InputName = GetInputName(input),
				InputNameFeedbackSupported = true,
				VideoInputResolution = supportsVideo ? GetVideoInputResolution(input) : null,
				VideoInputResolutionFeedbackSupport = supportsVideo,
				VideoInputSync = supportsVideo && GetVideoInputSyncState(input),
				VideoInputSyncFeedbackSupported = supportsVideo,
				VideoInputSyncType = supportsVideo ? GetVideoInputSyncType(input) : null,
				VideoInputSyncTypeFeedbackSupported = supportsVideo
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
				OutputName = GetOutputName(output),
				OutputNameFeedbackSupport = true,
				VideoOutputSource = supportsVideo ? GetActiveSourceIdName(output, eConnectionType.Video) : null,
				VideoOutputSourceFeedbackSupport = supportsVideo,
				AudioOutputSource = supportsAudio ? GetActiveSourceIdName(output, eConnectionType.Audio) : null,
				AudioOutputSourceFeedbackSupport = supportsAudio
			};
		}

		/// <summary>
		/// Performs the given route operation.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		public override bool Route(RouteOperation info)
		{
			if (m_Switcher == null)
				return false;

			eConnectionType type = info.ConnectionType;
			int input = info.LocalInput;
			int output = info.LocalOutput;

			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
								.Select(f => this.Route(input, output, f))
								.ToArray()
								.Unanimous(false);
			}

			switch (type)
			{
				case eConnectionType.Audio:
				case eConnectionType.Video:
					m_Switcher.Outputs[(uint)output].VideoOut = m_Switcher.Inputs[(uint)input];
					return m_Cache.SetInputForOutput(output, input, eConnectionType.Audio | eConnectionType.Video);

				default:
					// ReSharper disable once NotResolvedInText
					throw new ArgumentOutOfRangeException("type", string.Format("Unexpected value {0}", type));
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
			if (m_Switcher == null)
				return false;

			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
								.Select(f => ClearOutput(output, f))
								.ToArray()
								.Unanimous(false);
			}

			switch (type)
			{
				case eConnectionType.Audio:
				case eConnectionType.Video:
					m_Switcher.Outputs[(uint)output].VideoOut = null;
					return m_Cache.SetInputForOutput(output, null, eConnectionType.Audio | eConnectionType.Video);

				default:
					throw new ArgumentOutOfRangeException("type", string.Format("Unexpected value {0}", type));
			}
		}

		/// <summary>
		/// Returns true if a signal is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool GetSignalDetectedState(int input, eConnectionType type)
		{
			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
								.Select(f => GetSignalDetectedState(input, f))
								.Unanimous(false);
			}

			switch (type)
			{
				case eConnectionType.Audio:
					return true;
				case eConnectionType.Video:
					return m_Cache.GetSourceDetectedState(input, type);

				default:
					throw new ArgumentOutOfRangeException("type", string.Format("Unexpected value {0}", type));
			}
		}

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override bool ContainsInput(int input)
		{
			return input >= 1 && input <= Inputs;
		}

		/// <summary>
		/// Gets the input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override ConnectorInfo GetInput(int input)
		{
			if (!ContainsInput(input))
				throw new ArgumentOutOfRangeException("input", string.Format("{0} has no input with address {1}", GetType().Name, input));

			return new ConnectorInfo(input, eConnectionType.Audio | eConnectionType.Video);
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			return Enumerable.Range(1, Inputs).Select(i => new ConnectorInfo(i, eConnectionType.Audio | eConnectionType.Video));
		}

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override bool ContainsOutput(int output)
		{
			return output >= 1 && output <= Outputs;
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
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			return Enumerable.Range(1, Outputs).Select(i => new ConnectorInfo(i, eConnectionType.Audio | eConnectionType.Video));
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

#endregion

#region Private Methods

		private string GetInputId(ConnectorInfo info)
		{
			if (m_Switcher == null || m_Switcher.Inputs == null)
				return null;

			DMInput dmInput = Parent.GetDmInput(info.Address);
			return string.Format("{0} {1}", DmInputOutputUtils.GetInputTypeStringForInput(dmInput), info.Address);
		}

		private string GetInputName(ConnectorInfo input)
		{
			if (m_Switcher == null)
				return null;

			DMInput dmInput = Parent.GetDmInput(input.Address);
			return dmInput.NameFeedback.GetSerialValueOrDefault();
		}

		private bool GetVideoInputSyncState(ConnectorInfo info)
		{
			return GetSignalDetectedState(info.Address, eConnectionType.Video);
		}

		private string GetVideoInputSyncType(ConnectorInfo info)
		{
			if (m_Switcher == null)
				return null;

			bool syncState = GetSignalDetectedState(info.Address, eConnectionType.Video);
			if (!syncState)
				return string.Empty;

			return DmInputOutputUtils.GetInputTypeStringForInput(Parent.GetDmInput(info.Address));
		}

		private string GetVideoInputResolution(ConnectorInfo info)
		{
			if (m_Switcher == null)
				return null;

			bool syncState = GetSignalDetectedState(info.Address, eConnectionType.Video);
			if (!syncState)
				return string.Empty;

			return DmInputOutputUtils.GetResolutionStringForVideoInput(Parent.GetDmInput(info.Address));
		}

		private string GetOutputId(ConnectorInfo info)
		{
			if (m_Switcher == null || m_Switcher.Outputs == null)
				return null;

			DMOutput dmOutput = Parent.GetDmOutput(info.Address);
			return string.Format("{0} {1}", DmInputOutputUtils.GetOutputTypeStringForOutput(dmOutput), info.Address);
		}

		private string GetOutputName(ConnectorInfo output)
		{
			if (m_Switcher == null)
				return null;

			DMOutput dmOutput = Parent.GetDmOutput(output.Address);
			return dmOutput.NameFeedback.GetSerialValueOrDefault();
		}

		/// <summary>
		/// Returns true if video is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		private bool GetVideoDetectedFeedback(int input)
		{
			if (m_Switcher == null)
				return false;

			return m_Switcher.Inputs[(uint)input].VideoDetectedFeedback.GetBoolValueOrDefault();
		}

		/// <summary>
		/// Gets the input for the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		private int? GetInputFeedback(int output)
		{
			if (m_Switcher == null)
				return null;

			DMInput input = m_Switcher.HdmiOutputs[(uint)output].GetSafeVideoOutFeedback();
			return input == null ? null : (int?)input.Number;
		}

#endregion

#region Parent Callbacks

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected override void Subscribe(IHdMdNxM4kzEAdapter parent)
		{
			base.Subscribe(parent);

			parent.OnSwitcherChanged += ParentOnSwitcherChanged;
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected override void Unsubscribe(IHdMdNxM4kzEAdapter parent)
		{
			base.Unsubscribe(parent);

			parent.OnSwitcherChanged -= ParentOnSwitcherChanged;
		}

		private void ParentOnSwitcherChanged(ICrestronSwitchAdapter sender, Switch switcher)
		{
			SetSwitcher(switcher as Crestron.SimplSharpPro.DM.HdMdNxM4kzE);
		}

		private void SetSwitcher(Crestron.SimplSharpPro.DM.HdMdNxM4kzE switcher)
		{
			Unsubscribe(m_Switcher);
			m_Switcher = switcher;
			Subscribe(m_Switcher);

			UsbBreakawayEnabled = m_Switcher != null && m_Switcher.EnableUSBBreakawayFeedback.GetBoolValueOrDefault();

			RebuildCache();
		}

		private void RebuildCache()
		{
			m_Cache.Clear();

			// Source detection
			foreach (ConnectorInfo input in GetInputs())
			{
				bool detected = GetVideoDetectedFeedback(input.Address);
				m_Cache.SetSourceDetectedState(input.Address, eConnectionType.Audio | eConnectionType.Video, detected);
			}

			// Routing
			foreach (ConnectorInfo output in GetOutputs())
			{
				int? input = GetInputFeedback(output.Address);
				m_Cache.SetInputForOutput(output.Address, input, eConnectionType.Audio | eConnectionType.Video);
			}
		}

#endregion

#region Switcher Callbacks

		/// <summary>
		/// Subscribe to the switcher events.
		/// </summary>
		/// <param name="switcher"></param>
		private void Subscribe(Crestron.SimplSharpPro.DM.HdMdNxM4kzE switcher)
		{
			if (switcher == null)
				return;

			switcher.DMInputChange += SwitcherOnDmInputChange;
			switcher.DMOutputChange += SwitcherOnDmOutputChange;
			switcher.DMSystemChange += SwitcherOnDmSystemChange;
		}

		/// <summary>
		/// Unsubscribe to the switcher events.
		/// </summary>
		/// <param name="switcher"></param>
		private void Unsubscribe(Crestron.SimplSharpPro.DM.HdMdNxM4kzE switcher)
		{
			if (switcher == null)
				return;

			switcher.DMInputChange -= SwitcherOnDmInputChange;
			switcher.DMOutputChange -= SwitcherOnDmOutputChange;
			switcher.DMSystemChange -= SwitcherOnDmSystemChange;

			m_Inputs = null;
			m_Outputs = null;
		}

		/// <summary>
		/// Called when an input state changes.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		private void SwitcherOnDmInputChange(Switch device, DMInputEventArgs args)
		{
			int inputNumber = (int)args.Number;

			bool state = GetVideoDetectedFeedback(inputNumber);
			m_Cache.SetSourceDetectedState(inputNumber, eConnectionType.Audio | eConnectionType.Video, state);

			if (DmInputOutputUtils.GetIsEventIdResolutionEventId(args.EventId))
			{
				InputPort input = GetInputPort((int)(args.Number));
				ConnectorInfo info = GetInput((int)(args.Number));
				input.VideoInputResolution = GetVideoInputResolution(info);
			}
		}

		/// <summary>
		/// Called when an output state changes.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		private void SwitcherOnDmOutputChange(Switch device, DMOutputEventArgs args)
		{
			if (m_Switcher == null)
				return;

			if (args.EventId != DMOutputEventIds.VideoOutEventId)
				return;

			DMInput input = m_Switcher.HdmiOutputs[args.Number].GetSafeVideoOutFeedback();
			int? address = input == null ? null : (int?)input.Number;

			m_Cache.SetInputForOutput((int)args.Number, address, eConnectionType.Audio | eConnectionType.Video);
		}

		private void SwitcherOnDmSystemChange(Switch device, DMSystemEventArgs args)
		{
			if (m_Switcher == null)
				return;

			if (args.EventId == DMSystemEventIds.USBBreakawayEventId)
				UsbBreakawayEnabled = m_Switcher.EnableUSBBreakawayFeedback.GetBoolValueOrDefault();
		}

#endregion

#region Cache Callbacks

		private void Subscribe(SwitcherCache cache)
		{
			cache.OnActiveInputsChanged += CacheOnActiveInputsChanged;
			cache.OnSourceDetectionStateChange += CacheOnSourceDetectionStateChange;
			cache.OnActiveTransmissionStateChanged += CacheOnActiveTransmissionStateChanged;
			cache.OnRouteChange += CacheOnRouteChange;
		}

		private void Unsubscribe(SwitcherCache cache)
		{
			cache.OnActiveInputsChanged -= CacheOnActiveInputsChanged;
			cache.OnSourceDetectionStateChange -= CacheOnSourceDetectionStateChange;
			cache.OnActiveTransmissionStateChanged -= CacheOnActiveTransmissionStateChanged;
			cache.OnRouteChange -= CacheOnRouteChange;
		}

		private void CacheOnActiveInputsChanged(object sender, ActiveInputStateChangeEventArgs args)
		{
			OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(args));
		}

		private void CacheOnSourceDetectionStateChange(object sender, SourceDetectionStateChangeEventArgs args)
		{
			OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(args));

			InputPort inputPort = GetInputPort(args.Input);
			ConnectorInfo info = GetInput(args.Input);
			inputPort.VideoInputSync = args.State;
			inputPort.VideoInputSyncType = GetVideoInputSyncType(info);
		}

		private void CacheOnActiveTransmissionStateChanged(object sender, TransmissionStateEventArgs args)
		{
			OnActiveTransmissionStateChanged.Raise(this, new TransmissionStateEventArgs(args));
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

#endregion
	}
}
#endif