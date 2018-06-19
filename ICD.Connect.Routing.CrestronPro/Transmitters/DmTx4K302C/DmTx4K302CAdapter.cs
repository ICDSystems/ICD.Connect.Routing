#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM.Endpoints;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
#endif

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4K302C
{
#if SIMPLSHARP
	public sealed class DmTx4K302CAdapter : AbstractEndpointTransmitterBaseAdapter<DmTx4k302C, DmTx4K302CAdapterSettings>
#else
    public sealed class DmTx4K302CAdapter : AbstractEndpointTransmitterBaseAdapter<DmTx4K302CAdapterSettings>
#endif
	{
#if SIMPLSHARP
		private const int HDMI_INPUT_1 = 1;
		private const int HDMI_INPUT_2 = 2;
		private const int HDMI_OUTPUT = 1;

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
				return Transmitter.HdmiInputs[HDMI_INPUT_1].SyncDetectedFeedback.BoolValue ||
					   Transmitter.HdmiInputs[HDMI_INPUT_2].SyncDetectedFeedback.BoolValue;
			}
		}

		/// <summary>
		/// Returns true if a VGA input source is detected.
		/// </summary>
		[PublicAPI]
		public bool VgaDetected { get { return Transmitter.VgaInput.SyncDetectedFeedback.BoolValue; } }

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

				RaiseOnActiveTransmissionStateChanged(HDMI_OUTPUT,
				                                      eConnectionType.Audio | eConnectionType.Video,
				                                      m_ActiveTransmissionState);
			}
		}

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		public DmTx4K302CAdapter()
		{
			Controls.Add(new RouteSourceControl(this, 0));
		}

		#region Methods

		/// <summary>
		/// Subscribes to the transmitter events.
		/// </summary>
		/// <param name="transmitter"></param>
		protected override void Subscribe(DmTx4k302C transmitter)
		{
			base.Subscribe(transmitter);

			if (transmitter == null)
				return;

			transmitter.HdmiInputs[HDMI_INPUT_1].InputStreamChange += HdmiInputOnInputStreamChange;
			transmitter.HdmiInputs[HDMI_INPUT_2].InputStreamChange += HdmiInputOnInputStreamChange;
			transmitter.VgaInput.InputStreamChange += VgaInputOnInputStreamChange;
			transmitter.BaseEvent += TransmitterOnBaseEvent;
		}

		/// <summary>
		/// Unsubscribes from the transmitter events.
		/// </summary>
		/// <param name="transmitter"></param>
		protected override void Unsubscribe(DmTx4k302C transmitter)
		{
			base.Unsubscribe(transmitter);

			if (transmitter == null)
				return;

			transmitter.HdmiInputs[HDMI_INPUT_1].InputStreamChange -= HdmiInputOnInputStreamChange;
			transmitter.HdmiInputs[HDMI_INPUT_2].InputStreamChange -= HdmiInputOnInputStreamChange;
			transmitter.VgaInput.InputStreamChange -= VgaInputOnInputStreamChange;
			transmitter.BaseEvent -= TransmitterOnBaseEvent;
		}

		/// <summary>
		/// Called when the HDMI input stream changes.
		/// </summary>
		/// <param name="inputStream"></param>
		/// <param name="args"></param>
		private void HdmiInputOnInputStreamChange(EndpointInputStream inputStream, EndpointInputStreamEventArgs args)
		{
			if (args.EventId == DMInputEventIds.VideoDetectedEventId)
				ActiveTransmissionState = HdmiDetected || VgaDetected;
		}

		/// <summary>
		/// Called when the VGA input stream changes.
		/// </summary>
		/// <param name="inputStream"></param>
		/// <param name="args"></param>
		private void VgaInputOnInputStreamChange(EndpointInputStream inputStream, EndpointInputStreamEventArgs args)
		{
			if (args.EventId == DMInputEventIds.VideoOutEventId)
				ActiveTransmissionState = HdmiDetected || VgaDetected;
		}

		/// <summary>
		/// Called when the the transmitter raises an event.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		private void TransmitterOnBaseEvent(GenericBase device, BaseEventArgs args)
		{
			if (args.EventId != DMOutputEventIds.ContentLanModeEventId)
				return;

			// Ensure the device stays in auto routing mode
			Transmitter.VideoSource = eX02VideoSourceType.Auto;
			// Disable Free-Run
			Transmitter.VgaInput.FreeRun = eDmFreeRunSetting.Disabled;
		}

		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			yield return new ConnectorInfo(HDMI_OUTPUT, eConnectionType.Video | eConnectionType.Audio);
		}

		public override bool GetActiveTransmissionState(int output, eConnectionType type)
		{
			if (EnumUtils.HasMultipleFlags(type))
			{
				return
					EnumUtils.GetFlagsExceptNone(type)
							 .Select(f => GetActiveTransmissionState(output, f))
							 .Unanimous(false);
			}

			if (output != 1)
			{
				string message = string.Format("{0} has no {1} output at address {2}", this, type, output);
				throw new IndexOutOfRangeException(message);
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

		#endregion

		#region Settings

		public override DmTx4k302C InstantiateTransmitter(byte ipid, CrestronControlSystem controlSystem)
		{
			return new DmTx4k302C(ipid, controlSystem);
		}

		public override DmTx4k302C InstantiateTransmitter(byte ipid, DMInput input)
		{
			return new DmTx4k302C(ipid, input);
		}

		public override DmTx4k302C InstantiateTransmitter(DMInput input)
		{
			return new DmTx4k302C(input);
		}


		#endregion
#endif
	}
}
