#if SIMPLSHARP
namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kCDspBase
{
	public abstract class AbstractDmc4kCDspBaseAdapterRoutingControl<TParent> : AbstractInputCardAdapterRoutingControl<TParent>
		where TParent : ICardAdapter
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractDmc4kCDspBaseAdapterRoutingControl(TParent parent, int id)
			: base(parent, id)
		{
		}
	}
}
#endif
