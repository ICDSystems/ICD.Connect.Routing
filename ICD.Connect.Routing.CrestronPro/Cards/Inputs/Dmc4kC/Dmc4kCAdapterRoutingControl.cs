using ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kCBase;

#if SIMPLSHARP

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kC
{
	public sealed class Dmc4kCAdapterRoutingControl : AbstractDmc4kCBaseAdapterRoutingControl<Dmc4kCAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public Dmc4kCAdapterRoutingControl(Dmc4kCAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
