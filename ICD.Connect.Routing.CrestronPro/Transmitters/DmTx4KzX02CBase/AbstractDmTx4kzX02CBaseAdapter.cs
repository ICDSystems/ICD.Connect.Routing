using System;
using System.Collections.Generic;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
﻿using ICD.Connect.API.Nodes;
using ICD.Connect.Misc.CrestronPro.Devices;
using ICD.Connect.Misc.CrestronPro.Extensions;
#endif
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4KzX02CBase
{
#if SIMPLSHARP
// ReSharper disable once InconsistentNaming
	public abstract class AbstractDmTx4kzX02CBaseAdapter<TTransmitter, TSettings> :
		AbstractEndpointTransmitterSwitcherBaseAdapter<TTransmitter, TSettings>, IDmTx4kzX02CBaseAdapter
		where TTransmitter : DmTx4kzX02CBase
#else
	public abstract class AbstractDmTx4kzX02CBaseAdapter<TSettings> :
		AbstractEndpointTransmitterSwitcherBaseAdapter<TSettings>, IDmTx4kzX02CBaseAdapter
#endif
		where TSettings : IDmTx4kzX02CBaseAdapterSettings, new()
	{
		protected const int HDMI_INPUT_1 = 1;
		protected const int HDMI_INPUT_2 = 2;
		protected const int DM_OUTPUT = 1;
		protected const int HDMI_OUTPUT = 2;

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


		#endregion

		#region Methods

		/// <summary>
		/// Gets the source detect state from the Tx
		/// This is just a simple "is a laptop detected"
		/// Used for ActiveTransmission in AutoRouting mode
		/// Override to support additional inputs on a Tx
		/// </summary>
		/// <returns></returns>
		protected override bool GetSourceDetectionState()
		{
			return HdmiDetected;
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

			switch (info.LocalInput)
			{
				case HDMI_INPUT_1:
					SetVideoSource(eX02VideoSourceType.Hdmi1);
					break;

				case HDMI_INPUT_2:
					SetVideoSource(eX02VideoSourceType.Hdmi2);
					break;

				default:
					throw new IndexOutOfRangeException(string.Format("No input at address {0}", info.LocalInput));
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

			SetVideoSource(eX02VideoSourceType.AllDisabled);
			return true;
#endif
			return false;
		}

#if SIMPLSHARP

		protected void SetVideoSource(eX02VideoSourceType source)
		{
			if (Transmitter == null || Transmitter.VideoSourceFeedback == source)
				return;

			Transmitter.VideoSource = Transmitter.VideoSourceFeedback;
			Transmitter.VideoSource = source;
		}

		/// <summary>
		/// Override to implement AutoRouting on the transmitter
		/// </summary>
		protected override void SetTransmitterAutoRoutingFinal()
		{
			SetVideoSource(eX02VideoSourceType.Auto);
		}
#endif

#endregion

#if SIMPLSHARP

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
			{
				UpdateSourceDetectionState();
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
		private void TransmitterOnBaseEvent(GenericBase device, BaseEventArgs args)
		{
			if (args.EventId == EndpointTransmitterBase.VideoSourceFeedbackEventId)
				TransmitterOnVideoSourceFeedbackEvent(device, args);
		}

		/// <summary>
		/// Called when the transmitter raises the VideoSourceFeedback event
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		protected virtual void TransmitterOnVideoSourceFeedbackEvent(GenericBase device, BaseEventArgs args)
		{
			switch (Transmitter.VideoSourceFeedback)
			{
				case eX02VideoSourceType.Hdmi1:
					SwitcherCache.SetInputForOutput(DM_OUTPUT, HDMI_INPUT_1, eConnectionType.Audio | eConnectionType.Video);
					SwitcherCache.SetInputForOutput(HDMI_OUTPUT, HDMI_INPUT_1, eConnectionType.Audio | eConnectionType.Video);
					break;
				case eX02VideoSourceType.Hdmi2:
					SwitcherCache.SetInputForOutput(DM_OUTPUT, HDMI_INPUT_2, eConnectionType.Audio | eConnectionType.Video);
					SwitcherCache.SetInputForOutput(HDMI_OUTPUT, HDMI_INPUT_2, eConnectionType.Audio | eConnectionType.Video);
					break;
				case eX02VideoSourceType.Auto:
				case eX02VideoSourceType.AllDisabled:
					SwitcherCache.SetInputForOutput(DM_OUTPUT, null, eConnectionType.Audio | eConnectionType.Video);
					SwitcherCache.SetInputForOutput(HDMI_OUTPUT, null, eConnectionType.Audio | eConnectionType.Video);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

#endif

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
