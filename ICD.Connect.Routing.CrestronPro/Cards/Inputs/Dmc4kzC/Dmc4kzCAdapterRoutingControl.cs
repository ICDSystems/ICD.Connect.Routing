using ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kCBase;

#if !NETSTANDARD

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kzC
{
	public sealed class Dmc4kzCAdapterRoutingControl : AbstractDmc4kCBaseAdapterRoutingControl<Dmc4kzCAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public Dmc4kzCAdapterRoutingControl(Dmc4kzCAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
