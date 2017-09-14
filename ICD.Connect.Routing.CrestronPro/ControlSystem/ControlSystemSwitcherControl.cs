#if SIMPLSHARP
using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Misc.CrestronPro.Utils;
using ICD.Connect.Misc.CrestronPro.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Utils;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem
{
	public sealed class ControlSystemSwitcherControl : AbstractRouteSwitcherControl<ControlSystemDevice>
	{
		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;
		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;
		public override event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;
		public override event EventHandler<RouteChangeEventArgs> OnRouteChange;

		// Keeps track of source detection
		private readonly SwitcherCache m_Cache;

		private CrestronControlSystem m_SubscribedControlSystem;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		public ControlSystemSwitcherControl(ControlSystemDevice parent)
			: base(parent, 0)
		{
			m_Cache = new SwitcherCache();
			m_Cache.OnActiveInputsChanged += CacheOnActiveInputsChanged;
			m_Cache.OnSourceDetectionStateChange += CacheOnSourceDetectionStateChange;
			m_Cache.OnActiveTransmissionStateChanged += CacheOnActiveTransmissionStateChanged;
			m_Cache.OnRouteChange += CacheOnRouteChange;

			Subscribe(parent);
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

			Unsubscribe(Parent);
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

			DMInput switcherInput = Parent.GetDmInput(input);

			switch (type)
			{
				case eConnectionType.Video:
					return switcherInput.VideoDetectedFeedback.BoolValue;

				case eConnectionType.Audio:
					// No way of detecting audio?
					return true;

				case eConnectionType.Usb:
					return switcherInput.USBRoutedToFeedback.EndpointOnlineFeedback;

				default:
					return false;
			}
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
				                .Unanimous(false);
			}

			DMOutput switcherOutput = Parent.GetDmOutput(output);
			DMInput switcherInput = Parent.GetDmInput(input);

			switch (type)
			{
				case eConnectionType.Audio:
					switcherOutput.AudioOut = switcherInput;
					return switcherOutput.GetSafeAudioOutFeedback() == switcherInput;

				case eConnectionType.Video:
					switcherOutput.VideoOut = switcherInput;
					return switcherOutput.GetSafeVideoOutFeedback() == switcherInput;

				case eConnectionType.Usb:
					switcherOutput.USBRoutedTo = switcherInput;
					return switcherOutput.GetSafeUsbRoutedToFeedback() == switcherInput;

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
		/// <returns>True if unrouting successful.</returns>
		public override bool ClearOutput(int output, eConnectionType type)
		{
			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
				                .Select(t => ClearOutput(output, t))
				                .Unanimous(false);
			}

			DMOutput switcherOutput = Parent.GetDmOutput(output);

			switch (type)
			{
				case eConnectionType.Video:
					if (switcherOutput.GetSafeVideoOutFeedback() == null)
						return false;
					switcherOutput.VideoOut = null;
					return true;

				case eConnectionType.Audio:
					if (switcherOutput.GetSafeAudioOutFeedback() == null)
						return false;
					switcherOutput.AudioOut = null;
					return true;

				case eConnectionType.Usb:
					if (switcherOutput.GetSafeUsbRoutedToFeedback() == null)
						return false;
					switcherOutput.USBRoutedTo = null;
					return true;

				default:
					throw new ArgumentOutOfRangeException("type", string.Format("Unexpected value {0}", type));
			}
		}

		/// <summary>
		/// Gets the input for the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs(int output, eConnectionType type)
		{
			IcdHashSet<int> inputs = new IcdHashSet<int>();
			DMOutput switcherOutput = Parent.GetDmOutput(output);

			if (type.HasFlag(eConnectionType.Audio))
			{
				DMInput audioInput = switcherOutput.GetSafeAudioOutFeedback();
				if (audioInput != null)
					inputs.Add((int)audioInput.Number);
			}

			if (type.HasFlag(eConnectionType.Video))
			{
				DMInput videoInput = switcherOutput.GetSafeVideoOutFeedback();
				if (videoInput != null)
					inputs.Add((int)videoInput.Number);
			}

			if (type.HasFlag(eConnectionType.Usb))
			{
				DMInputOutputBase usbInput = switcherOutput.GetSafeUsbRoutedToFeedback();
				if (usbInput != null)
					inputs.Add((int)usbInput.Number);
			}

			return inputs.Select(i => GetInput(i));
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			IEnumerable<int> addresses = Parent.ControlSystem.SupportsSwitcherOutputs
				                             ? Enumerable.Range(1, Parent.ControlSystem.NumberOfSwitcherOutputs)
				                             : Enumerable.Empty<int>();

			return addresses.Select(i => GetOutput(i)).Where(c => c.ConnectionType != eConnectionType.None);
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			IEnumerable<int> addresses = Parent.ControlSystem.SupportsSwitcherInputs
				                             ? Enumerable.Range(1, Parent.ControlSystem.NumberOfSwitcherInputs)
				                             : Enumerable.Empty<int>();

			return addresses.Select(i => GetInput(i)).Where(c => c.ConnectionType != eConnectionType.None);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the connector info for the output at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override ConnectorInfo GetOutput(int address)
		{
			eCardInputOutputType type = Parent.GetDmOutput(address).CardInputOutputType;
			return new ConnectorInfo(address, GetConnectionType(type));
		}

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override bool ContainsInput(int input)
		{
			if (!Parent.ControlSystem.SupportsSwitcherInputs)
				return false;
			return input > 0 && input <= Parent.ControlSystem.NumberOfSwitcherInputs;
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

		#region Parent Callbacks

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="parent"></param>
		private void Subscribe(ControlSystemDevice parent)
		{
			parent.OnControlSystemChanged += ParentOnControlSystemChanged;
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		private void Unsubscribe(ControlSystemDevice parent)
		{
			parent.OnControlSystemChanged -= ParentOnControlSystemChanged;
		}

		/// <summary>
		/// Called when the parents wrapped control system changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="controlSystem"></param>
		private void ParentOnControlSystemChanged(ControlSystemDevice sender, CrestronControlSystem controlSystem)
		{
			SetControlSystem(controlSystem);
		}

		private void SetControlSystem(CrestronControlSystem controlSystem)
		{
			Unsubscribe(m_SubscribedControlSystem);
			m_SubscribedControlSystem = controlSystem;
			Subscribe(m_SubscribedControlSystem);

			if (m_SubscribedControlSystem == null || m_SubscribedControlSystem.SystemControl == null)
				return;

			if (m_SubscribedControlSystem.SystemControl.EnableAudioBreakaway.Supported)
				m_SubscribedControlSystem.SystemControl.EnableAudioBreakaway.BoolValue = true;

			if (m_SubscribedControlSystem.SystemControl.EnableUSBBreakaway.Supported)
				m_SubscribedControlSystem.SystemControl.EnableUSBBreakaway.BoolValue = true;
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
		/// Called when an input state changes.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		private void ControlSystemOnDmInputChange(Switch device, DMInputEventArgs args)
		{
			eConnectionType type = DmUtils.DmEventToConnectionType(args.EventId);

			foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(type))
				SourceDetectionChange((int)args.Number, flag);
		}

		/// <summary>
		/// Handles the detection change for individual connection types.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		private void SourceDetectionChange(int input, eConnectionType type)
		{
			bool state = GetSignalDetectedState(input, type);
			m_Cache.SetSourceDetectedState(input, type, state);
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
			int? input = GetInputs(output, type).Select(c => c.Address)
			                                    .FirstOrDefault();

			m_Cache.SetInputForOutput(output, input, type);
		}

		#endregion

		#region Cache Callbacks

		private void CacheOnRouteChange(object sender, RouteChangeEventArgs args)
		{
			OnRouteChange.Raise(this, new RouteChangeEventArgs(args.Output, args.Type));
		}

		private void CacheOnActiveTransmissionStateChanged(object sender, TransmissionStateEventArgs args)
		{
			OnActiveTransmissionStateChanged.Raise(this, new TransmissionStateEventArgs(args.Output, args.Type, args.State));
		}

		private void CacheOnSourceDetectionStateChange(object sender, SourceDetectionStateChangeEventArgs args)
		{
			OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(args.Input, args.Type, args.State));
		}

		private void CacheOnActiveInputsChanged(object sender, ActiveInputStateChangeEventArgs args)
		{
			OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(args.Input, args.Type, args.Active));
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
			       m_SubscribedControlSystem.SystemControl.EnableAudioBreakaway.Supported
				       ? m_SubscribedControlSystem.SystemControl.EnableAudioBreakawayFeedback.BoolValue.ToString()
				       : "Not Supported");
			addRow("USB Breakaway",
			       m_SubscribedControlSystem.SystemControl.EnableUSBBreakaway.Supported
				       ? m_SubscribedControlSystem.SystemControl.EnableUSBBreakawayFeedback.BoolValue.ToString()
				       : "Not Supported");
		}

		#endregion
	}
}
#endif