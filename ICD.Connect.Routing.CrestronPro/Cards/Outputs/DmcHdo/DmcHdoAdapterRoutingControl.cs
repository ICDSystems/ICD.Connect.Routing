#if SIMPLSHARP

namespace ICD.Connect.Routing.CrestronPro.Cards.Outputs.DmcHdo
{
// ReSharper disable once InconsistentNaming
	public sealed class DmcHdoAdapterRoutingControl : AbstractOutputCardAdapterRoutingControl<DmcHdoAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmcHdoAdapterRoutingControl(DmcHdoAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
