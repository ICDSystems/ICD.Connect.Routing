using ICD.Connect.Routing.CrestronPro.Cards.Outputs.Dmc4kCoHdBase;

#if !NETSTANDARD

namespace ICD.Connect.Routing.CrestronPro.Cards.Outputs.DmcCatoHd
{
// ReSharper disable once InconsistentNaming
	public sealed class DmcCatoHdAdapterRoutingControl : AbstractDmc4kCoHdBaseAdapterRoutingControl<DmcCatoHdAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmcCatoHdAdapterRoutingControl(DmcCatoHdAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
