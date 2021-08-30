#if !NETSTANDARD

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.DmcStr
{
	public sealed class DmcStrAdapterRoutingControl : AbstractInputCardAdapterRoutingControl<DmcStrAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmcStrAdapterRoutingControl(DmcStrAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
