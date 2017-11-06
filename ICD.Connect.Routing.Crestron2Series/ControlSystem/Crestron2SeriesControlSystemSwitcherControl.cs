using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Protocol.EventArguments;
using ICD.Connect.Protocol.XSig;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Utils;

namespace ICD.Connect.Routing.Crestron2Series.ControlSystem
{
	public sealed class Crestron2SeriesControlSystemSwitcherControl : AbstractRouteSwitcherControl<Crestron2SeriesControlSystem>
	{
		private const ushort ANALOG_VIDEO_HDMI_OUT_1 = 1;
		private const ushort ANALOG_VIDEO_HDMI_OUT_2 = 2;
		private const ushort ANALOG_VIDEO_DM_OUT_3 = 3;
		private const ushort ANALOG_VIDEO_DM_OUT_4 = 4;

		private const ushort ANALOG_AUDIO_HDMI_OUT_1 = 5;
		private const ushort ANALOG_AUDIO_HDMI_OUT_2 = 6;
		private const ushort ANALOG_AUDIO_DM_OUT_3 = 7;
		private const ushort ANALOG_AUDIO_DM_OUT_4 = 8;
		private const ushort ANALOG_AUDIO_PROG_OUT_5 = 9;
		private const ushort ANALOG_AUDIO_AUX1_OUT_6 = 10;
		private const ushort ANALOG_AUDIO_AUX2_OUT_7 = 11;

		private const ushort DIGITAL_VIDEO_DETECTED_1 = 1;
		private const ushort DIGITAL_VIDEO_DETECTED_2 = 2;
		private const ushort DIGITAL_VIDEO_DETECTED_3 = 3;
		private const ushort DIGITAL_VIDEO_DETECTED_4 = 4;
		private const ushort DIGITAL_VIDEO_DETECTED_5 = 5;
		private const ushort DIGITAL_VIDEO_DETECTED_6 = 6;
		private const ushort DIGITAL_VIDEO_DETECTED_7 = 7;

		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;
		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;
		public override event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;
		public override event EventHandler<RouteChangeEventArgs> OnRouteChange;

		// Keeps track of source detection
		private readonly SwitcherCache m_Cache;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		public Crestron2SeriesControlSystemSwitcherControl(Crestron2SeriesControlSystem parent)
			: base(parent, 0)
		{
			m_Cache = new SwitcherCache();
			m_Cache.OnActiveInputsChanged += CacheOnActiveInputsChanged;
			m_Cache.OnSourceDetectionStateChange += CacheOnSourceDetectionStateChange;
			m_Cache.OnActiveTransmissionStateChanged += CacheOnActiveTransmissionStateChanged;
			m_Cache.OnRouteChange += CacheOnRouteChange;

			Subscribe(parent);
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

			throw new NotImplementedException();

			/*
			DMOutput switcherOutput = Parent.GetDmOutput(output);
			DMInput switcherInput = Parent.GetDmInput(input);

			switch (type)
			{
				case eConnectionType.Audio:
					switcherOutput.AudioOut = switcherInput;
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
			 */
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

			throw new NotImplementedException();
			/*
			DMOutput switcherOutput = Parent.GetDmOutput(output);

			switch (type)
			{
				case eConnectionType.Video:
					switcherOutput.VideoOut = null;
					break;

				case eConnectionType.Audio:
					switcherOutput.AudioOut = null;
					break;

				case eConnectionType.Usb:
					switcherOutput.USBRoutedTo = null;
					break;

				default:
					throw new ArgumentOutOfRangeException("type", string.Format("Unexpected value {0}", type));
			}

			m_Cache.SetInputForOutput(output, null, type);
			return true;
			 */
		}

		/// <summary>
		/// Gets the connector info for the output at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override ConnectorInfo GetOutput(int address)
		{
			throw new NotImplementedException();
			/*
			eCardInputOutputType type = Parent.GetDmOutput(address).CardInputOutputType;
			return new ConnectorInfo(address, GetConnectionType(type));
			 */
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			throw new NotImplementedException();
			/*
			IEnumerable<int> addresses = Parent.ControlSystem.SupportsSwitcherOutputs
				                             ? Enumerable.Range(1, Parent.ControlSystem.NumberOfSwitcherOutputs)
				                             : Enumerable.Empty<int>();

			return addresses.Select(i => GetOutput(i)).Where(c => c.ConnectionType != eConnectionType.None);
			 */
		}

		/// <summary>
		/// Gets the connector info for the input at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override ConnectorInfo GetInput(int address)
		{
			throw new NotImplementedException();

			/*
			eCardInputOutputType type = Parent.GetDmInput(address).CardInputOutputType;
			return new ConnectorInfo(address, GetConnectionType(type));
			 */
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			throw new NotImplementedException();

			/*
			IEnumerable<int> addresses = Parent.ControlSystem.SupportsSwitcherInputs
				                             ? Enumerable.Range(1, Parent.ControlSystem.NumberOfSwitcherInputs)
				                             : Enumerable.Empty<int>();

			return addresses.Select(i => GetInput(i)).Where(c => c.ConnectionType != eConnectionType.None);
			 */
		}

		/// <summary>
		/// Gets the input for the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs(int output, eConnectionType type)
		{
			return m_Cache.GetInputsForOutput(output, type);
		}

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override bool ContainsInput(int input)
		{
			throw new NotImplementedException();

			/*
			if (!Parent.ControlSystem.SupportsSwitcherInputs)
				return false;
			return input > 0 && input <= Parent.ControlSystem.NumberOfSwitcherInputs;
			 */
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
			throw new NotImplementedException();

			/*
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
					return switcherInput.VideoDetectedFeedback != null && switcherInput.VideoDetectedFeedback.BoolValue;

				case eConnectionType.Audio:
					// No way of detecting audio?
					return true;

				case eConnectionType.Usb:
					return switcherInput.USBRoutedToFeedback != null && switcherInput.USBRoutedToFeedback.EndpointOnlineFeedback;

				default:
					return false;
			}
			 */
		}

		/// <summary>
		/// Gets the input for the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private IEnumerable<ConnectorInfo> GetInputsFeedback(int output, eConnectionType type)
		{
			throw new NotImplementedException();

			/*
			DMOutput switcherOutput = Parent.GetDmOutput(output);

			foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(type))
			{
				DMInputOutputBase input;

				switch (flag)
				{
					case eConnectionType.Audio:
						input = switcherOutput.GetSafeAudioOutFeedback();
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
			 */
		}

		#endregion

		#region Parent Callbacks

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="parent"></param>
		private void Subscribe(Crestron2SeriesControlSystem parent)
		{
			parent.OnSigEvent += ParentOnSigEvent;
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		private void Unsubscribe(Crestron2SeriesControlSystem parent)
		{
			parent.OnSigEvent -= ParentOnSigEvent;
		}

		/// <summary>
		/// Called on sig change from the parent.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="xSigEventArgs"></param>
		private void ParentOnSigEvent(object sender, XSigEventArgs xSigEventArgs)
		{
			if (xSigEventArgs.Data is AnalogXSig)
				HandleAnalogSigEvent((AnalogXSig)xSigEventArgs.Data);

			if (xSigEventArgs.Data is DigitalXSig)
				HandleDigitalSigEvent((DigitalXSig)xSigEventArgs.Data);
		}

		private void HandleAnalogSigEvent(AnalogXSig data)
		{
			switch (data.Index)
			{
				case (ANALOG_VIDEO_HDMI_OUT_1):
				case (ANALOG_VIDEO_HDMI_OUT_2):
				case (ANALOG_VIDEO_DM_OUT_3):
				case (ANALOG_VIDEO_DM_OUT_4):
				case (ANALOG_AUDIO_HDMI_OUT_1):
				case (ANALOG_AUDIO_HDMI_OUT_2):
				case (ANALOG_AUDIO_DM_OUT_3):
				case (ANALOG_AUDIO_DM_OUT_4):
				case (ANALOG_AUDIO_PROG_OUT_5):
				case (ANALOG_AUDIO_AUX1_OUT_6):
				case (ANALOG_AUDIO_AUX2_OUT_7):
					throw new NotImplementedException();
			}
		}

		private void HandleDigitalSigEvent(DigitalXSig data)
		{
			switch (data.Index)
			{
				case (DIGITAL_VIDEO_DETECTED_1):
				case (DIGITAL_VIDEO_DETECTED_2):
				case (DIGITAL_VIDEO_DETECTED_3):
				case (DIGITAL_VIDEO_DETECTED_4):
				case (DIGITAL_VIDEO_DETECTED_5):
				case (DIGITAL_VIDEO_DETECTED_6):
				case (DIGITAL_VIDEO_DETECTED_7):
					throw new NotImplementedException();
			}
		}

		#endregion

		#region Cache Callbacks

		private void CacheOnRouteChange(object sender, RouteChangeEventArgs args)
		{
			OnRouteChange.Raise(this, new RouteChangeEventArgs(args));
		}

		private void CacheOnActiveTransmissionStateChanged(object sender, TransmissionStateEventArgs args)
		{
			OnActiveTransmissionStateChanged.Raise(this, new TransmissionStateEventArgs(args));
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