using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4kX02CBase;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
using Crestron.SimplSharpPro.DM.Endpoints;
using ICD.Connect.API.Nodes;
using ICD.Connect.Misc.CrestronPro.Extensions;
#endif

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4K302C
{
#if SIMPLSHARP
	public sealed class DmTx4K302CAdapter : AbstractDmTx4kX02CBaseAdapter<DmTx4k302C, DmTx4K302CAdapterSettings>
#else
	public sealed class DmTx4K302CAdapter : AbstractDmTx4kX02CBaseAdapter<DmTx4K302CAdapterSettings>
#endif
	{
		private const int VGA_INPUT = 3;

		#region Properties

		/// <summary>
		/// Returns true if a VGA input source is detected.
		/// </summary>
		[PublicAPI]
		public bool VgaDetected
		{
			get
			{
#if SIMPLSHARP
				return Transmitter.VgaInput.SyncDetectedFeedback.GetBoolValueOrDefault();
#else
				return false;
#endif
			}
		}

		#endregion

		#region Methods

		protected override bool GetActiveTransmissionState()
		{
			return VgaDetected || base.GetActiveTransmissionState();
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
			return base.ContainsInput(input) || input == VGA_INPUT;
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

			yield return GetInput(VGA_INPUT);
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

							case VGA_INPUT:
								Transmitter.AudioSource = eX02AudioSourceType.AudioIn;
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

							case VGA_INPUT:
								Transmitter.VideoSource = eX02VideoSourceType.DisplayPort;
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

#endregion

#region Transmitter Callbacks

#if SIMPLSHARP
		/// <summary>
		/// Subscribes to the transmitter events.
		/// </summary>
		/// <param name="transmitter"></param>
		protected override void Subscribe(DmTx4k302C transmitter)
		{
			base.Subscribe(transmitter);

			if (transmitter == null)
				return;

			transmitter.VgaInput.InputStreamChange += VgaInputOnInputStreamChange;
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

			transmitter.VgaInput.InputStreamChange -= VgaInputOnInputStreamChange;
		}

		/// <summary>
		/// Called when the VGA input stream changes.
		/// </summary>
		/// <param name="inputStream"></param>
		/// <param name="args"></param>
		private void VgaInputOnInputStreamChange(EndpointInputStream inputStream, EndpointInputStreamEventArgs args)
		{
			if (args.EventId == EndpointInputStreamEventIds.SyncDetectedFeedbackEventId)
			{
				ActiveTransmissionState = GetActiveTransmissionState();
				SwitcherCache.SetSourceDetectedState(VGA_INPUT, eConnectionType.Video,
				                                     Transmitter.VgaInput.SyncDetectedFeedback.BoolValue);
				SwitcherCache.SetSourceDetectedState(VGA_INPUT, eConnectionType.Audio,
				                                     Transmitter.VgaInput.SyncDetectedFeedback.BoolValue);
			}
		}

		/// <summary>
		/// Called when the the transmitter raises an event.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		protected override void TransmitterOnBaseEvent(GenericBase device, BaseEventArgs args)
		{
			base.TransmitterOnBaseEvent(device, args);

			if (args.EventId == DMOutputEventIds.ContentLanModeEventId)
				// Disable Free-Run
				Transmitter.VgaInput.FreeRun = eDmFreeRunSetting.Disabled;

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
				case eX02VideoSourceType.Vga:
					SwitcherCache.SetInputForOutput(DM_OUTPUT, VGA_INPUT, eConnectionType.Video);
					SwitcherCache.SetInputForOutput(HDMI_OUTPUT, VGA_INPUT, eConnectionType.Video);
					break;
				case eX02VideoSourceType.Auto:
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
				case eX02AudioSourceType.AudioIn:
					SwitcherCache.SetInputForOutput(DM_OUTPUT, VGA_INPUT, eConnectionType.Audio);
					SwitcherCache.SetInputForOutput(HDMI_OUTPUT, VGA_INPUT, eConnectionType.Audio);
					break;
				case eX02AudioSourceType.Auto:
				case eX02AudioSourceType.AllDisabled:
					SwitcherCache.SetInputForOutput(DM_OUTPUT, null, eConnectionType.Audio);
					SwitcherCache.SetInputForOutput(HDMI_OUTPUT, null, eConnectionType.Audio);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
#endif

#endregion

#region Settings

#if SIMPLSHARP
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
				addRow("VGA Sync", Transmitter.VgaInput.SyncDetectedFeedback.BoolValue);
		}

#endif
#endregion
	}
}
