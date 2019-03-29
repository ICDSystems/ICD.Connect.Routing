using ICD.Common.Properties;
using ICD.Connect.Misc.CrestronPro.Extensions;
using ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4kX02CBase;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
using Crestron.SimplSharpPro.DM.Endpoints;
#endif

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4K302C
{
#if SIMPLSHARP
	public sealed class DmTx4K302CAdapter : AbstractDmTx4kX02CBaseAdapter<DmTx4k302C, DmTx4K302CAdapterSettings>
#else
	public sealed class DmTx4K302CAdapter : AbstractDmTx4kX02CBaseAdapter<DmTx4K302CAdapterSettings>
#endif
	{
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
			if (args.EventId == DMInputEventIds.VideoOutEventId)
				ActiveTransmissionState = GetActiveTransmissionState();
		}

		protected override bool GetActiveTransmissionState()
		{
			return VgaDetected || base.GetActiveTransmissionState();
		}

		/// <summary>
		/// Called when the the transmitter raises an event.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		protected override void TransmitterOnBaseEvent(GenericBase device, BaseEventArgs args)
		{
			base.TransmitterOnBaseEvent(device, args);

			if (args.EventId != DMOutputEventIds.ContentLanModeEventId)
				return;

			// Disable Free-Run
			Transmitter.VgaInput.FreeRun = eDmFreeRunSetting.Disabled;
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
	}
}
