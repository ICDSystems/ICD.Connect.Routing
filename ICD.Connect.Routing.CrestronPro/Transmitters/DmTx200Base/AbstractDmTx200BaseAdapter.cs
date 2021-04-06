using System;
using System.Collections.Generic;
using System.Linq;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
using ICD.Connect.Misc.CrestronPro.Devices;
using ICD.Connect.Misc.CrestronPro.Extensions;
#endif
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200Base
{
	/// <summary>
	/// Base class for DmTx200 device adapters.
	/// </summary>
	/// <typeparam name="TSettings"></typeparam>
#if SIMPLSHARP
	/// <typeparam name="TTransmitter"></typeparam>
	public abstract class AbstractDmTx200BaseAdapter<TTransmitter, TSettings> :
		AbstractEndpointTransmitterSwitcherBaseAdapter<TTransmitter, TSettings>, IDmTx200BaseAdapter
		where TTransmitter : Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base
#else
	public abstract class AbstractDmTx200BaseAdapter<TSettings> : AbstractEndpointTransmitterSwitcherBaseAdapter<TSettings>, IDmTx200BaseAdapter
#endif
		where TSettings : IDmTx200BaseAdapterSettings, new()
	{
		protected const int INPUT_HDMI = 1;
		protected const int INPUT_VGA = 2;
		private const int OUTPUT_DM = 1;

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
				return Transmitter != null && Transmitter.HdmiInput.SyncDetectedFeedback.GetBoolValueOrDefault();
#else
				return false;
#endif
			}
		}

		/// <summary>
		/// Returns true if a VGA input source is detected.
		/// </summary>
		[PublicAPI]
		public bool VgaDetected
		{
			get
			{
#if SIMPLSHARP
				return Transmitter != null && Transmitter.VgaInput.SyncDetectedFeedback.GetBoolValueOrDefault();
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
			private set
			{
				if (value == m_ActiveTransmissionState)
					return;

				m_ActiveTransmissionState = value;

				RaiseActiveTransmissionStateChanged(OUTPUT_DM, eConnectionType.Audio | eConnectionType.Video, m_ActiveTransmissionState);
			}
		}

		#endregion

		#region Methods

#if SIMPLSHARP
		/// <summary>
		/// Called when the wrapped transmitter is assigned.
		/// </summary>
		/// <param name="transmitter"></param>
		protected override void ConfigureTransmitter(TTransmitter transmitter)
		{
			if (transmitter == null)
				return;

			transmitter.VideoSource = Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Auto;
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

			if (io == eInputOuptut.Input && address == 1)
				return Transmitter.HdmiInput.StreamCec;

			string message = string.Format("No CecPort at address {1}:{2} for device {0}", this, io, address);
			throw new InvalidOperationException(message);
		}
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
			if (input != INPUT_HDMI && input != INPUT_VGA)
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
			return input == INPUT_HDMI || input == INPUT_VGA;
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			yield return GetInput(INPUT_HDMI);
			yield return GetInput(INPUT_VGA);
		}

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override ConnectorInfo GetOutput(int output)
		{
			if (output != OUTPUT_DM)
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
			return output == OUTPUT_DM;
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
					yield return GetOutput(OUTPUT_DM);
					break;

				default:
					throw new ArgumentException("type");
			}
		}

		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			yield return new ConnectorInfo(OUTPUT_DM, (eConnectionType.Audio | eConnectionType.Video));
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
							case INPUT_HDMI:
								Transmitter.AudioSource = Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Digital;
								break;

							case INPUT_VGA:
								Transmitter.AudioSource = Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Analog;
								break;

							default:
								throw new IndexOutOfRangeException(string.Format("No input at address {0}", info.LocalInput));
						}
						break;
					case eConnectionType.Video:
						switch (info.LocalInput)
						{
							case INPUT_HDMI:
								Transmitter.VideoSource = Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Digital;
								break;

							case INPUT_VGA:
								Transmitter.VideoSource = Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Analog;
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

			Transmitter.VideoSource = Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Disable;
			Transmitter.AudioSource = Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Disable;
			return true;
#endif
			return false;
		}

		#endregion

		#region Transmitter Callbacks
#if SIMPLSHARP
		/// <summary>
		/// Subscribes to the transmitter events.
		/// </summary>
		/// <param name="transmitter"></param>
		protected override void Subscribe(TTransmitter transmitter)
		{
			base.Subscribe(transmitter);

			if (transmitter == null)
				return;

			transmitter.HdmiInput.InputStreamChange += HdmiInputOnInputStreamChange;
			transmitter.VgaInput.InputStreamChange += VgaInputOnInputStreamChange;
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

			transmitter.HdmiInput.InputStreamChange -= HdmiInputOnInputStreamChange;
			transmitter.VgaInput.InputStreamChange -= VgaInputOnInputStreamChange;
			transmitter.BaseEvent -= TransmitterOnBaseEvent;
		}

		/// <summary>
		/// Called when the VGA input stream changes.
		/// </summary>
		/// <param name="inputStream"></param>
		/// <param name="args"></param>
		private void VgaInputOnInputStreamChange(EndpointInputStream inputStream, EndpointInputStreamEventArgs args)
		{
			UpdateActiveTransmissionState();

			SwitcherCache.SetSourceDetectedState(INPUT_VGA, eConnectionType.Video, VgaDetected);
			SwitcherCache.SetSourceDetectedState(INPUT_VGA, eConnectionType.Audio, VgaDetected);
		}

		/// <summary>
		/// Called when the HDMI input stream changes.
		/// </summary>
		/// <param name="inputStream"></param>
		/// <param name="args"></param>
		private void HdmiInputOnInputStreamChange(EndpointInputStream inputStream, EndpointInputStreamEventArgs args)
		{
			UpdateActiveTransmissionState();

			SwitcherCache.SetSourceDetectedState(INPUT_HDMI, eConnectionType.Video, HdmiDetected);
			SwitcherCache.SetSourceDetectedState(INPUT_HDMI, eConnectionType.Audio, HdmiDetected);
		}

		private void UpdateActiveTransmissionState()
		{
			ActiveTransmissionState = HdmiDetected || VgaDetected;
		}

		/// <summary>
		/// Called when the the transmitter raises an event.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		protected virtual void TransmitterOnBaseEvent(GenericBase device, BaseEventArgs args)
		{
			if (args.EventId == EndpointTransmitterBase.AudioSourceFeedbackEventId ||
				args.EventId == EndpointTransmitterBase.VideoSourceFeedbackEventId)
				UpdateActiveTransmissionState();

			TTransmitter transmitter = device as TTransmitter;
			if (transmitter == null)
				return;

			if (args.EventId == DMOutputEventIds.ContentLanModeEventId)
			{
				// Disable Free-Run
				transmitter.VgaInput.FreeRun = eDmFreeRunSetting.Disabled;
			}

			// Ensure auto-routing if applicable
			if (UseAutoRouting)
			{
				transmitter.VideoSource = Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Auto;
				transmitter.AudioSource = Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Auto;
			}

			switch (transmitter.VideoSourceFeedback)
			{
				case Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Digital:
					SwitcherCache.SetInputForOutput(OUTPUT_DM, INPUT_HDMI, eConnectionType.Video);
					break;
				case Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Analog:
					SwitcherCache.SetInputForOutput(OUTPUT_DM, INPUT_VGA, eConnectionType.Video);
					break;
				case Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Auto:
				case Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Disable:
					SwitcherCache.SetInputForOutput(OUTPUT_DM, null, eConnectionType.Video);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			switch (transmitter.AudioSourceFeedback)
			{
				case Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Digital:
					SwitcherCache.SetInputForOutput(OUTPUT_DM, INPUT_HDMI, eConnectionType.Audio);
					break;
				case Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Analog:
					SwitcherCache.SetInputForOutput(OUTPUT_DM, INPUT_VGA, eConnectionType.Audio);
					break;
				case Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Auto:
				case Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Disable:
					SwitcherCache.SetInputForOutput(OUTPUT_DM, null, eConnectionType.Audio);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
#endif
		#endregion

	}
}
