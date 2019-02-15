using ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kCDspBase;

#if SIMPLSHARP

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kzCDsp
{
	public sealed class Dmc4kzCDspAdapterRoutingControl : AbstractDmc4kCDspBaseAdapterRoutingControl<Dmc4kzCDspAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public Dmc4kzCDspAdapterRoutingControl(Dmc4kzCDspAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
