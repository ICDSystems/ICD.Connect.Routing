
#if SIMPLSHARP

namespace ICD.Connect.Routing.CrestronPro.Cards.Outputs.DmcSoHd
{
// ReSharper disable once InconsistentNaming
	public sealed class DmcSoHdAdapterRoutingControl : AbstractOutputCardAdapterRoutingControl<DmcSoHdAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmcSoHdAdapterRoutingControl(DmcSoHdAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
