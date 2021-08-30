using ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kHdBase;

#if !NETSTANDARD

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kHd
{
	public sealed class Dmc4kHdAdapterRoutingControl : AbstractDmc4kHdBaseAdapterRoutingControl<Dmc4kHdAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public Dmc4kHdAdapterRoutingControl(Dmc4kHdAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
