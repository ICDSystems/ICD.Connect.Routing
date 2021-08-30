#if !NETSTANDARD
namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kHdDspBase
{
	public abstract class AbstractDmc4kHdDspBaseAdapterRoutingControl<TParent> : AbstractInputCardAdapterRoutingControl<TParent>
		where TParent : IInputCardAdapter
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractDmc4kHdDspBaseAdapterRoutingControl(TParent parent, int id)
			: base(parent, id)
		{
		}
	}
}
#endif
