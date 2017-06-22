using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200Base
{
	public abstract class AbstractDmTxBaseSourceControl<TDevice, TDeviceSettings, TTransmitter> : AbstractRouteSourceControl<TDevice>
		where TDevice : AbstractDmTx200BaseAdapter<TTransmitter, TDeviceSettings>, IDevice
		where TDeviceSettings : AbstractDmTx200BaseAdapterSettings, new()
		where TTransmitter : global::Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base
	{
		private const int INPUT_HDMI = 1;
		private const int OUTPUT_HDMI = 1;

		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		private TTransmitter m_Transmitter;
		private bool m_ActiveTransmissionState;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		protected AbstractDmTxBaseSourceControl(TDevice parent)
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
		public bool HdmiDetected { get { return Parent.Transmitter.HdmiInput.SyncDetectedFeedback.BoolValue; } }

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
													   new TransmissionStateEventArgs(OUTPUT_HDMI,
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
			yield return new ConnectorInfo(1, eConnectionType.Video | eConnectionType.Audio);
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
		private void Subscribe(TDevice parent)
		{
			parent.OnTransmitterChanged += TransmitterOnTransmitterChanged;
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		private void Unsubscribe(TDevice parent)
		{
			parent.OnTransmitterChanged -= TransmitterOnTransmitterChanged;
		}

		/// <summary>
		/// Called when the parents wrapped transmitter changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="transmitter"></param>
		private void TransmitterOnTransmitterChanged(object sender, TTransmitter transmitter)
		{
			SetTransmitter(transmitter);
		}

		private void SetTransmitter(TTransmitter transmitter)
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
		private void Subscribe(TTransmitter transmitter)
		{
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
		private void Unsubscribe(TTransmitter transmitter)
		{
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
			Parent.Transmitter.VideoSource = global::Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Auto;
			// Disable Free-Run
			Parent.Transmitter.VgaInput.FreeRun = eDmFreeRunSetting.Disabled;
		}

		#endregion
	}
}
