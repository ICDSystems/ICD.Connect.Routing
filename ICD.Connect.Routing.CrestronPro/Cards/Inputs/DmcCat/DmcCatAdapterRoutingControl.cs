#if SIMPLSHARP

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.DmcCat
{
	public sealed class DmcCatAdapterRoutingControl : AbstractInputCardAdapterRoutingControl<DmcCatAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmcCatAdapterRoutingControl(DmcCatAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
