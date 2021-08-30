#if !NETSTANDARD
namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kCBase
{
	public abstract class AbstractDmc4kCBaseAdapterRoutingControl<TParent> : AbstractInputCardAdapterRoutingControl<TParent>
		where TParent : IInputCardAdapter
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractDmc4kCBaseAdapterRoutingControl(TParent parent, int id)
			: base(parent, id)
		{
		}
	}
}
#endif