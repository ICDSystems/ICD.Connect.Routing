#if SIMPLSHARP
using ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200Base;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx201S
{
	public sealed class DmTx201CSourceControl :
		AbstractDmTxBaseSourceControl
			<DmTx201CAdapter, Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201C>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmTx201CSourceControl(DmTx201CAdapter parent, int id)
			: base(parent, id)
		{
		}
	}
}

#endif
