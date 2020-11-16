using ICD.Connect.Routing.CrestronPro.Receivers.AbstractDmRmc4kScalerC;
#if SIMPLSHARP
using System;
using System.Collections.Generic;
using ICD.Connect.Misc.CrestronPro.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.Devices;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Utils;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
#endif

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmc4kzScalerC
{
#if SIMPLSHARP
	// ReSharper disable once InconsistentNaming
	public sealed class DmRmc4kzScalerCAdapter :
		AbstractDmRmc4KScalerCAdapter
			<Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kzScalerC, DmRmc4kzScalerCAdapterSettings>, IRouteSwitcherDevice
	{
		private const int HDMI_INPUT_ADDRESS = 2;      

		private readonly SwitcherCache m_SwitcherCache;

		public DmRmc4kzScalerCAdapter()
		{
			m_SwitcherCache = new SwitcherCache();
			Subscribe(m_SwitcherCache);

			//Set detected to true for all inputs
			foreach (ConnectorInfo input in GetInputs())
				m_SwitcherCache.SetSourceDetectedState(input.Address, input.ConnectionType, true);
		}

	#region Instantiate Receiver

		/// <summary>
		/// Instantiates the receiver with the given IPID against the control system.
		/// </summary>
		/// <param name="ipid"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		public override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kzScalerC InstantiateReceiver(byte ipid, CrestronControlSystem controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kzScalerC(ipid, controlSystem);
		}

		/// <summary>
		/// Instantiates the receiver against the given DM Ouptut and configures it with the given IPID.
		/// </summary>
		/// <param name="ipid"></param>
		/// <param name="output"></param>
		/// <returns></returns>
		public override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kzScalerC InstantiateReceiver(byte ipid, DMOutput output)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kzScalerC(ipid, output);
		}

		/// <summary>
		/// Instantiates the receiver against the given DM Output.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kzScalerC InstantiateReceiver(DMOutput output)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kzScalerC(output);
		}

	#endregion

		/// <summary>
		/// Override to add additional/different controls (ie RouteSwitcherControl) to the device
		/// <remarks>This is called from the abstract contructor, so the concrete constructor
		/// will not have been called yet. Use with caution.</remarks>
		/// </summary>
		protected override IRouteMidpointControl CreateRouteControl()
		{
			return new RouteSwitcherControl(this, 0);
		}

		/// <summary>
		/// Release resources
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			Unsubscribe(m_SwitcherCache);
		}

		/// <summary>
		/// Caculates the source currently routed to the output
		/// Since this device doesn't have a clear output option, we blank it instead
		/// </summary>
		private void UpdateOutputRoute()
		{
			if (Receiver == null)
				return;

			m_SwitcherCache.SetInputForOutput(OUTPUT_ADDRESS,
			                                  Receiver.HdmiOutput.BlankEnabledFeedback.BoolValue
				                                  ? null
				                                  : AudioVideoSourceToInputAddress(Receiver.AudioVideoSourceFeedback),
			                                  eConnectionType.Audio | eConnectionType.Video);
		}

		#region Ports

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="io"></param>
		/// <param name="address"></param>
		/// <returns></returns>
		public override Cec GetCecPort(eInputOuptut io, int address)
		{
			if (Receiver == null)
				throw new InvalidOperationException("No DmRx instantiated");

			if (io == eInputOuptut.Input && address == HDMI_INPUT_ADDRESS)
				return Receiver.HdmiIn.StreamCec;

			return base.GetCecPort(io, address);
		}

	#endregion

	#region IRouteSwitcherDevice

		/// <summary>
		/// Returns true if a signal is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool GetSignalDetectedState(int input, eConnectionType type)
		{
			return m_SwitcherCache.GetSourceDetectedState(input, type);
		}

		/// <summary>
		/// Gets the outputs for the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs(int input, eConnectionType type)
		{
			return m_SwitcherCache.GetOutputsForInput(input, type);
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
			return m_SwitcherCache.GetInputConnectorInfoForOutput(output, type);
		}



		/// <summary>
		/// Performs the given route operation.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		public bool Route(RouteOperation info)
		{
			if (!ContainsInput(info.LocalInput))
				throw new IndexOutOfRangeException(string.Format("No input at address {0}", info.LocalInput));
			if (!ContainsOutput(info.LocalOutput))
				throw new IndexOutOfRangeException(string.Format("No output at address {0}", info.LocalOutput));

			if (Receiver == null)
				throw new InvalidOperationException("No DmRx instantiated");

			// Disable blank to un-clear the output
			Receiver.HdmiOutput.BlankDisabled();
			Receiver.AudioVideoSource = InputAddressToAudioVideoSource(info.LocalInput);
			return true;
		}

		/// <summary>
		/// Stops routing to the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns>True if successfully cleared.</returns>
		public bool ClearOutput(int output, eConnectionType type)
		{
			if (Receiver == null)
				throw new InvalidOperationException("No DmRx instantiated");

			// This device doesn't support clearing its output, so we blank it instead
			Receiver.HdmiOutput.BlankEnabled();
			return true;
		}

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override bool ContainsInput(int input)
		{
			return input == HDMI_INPUT_ADDRESS || base.ContainsInput(input);
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			foreach (ConnectorInfo input in GetBaseInputs())
				yield return input;

			yield return GetInput(HDMI_INPUT_ADDRESS);
		}

		private IEnumerable<ConnectorInfo> GetBaseInputs()
		{
			return base.GetInputs();
		}

	#endregion

	#region Recevier Callbacks

		/// <summary>
		/// Subscribe to the scaler events.
		/// </summary>
		/// <param name="scaler"></param>
		protected override void Subscribe(Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kzScalerC scaler)
		{
			base.Subscribe(scaler);

			if (scaler == null)
				return;

			scaler.BaseEvent += ScalerOnBaseEvent;
			scaler.HdmiOutput.OutputStreamChange += HdmiOutputOnOutputStreamChange;
		}

		/// <summary>
		/// Subscribe to the scaler events.
		/// </summary>
		/// <param name="scaler"></param>
		protected override void Unsubscribe(Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kzScalerC scaler)
		{
			base.Subscribe(scaler);

			if (scaler == null)
				return;

			scaler.BaseEvent -= ScalerOnBaseEvent;
			scaler.HdmiOutput.OutputStreamChange -= HdmiOutputOnOutputStreamChange;
		}

		private void ScalerOnBaseEvent(GenericBase device, BaseEventArgs args)
		{
			if (args.EventId == EndpointOutputStreamEventIds.SelectedSourceFeedbackEventId ||
			    args.EventId == EndpointOutputStreamEventIds.AudioVideoSourceFeedbackEventId)
				UpdateOutputRoute();
		}

		private void HdmiOutputOnOutputStreamChange(EndpointOutputStream outputStream, EndpointOutputStreamEventArgs args)
		{
			if (args.EventId == EndpointOutputStreamEventIds.BlankEnabledFeedbackEventId ||
			    args.EventId == EndpointOutputStreamEventIds.BlankDisabledFeedbackEventId)
				UpdateOutputRoute();
		}

		#endregion

	#region Switcher Cache Callbacks

		private void Subscribe(SwitcherCache switcherCache)
		{
			if (switcherCache == null)
				return;

			switcherCache.OnActiveInputsChanged += SwitcherCacheOnActiveInputsChanged;
			switcherCache.OnActiveTransmissionStateChanged += SwitcherCacheOnActiveTransmissionStateChanged;
			switcherCache.OnRouteChange += SwitcherCacheOnRouteChange;
			switcherCache.OnSourceDetectionStateChange += SwitcherCacheOnSourceDetectionStateChange;
		}

		private void Unsubscribe(SwitcherCache switcherCache)
		{
			if (switcherCache == null)
				return;

			switcherCache.OnActiveInputsChanged -= SwitcherCacheOnActiveInputsChanged;
			switcherCache.OnActiveTransmissionStateChanged -= SwitcherCacheOnActiveTransmissionStateChanged;
			switcherCache.OnRouteChange -= SwitcherCacheOnRouteChange;
			switcherCache.OnSourceDetectionStateChange -= SwitcherCacheOnSourceDetectionStateChange;
		}

		private void SwitcherCacheOnActiveInputsChanged(object sender, ActiveInputStateChangeEventArgs args)
		{
			RaiseActiveInputsChanged(args.Input, args.Type, args.Active);
		}

		private void SwitcherCacheOnActiveTransmissionStateChanged(object sender, TransmissionStateEventArgs args)
		{
			RaiseActiveTransmissionStateChanged(args.Output, args.Type, args.State);
		}

		private void SwitcherCacheOnRouteChange(object sender, RouteChangeEventArgs args)
		{
			RaiseRouteChange(args.OldInput, args.NewInput, args.Output, args.Type);
		}

		private void SwitcherCacheOnSourceDetectionStateChange(object sender, SourceDetectionStateChangeEventArgs args)
		{
			RaiseSourceDetectionStateChange(args.Input, args.Type, args.State);
		}

	#endregion

	#region Private Methodds

		private static Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kzScalerC.eAudioVideoSource InputAddressToAudioVideoSource
			(int input)
		{
			switch (input)
			{
				case DM_INPUT_ADDRESS:
					return Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kzScalerC.eAudioVideoSource.DM;
				case HDMI_INPUT_ADDRESS:
					return Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kzScalerC.eAudioVideoSource.HDMI;
				default:
					throw new ArgumentOutOfRangeException("input", string.Format("No source for input address {0}", input));
			}
		}

		private static int? AudioVideoSourceToInputAddress(
			Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kzScalerC.eAudioVideoSource source)
		{
			switch (source)
			{
				case Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kzScalerC.eAudioVideoSource.NoSource:
					return null;
				case Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kzScalerC.eAudioVideoSource.DM:
					return DM_INPUT_ADDRESS;
				case Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kzScalerC.eAudioVideoSource.HDMI:
					return HDMI_INPUT_ADDRESS;
				default:
					throw new ArgumentOutOfRangeException("source");
			}
		}

	#endregion
	}
#else
	// ReSharper disable once InconsistentNaming
	public sealed class DmRmc4kzScalerCAdapter : AbstractDmRmc4kScalerCAdapter<DmRmc4kzScalerCAdapterSettings>
	{ }
#endif
}