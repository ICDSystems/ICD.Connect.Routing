#if SIMPLSHARP

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.DmcF
{
	public sealed class DmcFAdapterRoutingControl : AbstractInputCardAdapterRoutingControl<DmcFAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmcFAdapterRoutingControl(DmcFAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
