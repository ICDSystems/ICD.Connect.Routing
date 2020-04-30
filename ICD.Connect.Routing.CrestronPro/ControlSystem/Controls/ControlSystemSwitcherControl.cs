#if SIMPLSHARP
using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Nodes;
using ICD.Connect.Misc.CrestronPro.Extensions;
using ICD.Connect.Misc.CrestronPro.Utils;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.CrestronPro.DigitalMedia;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Utils;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls
{
	public sealed class ControlSystemSwitcherControl : AbstractRouteSwitcherControl<ControlSystemDevice>
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

		// Keeps track of source detection
		private readonly SwitcherCache m_Cache;

		private CrestronControlSystem m_SubscribedControlSystem;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public ControlSystemSwitcherControl(ControlSystemDevice parent, int id)
			: base(parent, id)
		{
			m_Cache = new SwitcherCache();
			Subscribe(m_Cache);

			SetControlSystem(parent.ControlSystem);
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnActiveTransmissionStateChanged = null;
			OnSourceDetectionStateChange = null;
			OnActiveInputsChanged = null;
			OnRouteChange = null;

			base.DisposeFinal(disposing);

			Unsubscribe(m_Cache);
			SetControlSystem(null);
		}

		#region Routing

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
				VideoInputSync = supportsVideo && GetVideoInputSyncState(input),
				VideoInputSyncFeedbackSupported = supportsVideo,
				VideoInputSyncType = supportsVideo ? GetVideoInputSyncType(input) : null,
				VideoInputSyncTypeFeedbackSupported = supportsVideo,
				VideoInputResolution = supportsVideo ? GetVideoInputResolution(input) : null,
				VideoInputResolutionFeedbackSupport = supportsVideo
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

			DMOutput switcherOutput = Parent.GetDmOutput(output);
			DMInput switcherInput = Parent.GetDmInput(input);

			switch (type)
			{
				case eConnectionType.Audio:
					try
					{
						switcherOutput.AudioOut = switcherInput;
					}
					catch (NotSupportedException)
					{
						try
						{
							// DMPS 4K
							switcherOutput.AudioOutSource = GetAudioSourceForInput(input);
						}
						catch (Exception e)
						{
							Log(eSeverity.Error, "Failed to route audio input {0} to output {1} - {2}", input, output, e.Message);
							return false;
						}
					}

					break;

				case eConnectionType.Video:
					switcherOutput.VideoOut = switcherInput;
					break;

				case eConnectionType.Usb:
					switcherOutput.USBRoutedTo = switcherInput;
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
			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
				                .Select(t => ClearOutput(output, t))
				                .ToArray()
				                .Unanimous(false);
			}

			DMOutput switcherOutput = Parent.GetDmOutput(output);

			switch (type)
			{
				case eConnectionType.Video:
					switcherOutput.VideoOut = null;
					break;

				case eConnectionType.Audio:
					try
					{
						switcherOutput.AudioOut = null;
					}
					catch (NotSupportedException)
					{
						try
						{
							// DMPS 4K
							switcherOutput.AudioOutSource = GetAudioSourceForInput(null);
						}
						catch (Exception e)
						{
							Log(eSeverity.Error, "Failed to clear audio output {0} - {1}", output, e.Message);
							return false;
						}
					}

					break;

				case eConnectionType.Usb:
					switcherOutput.USBRoutedTo = null;
					break;

				default:
					throw new ArgumentOutOfRangeException("type", string.Format("Unexpected value {0}", type));
			}

			m_Cache.SetInputForOutput(output, null, type);
			return true;
		}

		/// <summary>
		/// Gets the connector info for the output at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override ConnectorInfo GetOutput(int address)
		{
			if (!ContainsOutput(address))
				throw new ArgumentOutOfRangeException("address");

			eCardInputOutputType type = Parent.GetDmOutput(address).CardInputOutputType;
			return new ConnectorInfo(address, GetConnectionType(type));
		}

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override bool ContainsOutput(int output)
		{
			CrestronCollection<ICardInputOutputType> outputs = Parent.ControlSystem.SwitcherOutputs;
			return outputs != null && outputs.Contains((uint)output);
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			IEnumerable<int> addresses = Parent.ControlSystem.SupportsSwitcherOutputs
				                             ? (Parent.ControlSystem.SwitcherOutputs as
				                                ReadOnlyCollection<uint, ICardInputOutputType>).Select(kvp => (int)kvp.Key)
				                             : Enumerable.Empty<int>();

			return addresses.Order().Select(i => GetOutput(i)).Where(c => c.ConnectionType != eConnectionType.None);
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
		/// Gets the connector info for the input at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override ConnectorInfo GetInput(int address)
		{
			eCardInputOutputType type = Parent.GetDmInput(address).CardInputOutputType;
			return new ConnectorInfo(address, GetConnectionType(type));
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			IEnumerable<int> addresses = Parent.ControlSystem.SupportsSwitcherInputs
				                             ? (Parent.ControlSystem.SwitcherInputs as
				                                ReadOnlyCollection<uint, ICardInputOutputType>).Select(kvp => (int)kvp.Key)
				                             : Enumerable.Empty<int>();

			return addresses.Order().Select(i => GetInput(i)).Where(c => c.ConnectionType != eConnectionType.None);
		}

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override bool ContainsInput(int input)
		{
			CrestronCollection<ICardInputOutputType> inputs = Parent.ControlSystem.SwitcherInputs;
			return inputs != null && inputs.Contains((uint)input);
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
			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
				                .Select(t => GetSignalDetectedFeedback(input, t))
				                .Unanimous(false);
			}

			DMInput switcherInput = Parent.GetDmInput(input);

			// The feedback sigs are null while the program is starting up
			switch (type)
			{
				case eConnectionType.Video:
					return switcherInput.VideoDetectedFeedback.Type == eSigType.Bool && switcherInput.VideoDetectedFeedback.GetBoolValueOrDefault();

				case eConnectionType.Audio:
					// No way of detecting audio?
					return true;

				case eConnectionType.Usb:
					return switcherInput.USBRoutedToFeedback != null && switcherInput.USBRoutedToFeedback.EndpointOnlineFeedback;

				default:
					return false;
			}
		}

		/// <summary>
		/// Gets the input for the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private IEnumerable<ConnectorInfo> GetInputsFeedback(int output, eConnectionType type)
		{
			DMOutput switcherOutput = Parent.GetDmOutput(output);

			foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(type))
			{
				DMInputOutputBase input;

				switch (flag)
				{
					case eConnectionType.Audio:
						try
						{
							// DMPS3 4K
							int? inputAddress = GetInputForAudioSource(switcherOutput.AudioOutSourceFeedback);
							input = inputAddress == null ? null : Parent.GetDmInput((int)inputAddress);
						}
						catch (NotSupportedException)
						{
							// DMPS3
							input = switcherOutput.GetSafeAudioOutFeedback();
						}
						break;
					case eConnectionType.Video:
						input = switcherOutput.GetSafeVideoOutFeedback();
						break;
					case eConnectionType.Usb:
						input = switcherOutput.GetSafeUsbRoutedToFeedback();
						break;
					default:
						continue;
				}

				if (input == null)
					continue;

				yield return new ConnectorInfo((int)input.Number, flag);
			}
		}

		/// <summary>
		/// Gets the input for the given AudioOutSource value.
		/// 
		/// TODO - This does not support analog inputs
		/// </summary>
		/// <param name="audioOutSource"></param>
		/// <returns></returns>
		private int? GetInputForAudioSource(eDmps34KAudioOutSource audioOutSource)
		{
			switch (audioOutSource)
			{
				case eDmps34KAudioOutSource.NoRoute:
					return null;

				case eDmps34KAudioOutSource.Analog1:
				case eDmps34KAudioOutSource.Analog2:
				case eDmps34KAudioOutSource.Analog3:
				case eDmps34KAudioOutSource.Analog4:
				case eDmps34KAudioOutSource.Analog5:
					return null;

				case eDmps34KAudioOutSource.Hdmi1:
					return 1;
				case eDmps34KAudioOutSource.Hdmi2:
					return 2;
				case eDmps34KAudioOutSource.Hdmi3:
					return 3;
				case eDmps34KAudioOutSource.Hdmi4:
					return 4;
				case eDmps34KAudioOutSource.Hdmi5:
					return 5;
				case eDmps34KAudioOutSource.Hdmi6:
					return 6;
				case eDmps34KAudioOutSource.Dm7:
					return 7;
				case eDmps34KAudioOutSource.Dm8:
					return 8;

				case eDmps34KAudioOutSource.AirMedia8:
				case eDmps34KAudioOutSource.AirMedia9:
					return null;

				default:
					throw new ArgumentOutOfRangeException("audioOutSource");
			}
		}

		/// <summary>
		/// Gets the AudioOutSource value for the given input.
		/// 
		/// TODO - This does not support analog inputs
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		private eDmps34KAudioOutSource GetAudioSourceForInput(int? input)
		{
			switch (input)
			{
				case null:
					return eDmps34KAudioOutSource.NoRoute;
				case 1:
					return eDmps34KAudioOutSource.Hdmi1;
				case 2:
					return eDmps34KAudioOutSource.Hdmi2;
				case 3:
					return eDmps34KAudioOutSource.Hdmi3;
				case 4:
					return eDmps34KAudioOutSource.Hdmi4;
				case 5:
					return eDmps34KAudioOutSource.Hdmi5;
				case 6:
					return eDmps34KAudioOutSource.Hdmi6;
				case 7:
					return eDmps34KAudioOutSource.Dm7;
				case 8:
					return eDmps34KAudioOutSource.Dm8;

				default:
					throw new ArgumentOutOfRangeException("input");
			}
		}

		private void SetControlSystem(CrestronControlSystem controlSystem)
		{
			Unsubscribe(m_SubscribedControlSystem);
			m_SubscribedControlSystem = controlSystem;
			Subscribe(m_SubscribedControlSystem);

			// If usb and audio breakaway are supported, always enable them
			if (m_SubscribedControlSystem != null && m_SubscribedControlSystem.SystemControl != null)
			{
				if (m_SubscribedControlSystem.SystemControl.EnableAudioBreakaway.Supported &&
				    m_SubscribedControlSystem.SystemControl.EnableAudioBreakaway.Type == eSigType.Bool)
				{
					m_SubscribedControlSystem.SystemControl.EnableAudioBreakaway.BoolValue = true;
					AudioBreakawayEnabled = true;
				}

				if (m_SubscribedControlSystem.SystemControl.EnableUSBBreakaway.Supported &&
				    m_SubscribedControlSystem.SystemControl.EnableUSBBreakaway.Type == eSigType.Bool)
				{
					m_SubscribedControlSystem.SystemControl.EnableUSBBreakaway.BoolValue = true;
					UsbBreakawayEnabled = true;
				}
			}

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

		private string GetInputId(ConnectorInfo info)
		{
			if (m_SubscribedControlSystem == null || m_SubscribedControlSystem.SwitcherInputs == null)
				return null;

			DMInput dmInput = Parent.GetDmInput(info.Address);

			return string.Format("{0} {1}", DmInputOutputUtils.GetInputTypeStringForInput(dmInput), info.Address);
		}

		private string GetInputName(ConnectorInfo info)
		{
			if (m_SubscribedControlSystem == null || m_SubscribedControlSystem.SwitcherInputs == null)
				return null;

			DMInput dmInput = Parent.GetDmInput(info.Address);
			return dmInput.NameFeedback.Type == eSigType.NA ? null : dmInput.NameFeedback.GetSerialValueOrDefault();
		}

		private string GetVideoInputSyncType(ConnectorInfo info)
		{
			if (m_SubscribedControlSystem == null || m_SubscribedControlSystem.SwitcherInputs == null)
				return null;

			bool syncState = GetSignalDetectedState(info.Address, eConnectionType.Video);
			if (!syncState)
				return string.Empty;

			DMInput dmInput = Parent.GetDmInput(info.Address);

			if (dmInput.CardInputOutputType == eCardInputOutputType.Dmps3HdmiVgaInput)
			{
				Card.Dmps3HdmiVgaInput castInput = dmInput as Card.Dmps3HdmiVgaInput;
				if (castInput == null)
					return string.Empty;

				if (castInput.HdmiSyncDetected.GetBoolValueOrDefault())
					return "HDMI";

				if (castInput.VgaSyncDetectedFeedback.GetBoolValueOrDefault())
					return "VGA";

				return string.Empty;
			}

			if (dmInput.CardInputOutputType == eCardInputOutputType.Dmps3HdmiVgaBncInput)
			{
				Card.Dmps3HdmiVgaBncInput castInput = dmInput as Card.Dmps3HdmiVgaBncInput;
				if (castInput == null)
					return string.Empty;

				if (castInput.HdmiSyncDetected.GetBoolValueOrDefault())
					return "HDMI";

				if (castInput.VgaSyncDetectedFeedback.GetBoolValueOrDefault())
					return "VGA";

				if (castInput.BncSyncDetected.GetBoolValueOrDefault())
					return "BNC";

				return string.Empty;
			}

			return DmInputOutputUtils.GetInputTypeStringForInput(dmInput);
		}

		private bool GetVideoInputSyncState(ConnectorInfo info)
		{
			return GetSignalDetectedState(info.Address, eConnectionType.Video);
		}

		private string GetVideoInputResolution(ConnectorInfo info)
		{
			if (m_SubscribedControlSystem == null || m_SubscribedControlSystem.SwitcherInputs == null)
				return null;

			bool syncState = GetSignalDetectedState(info.Address, eConnectionType.Video);
			if (!syncState)
			{
				return string.Empty;
			}

			DMInput dmInput = Parent.GetDmInput(info.Address);

			return DmInputOutputUtils.GetResolutionStringForVideoInput(dmInput);
		}

		private string GetOutputId(ConnectorInfo info)
		{
			if (m_SubscribedControlSystem == null || m_SubscribedControlSystem.SwitcherInputs == null)
				return null;

			DMOutput dmOutput = Parent.GetDmOutput(info.Address);

			return string.Format("{0} {1}", DmInputOutputUtils.GetOutputTypeStringForOutput(dmOutput), info.Address);
		}

		private string GetOutputName(ConnectorInfo info)
		{
			if (m_SubscribedControlSystem == null || m_SubscribedControlSystem.SwitcherInputs == null)
				return null;

			DMOutput dmOutput = Parent.GetDmOutput(info.Address);

			return dmOutput.NameFeedback.GetSerialValueOrDefault();
		}

		private string GetAudioOutputName(ConnectorInfo info)
		{
			if (m_SubscribedControlSystem == null || m_SubscribedControlSystem.SwitcherInputs == null)
				return null;

			DMOutput dmOutput = Parent.GetDmOutput(info.Address);

			return string.Format("{0} {1}", dmOutput.NameFeedback.GetSerialValueOrDefault(), info.Address);
		}

		#endregion

		#region CrestronControlSystem Callbacks

		/// <summary>
		/// Subscribe to the ControlSystem events.
		/// </summary>
		/// <param name="controlSystem"></param>
		private void Subscribe(CrestronControlSystem controlSystem)
		{
			if (controlSystem == null)
				return;

			// Internally Crestron is simply exposing an internal DMPS3 event, so
			// subscribing/unsubscribing will raise a null ref if there is no DMPS3.
			if (controlSystem.SupportsSwitcherInputs)
				controlSystem.DMInputChange += ControlSystemOnDmInputChange;
			if (controlSystem.SupportsSwitcherOutputs)
				controlSystem.DMOutputChange += ControlSystemOnDmOutputChange;
		}

		/// <summary>
		/// Unsubscribe from the ControlSystem events.
		/// </summary>
		/// <param name="controlSystem"></param>
		private void Unsubscribe(CrestronControlSystem controlSystem)
		{
			if (controlSystem == null)
				return;

			if (controlSystem.SupportsSwitcherInputs)
				controlSystem.DMInputChange -= ControlSystemOnDmInputChange;
			if (controlSystem.SupportsSwitcherOutputs)
				controlSystem.DMOutputChange -= ControlSystemOnDmOutputChange;
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
		private void ControlSystemOnDmInputChange(Switch device, DMInputEventArgs args)
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
		private void ControlSystemOnDmOutputChange(Switch device, DMOutputEventArgs args)
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

		/// <summary>
		/// Gets the connection types for the card IO type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		private static eConnectionType GetConnectionType(eCardInputOutputType type)
		{
			switch (type)
			{
					// Unsure - unused?
				case eCardInputOutputType.Dmps3StreamingReceive:
				case eCardInputOutputType.Dmps3CodecOutput:
				case eCardInputOutputType.Dmps3StreamingTransmit:
				case eCardInputOutputType.Dmps3DigitalMixOutput:
					return eConnectionType.None;

				case eCardInputOutputType.NA:
					return eConnectionType.None;

				case eCardInputOutputType.Dmps3HdmiInputWithoutAnalogAudio:
				case eCardInputOutputType.Dmps3HdmiInput:
				case eCardInputOutputType.Dmps3HdmiVgaInput:
				case eCardInputOutputType.Dmps3HdmiVgaBncInput:
				case eCardInputOutputType.Dmps3AirMediaInput:
				case eCardInputOutputType.Dmps3HdmiOutput:
				case eCardInputOutputType.Dmps3DmInput:
				case eCardInputOutputType.Dmps3DmOutput:
				case eCardInputOutputType.Dmps3DmHdmiAudioOutput:
				case eCardInputOutputType.Dmps3HdmiAudioOutput:
				case eCardInputOutputType.Dmps3HdmiOutputBackend:
				case eCardInputOutputType.Dmps3DmOutputBackend:
				case eCardInputOutputType.Dm8x14k:
					return eConnectionType.Video | eConnectionType.Audio;

				case eCardInputOutputType.Dmps3VgaInput:
					return eConnectionType.Video;

				case eCardInputOutputType.Dmps3AnalogAudioInput:
				case eCardInputOutputType.Dmps3Aux1Output:
				case eCardInputOutputType.Dmps3Aux2Output:
				case eCardInputOutputType.Dmps3SipInput:
				case eCardInputOutputType.Dmps3ProgramOutput:
				case eCardInputOutputType.Dmps3DialerOutput:
				case eCardInputOutputType.Dmps3AecOutput:
					return eConnectionType.Audio;

				default:
					throw new ArgumentOutOfRangeException("type", string.Format("Unexpected value {0}", type));
			}
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

			if (!args.Type.HasFlag(eConnectionType.Video))
				return;
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

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			if (m_SubscribedControlSystem == null || m_SubscribedControlSystem.SystemControl == null)
				return;

			addRow("Audio Breakaway",
			       m_SubscribedControlSystem.SystemControl.EnableAudioBreakawayFeedback.Supported
				       ? m_SubscribedControlSystem.SystemControl.EnableAudioBreakawayFeedback.GetBoolValueOrDefault().ToString()
				       : "Not Supported");
			addRow("USB Breakaway",
			       m_SubscribedControlSystem.SystemControl.EnableUSBBreakawayFeedback.Supported
				       ? m_SubscribedControlSystem.SystemControl.EnableUSBBreakawayFeedback.GetBoolValueOrDefault().ToString()
				       : "Not Supported");
		}

		#endregion
	}
}

#endif
