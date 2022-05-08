using System;
using ICD.Connect.Routing.Controls;
#if !NETSTANDARD
using Crestron.SimplSharpPro.DM;
#endif
using ICD.Connect.Misc.CrestronPro.Devices;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxBaseClass;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxD3X
{
#if !NETSTANDARD
	public abstract class AbstractDmNvxD3XAdapter<TSwitcher, TSettings> :
		AbstractDmNvxBaseClassAdapter<TSwitcher, TSettings>, IDmNvxD3XAdapter
		where TSwitcher : Crestron.SimplSharpPro.DM.Streaming.DmNvxD3x
#else
	public abstract class AbstractDmNvxD3XAdapter<TSettings> : AbstractDmNvxBaseClassAdapter<TSettings>, IDmNvxD3XAdapter
#endif
		where TSettings : IDmNvxD3XAdapterSettings, new()
	{
#if !NETSTANDARD
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

			if (io == eInputOuptut.Output && address == 1)
				return Streamer.HdmiOut.StreamCec;

			string message = string.Format("No CecPort at address {1}:{2} for device {0}", this, io, address);
			throw new InvalidOperationException(message);
		}

		/// <summary>
		/// Get the switcher control for this device
		/// </summary>
		/// <returns></returns>
		protected override IRouteSwitcherControl GetSwitcherControl()
		{
			return new DmNvxD3XSwitcherControl(this, 0);
		}
#endif
	}
}
