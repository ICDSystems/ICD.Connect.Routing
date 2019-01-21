using System;
#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
#endif
using ICD.Connect.Misc.CrestronPro.Devices;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxBaseClass;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvx35X
{
#if SIMPLSHARP
	public abstract class AbstractDmNvx35XAdapter<TSwitcher, TSettings> :
		AbstractDmNvxBaseClassAdapter<TSwitcher, TSettings>, IDmNvx35XAdapter
		where TSwitcher : Crestron.SimplSharpPro.DM.Streaming.DmNvx35x
#else
	public abstract class AbstractDmNvx35XAdapter<TSettings> : AbstractDmNvxBaseClassAdapter<TSettings>, IDmNvx35XAdapter
#endif
		where TSettings : IDmNvx35XAdapterSettings, new()
	{
		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override Cec GetCecPort(eInputOuptut io, int address)
		{
			if (Streamer == null)
				throw new InvalidOperationException("No streamer instantiated");

			if (io == eInputOuptut.Output && address == 1)
				return Streamer.HdmiOut.StreamCec;
			if (io == eInputOuptut.Input)
			{
				switch (address)
				{
					case 1:
						return Streamer.HdmiIn[1].StreamCec;
					case 2:
						return Streamer.HdmiIn[2].StreamCec;
				}
			}

			string message = string.Format("No CecPort at address {1}:{2} for device {0}", this, io, address);
			throw new InvalidOperationException(message);
		}
	}
}
