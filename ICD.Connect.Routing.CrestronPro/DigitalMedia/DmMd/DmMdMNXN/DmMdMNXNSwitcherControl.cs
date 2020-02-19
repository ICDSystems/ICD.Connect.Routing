#if SIMPLSHARP
using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Misc.CrestronPro.Utils;
using ICD.Connect.Misc.CrestronPro.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Utils;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd.DmMdMNXN
{
	// ReSharper disable once InconsistentNaming
	public sealed class DmMdMNXNSwitcherControl : AbstractRouteSwitcherControl<IDmMdMNXNAdapter>
	{
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

		/// <summary>
		/// Called when a route changes.
		/// </summary>
		public override event EventHandler<RouteChangeEventArgs> OnRouteChange;

		private readonly SwitcherCache m_Cache;

		[CanBeNull]
		private DmMDMnxn m_Switcher;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		public DmMdMNXNSwitcherControl(IDmMdMNXNAdapter parent)
			: base(parent, 0)
		{
			m_Cache = new SwitcherCache();
			Subscribe(m_Cache);

			SetSwitcher(parent.Switcher);
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			OnSourceDetectionStateChange = null;
			OnRouteChange = null;
			OnActiveTransmissionStateChanged = null;
			OnActiveInputsChanged = null;

			base.DisposeFinal(disposing);

			// Unsubscribe and unregister.
			Unsubscribe(m_Cache);
			SetSwitcher(null);
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
			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
								.Select(t => GetSignalDetectedState(input, t))
								.Unanimous(false);
			}

			return m_Cache.GetSourceDetectedState(input, type);
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
		/// Routes the input to the given output.
		/// </summary>
		/// <param name="info"></param>
		/// <returns>True if routing successful.</returns>
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
								.Select(t => this.Route(input, output, t))
								.ToArray()
								.Unanimous(false);
			}

			DMOutput switcherOutput = m_Switcher.Outputs[(uint)output];
			DMInput switcherInput = m_Switcher.Inputs[(uint)input];

			switch (type)
			{
				case eConnectionType.Audio:
					switcherOutput.AudioOut = switcherInput;
					break;

				case eConnectionType.Video:
					switcherOutput.VideoOut = switcherInput;
					break;

				default:
					// ReSharper disable once NotResolvedInText
					throw new ArgumentOutOfRangeException("type", string.Format("Unexpected value {0}", type));
			}

			return m_Cache.SetInputForOutput(output, input, type);
		}

		/// <summary>
		/// Stops routing to the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns>True if unrouting successful.</returns>
		public override bool ClearOutput(int output, eConnectionType type)
		{
			if (m_Switcher == null)
				return false;

			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
								.Select(t => ClearOutput(output, t))
								.ToArray()
								.Unanimous(false);
			}

			DMOutput switcherOutput = m_Switcher.Outputs[(uint)output];

			switch (type)
			{
				case eConnectionType.Video:
					switcherOutput.VideoOut = null;
					break;

				case eConnectionType.Audio:
					switcherOutput.AudioOut = null;
					break;

				default:
					throw new ArgumentOutOfRangeException("type", string.Format("Unexpected value {0}", type));
			}

			return m_Cache.SetInputForOutput(output, null, type);
		}

		private string GetInputId(ConnectorInfo info)
		{
			if (m_Switcher == null || m_Switcher.Inputs == null)
				return null;

			DMInput dmInput = Parent.GetDmInput(info.Address);
			return string.Format("{0} {1}", DmInputOutputUtils.GetInputTypeStringForInput(dmInput), info.Address);
		}

		private string GetInputName(ConnectorInfo info)
		{
			if (m_Switcher == null)
				return null;

			DMInput dmInput = Parent.GetDmInput(info.Address);
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
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override ConnectorInfo GetOutput(int address)
		{
			return new ConnectorInfo(address, eConnectionType.Audio | eConnectionType.Video);
		}

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override bool ContainsOutput(int output)
		{
			if (m_Switcher == null)
				return false;

			return output >= 1 && output <= m_Switcher.NumberOfOutputs;
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			int outputs = m_Switcher == null ? 0 : m_Switcher.NumberOfOutputs;

			return Enumerable.Range(1, outputs)
							 .Select(i => new ConnectorInfo(i, eConnectionType.Audio | eConnectionType.Video));
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
			if (m_Switcher == null)
				return false;

			return input >= 1 && input <= m_Switcher.NumberOfInputs;
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
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			int inputs = m_Switcher == null ? 0 : m_Switcher.NumberOfInputs;

			return Enumerable.Range(1, inputs)
							 .Select(i => new ConnectorInfo(i, eConnectionType.Audio | eConnectionType.Video));
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns true if a signal is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private bool GetSignalDetectedFeedback(int input, eConnectionType type)
		{
			if (m_Switcher == null)
				return false;

			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
								.Select(t => GetSignalDetectedFeedback(input, t))
								.Unanimous(false);
			}

			DMInput switcherInput = m_Switcher.Inputs[(uint)input];

			switch (type)
			{
				case eConnectionType.Video:
					BoolOutputSig videoFeedbackSig = switcherInput.VideoDetectedFeedback;
					return videoFeedbackSig != null && videoFeedbackSig.GetBoolValueOrDefault();

				case eConnectionType.Audio:
					// No way of detecting audio?
					return true;

				case eConnectionType.Usb:
					DMInputOutputBase usbFeedbackSig = switcherInput.USBRoutedToFeedback;
					return usbFeedbackSig != null && usbFeedbackSig.EndpointOnlineFeedback;

				default:
					return false;
			}
		}

		/// <summary>
		/// Gets the routed inputs for the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private IEnumerable<ConnectorInfo> GetInputsFeedback(int output, eConnectionType type)
		{
			if (m_Switcher == null)
				yield break;

			DMOutput switcherOutput = m_Switcher.Outputs[(uint)output];

			foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(type))
			{
				DMInput input;

				switch (flag)
				{
					case eConnectionType.Audio:
						input = switcherOutput.GetSafeAudioOutFeedback();
						break;
					case eConnectionType.Video:
						input = switcherOutput.GetSafeVideoOutFeedback();
						break;
					default:
						continue;
				}

				if (input == null)
					continue;

				yield return new ConnectorInfo((int)input.Number, flag);
			}
		}

		#endregion

		#region Parent Callbacks

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected override void Subscribe(IDmMdMNXNAdapter parent)
		{
			base.Subscribe(parent);

			parent.OnSwitcherChanged += ParentOnSwitcherChanged;
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected override void Unsubscribe(IDmMdMNXNAdapter parent)
		{
			base.Unsubscribe(parent);

			parent.OnSwitcherChanged -= ParentOnSwitcherChanged;
		}

		private void ParentOnSwitcherChanged(ICrestronSwitchAdapter crestronSwitchAdapter, Switch switcher)
		{
			SetSwitcher(crestronSwitchAdapter.Switcher as DmMDMnxn);
		}

		/// <summary>
		/// Sets the wrapped switcher.
		/// </summary>
		/// <param name="switcher"></param>
		private void SetSwitcher(DmMDMnxn switcher)
		{
			if (switcher == m_Switcher)
				return;

			Unsubscribe(m_Switcher);
			m_Switcher = switcher;
			Subscribe(m_Switcher);

			AudioBreakawayEnabled = m_Switcher != null && m_Switcher.EnableAudioBreakawayFeedback.GetBoolValueOrDefault();
			UsbBreakawayEnabled = m_Switcher != null && m_Switcher.EnableUSBBreakawayFeedback.GetBoolValueOrDefault();

			RebuildCache();
		}

		private void RebuildCache()
		{
			m_Cache.Clear();

			// Source detection
			foreach (ConnectorInfo input in GetInputs())
			{
				foreach (eConnectionType type in EnumUtils.GetValuesExceptNone<eConnectionType>())
				{
					bool detected = GetSignalDetectedFeedback(input.Address, type);
					m_Cache.SetSourceDetectedState(input.Address, type, detected);
				}
			}

			// Routing
			foreach (ConnectorInfo output in GetOutputs())
			{
				foreach (ConnectorInfo input in GetInputsFeedback(output.Address, EnumUtils.GetFlagsAllValue<eConnectionType>()))
					m_Cache.SetInputForOutput(output.Address, input.Address, eConnectionType.Audio | eConnectionType.Video);
			}
		}

		#endregion

		#region Switcher Callbacks

		/// <summary>
		/// Subscribe to the switcher events.
		/// </summary>
		/// <param name="switcher"></param>
		private void Subscribe(DmMDMnxn switcher)
		{
			if (switcher == null)
				return;

			switcher.DMInputChange += SwitcherOnDmInputChange;
			switcher.DMOutputChange += SwitcherOnDmOutputChange;
			switcher.DMSystemChange += SwitcherOnDmSystemChange;
		}

		/// <summary>
		/// Unsubscribe from the switcher events.
		/// </summary>
		/// <param name="switcher"></param>
		private void Unsubscribe(DmMDMnxn switcher)
		{
			if (switcher == null)
				return;

			switcher.DMInputChange -= SwitcherOnDmInputChange;
			switcher.DMOutputChange -= SwitcherOnDmOutputChange;
			switcher.DMSystemChange -= SwitcherOnDmSystemChange;
		}

		/// <summary>
		/// Handles the detection change for individual connection types.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		private void SourceDetectionChange(int input, eConnectionType type)
		{
			bool state = GetSignalDetectedFeedback(input, type);
			m_Cache.SetSourceDetectedState(input, type, state);
		}

		/// <summary>
		/// Called when an input state changes.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		private void SwitcherOnDmInputChange(Switch device, DMInputEventArgs args)
		{
			eConnectionType type = DmUtils.DmEventToConnectionType(args.EventId);

			foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(type))
				SourceDetectionChange((int)args.Number, flag);

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
			eConnectionType type;

			switch (args.EventId)
			{
				case DMOutputEventIds.VideoOutEventId:
					type = eConnectionType.Video;
					break;

				case DMOutputEventIds.AudioOutEventId:
					type = eConnectionType.Audio;
					break;

				case DMOutputEventIds.UsbRoutedToEventId:
					type = eConnectionType.Usb;
					break;

				default:
					return;
			}

			int output = (int)args.Number;
			int? input = GetInputsFeedback(output, type).Select(c => (int?)c.Address)
														.FirstOrDefault();

			m_Cache.SetInputForOutput(output, input, type);
		}

		private void SwitcherOnDmSystemChange(Switch device, DMSystemEventArgs args)
		{
			if (m_Switcher == null)
				return;

			if (args.EventId == DMSystemEventIds.AudioBreakawayEventId)
				AudioBreakawayEnabled = m_Switcher.EnableAudioBreakawayFeedback.GetBoolValueOrDefault();

			if (args.EventId == DMSystemEventIds.USBBreakawayEventId)
				UsbBreakawayEnabled = m_Switcher.EnableUSBBreakawayFeedback.GetBoolValueOrDefault();
		}

		#endregion

		#region Cache Callbacks

		/// <summary>
		/// Subscribe to the switcher cache events.
		/// </summary>
		/// <param name="cache"></param>
		private void Subscribe(SwitcherCache cache)
		{
			cache.OnActiveInputsChanged += CacheOnActiveInputsChanged;
			cache.OnSourceDetectionStateChange += CacheOnSourceDetectionStateChange;
			cache.OnActiveTransmissionStateChanged += CacheOnActiveTransmissionStateChanged;
			cache.OnRouteChange += CacheOnRouteChange;
		}

		/// <summary>
		/// Unsubscribe from the switcher cache events.
		/// </summary>
		/// <param name="cache"></param>
		private void Unsubscribe(SwitcherCache cache)
		{
			cache.OnActiveInputsChanged -= CacheOnActiveInputsChanged;
			cache.OnSourceDetectionStateChange -= CacheOnSourceDetectionStateChange;
			cache.OnActiveTransmissionStateChanged -= CacheOnActiveTransmissionStateChanged;
			cache.OnRouteChange -= CacheOnRouteChange;
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
			inputPort.VideoInputSyncType = GetVideoInputSyncType(info);
		}

		private void CacheOnActiveInputsChanged(object sender, ActiveInputStateChangeEventArgs args)
		{
			OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(args));
		}

		#endregion
	}
}

#endif
