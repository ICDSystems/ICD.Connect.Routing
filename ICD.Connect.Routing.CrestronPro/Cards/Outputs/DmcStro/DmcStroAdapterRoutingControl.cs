
#if SIMPLSHARP

namespace ICD.Connect.Routing.CrestronPro.Cards.Outputs.DmcStro
{
// ReSharper disable once InconsistentNaming
	public sealed class DmcStroAdapterRoutingControl : AbstractOutputCardAdapterRoutingControl<DmcStroAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmcStroAdapterRoutingControl(DmcStroAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
