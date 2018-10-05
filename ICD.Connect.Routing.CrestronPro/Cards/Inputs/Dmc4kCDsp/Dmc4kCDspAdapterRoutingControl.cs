#if SIMPLSHARP

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kCDsp
{
	public sealed class Dmc4kCDspAdapterRoutingControl : AbstractInputCardAdapterRoutingControl<Dmc4kCDspAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public Dmc4kCDspAdapterRoutingControl(Dmc4kCDspAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
