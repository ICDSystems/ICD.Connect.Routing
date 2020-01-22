using ICD.Connect.Routing.CrestronPro.Cards.Outputs.Dmc4kCoHdBase;

#if SIMPLSHARP

namespace ICD.Connect.Routing.CrestronPro.Cards.Outputs.Dmc4kzHdo
{
// ReSharper disable once InconsistentNaming
	public sealed class Dmc4kzHdoAdapterRoutingControl : AbstractDmc4kCoHdBaseAdapterRoutingControl<Dmc4kzHdoAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public Dmc4kzHdoAdapterRoutingControl(Dmc4kzHdoAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
