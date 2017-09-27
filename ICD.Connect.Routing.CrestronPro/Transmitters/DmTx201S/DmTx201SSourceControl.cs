#if SIMPLSHARP
using ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200Base;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx201S
{
	public sealed class DmTx201SSourceControl :
		AbstractDmTxBaseSourceControl
			<DmTx201SAdapter, Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201S>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		public DmTx201SSourceControl(DmTx201SAdapter parent)
			: base(parent)
		{
		}
	}
}
#endif
