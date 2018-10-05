using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Commands;
using ICD.Connect.Protocol.EventArguments;
using ICD.Connect.Protocol.XSig;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Utils;

namespace ICD.Connect.Routing.Crestron2Series.Devices.ControlSystem
{
	public sealed class Dmps300CControlSystemSwitcherControl : AbstractRouteSwitcherControl<Dmps300CControlSystem>
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

		private const ushort DIGITAL_VIDEO_DETECTED_1 = 401;
		private const ushort DIGITAL_VIDEO_DETECTED_2 = 402;
		private const ushort DIGITAL_VIDEO_DETECTED_3 = 403;
		private const ushort DIGITAL_VIDEO_DETECTED_4 = 404;
		private const ushort DIGITAL_VIDEO_DETECTED_5 = 405;
		private const ushort DIGITAL_VIDEO_DETECTED_6 = 406;
		private const ushort DIGITAL_VIDEO_DETECTED_7 = 407;
		private bool m_debug;

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

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		public Dmps300CControlSystemSwitcherControl(Dmps300CControlSystem parent)
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
			return EnumUtils.GetFlagsExceptNone(type)
			                .Select(t => m_Cache.GetSourceDetectedState(input, t))
			                .Unanimous(false);
		}

		/// <summary>
		/// Routes the input to the given output.
		/// </summary>
		/// <param name="info"></param>
		/// <returns>True if routing successful.</returns>
		public override bool Route(RouteOperation info)
		{
			if(m_debug)
				Log(eSeverity.Debug, "DMPS route input {0} -> output {1}, {2}", info.LocalInput, info.LocalOutput, info.ConnectionType);

			if (info == null)
				throw new ArgumentNullException("info");

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

			ushort index = GetOutputIndex(output, type);
			return Parent.SendData(new AnalogXSig((ushort)input, index)) &&
			       m_Cache.SetInputForOutput(output, input, type);
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

			ushort index = GetOutputIndex(output, type);

			if (!Parent.SendData(new AnalogXSig(0, index)))
				return false;

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
			switch (address)
			{
				case 1:
				case 2:
				case 3:
				case 4:
					return new ConnectorInfo(address, eConnectionType.Audio | eConnectionType.Video);
				
				case 5:
				case 6:
				case 7:
					return new ConnectorInfo(address, eConnectionType.Audio);

				default:
					throw new ArgumentOutOfRangeException("address");
			}
		}

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override bool ContainsOutput(int output)
		{
			return output >= 1 && output <= 7;
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			return Enumerable.Range(1, 7).Select(i => GetOutput(i));
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
			switch (address)
			{
				case 1:
				case 2:
				case 3:
				case 4:
				case 5:
				case 6:
				case 7:
					return new ConnectorInfo(address, eConnectionType.Audio | eConnectionType.Video);

				default:
					throw new ArgumentOutOfRangeException("address");
			}
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			return Enumerable.Range(1, 7).Select(i => GetInput(i));
		}

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override bool ContainsInput(int input)
		{
			return input >= 1 && input <= 7;
		}

		#endregion

		/// <summary>
		/// Gets the sig index for the given output and type.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private ushort GetOutputIndex(int output, eConnectionType type)
		{
			switch (type)
			{
				case eConnectionType.Audio:
					switch (output)
					{
						case 1:
							return ANALOG_AUDIO_HDMI_OUT_1;
						case 2:
							return ANALOG_AUDIO_HDMI_OUT_2;
						case 3:
							return ANALOG_AUDIO_DM_OUT_3;
						case 4:
							return ANALOG_AUDIO_DM_OUT_4;
						case 5:
							return ANALOG_AUDIO_PROG_OUT_5;
						case 6:
							return ANALOG_AUDIO_AUX1_OUT_6;
						case 7:
							return ANALOG_AUDIO_AUX2_OUT_7;
						default:
							throw new ArgumentOutOfRangeException("output");
					}

				case eConnectionType.Video:
					switch (output)
					{
						case 1:
							return ANALOG_VIDEO_HDMI_OUT_1;
						case 2:
							return ANALOG_VIDEO_HDMI_OUT_2;
						case 3:
							return ANALOG_VIDEO_DM_OUT_3;
						case 4:
							return ANALOG_VIDEO_DM_OUT_4;

						default:
							throw new ArgumentOutOfRangeException("output");
					}
				default:
					throw new ArgumentOutOfRangeException("type");
			}
		}

		#region Parent Callbacks

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="parent"></param>
		private void Subscribe(Dmps300CControlSystem parent)
		{
			parent.OnSigEvent += ParentOnSigEvent;
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		private void Unsubscribe(Dmps300CControlSystem parent)
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
			int? input = data.Value == 0 ? (int?)null : data.Value;

			switch (data.Index)
			{
				case ANALOG_VIDEO_HDMI_OUT_1:
					m_Cache.SetInputForOutput(1, input, eConnectionType.Video);
					break;
				case ANALOG_VIDEO_HDMI_OUT_2:
					m_Cache.SetInputForOutput(2, input, eConnectionType.Video);
					break;
				case ANALOG_VIDEO_DM_OUT_3:
					m_Cache.SetInputForOutput(3, input, eConnectionType.Video);
					break;
				case ANALOG_VIDEO_DM_OUT_4:
					m_Cache.SetInputForOutput(4, input, eConnectionType.Video);
					break;
				case ANALOG_AUDIO_HDMI_OUT_1:
					m_Cache.SetInputForOutput(1, input, eConnectionType.Audio);
					break;
				case ANALOG_AUDIO_HDMI_OUT_2:
					m_Cache.SetInputForOutput(2, input, eConnectionType.Audio);
					break;
				case ANALOG_AUDIO_DM_OUT_3:
					m_Cache.SetInputForOutput(3, input, eConnectionType.Audio);
					break;
				case ANALOG_AUDIO_DM_OUT_4:
					m_Cache.SetInputForOutput(4, input, eConnectionType.Audio);
					break;
				case ANALOG_AUDIO_PROG_OUT_5:
					m_Cache.SetInputForOutput(5, input, eConnectionType.Audio);
					break;
				case ANALOG_AUDIO_AUX1_OUT_6:
					m_Cache.SetInputForOutput(6, input, eConnectionType.Audio);
					break;
				case ANALOG_AUDIO_AUX2_OUT_7:
					m_Cache.SetInputForOutput(7, input, eConnectionType.Audio);
					break;
			}
		}

		private void HandleDigitalSigEvent(DigitalXSig data)
		{
			switch (data.Index)
			{
				case DIGITAL_VIDEO_DETECTED_1:
					m_Cache.SetSourceDetectedState(1, eConnectionType.Video, data.Value);
					break;
				case DIGITAL_VIDEO_DETECTED_2:
					m_Cache.SetSourceDetectedState(2, eConnectionType.Video, data.Value);
					break;
				case DIGITAL_VIDEO_DETECTED_3:
					m_Cache.SetSourceDetectedState(3, eConnectionType.Video, data.Value);
					break;
				case DIGITAL_VIDEO_DETECTED_4:
					m_Cache.SetSourceDetectedState(4, eConnectionType.Video, data.Value);
					break;
				case DIGITAL_VIDEO_DETECTED_5:
					m_Cache.SetSourceDetectedState(5, eConnectionType.Video, data.Value);
					break;
				case DIGITAL_VIDEO_DETECTED_6:
					m_Cache.SetSourceDetectedState(6, eConnectionType.Video, data.Value);
					break;
				case DIGITAL_VIDEO_DETECTED_7:
					m_Cache.SetSourceDetectedState(7, eConnectionType.Video, data.Value);
					break;
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

		#region Console

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (var cmd in GetBaseConsoleCommands())
				yield return cmd;

			yield return new ConsoleCommand("EnableSwitchNotification", "prints debug info when the dmps is asked to make a route", ()=> m_debug=true);
		}

		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion
	}
}