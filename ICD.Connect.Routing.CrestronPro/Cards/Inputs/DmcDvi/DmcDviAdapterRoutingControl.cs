#if SIMPLSHARP

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.DmcDvi
{
	public sealed class DmcDviAdapterRoutingControl : AbstractInputCardAdapterRoutingControl<DmcDviAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmcDviAdapterRoutingControl(DmcDviAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
