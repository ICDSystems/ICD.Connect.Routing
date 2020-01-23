#if SIMPLSHARP

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.DmcSDsp
{
	public sealed class DmcSDspAdapterRoutingControl : AbstractInputCardAdapterRoutingControl<DmcSDspAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmcSDspAdapterRoutingControl(DmcSDspAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
