#if !NETSTANDARD

namespace ICD.Connect.Routing.CrestronPro.Cards.Outputs.DmcCoHd
{
// ReSharper disable once InconsistentNaming
	public sealed class DmcCoHdAdapterRoutingControl : AbstractOutputCardAdapterRoutingControl<DmcCoHdAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmcCoHdAdapterRoutingControl(DmcCoHdAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
