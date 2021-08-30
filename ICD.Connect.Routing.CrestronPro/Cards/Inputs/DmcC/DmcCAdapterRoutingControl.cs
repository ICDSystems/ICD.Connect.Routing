#if !NETSTANDARD

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.DmcC
{
	public sealed class DmcCAdapterRoutingControl : AbstractInputCardAdapterRoutingControl<DmcCAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmcCAdapterRoutingControl(DmcCAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
