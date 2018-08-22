#if SIMPLSHARP

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.DmcHd
{
	public sealed class DmcHdAdapterRoutingControl : AbstractInputCardAdapterRoutingControl<DmcHdAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmcHdAdapterRoutingControl(DmcHdAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
