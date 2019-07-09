#if SIMPLSHARP

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.DmcHdDsp
{
	public sealed class DmcHdDspAdapterRoutingControl : AbstractInputCardAdapterRoutingControl<DmcHdDspAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmcHdDspAdapterRoutingControl(DmcHdDspAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
