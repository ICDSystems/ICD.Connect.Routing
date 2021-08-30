#if !NETSTANDARD
namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kHdBase
{
	public abstract class AbstractDmc4kHdBaseAdapterRoutingControl<TParent> : AbstractInputCardAdapterRoutingControl<TParent>
		where TParent : IInputCardAdapter
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractDmc4kHdBaseAdapterRoutingControl(TParent parent, int id)
			: base(parent, id)
		{
		}
	}
}
#endif
