#if !NETSTANDARD

namespace ICD.Connect.Routing.CrestronPro.Cards.Outputs.DmcFo
{
// ReSharper disable once InconsistentNaming
	public sealed class DmcFoAdapterRoutingControl : AbstractOutputCardAdapterRoutingControl<DmcFoAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmcFoAdapterRoutingControl(DmcFoAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
