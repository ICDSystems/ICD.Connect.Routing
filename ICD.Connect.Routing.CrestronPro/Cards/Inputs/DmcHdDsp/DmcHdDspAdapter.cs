using System;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Settings;
#if !NETSTANDARD
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro.Extensions;
#endif

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.DmcHdDsp
{
#if !NETSTANDARD
	public sealed class DmcHdDspAdapter :
		AbstractInputCardAdapter<Crestron.SimplSharpPro.DM.Cards.DmcHdDsp, DmcHdDspAdapterSettings>
	{
		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(DmcHdDspAdapterSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new DmcHdDspAdapterRoutingControl(this, 0));
		}

		protected override bool GetIsOnlineStatus()
		{
			return true;
			//TODO: Crestron api broken, re enable this line when a resolution comes back from them
			return Card != null && Card.PresentFeedback.GetBoolValueOrDefault();
		}

		/// <summary>
		/// Instantiates an external card.
		/// </summary>
		/// <param name="cresnetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Cards.DmcHdDsp InstantiateCardExternal(byte cresnetId, CrestronControlSystem controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Cards.DmcHdDsp(cresnetId, controlSystem);
		}

		/// <summary>
		/// Instantiates an internal card.
		/// </summary>
		/// <param name="cardNumber"></param>
		/// <param name="switcher"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Cards.DmcHdDsp InstantiateCardInternal(uint cardNumber, Switch switcher)
		{
			return new Crestron.SimplSharpPro.DM.Cards.DmcHdDsp(cardNumber, switcher);
		}
	}
#else
	public sealed class DmcHdDspAdapter : AbstractInputCardAdapter<DmcHdDspAdapterSettings>
	{
	}
#endif
}
