#if !NETSTANDARD

namespace ICD.Connect.Routing.CrestronPro.Cards.Outputs.Dmc4kCoHdBase
{
// ReSharper disable once InconsistentNaming
	public abstract class AbstractDmc4kCoHdBaseAdapterRoutingControl<TParent> : AbstractOutputCardAdapterRoutingControl<TParent>
		where TParent : IOutputCardAdapter
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractDmc4kCoHdBaseAdapterRoutingControl(TParent parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
