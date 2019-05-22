using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Connect.Misc.CrestronPro.Devices;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
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
		AbstractEndpointTransmitterBaseAdapter<TTransmitter, TSettings>, IDmTx200BaseAdapter
		where TTransmitter : Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base
#else
	public abstract class AbstractDmTx200BaseAdapter<TSettings> : AbstractEndpointTransmitterBaseAdapter<TSettings>, IDmTx200BaseAdapter
#endif
		where TSettings : IDmTx200BaseAdapterSettings, new()
	{
		private const int OUTPUT_HDMI = 1;

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

				RaiseOnActiveTransmissionStateChanged(OUTPUT_HDMI, eConnectionType.Audio | eConnectionType.Video, m_ActiveTransmissionState);
			}
		}

#endregion

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

#region Methods

		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			yield return new ConnectorInfo(1, (eConnectionType.Audio | eConnectionType.Video));
		}

		public override bool GetActiveTransmissionState(int output, eConnectionType type)
		{
			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
				                .Select(f => GetActiveTransmissionState(output, f))
				                .Unanimous(false);
			}

			if (output != 1)
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
			ActiveTransmissionState = HdmiDetected || VgaDetected;
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

			TTransmitter transmitter = device as TTransmitter;
			if (transmitter == null)
				return;

			// Ensure the device stays in auto routing mode
			transmitter.VideoSource = Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Auto;
			// Disable Free-Run
			transmitter.VgaInput.FreeRun = eDmFreeRunSetting.Disabled;
		}
#endif

#endregion
	}
}
