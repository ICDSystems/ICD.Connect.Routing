using ICD.Connect.Routing.CrestronPro.Cards.Outputs.Dmc4kCoHdBase;

#if SIMPLSHARP

namespace ICD.Connect.Routing.CrestronPro.Cards.Outputs.Dmc4kzCoHd
{
// ReSharper disable once InconsistentNaming
	public sealed class Dmc4kzCoHdAdapterRoutingControl : AbstractDmc4kCoHdBaseAdapterRoutingControl<Dmc4kzCoHdAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public Dmc4kzCoHdAdapterRoutingControl(Dmc4kzCoHdAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
