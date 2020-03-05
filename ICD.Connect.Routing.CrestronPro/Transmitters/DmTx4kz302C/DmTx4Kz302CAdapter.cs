using ICD.Connect.API.Nodes;
using ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4Kz302C;
using ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4kzX02CBase;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
#endif

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4kz302C
{
#if SIMPLSHARP
	public sealed class DmTx4Kz302CAdapter : AbstractDmTx4kzX02CBaseAdapter<Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx4kz302C, DmTx4Kz302CAdapterSettings>
#else
	public sealed class DmTx4Kz302CAdapter : AbstractDmTx4kzX02CBaseAdapter<DmTx4Kz302CAdapterSettings>
#endif
	{

#if SIMPLSHARP
		protected override bool GetActiveTransmissionState()
		{
			return base.GetActiveTransmissionState() || Transmitter.DisplayPortInput.SyncDetectedFeedback.BoolValue;
		}

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

		private void DisplayPortInputOnInputStreamChange(EndpointInputStream inputStream, EndpointInputStreamEventArgs args)
		{
			if (args.EventId == EndpointInputStreamEventIds.SyncDetectedFeedbackEventId)
				ActiveTransmissionState = GetActiveTransmissionState();
		}
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
