using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Misc.CrestronPro.Devices;
using ICD.Connect.Settings;
#if !NETSTANDARD
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using ICD.Connect.Misc.CrestronPro.Extensions;
#endif

namespace ICD.Connect.Routing.CrestronPro.Cards.Outputs.DmcHdo
{
#if !NETSTANDARD
	// ReSharper disable once InconsistentNaming
	public sealed class DmcHdoAdapter : AbstractOutputCardAdapter<DmcHdoSingle, DmcHdoAdapterSettings>, IPortParent
	{
		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(DmcHdoAdapterSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new DmcHdoAdapterRoutingControl(this, 0));
		}

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return true;
			//TODO: Crestron api broken, re enable this line when a resolution comes back from them
			return Card != null &&
			       GetInternalCards().Select(internalCard => internalCard as Crestron.SimplSharpPro.DM.Cards.Dmc4kHdo)
			                         .All(internalBase => internalBase == null || internalBase.PresentFeedback.GetBoolValueOrDefault());
		}

		public override IEnumerable<CardDevice> GetInternalCards()
		{
			if (Card == null)
				yield break;

			yield return Card.Card1;
			yield return Card.Card2;
		}

		/// <summary>
		/// Instantiates an internal card.
		/// </summary>
		/// <param name="cardNumber"></param>
		/// <param name="switcher"></param>
		/// <returns></returns>
		protected override DmcHdoSingle InstantiateCardInternal(uint cardNumber, Switch switcher)
		{
			return new DmcHdoSingle(cardNumber, switcher);
		}

		#region IPortParent

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public ComPort GetComPort(int address)
		{
			string message = string.Format("{0} has no {1}", this, typeof(ComPort).Name);
			throw new NotSupportedException(message);
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public IROutputPort GetIrOutputPort(int address)
		{
			string message = string.Format("{0} has no {1}", this, typeof(IROutputPort).Name);
			throw new NotSupportedException(message);
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public Relay GetRelayPort(int address)
		{
			string message = string.Format("{0} has no {1}", this, typeof(Relay).Name);
			throw new NotSupportedException(message);
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public Versiport GetIoPort(int address)
		{
			string message = string.Format("{0} has no {1}", this, typeof(Versiport).Name);
			throw new NotSupportedException(message);
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public DigitalInput GetDigitalInputPort(int address)
		{
			string message = string.Format("{0} has no {1}", this, typeof(DigitalInput).Name);
			throw new NotSupportedException(message);
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="io"></param>
		/// <param name="address"></param>
		/// <returns></returns>
		public Cec GetCecPort(eInputOuptut io, int address)
		{
			if (io == eInputOuptut.Output)
			{
				switch (address)
				{
					case 1:
						return Card.Card1.HdmiOutput.StreamCec;
					case 2:
						return Card.Card2.HdmiOutput.StreamCec;
				}
			}

			string message = string.Format("{0} has no {1} at address {2}:{3}", this, typeof(Cec).Name, io, address);
			throw new ArgumentException(message);
		}

		#endregion
	}

	
#else
	public sealed class DmcHdoAdapter : AbstractOutputCardAdapter<DmcHdoAdapterSettings>
	{
	    protected override bool GetIsOnlineStatus()
	    {
	        return false;
	    }
	}
#endif
}
