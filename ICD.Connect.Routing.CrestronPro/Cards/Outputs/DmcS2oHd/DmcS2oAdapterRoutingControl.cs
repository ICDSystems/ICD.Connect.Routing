
#if !NETSTANDARD

namespace ICD.Connect.Routing.CrestronPro.Cards.Outputs.DmcS2oHd
{
// ReSharper disable once InconsistentNaming
	public sealed class DmcS2oHdAdapterRoutingControl : AbstractOutputCardAdapterRoutingControl<DmcS2oHdAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmcS2oHdAdapterRoutingControl(DmcS2oHdAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
