using System;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Settings;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro.Extensions;
using ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kCDspBase;
#endif

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kzCDsp
{
#if SIMPLSHARP
	public sealed class Dmc4kzCDspAdapter :
		AbstractDmc4kCDspBaseAdapter<Crestron.SimplSharpPro.DM.Cards.Dmc4kzCDsp, Dmc4kzCDspAdapterSettings>
	{
		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(Dmc4kzCDspAdapterSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new Dmc4kzCDspAdapterRoutingControl(this, 0));
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
		protected override Crestron.SimplSharpPro.DM.Cards.Dmc4kzCDsp InstantiateCardExternal(byte cresnetId,
		                                                                                     CrestronControlSystem
			                                                                                     controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Cards.Dmc4kzCDsp(cresnetId, controlSystem);
		}

		/// <summary>
		/// Instantiates an internal card.
		/// </summary>
		/// <param name="cardNumber"></param>
		/// <param name="switcher"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Cards.Dmc4kzCDsp InstantiateCardInternal(uint cardNumber, Switch switcher)
		{
			return new Crestron.SimplSharpPro.DM.Cards.Dmc4kzCDsp(cardNumber, switcher);
		}
	}
#else
	public sealed class Dmc4kzCDspAdapter : AbstractInputCardAdapter<Dmc4kzCDspAdapterSettings>
	{
	}
#endif
}
