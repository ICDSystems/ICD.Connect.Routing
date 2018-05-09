#if SIMPLSHARP
using ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200Base;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx201S
{
	public sealed class DmTx201S2SourceControl :
		AbstractDmTxBaseSourceControl
			<DmTx201S2Adapter, Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201S2>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmTx201S2SourceControl(DmTx201S2Adapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
