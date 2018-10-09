#if SIMPLSHARP
using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.Dm100xStrBase;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmRmc100Str
{
	public sealed class DmRmc100StrMidpointControl : AbstractRouteMidpointControl<DmRmc100StrAdapter>
	{
		private Crestron.SimplSharpPro.DM.Streaming.DmRmc100Str m_Switcher;

		/// <summary>
		/// Raised when an input source status changes.
		/// </summary>
		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;

		/// <summary>
		/// Raised when the device starts/stops actively using an input, e.g. unroutes an input.
		/// </summary>
		public override event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;

		/// <summary>
		/// Raised when the device starts/stops actively transmitting on an output.
		/// </summary>
		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmRmc100StrMidpointControl(DmRmc100StrAdapter parent, int id)
			: base(parent, id)
		{
			parent.OnStreamerChanged += ParentOnStreamerChanged;

			SetSwitcher(parent.Streamer);
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnSourceDetectionStateChange = null;
			OnActiveInputsChanged = null;
			OnActiveTransmissionStateChanged = null;

			Parent.OnStreamerChanged -= ParentOnStreamerChanged;

			base.DisposeFinal(disposing);

			SetSwitcher(null);
		}

		#region Methods

		/// <summary>
		/// Returns true if a signal is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool GetSignalDetectedState(int input, eConnectionType type)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override ConnectorInfo GetInput(int input)
		{
			if (input != 1)
				throw new ArgumentOutOfRangeException("input");

			// Media stream
			return new ConnectorInfo(1, eConnectionType.Audio | eConnectionType.Video);
		}

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override bool ContainsInput(int input)
		{
			return input == 1;
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			yield return GetInput(1);
		}

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override ConnectorInfo GetOutput(int address)
		{
			switch (address)
			{
				case 1:
					return new ConnectorInfo(address, eConnectionType.Audio | eConnectionType.Video);

				case 2:
					return new ConnectorInfo(address, eConnectionType.Audio);

				default:
					throw new ArgumentOutOfRangeException("address");
			}
		}

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override bool ContainsOutput(int output)
		{
			return output == 1 || output == 2;
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			yield return GetOutput(1);
			yield return GetOutput(2);
		}

		/// <summary>
		/// Gets the outputs for the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs(int input, eConnectionType type)
		{
			return GetOutputs().Where(o => o.ConnectionType.HasFlags(type));
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
			if (ContainsOutput(output))
				return GetInput(1);

			throw new ArgumentOutOfRangeException("output");
		}

		#endregion

		#region Switcher Callbacks

		/// <summary>
		/// Called when the parents wrapped switcher device changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="switcher"></param>
		private void ParentOnStreamerChanged(IDm100XStrBaseAdapter sender, Crestron.SimplSharpPro.DM.Streaming.Dm100xStrBase switcher)
		{
			SetSwitcher(switcher as Crestron.SimplSharpPro.DM.Streaming.DmRmc100Str);
		}
	
		/// <summary>
		/// Set the wrapped switcher.
		/// </summary>
		/// <param name="switcher"></param>
		private void SetSwitcher(Crestron.SimplSharpPro.DM.Streaming.DmRmc100Str switcher)
		{
			if (switcher == m_Switcher)
				return;

			Unsubscribe(m_Switcher);
			m_Switcher = switcher;
			Subscribe(m_Switcher);
		}

		/// <summary>
		/// Subscribe to the switcher events.
		/// </summary>
		/// <param name="switcher"></param>
		private void Subscribe(Crestron.SimplSharpPro.DM.Streaming.DmRmc100Str switcher)
		{
			if (switcher == null)
				return;

			switcher.BaseEvent += DmRmc100StrEventHandler;
			switcher.SourceReceive.StreamChange += DmRmc100Str_StreamChangeEventHandler;
		}

		/// <summary>
		/// Unsubscribe from the switcher events.
		/// </summary>
		/// <param name="switcher"></param>
		private void Unsubscribe(Crestron.SimplSharpPro.DM.Streaming.DmRmc100Str switcher)
		{
			if (switcher == null)
				return;

			switcher.BaseEvent -= DmRmc100StrEventHandler;
			switcher.SourceReceive.StreamChange -= DmRmc100Str_StreamChangeEventHandler;
		}

		/// 
		//Method to handle top level sig change events for DM-RMC-100-STR Device.
		private void DmRmc100StrEventHandler(GenericBase device, BaseEventArgs args)
		{
			if (m_Switcher == null)
				return;
		}

		/// 
		//Method to handle HDMI Out and Stream Receive sig change events for DM-RMC-100-STR Device.
		private void DmRmc100Str_StreamChangeEventHandler(Stream stream1, StreamEventArgs args)
		{
		    if (args.EventId == DMInputEventIds.StatisticsEnableEventId)
		    {
				IcdConsole.PrintLine("{0} device Statistics Enable Event Occurred\r\n", stream1.ToString());
		    }
		}

		#endregion
	}
}
#endif
