using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Connect.Misc.CrestronPro.Devices;
﻿using ICD.Connect.API.Nodes;
using ICD.Connect.Misc.CrestronPro.Extensions;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Misc.CrestronPro.Devices;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4KzX02CBase
{
#if SIMPLSHARP
	public abstract class AbstractDmTx4kzX02CBaseAdapter<TTransmitter, TSettings> :
		AbstractEndpointTransmitterBaseAdapter<TTransmitter, TSettings>, IDmTx4kzX02CBaseAdapter
		where TTransmitter : Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx4kzX02CBase
#else
	public abstract class AbstractDmTx4kzX02CBaseAdapter<TSettings> :
		AbstractEndpointTransmitterBaseAdapter<TSettings>, IDmTx4kzX02CBaseAdapter
#endif
		where TSettings : IDmTx4kzX02CBaseAdapterSettings, new()
	{
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
#if SIMPLSHARP
				return Transmitter.HdmiInputs[HDMI_INPUT_1].SyncDetectedFeedback.BoolValue ||
					   Transmitter.HdmiInputs[HDMI_INPUT_2].SyncDetectedFeedback.BoolValue;
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

				RaiseOnActiveTransmissionStateChanged(HDMI_OUTPUT,
													  eConnectionType.Audio | eConnectionType.Video,
													  m_ActiveTransmissionState);
			}
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
				ActiveTransmissionState = GetActiveTransmissionState();
		}

		/// <summary>
		/// Called when the the transmitter raises an event.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		protected virtual void TransmitterOnBaseEvent(GenericBase device, BaseEventArgs args)
		{
			if (args.EventId != EndpointTransmitterBase.VideoSourceFeedbackEventId)
				return;

			// Ensure the device stays in auto routing mode
			Transmitter.VideoSource = eX02VideoSourceType.Auto;
		}

#endif

#endregion

		#region Methods

#if SIMPLSHARP

		protected virtual bool GetActiveTransmissionState()
		{
			return HdmiDetected;
		}
#endif

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

		#endregion

#if SIMPLSHARP
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
