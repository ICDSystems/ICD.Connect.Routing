#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
#endif

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.DmcC
{
#if SIMPLSHARP
	public sealed class DmcCAdapter : AbstractCardAdapter<Crestron.SimplSharpPro.DM.Cards.DmcC, DmcCAdapterSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public DmcCAdapter()
		{
			Controls.Add(new DmcCAdapterRoutingControl(this, 0));
		}

		/// <summary>
		/// Instantiates an external card.
		/// </summary>
		/// <param name="cresnetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Cards.DmcC InstantiateCardExternal(byte cresnetId,
		                                                                                 CrestronControlSystem controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Cards.DmcC(cresnetId, controlSystem);
		}

		/// <summary>
		/// Instantiates an internal card.
		/// </summary>
		/// <param name="cardNumber"></param>
		/// <param name="switcher"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Cards.DmcC InstantiateCardInternal(uint cardNumber, Switch switcher)
		{
			return new Crestron.SimplSharpPro.DM.Cards.DmcC(cardNumber, switcher);
		}
	}
#else
	public sealed class DmcCAdapter : AbstractCardAdapter<DmcCAdapterSettings>
	{
	}
#endif
}