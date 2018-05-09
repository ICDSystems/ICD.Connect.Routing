#if SIMPLSHARP
using ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200Base;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200C2G
{
	public sealed class DmTx200C2GSourceControl :
		AbstractDmTxBaseSourceControl
			<DmTx200C2GAdapter, Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200C2G>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmTx200C2GSourceControl(DmTx200C2GAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
