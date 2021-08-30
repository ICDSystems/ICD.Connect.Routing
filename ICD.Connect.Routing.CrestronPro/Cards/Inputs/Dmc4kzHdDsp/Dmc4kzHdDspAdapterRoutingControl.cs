using ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kHdDspBase;

#if !NETSTANDARD

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kzHdDsp
{
	public sealed class Dmc4kzHdDspAdapterRoutingControl : AbstractDmc4kHdDspBaseAdapterRoutingControl<Dmc4kzHdDspAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public Dmc4kzHdDspAdapterRoutingControl(Dmc4kzHdDspAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
