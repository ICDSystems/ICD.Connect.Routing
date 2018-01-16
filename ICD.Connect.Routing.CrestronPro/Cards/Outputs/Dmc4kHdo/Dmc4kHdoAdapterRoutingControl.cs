
#if SIMPLSHARP

namespace ICD.Connect.Routing.CrestronPro.Cards.Outputs.Dmc4kHdo
{
// ReSharper disable once InconsistentNaming
	public sealed class Dmc4kHdoAdapterRoutingControl : AbstractOutputCardAdapterRoutingControl<Dmc4kHdoAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public Dmc4kHdoAdapterRoutingControl(Dmc4kHdoAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
