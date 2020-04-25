using System;
#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
#endif
using ICD.Connect.Misc.CrestronPro.Devices;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxBaseClass;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxE3X
{
#if SIMPLSHARP
	public abstract class AbstractDmNvxE3XAdapter<TSwitcher, TSettings> :
		AbstractDmNvxBaseClassAdapter<TSwitcher, TSettings>, IDmNvxE3XAdapter
		where TSwitcher : Crestron.SimplSharpPro.DM.Streaming.DmNvxE3x
#else
	public abstract class AbstractDmNvxE3XAdapter<TSettings> : AbstractDmNvxBaseClassAdapter<TSettings>, IDmNvxE3XAdapter
#endif
		where TSettings : IDmNvxE3XAdapterSettings, new()
	{
#if SIMPLSHARP
		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="io"></param>
		/// <param name="address"></param>
		/// <returns></returns>
		public override Cec GetCecPort(eInputOuptut io, int address)
		{
			if (Streamer == null)
				throw new InvalidOperationException("No streamer instantiated");

			if (io == eInputOuptut.Input)
			{
				switch (address)
				{
					case 1:
						return Streamer.HdmiIn[1].StreamCec;
				}
			}

			string message = string.Format("No CecPort at address {1}:{2} for device {0}", this, io, address);
			throw new InvalidOperationException(message);
		}
#endif
	}
}
