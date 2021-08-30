using System;
#if !NETSTANDARD
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
#endif

namespace ICD.Connect.Routing.CrestronPro.HDBaseT
{
#if !NETSTANDARD
	public abstract class AbstractHdBaseTWithIrBaseAdapter<TDevice, TSettings> : AbstractHdBaseTBaseAdapter<TDevice, TSettings>, IHdBaseTWithIrBaseAdapter
		where TDevice : HDBaseTWithIrBase
#else
	public abstract class AbstractHdBaseTWithIrBaseAdapter<TSettings> : AbstractHdBaseTBaseAdapter<TSettings>, IHdBaseTWithIrBaseAdapter
#endif
		where TSettings : IHdBaseTWithIrBaseAdapterSettings, new()
	{
#if !NETSTANDARD
		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override IROutputPort GetIrOutputPort(int address)
		{
			if (Device == null)
				throw new InvalidOperationException("No device instantiated");

			return Device.IROutputPorts[(uint)address];
		}
#endif
	}

	public abstract class AbstractHdBaseTWithIrBaseAdapterSettings : AbstractHdBaseTBaseAdapterSettings, IHdBaseTWithIrBaseAdapterSettings
	{
	}

	public interface IHdBaseTWithIrBaseAdapter : IHdBaseTBaseAdapter
	{
	}

	public interface IHdBaseTWithIrBaseAdapterSettings : IHdBaseTBaseAdapterSettings
	{
	}
}
