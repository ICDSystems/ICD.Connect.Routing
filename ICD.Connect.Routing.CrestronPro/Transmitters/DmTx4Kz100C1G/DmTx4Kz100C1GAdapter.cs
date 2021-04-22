using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Settings;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
using ICD.Connect.Misc.CrestronPro.Devices;
using ICD.Connect.Misc.CrestronPro.Extensions;
#endif

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4Kz100C1G
{
	/// <summary>
	/// DmTx4Kz100C1GAdapter wraps a DmTx4kz100C1G to provide a routing device.
	/// </summary>
#if SIMPLSHARP
	public sealed class DmTx4Kz100C1GAdapter : AbstractEndpointTransmitterBaseAdapter<DmTx4kz100C1G, DmTx4Kz100C1GAdapterSettings>
#else
	public sealed class DmTx4Kz100C1GAdapter : AbstractEndpointTransmitterBaseAdapter<DmTx4Kz100C1GAdapterSettings>
#endif
	{
		private const int OUTPUT_DM = 1;
		private const int INPUT_HDMI = 1;

		private bool m_ActiveTransmissionState;

		public override event EventHandler<RouteChangeEventArgs> OnRouteChange;

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
		/// Instantiates the transmitter with the given IPID against the control system.
		/// </summary>
		/// <param name="ipid"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		public override DmTx4kz100C1G InstantiateTransmitter(byte ipid, CrestronControlSystem controlSystem)
		{
			return new DmTx4kz100C1G(ipid, controlSystem);
		}

		/// <summary>
		/// Instantiates the transmitter against the given DM Input and configures it with the given IPID.
		/// </summary>
		/// <param name="ipid"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public override DmTx4kz100C1G InstantiateTransmitter(byte ipid, DMInput input)
		{
			return new DmTx4kz100C1G(ipid, input);
		}

		/// <summary>
		/// Instantiates the transmitter against the given DM Input.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override DmTx4kz100C1G InstantiateTransmitter(DMInput input)
		{
			return new DmTx4kz100C1G(input);
		}

		/// <summary>
		/// Override to implement AutoRouting on the transmitter
		/// </summary>
		protected override void SetTransmitterAutoRoutingFinal()
		{
			//No Switching on this Tx
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
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(DmTx4Kz100C1GAdapterSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new RouteMidpointControl(this, 0));
		}

		#endregion

		#region Routing Methods

		/// <summary>
		/// Gets the input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override ConnectorInfo GetInput(int input)
		{
			if (input != INPUT_HDMI)
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
			return input == INPUT_HDMI;
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			yield return GetInput(INPUT_HDMI);
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
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			yield return GetOutput(OUTPUT_DM);
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

			switch (type)
			{
				case eConnectionType.Audio:
				case eConnectionType.Video:
				case eConnectionType.Audio | eConnectionType.Video:
					return GetInput(INPUT_HDMI);

				default:
					throw new ArgumentException("type");
			}
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

		/// <summary>
		/// Returns true if a signal is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool GetSignalDetectedState(int input, eConnectionType type)
		{
#if SIMPLSHARP
			return Transmitter.HdmiInput.SyncDetectedFeedback.BoolValue;
#else
			return false;
#endif
		}

		#endregion

#if SIMPLSHARP
		#region Transmitter Callbacks

		/// <summary>
		/// Subscribes to the transmitter events.
		/// </summary>
		/// <param name="transmitter"></param>
		protected override void Subscribe(DmTx4kz100C1G transmitter)
		{
			base.Subscribe(transmitter);

			if (transmitter == null)
				return;

			transmitter.HdmiInput.InputStreamChange += HdmiInputOnInputStreamChange;
			transmitter.BaseEvent += TransmitterOnBaseEvent;
		}

		/// <summary>
		/// Unsubscribes from the transmitter events.
		/// </summary>
		/// <param name="transmitter"></param>
		protected override void Unsubscribe(DmTx4kz100C1G transmitter)
		{
			base.Unsubscribe(transmitter);

			if (transmitter == null)
				return;

			transmitter.HdmiInput.InputStreamChange -= HdmiInputOnInputStreamChange;
			transmitter.BaseEvent -= TransmitterOnBaseEvent;
		}

		/// <summary>
		/// Called when the HDMI input stream changes.
		/// </summary>
		/// <param name="inputStream"></param>
		/// <param name="args"></param>
		private void HdmiInputOnInputStreamChange(EndpointInputStream inputStream, EndpointInputStreamEventArgs args)
		{
			UpdateActiveTransmissionState();
		}

		private void UpdateActiveTransmissionState()
		{
			ActiveTransmissionState = HdmiDetected;
		}

		/// <summary>
		/// Called when the the transmitter raises an event.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		private void TransmitterOnBaseEvent(GenericBase device, BaseEventArgs args)
		{
			if (args.EventId == EndpointTransmitterBase.AudioSourceFeedbackEventId ||
				args.EventId == EndpointTransmitterBase.VideoSourceFeedbackEventId)
				UpdateActiveTransmissionState();

			if (args.EventId != DMOutputEventIds.ContentLanModeEventId)
				return;

			DmTx4kz100C1G transmitter = device as DmTx4kz100C1G;
			if (transmitter == null)
				return;

			// Ensure the device stays in auto routing mode
			transmitter.VideoSource = Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Auto;
		}

		#endregion
#endif
	}
}