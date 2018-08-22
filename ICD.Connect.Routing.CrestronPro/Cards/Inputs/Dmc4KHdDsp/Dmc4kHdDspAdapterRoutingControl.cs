#if SIMPLSHARP

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4KHdDsp
{
	public sealed class Dmc4kHdDspAdapterRoutingControl : AbstractInputCardAdapterRoutingControl<Dmc4kHdDspAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public Dmc4kHdDspAdapterRoutingControl(Dmc4kHdDspAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
