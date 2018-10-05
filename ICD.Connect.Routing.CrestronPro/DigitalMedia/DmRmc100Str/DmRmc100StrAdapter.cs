﻿#if SIMPLSHARP
using Crestron.SimplSharpPro;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.Dm100xStrBase;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmRmc100Str
{
#if SIMPLSHARP
	public sealed class DmRmc100StrAdapter :
		AbstractDm100XStrBaseAdapter<Crestron.SimplSharpPro.DM.Streaming.DmRmc100Str, DmRmc100StrAdapterSettings>
#else
	public sealed class DmRmc100StrAdapter : AbstractDm100XStrBaseAdapter<DmRmc100StrAdapterSettings>
#endif
	{
#if SIMPLSHARP
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="ethernetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Streaming.DmRmc100Str InstantiateSwitcher(uint ethernetId,
		                                                                                       CrestronControlSystem
			                                                                                       controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Streaming.DmRmc100Str(ethernetId, controlSystem);
		}
#endif
	}

	[KrangSettings("DmRmc100Str", typeof(DmRmc100StrAdapter))]
	public sealed class DmRmc100StrAdapterSettings : AbstractDm100XStrBaseAdapterSettings
	{
	}
}
