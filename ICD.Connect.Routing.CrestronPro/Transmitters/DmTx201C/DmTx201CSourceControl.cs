using ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200Base;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx201C
{
	public sealed class DmTx201CSourceControl :
		AbstractDmTxBaseSourceControl
			<DmTx201CAdapter, DmTx201CAdapterSettings, global::Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201C>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		public DmTx201CSourceControl(DmTx201CAdapter parent)
			: base(parent)
		{
		}
	}
}
