#if !NETSTANDARD

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.DmcCDsp
{
	public sealed class DmcCDspAdapterRoutingControl : AbstractInputCardAdapterRoutingControl<DmcCDspAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmcCDspAdapterRoutingControl(DmcCDspAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
