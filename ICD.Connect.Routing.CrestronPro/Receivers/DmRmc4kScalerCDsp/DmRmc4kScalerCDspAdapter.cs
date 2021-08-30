using System;
using ICD.Connect.Misc.CrestronPro.Devices;
using ICD.Connect.Routing.CrestronPro.Receivers.AbstractDmRmcScalerC;
#if !NETSTANDARD
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
#endif

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmc4kScalerCDsp
{
	/// <summary>
	/// DmRmc4kScalerCDspAdapter wraps a DmRmc4kScalerCDsp to provide a routing device.
	/// </summary>
#if !NETSTANDARD
// ReSharper disable once InconsistentNaming
	public sealed class DmRmc4kScalerCDspAdapter :
		AbstractDmRmcScalerCAdapter
			<Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerCDsp, DmRmc4kScalerCDspAdapterSettings>
	{

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="io"></param>
		/// <param name="address"></param>
		/// <returns></returns>
		public override Cec GetCecPort(eInputOuptut io, int address)
		{
			if (Receiver == null)
				throw new InvalidOperationException("No scaler instantiated");

			if (io == eInputOuptut.Input && address == DM_INPUT_ADDRESS)
				return Receiver.DmInput.StreamCec;

			return base.GetCecPort(io, address);
		}

		public override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerCDsp InstantiateReceiver(byte ipid,
		                                                                                                    CrestronControlSystem
			                                                                                                    controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerCDsp(ipid, controlSystem);
		}

		public override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerCDsp InstantiateReceiver(byte ipid,
		                                                                                                    DMOutput output)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerCDsp(ipid, output);
		}

		public override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerCDsp InstantiateReceiver(DMOutput output)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerCDsp(output);
		}
	}

#else
    public sealed class DmRmc4kScalerCDspAdapter : AbstractDmRmcScalerCAdapter<DmRmc4kScalerCDspAdapterSettings>
    {
    }
#endif
}
