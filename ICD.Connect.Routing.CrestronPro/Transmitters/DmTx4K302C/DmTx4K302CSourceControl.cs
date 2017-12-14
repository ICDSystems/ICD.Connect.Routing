#if SIMPLSHARP
using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4K302C
{
	public sealed class DmTx4K302CSourceControl : AbstractRouteSourceControl<DmTx4K302CAdapter>
	{
		private const int HDMI_INPUT_1 = 1;
		private const int HDMI_INPUT_2 = 2;
		private const int HDMI_OUTPUT = 1;

		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		private DmTx4k302C m_Transmitter;
		private bool m_ActiveTransmissionState;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		public DmTx4K302CSourceControl(DmTx4K302CAdapter parent)
			: base(parent, 0)
		{
			Subscribe(parent);
			SetTransmitter(parent.Transmitter);
		}

		#region Properties

		/// <summary>
		/// Returns true if an HDMI input source is detected.
		/// </summary>
		[PublicAPI]
		public bool HdmiDetected
		{
			get
			{
				return Parent.Transmitter.HdmiInputs[HDMI_INPUT_1].SyncDetectedFeedback.BoolValue ||
				       Parent.Transmitter.HdmiInputs[HDMI_INPUT_2].SyncDetectedFeedback.BoolValue;
			}
		}

		/// <summary>
		/// Returns true if a VGA input source is detected.
		/// </summary>
		[PublicAPI]
		public bool VgaDetected { get { return Parent.Transmitter.VgaInput.SyncDetectedFeedback.BoolValue; } }

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

				OnActiveTransmissionStateChanged.Raise(this,
				                                       new TransmissionStateEventArgs(HDMI_OUTPUT,
				                                                                      eConnectionType.Audio | eConnectionType.Video,
				                                                                      m_ActiveTransmissionState));
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			OnActiveTransmissionStateChanged = null;

			base.DisposeFinal(disposing);

			Unsubscribe(Parent);
			SetTransmitter(null);
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			yield return new ConnectorInfo(HDMI_OUTPUT, eConnectionType.Video | eConnectionType.Audio);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns true if the device is actively transmitting on the given output.
		/// This is NOT the same as sending video, since some devices may send an
		/// idle signal by default.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
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
				throw new KeyNotFoundException(message);
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

		#region Parent Callbacks

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="parent"></param>
		private void Subscribe(DmTx4K302CAdapter parent)
		{
			parent.OnTransmitterChanged += ParentOnTransmitterChanged;
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		private void Unsubscribe(DmTx4K302CAdapter parent)
		{
			parent.OnTransmitterChanged -= ParentOnTransmitterChanged;
		}

		/// <summary>
		/// Called when the parents wrapped transmitter changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="transmitter"></param>
		private void ParentOnTransmitterChanged(IEndpointTransmitterBaseAdapter sender, EndpointTransmitterBase transmitter)
		{
			SetTransmitter(transmitter as DmTx4k302C);
		}

		private void SetTransmitter(DmTx4k302C transmitter)
		{
			Unsubscribe(m_Transmitter);
			m_Transmitter = transmitter;
			Subscribe(m_Transmitter);
		}

		#endregion

		#region Transmitter callbacks

		/// <summary>
		/// Subscribes to the transmitter events.
		/// </summary>
		/// <param name="transmitter"></param>
		private void Subscribe(DmTx4k302C transmitter)
		{
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
		private void Unsubscribe(DmTx4k302C transmitter)
		{
			if (transmitter == null)
				return;

			transmitter.HdmiInputs[HDMI_INPUT_1].InputStreamChange += HdmiInputOnInputStreamChange;
			transmitter.HdmiInputs[HDMI_INPUT_2].InputStreamChange += HdmiInputOnInputStreamChange;
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
			if (args.EventId == DMInputEventIds.VideoOutEventId)
				ActiveTransmissionState = HdmiDetected || VgaDetected;
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
		/// Called when the the transmitter raises an event.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		private void TransmitterOnBaseEvent(GenericBase device, BaseEventArgs args)
		{
			if (args.EventId != DMOutputEventIds.ContentLanModeEventId)
				return;

			// Ensure the device stays in auto routing mode
			m_Transmitter.VideoSource = eX02VideoSourceType.Auto;
			// Disable Free-Run
			m_Transmitter.VgaInput.FreeRun = eDmFreeRunSetting.Disabled;
		}

		#endregion
	}
}
#endif