#if !NETSTANDARD

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.DmcCatDsp
{
	public sealed class DmcCatDspAdapterRoutingControl : AbstractInputCardAdapterRoutingControl<DmcCatDspAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmcCatDspAdapterRoutingControl(DmcCatDspAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
