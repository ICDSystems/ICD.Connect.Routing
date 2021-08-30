#if !NETSTANDARD

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.DmcS
{
	public sealed class DmcSAdapterRoutingControl : AbstractInputCardAdapterRoutingControl<DmcSAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmcSAdapterRoutingControl(DmcSAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
