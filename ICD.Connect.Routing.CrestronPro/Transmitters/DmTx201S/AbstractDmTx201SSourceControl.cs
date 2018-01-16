#if SIMPLSHARP
using ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200Base;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx201S
{
	public abstract class AbstractDmTx201SSourceControl<TDevice, TTransmitter> :
		AbstractDmTxBaseSourceControl<TDevice, TTransmitter>
		where TDevice : IDmTx201SAdapter
		where TTransmitter : Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201S
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		protected AbstractDmTx201SSourceControl(TDevice parent)
			: base(parent)
		{
		}
	}
}

#endif
