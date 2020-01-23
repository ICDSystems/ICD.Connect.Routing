#if SIMPLSHARP

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.DmcSdi
{
	public sealed class DmcSdiAdapterRoutingControl : AbstractInputCardAdapterRoutingControl<DmcSdiAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmcSdiAdapterRoutingControl(DmcSdiAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
