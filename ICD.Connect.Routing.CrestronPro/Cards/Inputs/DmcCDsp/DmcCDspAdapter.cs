#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
#endif

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.DmcCDsp
{
#if SIMPLSHARP
	public sealed class DmcCDspAdapter : AbstractInputCardAdapter<Crestron.SimplSharpPro.DM.Cards.DmcCDsp, DmcCDspAdapterSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public DmcCDspAdapter()
		{
			Controls.Add(new DmcCDspAdapterRoutingControl(this, 0));
		}

		/// <summary>
		/// Instantiates an external card.
		/// </summary>
		/// <param name="cresnetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Cards.DmcCDsp InstantiateCardExternal(byte cresnetId,
		                                                                                 CrestronControlSystem controlSystem)
		{
		    return new Crestron.SimplSharpPro.DM.Cards.DmcCDsp(cresnetId, controlSystem);
		}

		/// <summary>
		/// Instantiates an internal card.
		/// </summary>
		/// <param name="cardNumber"></param>
		/// <param name="switcher"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Cards.DmcCDsp InstantiateCardInternal(uint cardNumber, Switch switcher)
		{
            return new Crestron.SimplSharpPro.DM.Cards.DmcCDsp(cardNumber, switcher);
		}
	}
#else
	public sealed class DmcCDspAdapter : AbstractInputCardAdapter<DmcCDspAdapterSettings>
	{
	}
#endif
}
