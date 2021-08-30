using ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kHdBase;

#if !NETSTANDARD

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kzHd
{
	public sealed class Dmc4kzHdAdapterRoutingControl : AbstractDmc4kHdBaseAdapterRoutingControl<Dmc4kzHdAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public Dmc4kzHdAdapterRoutingControl(Dmc4kzHdAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
