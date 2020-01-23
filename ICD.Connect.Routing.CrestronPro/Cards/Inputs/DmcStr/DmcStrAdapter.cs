#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
#endif

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.DmcStr
{
#if SIMPLSHARP
	public sealed class DmcStrAdapter : AbstractInputCardAdapter<Crestron.SimplSharpPro.DM.Cards.DmcStr, DmcStrAdapterSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public DmcStrAdapter()
		{
			Controls.Add(new DmcStrAdapterRoutingControl(this, 0));
		}

		protected override bool GetIsOnlineStatus()
		{
			return true;
		}

		/// <summary>
		/// Instantiates an external card.
		/// </summary>
		/// <param name="cresnetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Cards.DmcStr InstantiateCardExternal(byte cresnetId,
		                                                                                CrestronControlSystem controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Cards.DmcStr(cresnetId, controlSystem);
		}

		/// <summary>
		/// Instantiates an internal card.
		/// </summary>
		/// <param name="cardNumber"></param>
		/// <param name="switcher"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Cards.DmcStr InstantiateCardInternal(uint cardNumber, Switch switcher)
		{
			return new Crestron.SimplSharpPro.DM.Cards.DmcStr(cardNumber, switcher);
		}
	}
#else
	public sealed class DmcStrAdapter : AbstractInputCardAdapter<DmcStrAdapterSettings>
	{
	}
#endif
}
