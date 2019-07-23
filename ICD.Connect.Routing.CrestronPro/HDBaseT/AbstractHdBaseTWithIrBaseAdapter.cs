﻿using System;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
#endif

namespace ICD.Connect.Routing.CrestronPro.HDBaseT
{
#if SIMPLSHARP
	public abstract class AbstractHdBaseTWithIrBaseAdapter<TDevice, TSettings> : AbstractHdBaseTBaseAdapter<TDevice, TSettings>, IHdBaseTWithIrBaseAdapter
		where TDevice : HDBaseTWithIrBase
#else
	public abstract class AbstractHdBaseTWithIrBaseAdapter<TSettings> : AbstractHdBaseTBaseAdapter<TSettings>, IHdBaseTWithIrBaseAdapter
#endif
		where TSettings : IHdBaseTWithIrBaseAdapterSettings, new()
	{
#if SIMPLSHARP
		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override IROutputPort GetIrOutputPort(int address)
		{
			throw new NotImplementedException();
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
