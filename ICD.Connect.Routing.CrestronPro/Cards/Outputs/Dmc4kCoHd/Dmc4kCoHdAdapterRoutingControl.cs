
#if SIMPLSHARP

namespace ICD.Connect.Routing.CrestronPro.Cards.Outputs.Dmc4kCoHd
{
// ReSharper disable once InconsistentNaming
	public sealed class Dmc4kCoHdAdapterRoutingControl : AbstractOutputCardAdapterRoutingControl<Dmc4kCoHdAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public Dmc4kCoHdAdapterRoutingControl(Dmc4kCoHdAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
