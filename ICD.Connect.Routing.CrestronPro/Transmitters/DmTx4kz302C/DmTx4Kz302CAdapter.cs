using System;
using System.Collections.Generic;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
using ICD.Connect.API.Nodes;
using ICD.Connect.Routing.Connections;
#endif
using ICD.Common.Properties;
using ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4KzX02CBase;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4kz302C
{
#if SIMPLSHARP
	public sealed class DmTx4Kz302CAdapter : AbstractDmTx4kzX02CBaseAdapter<Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx4kz302C, DmTx4Kz302CAdapterSettings>
#else
	public sealed class DmTx4Kz302CAdapter : AbstractDmTx4kzX02CBaseAdapter<DmTx4Kz302CAdapterSettings>
#endif
	{
		private const int DISPLAY_PORT_INPUT = 3;

		#region Properties

		/// <summary>
		/// Returns true if a display port input source is detected.
		/// </summary>
		[PublicAPI]
		public bool DisplayPortDetected
		{
			get
			{
#if SIMPLSHARP
				return Transmitter.DisplayPortInput.SyncDetectedFeedback.BoolValue;
#else
				return false;
#endif
			}
		}

		#endregion

#if SIMPLSHARP

		#region Methods

		protected override bool GetActiveTransmissionState()
		{
			return base.GetActiveTransmissionState() || DisplayPortDetected;
		}

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
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override bool ContainsInput(int input)
		{
			return base.ContainsInput(input) || input == DISPLAY_PORT_INPUT;
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			foreach (ConnectorInfo input in GetBaseInputs())
			{
				yield return input;
			}

			yield return GetInput(DISPLAY_PORT_INPUT);
		}

		/// <summary>
		/// Returns the base inputs, workaround for unverifiable code warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<ConnectorInfo> GetBaseInputs()
		{
			return base.GetInputs();
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

			if (Transmitter == null)
				throw new InvalidOperationException("No DmTx instantiated");

			switch (info.LocalInput)
			{
				case DISPLAY_PORT_INPUT:
					SetVideoSource(eX02VideoSourceType.DisplayPort);
					break;

				default:
					return base.Route(info);
			}
			return true;
		}

		#endregion

		#region Transmitter Callbacks

		/// <summary>
		/// Subscribes to the transmitter events.
		/// </summary>
		/// <param name="transmitter"></param>
		protected override void Subscribe(Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx4kz302C transmitter)
		{
			base.Subscribe(transmitter);

			if (transmitter == null)
				return;

			transmitter.DisplayPortInput.InputStreamChange += DisplayPortInputOnInputStreamChange;
		}

		/// <summary>
		/// Unsubscribes from the transmitter events.
		/// </summary>
		/// <param name="transmitter"></param>
		protected override void Unsubscribe(Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx4kz302C transmitter)
		{
			base.Unsubscribe(transmitter);

			if (transmitter == null)
				return;

			transmitter.DisplayPortInput.InputStreamChange -= DisplayPortInputOnInputStreamChange;
		}

		/// <summary>
		/// Called when the the transmitter raises an event.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		protected override void TransmitterOnBaseEvent(GenericBase device, BaseEventArgs args)
		{
			if (args.EventId != EndpointTransmitterBase.VideoSourceFeedbackEventId)
				return;

			// Ensure the device stays in auto routing mode if applicable.
			if (UseAutoRouting)
			{
				Transmitter.VideoSource = eX02VideoSourceType.Auto;
			}

			switch (Transmitter.VideoSourceFeedback)
			{
				case eX02VideoSourceType.Auto:
				case eX02VideoSourceType.AllDisabled:
					SwitcherCache.SetInputForOutput(DM_OUTPUT, null, eConnectionType.Audio | eConnectionType.Video);
					SwitcherCache.SetInputForOutput(HDMI_OUTPUT, null, eConnectionType.Audio | eConnectionType.Video);
					break;
				case eX02VideoSourceType.Hdmi1:
					SwitcherCache.SetInputForOutput(DM_OUTPUT, HDMI_INPUT_1, eConnectionType.Audio | eConnectionType.Video);
					SwitcherCache.SetInputForOutput(HDMI_OUTPUT, HDMI_INPUT_1, eConnectionType.Audio | eConnectionType.Video);
					break;
				case eX02VideoSourceType.Hdmi2:
					SwitcherCache.SetInputForOutput(DM_OUTPUT, HDMI_INPUT_2, eConnectionType.Audio | eConnectionType.Video);
					SwitcherCache.SetInputForOutput(HDMI_OUTPUT, HDMI_INPUT_2, eConnectionType.Audio | eConnectionType.Video);
					break;
				case eX02VideoSourceType.Vga:
					SwitcherCache.SetInputForOutput(DM_OUTPUT, DISPLAY_PORT_INPUT, eConnectionType.Audio | eConnectionType.Video);
					SwitcherCache.SetInputForOutput(HDMI_OUTPUT, DISPLAY_PORT_INPUT, eConnectionType.Audio | eConnectionType.Video);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void DisplayPortInputOnInputStreamChange(EndpointInputStream inputStream, EndpointInputStreamEventArgs args)
		{
			if (args.EventId == EndpointInputStreamEventIds.SyncDetectedFeedbackEventId)
			{
				ActiveTransmissionState = GetActiveTransmissionState();
				SwitcherCache.SetSourceDetectedState(DISPLAY_PORT_INPUT, eConnectionType.Audio,
				                                     Transmitter.DisplayPortInput.SyncDetectedFeedback.BoolValue);
				SwitcherCache.SetSourceDetectedState(DISPLAY_PORT_INPUT, eConnectionType.Video,
				                                     Transmitter.DisplayPortInput.SyncDetectedFeedback.BoolValue);
			}
		}

		#endregion
#endif

		#region Settings

#if SIMPLSHARP
		public override Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx4kz302C InstantiateTransmitter(byte ipid, CrestronControlSystem controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx4kz302C(ipid, controlSystem);
		}

		public override Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx4kz302C InstantiateTransmitter(byte ipid, DMInput input)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx4kz302C(ipid, input);
		}

		public override Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx4kz302C InstantiateTransmitter(DMInput input)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx4kz302C(input);
		}
#endif

		#endregion

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
				addRow("DisplayPort Sync", Transmitter.DisplayPortInput.SyncDetectedFeedback.BoolValue);
		}

#endif
		#endregion
	}
}
