#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
#endif

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kHdo
{
#if SIMPLSHARP
// ReSharper disable once InconsistentNaming
	public sealed class Dmc4kHdoAdapter : AbstractCardAdapter<Crestron.SimplSharpPro.DM.Cards.Dmc4kHdo, Dmc4kHdoAdapterSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public Dmc4kHdoAdapter()
		{
			Controls.Add(new Dmc4kHdoAdapterRoutingControl(this, 0));
		}

		/// <summary>
		/// Instantiates an external card.
		/// </summary>
		/// <param name="cresnetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Cards.Dmc4kHdo InstantiateCardExternal(byte cresnetId,
		                                                                                 CrestronControlSystem controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Cards.Dmc4kHdo(cresnetId, controlSystem);
		}

		/// <summary>
		/// Instantiates an internal card.
		/// </summary>
		/// <param name="cardNumber"></param>
		/// <param name="switcher"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Cards.Dmc4kHdo InstantiateCardInternal(uint cardNumber, Switch switcher)
		{
			return new Crestron.SimplSharpPro.DM.Cards.Dmc4kHdo(cardNumber, switcher, null , null);
		}
	}
#else
	public sealed class DmcCAdapter : AbstractCardAdapter<DmcCAdapterSettings>
	{
	}
#endif
}
