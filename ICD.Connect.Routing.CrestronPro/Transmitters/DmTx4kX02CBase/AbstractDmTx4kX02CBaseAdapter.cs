using System;
using System.Collections.Generic;
using System.Linq;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
using ICD.Connect.Misc.CrestronPro.Extensions;
using ICD.Connect.Misc.CrestronPro.Devices;
using ICD.Connect.API.Nodes;
#endif
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4kX02CBase
{
#if SIMPLSHARP
	public abstract class AbstractDmTx4kX02CBaseAdapter<TTransmitter, TSettings> :
		AbstractEndpointTransmitterSwitcherBaseAdapter<TTransmitter, TSettings>, IDmTx4kX02CBaseAdapter
		where TTransmitter : Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx4kX02CBase
#else
	public abstract class AbstractDmTx4kX02CBaseAdapter<TSettings> :
		AbstractEndpointTransmitterSwitcherBaseAdapter<TSettings>, IDmTx4kX02CBaseAdapter
#endif
		where TSettings : IDmTx4kX02CBaseAdapterSettings, new()
	{
		protected const int HDMI_INPUT_1 = 1;
		protected const int HDMI_INPUT_2 = 2;
		protected const int DM_OUTPUT = 1;
		protected const int HDMI_OUTPUT = 2;

		private bool m_ActiveTransmissionState;

		#region Properties

		/// <summary>
		/// Returns true if an HDMI input source is detected.
		/// </summary>
		[PublicAPI]
		public bool HdmiDetected
		{
			get
			{
#if SIMPLSHARP
				return Transmitter.HdmiInputs[HDMI_INPUT_1].SyncDetectedFeedback.GetBoolValueOrDefault() ||
					   Transmitter.HdmiInputs[HDMI_INPUT_2].SyncDetectedFeedback.GetBoolValueOrDefault();
#else
				return false;
#endif
			}
		}

		/// <summary>
		/// Returns true when the device is actively transmitting video.
		/// </summary>
		[PublicAPI]
		public bool ActiveTransmissionState
		{
			get { return m_ActiveTransmissionState; }
			protected set
			{
				if (value == m_ActiveTransmissionState)
					return;

				m_ActiveTransmissionState = value;

				RaiseActiveTransmissionStateChanged(HDMI_OUTPUT,
													  eConnectionType.Audio | eConnectionType.Video,
													  m_ActiveTransmissionState);
			}
		}

		#endregion

		#region Methods

#if SIMPLSHARP
#endif
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

			return SwitcherCache.GetSourceDetectedState(input, type);
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
		/// Gets the input routed to the given output matching the given type.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">Type has multiple flags.</exception>
		public override ConnectorInfo? GetInput(int output, eConnectionType type)
		{
			if (!ContainsOutput(output))
				throw new ArgumentException(string.Format("{0} has no output at address {1}", this, output));

			if (EnumUtils.HasMultipleFlags(type))
				throw new ArgumentException("Type must have a single flag", "type");

			return SwitcherCache.GetInputConnectorInfoForOutput(output, type);
		}

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override bool ContainsInput(int input)
		{
			return input == HDMI_INPUT_1 || input == HDMI_INPUT_2;
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			yield return GetInput(HDMI_INPUT_1);
			yield return GetInput(HDMI_INPUT_2);
		}

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override ConnectorInfo GetOutput(int output)
		{
			if (!ContainsOutput(output))
				throw new ArgumentOutOfRangeException("output");

			return new ConnectorInfo(output, eConnectionType.Audio | eConnectionType.Video);
		}

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override bool ContainsOutput(int output)
		{
			return output == DM_OUTPUT || output == HDMI_OUTPUT;
		}

		/// <summary>
		/// Gets the outputs for the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs(int input, eConnectionType type)
		{
			if (!ContainsInput(input))
				throw new ArgumentException(string.Format("{0} has no output at address {1}", this, input));

			switch (type)
			{
				case eConnectionType.Audio:
				case eConnectionType.Video:
				case eConnectionType.Audio | eConnectionType.Video:
					yield return GetOutput(DM_OUTPUT);
					yield return GetOutput(HDMI_OUTPUT);
					break;

				default:
					throw new ArgumentException("type");
			}
		}

		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			yield return new ConnectorInfo(DM_OUTPUT, (eConnectionType.Audio | eConnectionType.Video));
			yield return new ConnectorInfo(HDMI_OUTPUT, (eConnectionType.Audio | eConnectionType.Video));
		}

		public override bool GetActiveTransmissionState(int output, eConnectionType type)
		{
			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
								.Select(f => GetActiveTransmissionState(output, f))
								.Unanimous(false);
			}

			if (!ContainsOutput(output))
			{
				string message = string.Format("{0} has no {1} output at address {2}", this, type, output);
				throw new ArgumentOutOfRangeException("output", message);
			}

			switch (type)
			{
				case eConnectionType.Audio:
				case eConnectionType.Video:
					return ActiveTransmissionState;

				default:
					throw new ArgumentOutOfRangeException("type");
			}
		}

		/// <summary>
		/// Performs the given route operation.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		public override bool Route(RouteOperation info)
		{
			if (!ContainsInput(info.LocalInput))
				throw new IndexOutOfRangeException(string.Format("No input at address {0}", info.LocalInput));
			if (!ContainsOutput(info.LocalOutput))
				throw new IndexOutOfRangeException(string.Format("No output at address {0}", info.LocalOutput));
#if SIMPLSHARP
			if (Transmitter == null)
				throw new InvalidOperationException("No DmTx instantiated");

			foreach (eConnectionType type in EnumUtils.GetFlagsExceptNone(info.ConnectionType))
			{
				switch (type)
				{
					case eConnectionType.Audio:
						switch (info.LocalInput)
						{
							case HDMI_INPUT_1:
								Transmitter.AudioSource = eX02AudioSourceType.Hdmi1;
								break;

							case HDMI_INPUT_2:
								Transmitter.AudioSource = eX02AudioSourceType.Hdmi2;
								break;

							default:
								throw new IndexOutOfRangeException(string.Format("No input at address {0}", info.LocalInput));
						}
						break;
					case eConnectionType.Video:
						switch (info.LocalInput)
						{
							case HDMI_INPUT_1:
								Transmitter.VideoSource = eX02VideoSourceType.Hdmi1;
								break;

							case HDMI_INPUT_2:
								Transmitter.VideoSource = eX02VideoSourceType.Hdmi2;
								break;

							default:
								throw new IndexOutOfRangeException(string.Format("No input at address {0}", info.LocalInput));
						}
						break;
					default:
						throw new InvalidOperationException("Connection type unsupported");
				}
			}
			return true;
#endif
			return false;
		}

		/// <summary>
		/// Stops routing to the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns>True if successfully cleared.</returns>
		public override bool ClearOutput(int output, eConnectionType type)
		{
#if SIMPLSHARP
			if (Transmitter == null)
				throw new InvalidOperationException("No DmTx instantiated");

			Transmitter.VideoSource = eX02VideoSourceType.AllDisabled;
			Transmitter.AudioSource = eX02AudioSourceType.AllDisabled;
			return true;
#endif
			return false;
		}

		protected virtual bool GetActiveTransmissionState()
		{
			return HdmiDetected;
		}

#endregion

#if SIMPLSHARP
#region Transmitter Callbacks

		/// <summary>
		/// Subscribes to the transmitter events.
		/// </summary>
		/// <param name="transmitter"></param>
		protected override void Subscribe(TTransmitter transmitter)
		{
			base.Subscribe(transmitter);

			if (transmitter == null)
				return;

			transmitter.HdmiInputs[HDMI_INPUT_1].InputStreamChange += HdmiInputOnInputStreamChange;
			transmitter.HdmiInputs[HDMI_INPUT_2].InputStreamChange += HdmiInputOnInputStreamChange;
			transmitter.BaseEvent += TransmitterOnBaseEvent;
		}

		/// <summary>
		/// Unsubscribes from the transmitter events.
		/// </summary>
		/// <param name="transmitter"></param>
		protected override void Unsubscribe(TTransmitter transmitter)
		{
			base.Unsubscribe(transmitter);

			if (transmitter == null)
				return;

			transmitter.HdmiInputs[HDMI_INPUT_1].InputStreamChange -= HdmiInputOnInputStreamChange;
			transmitter.HdmiInputs[HDMI_INPUT_2].InputStreamChange -= HdmiInputOnInputStreamChange;
			transmitter.BaseEvent -= TransmitterOnBaseEvent;
		}

		/// <summary>
		/// Called when the HDMI input stream changes.
		/// </summary>
		/// <param name="inputStream"></param>
		/// <param name="args"></param>
		private void HdmiInputOnInputStreamChange(EndpointInputStream inputStream, EndpointInputStreamEventArgs args)
		{
			if (args.EventId == EndpointInputStreamEventIds.SyncDetectedFeedbackEventId)
			{
				ActiveTransmissionState = GetActiveTransmissionState();
				SwitcherCache.SetSourceDetectedState(HDMI_INPUT_1, eConnectionType.Video,
													 Transmitter.HdmiInputs[HDMI_INPUT_1].SyncDetectedFeedback.BoolValue);
				SwitcherCache.SetSourceDetectedState(HDMI_INPUT_1, eConnectionType.Audio,
													 Transmitter.HdmiInputs[HDMI_INPUT_1].SyncDetectedFeedback.BoolValue);
				SwitcherCache.SetSourceDetectedState(HDMI_INPUT_2, eConnectionType.Video,
													 Transmitter.HdmiInputs[HDMI_INPUT_2].SyncDetectedFeedback.BoolValue);
				SwitcherCache.SetSourceDetectedState(HDMI_INPUT_2, eConnectionType.Audio,
													 Transmitter.HdmiInputs[HDMI_INPUT_2].SyncDetectedFeedback.BoolValue);
			}
		}

		/// <summary>
		/// Called when the the transmitter raises an event.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		protected virtual void TransmitterOnBaseEvent(GenericBase device, BaseEventArgs args)
		{
			if (args.EventId != EndpointTransmitterBase.VideoSourceFeedbackEventId || args.EventId != EndpointTransmitterBase.AudioSourceFeedbackEventId)
				return;

			// Ensure the device stays in auto routing mode if applicable
			if (UseAutoRouting)
			{
				Transmitter.VideoSource = eX02VideoSourceType.Auto;
				Transmitter.AudioSource = eX02AudioSourceType.Auto;
			}

			switch (Transmitter.VideoSourceFeedback)
			{
				case eX02VideoSourceType.Hdmi1:
					SwitcherCache.SetInputForOutput(DM_OUTPUT, HDMI_INPUT_1, eConnectionType.Video);
					SwitcherCache.SetInputForOutput(HDMI_OUTPUT, HDMI_INPUT_1, eConnectionType.Video);
					break;
				case eX02VideoSourceType.Hdmi2:
					SwitcherCache.SetInputForOutput(DM_OUTPUT, HDMI_INPUT_2, eConnectionType.Video);
					SwitcherCache.SetInputForOutput(HDMI_OUTPUT, HDMI_INPUT_2, eConnectionType.Video);
					break;
				case eX02VideoSourceType.Auto:
				case eX02VideoSourceType.Vga:
				case eX02VideoSourceType.AllDisabled:
					SwitcherCache.SetInputForOutput(DM_OUTPUT, null, eConnectionType.Video);
					SwitcherCache.SetInputForOutput(HDMI_OUTPUT, null, eConnectionType.Video);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			switch (Transmitter.AudioSourceFeedback)
			{
				case eX02AudioSourceType.Hdmi1:
					SwitcherCache.SetInputForOutput(DM_OUTPUT, HDMI_INPUT_1, eConnectionType.Audio);
					SwitcherCache.SetInputForOutput(HDMI_OUTPUT, HDMI_INPUT_1, eConnectionType.Audio);
					break;
				case eX02AudioSourceType.Hdmi2:
					SwitcherCache.SetInputForOutput(DM_OUTPUT, HDMI_INPUT_2, eConnectionType.Audio);
					SwitcherCache.SetInputForOutput(HDMI_OUTPUT, HDMI_INPUT_2, eConnectionType.Audio);
					break;
				case eX02AudioSourceType.Auto:
				case eX02AudioSourceType.AudioIn:
				case eX02AudioSourceType.AllDisabled:
					SwitcherCache.SetInputForOutput(DM_OUTPUT, null, eConnectionType.Audio);
					SwitcherCache.SetInputForOutput(HDMI_OUTPUT, null, eConnectionType.Audio);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

#endregion

#region IO

		/// <summary>
		/// Gets the port at the given addres.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override ComPort GetComPort(int address)
		{
			if (Transmitter == null)
				throw new InvalidOperationException("No transmitter instantiated");

			if (address >= 1 && address <= Transmitter.NumberOfComPorts)
				return Transmitter.ComPorts[(uint)address];

			return base.GetComPort(address);
		}

		/// <summary>
		/// Gets the port at the given addres.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override IROutputPort GetIrOutputPort(int address)
		{
			if (Transmitter == null)
				throw new InvalidOperationException("No transmitter instantiated");

			if (address >= 1 && address <= Transmitter.NumberOfIROutputPorts)
				return Transmitter.IROutputPorts[(uint)address];

			return base.GetIrOutputPort(address);
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="io"></param>
		/// <param name="address"></param>
		/// <returns></returns>
		public override Cec GetCecPort(eInputOuptut io, int address)
		{
			if (Transmitter == null)
				throw new InvalidOperationException("No DmTx instantiated");

			if (io == eInputOuptut.Input)
			{
				switch (address)
				{
					case 1:
						return Transmitter.HdmiInputs[1].StreamCec;
					case 2:
						return Transmitter.HdmiInputs[2].StreamCec;
				}
			}
			if (io == eInputOuptut.Output && address == 1)
				return Transmitter.HdmiOutput.StreamCec;


			string message = string.Format("No CecPort at address {1}:{2} for device {0}", this, io, address);
			throw new InvalidOperationException(message);
		}

#endregion
#endif
#region Console

#if SIMPLSHARP
		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			if (Transmitter != null)
			{
				addRow("Source", Transmitter.VideoSourceFeedback);
				addRow("HDMI 1 Sync", Transmitter.HdmiInputs[HDMI_INPUT_1].SyncDetectedFeedback.GetBoolValueOrDefault());
				addRow("HDMI 2 Sync", Transmitter.HdmiInputs[HDMI_INPUT_2].SyncDetectedFeedback.GetBoolValueOrDefault());
			}
		}
#endif
#endregion
	}
}
