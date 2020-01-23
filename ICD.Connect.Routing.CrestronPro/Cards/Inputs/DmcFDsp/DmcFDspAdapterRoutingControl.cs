#if SIMPLSHARP

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.DmcFDsp
{
	public sealed class DmcFDspAdapterRoutingControl : AbstractInputCardAdapterRoutingControl<DmcFDspAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmcFDspAdapterRoutingControl(DmcFDspAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
