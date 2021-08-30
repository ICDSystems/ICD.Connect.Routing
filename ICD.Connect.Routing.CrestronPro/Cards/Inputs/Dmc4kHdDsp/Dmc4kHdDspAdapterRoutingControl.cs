using ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kHdDspBase;

#if !NETSTANDARD

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kHdDsp
{
	public sealed class Dmc4kHdDspAdapterRoutingControl : AbstractDmc4kHdDspBaseAdapterRoutingControl<Dmc4kHdDspAdapter>
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
